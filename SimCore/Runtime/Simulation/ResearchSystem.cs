using System;
using System.Collections.Generic;

namespace LegoSpaceRTS.SimCore
{
public struct ResearchJob
{
    public ContentId ResearchType;
    public EntityId FundingBank;
    public EntityId CrystalFundingBank;
    public EntityId EnergyDomainRoot;
    public ushort OreCost;
    public ushort EnergyCost;
    public byte CrystalCost;
    public ushort TotalTicks;
    public ushort RemainingTicks;
}

public sealed class ResearchSystem : ISimSystem
{
    public void Step(SimulationWorld world)
    {
        if (world.ResearchJobs.Count == 0) return;
        List<uint> providers = new(world.ResearchJobs.Keys); providers.Sort();
        for (int i = 0; i < providers.Count; i++)
        {
            EntityId provider = new(providers[i]);
            if (!world.Entities.Exists(provider) || !BrownoutSystem.IsOperational(world, provider)) continue;
            ResearchJob job = world.ResearchJobs[providers[i]];
            if (!world.Content.TryGetResearch(job.ResearchType, out ResearchDefinition definition) ||
                !PrerequisitesMet(world, world.Entities.Ownership.Get(provider).PlayerSlot, definition, whileResearchingOnly: true)) continue;
            if (job.RemainingTicks > 0) job.RemainingTicks--;
            if (job.RemainingTicks > 0) { world.ResearchJobs[providers[i]] = job; continue; }
            byte player = world.Entities.Ownership.Get(provider).PlayerSlot;
            world.CompletedResearch.Add(Key(player, job.ResearchType));
            world.ResearchJobs.Remove(providers[i]);
            ApplyKnownEffects(world, player, definition);
        }
    }

    public static bool TryStart(SimulationWorld world, byte playerSlot, EntityId provider, ContentId researchType)
    {
        if (world.ResearchJobs.ContainsKey(provider.Value) || HasCompleted(world, playerSlot, researchType) ||
            !world.Entities.Ownership.TryGet(provider, out Ownership owner) || owner.PlayerSlot != playerSlot ||
            !world.Entities.Building.TryGet(provider, out Building building) || building.State != BuildingState.Completed ||
            !world.Content.TryGetResearch(researchType, out ResearchDefinition definition) || definition.SourceBuildingType != building.Type ||
            !PrerequisitesMet(world, playerSlot, definition, false)) return false;
        EntityId component = WorksiteGraphSystem.TryGetComponentForEntity(world, provider, out EntityId root) ? root : EntityId.None;
        if (!ProductionSystem.TryFindFundingBank(world, playerSlot, component, ResourceType.Ore, definition.OreCost, provider, out EntityId oreBank) ||
            !ProductionSystem.TryFindFundingBank(world, playerSlot, component, ResourceType.Crystal, definition.CrystalCost, provider, out EntityId crystalBank) ||
            !EnergyDomainSystem.TryResolveForEntity(world, provider, playerSlot, out EntityId energyRoot) ||
            !EnergyDomainSystem.CanSpend(world, energyRoot, definition.EnergyCost)) return false;
        if (!EnergyDomainSystem.TrySpend(world, energyRoot, definition.EnergyCost)) return false;
        if (!ProductionSystem.TrySpend(world, playerSlot, oreBank, ResourceType.Ore, definition.OreCost))
        {
            EnergyDomainSystem.Refund(world, energyRoot, definition.EnergyCost);
            return false;
        }
        if (!ProductionSystem.TrySpend(world, playerSlot, crystalBank, ResourceType.Crystal, definition.CrystalCost))
        {
            ProductionSystem.RefundResource(world, playerSlot, ResourceType.Ore, definition.OreCost, oreBank);
            EnergyDomainSystem.Refund(world, energyRoot, definition.EnergyCost);
            return false;
        }
        world.ResearchJobs.Add(provider.Value, new ResearchJob
        {
            ResearchType = researchType, FundingBank = oreBank, CrystalFundingBank = crystalBank, EnergyDomainRoot = energyRoot,
            OreCost = definition.OreCost, EnergyCost = definition.EnergyCost, CrystalCost = definition.CrystalCost,
            TotalTicks = definition.ResearchTicks, RemainingTicks = definition.ResearchTicks
        });
        return true;
    }

    public static bool TryCancel(SimulationWorld world, byte playerSlot, EntityId provider)
    {
        if (!world.ResearchJobs.TryGetValue(provider.Value, out ResearchJob job) ||
            !world.Entities.Ownership.TryGet(provider, out Ownership owner) || owner.PlayerSlot != playerSlot) return false;
        int progress = job.TotalTicks - job.RemainingTicks;
        ProductionSystem.RefundResource(world, playerSlot, ResourceType.Ore,
            CancellationAccounting.Refund(job.OreCost, progress, job.TotalTicks), job.FundingBank);
        if (world.Entities.EnergyDomain.Has(job.EnergyDomainRoot)) EnergyDomainSystem.Refund(world, job.EnergyDomainRoot,
            CancellationAccounting.Refund(job.EnergyCost, progress, job.TotalTicks));
        if (!CancellationAccounting.CrystalsCommitted(progress, job.TotalTicks))
            ProductionSystem.RefundResource(world, playerSlot, ResourceType.Crystal, job.CrystalCost, job.CrystalFundingBank);
        world.ResearchJobs.Remove(provider.Value);
        return true;
    }

    public static bool HasCompleted(SimulationWorld world, byte playerSlot, ContentId researchType)
        => world.CompletedResearch.Contains(Key(playerSlot, researchType));

    internal static ulong Key(byte playerSlot, ContentId researchType) => ((ulong)playerSlot << 32) | researchType.Value;

    private static bool PrerequisitesMet(SimulationWorld world, byte playerSlot, ResearchDefinition definition, bool whileResearchingOnly)
    {
        for (int i = 0; i < definition.PrerequisiteGroups.Length; i++)
        {
            bool met = false;
            ResearchPrerequisiteDefinition[] alternatives = definition.PrerequisiteGroups[i].Alternatives;
            for (int j = 0; j < alternatives.Length && !met; j++)
            {
                ResearchPrerequisiteDefinition p = alternatives[j];
                if (whileResearchingOnly && p.Persistence != ResearchPrerequisitePersistence.WhileResearching) { met = true; continue; }
                met = p.Kind switch
                {
                    ResearchPrerequisiteKind.Research => HasCompleted(world, playerSlot, p.TargetId),
                    ResearchPrerequisiteKind.Building => ActionPrerequisites.HasCompletedBuilding(world, playerSlot, p.TargetId),
                    ResearchPrerequisiteKind.StateThreshold => StateThresholdMet(world, playerSlot, p),
                    _ => false
                };
            }
            if (!met) return false;
        }
        return true;
    }

    private static bool StateThresholdMet(SimulationWorld world, byte playerSlot, ResearchPrerequisiteDefinition prerequisite)
    {
        if (prerequisite.TargetStableKey == "state.ali.committed_crystals")
        {
            int count = 0; IReadOnlyList<EntityId> alive = world.Entities.Alive;
            for (int i = 0; i < alive.Count; i++) if (world.Entities.Ownership.TryGet(alive[i], out Ownership owner) && owner.PlayerSlot == playerSlot && world.Entities.ResonanceCore.TryGet(alive[i], out ResonanceCore core)) count += ResonanceCoreSystem.CountCommitted(core);
            return count >= prerequisite.MinimumValue;
        }
        return false;
    }

    private static void ApplyKnownEffects(SimulationWorld world, byte playerSlot, ResearchDefinition definition)
    {
        IReadOnlyList<EntityId> alive = world.Entities.Alive;
        for (int i = 0; i < alive.Count; i++)
        {
            EntityId id = alive[i];
            if (!world.Entities.Ownership.TryGet(id, out Ownership owner) || owner.PlayerSlot != playerSlot) continue;
            if (definition.Id == StableId.FromKey("research.ast.field_survey_package") && world.Entities.MissionRefitState.Has(id)) world.Entities.MissionRefitState.Get(id).SurveyUnlocked = true;
            if (definition.Id == StableId.FromKey("research.ali.expanded_resonance_lattice") && world.Entities.ResonanceCore.Has(id)) world.Entities.ResonanceCore.Get(id).ExpandedLatticeUnlocked = true;
            if (definition.Id == StableId.FromKey("research.mar.redundant_routing") && world.Entities.TubeStation.Has(id)) TubeGraphSystem.TrySetRedundantRouting(world, playerSlot, id, true);
            if (definition.Id == StableId.FromKey("research.mar.hypersled_throughput") && world.Entities.TubeStation.Has(id)) TubeGraphSystem.TrySetHypersledThroughput(world, playerSlot, id, true);
        }
        if (definition.Id == StableId.FromKey("research.ali.resonance_initiation")) world.GetAlienChargeRef(playerSlot).ResonanceInitiationUnlocked = true;
    }
}
}
