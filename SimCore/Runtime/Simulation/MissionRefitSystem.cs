using System.Collections.Generic;

namespace LegoSpaceRTS.SimCore
{
public sealed class MissionRefitSystem : ISimSystem
{
    public const ushort FirstSurveyInstallOre = 25;
    public const ushort FirstSurveyInstallEnergy = 10;
    public const ushort FirstSurveyInstallTicks = 18 * EnergyDomainSystem.TicksPerSecond;
    public const ushort LaterSwapOre = 8;
    public const ushort LaterSwapEnergy = 5;
    public const ushort LaterSwapTicks = 10 * EnergyDomainSystem.TicksPerSecond;
    public const ushort ConfigurationLockTicks = 20 * EnergyDomainSystem.TicksPerSecond;

    private static readonly ContentId T3TrikeType = StableId.FromKey("unit.ast.t3_trike");

    public void Step(SimulationWorld world)
    {
        IReadOnlyList<EntityId> alive = world.Entities.Alive;
        for (int i = 0; i < alive.Count; i++)
        {
            EntityId id = alive[i];
            if (!IsT3(world, id)) continue;
            EnsureState(world, id);
            ref MissionRefitState state = ref world.Entities.MissionRefitState.Get(id);
            if (!world.Entities.MissionRefitJob.Has(id))
            {
                if (state.ConfigurationLockTicks > 0) state.ConfigurationLockTicks--;
                continue;
            }
            ref MissionRefitJob job = ref world.Entities.MissionRefitJob.Get(id);
            if (job.RemainingTicks > 0) job.RemainingTicks--;
            if (job.RemainingTicks > 0) continue;
            state.CurrentConfiguration = job.NewConfiguration;
            state.OwnedConfigurationMask |= ConfigurationBit(job.NewConfiguration);
            state.ConfigurationLockTicks = ConfigurationLockTicks;
            ApplyT3Configuration(world, id, state.CurrentConfiguration);
            world.Entities.MissionRefitJob.Remove(id);
        }
    }

    public static bool TryStartT3Refit(SimulationWorld world, byte playerSlot, EntityId unit, MissionConfiguration target)
    {
        if (!IsT3(world, unit) || !world.Entities.Ownership.TryGet(unit, out Ownership ownership) || ownership.PlayerSlot != playerSlot ||
            world.Entities.MissionRefitJob.Has(unit) || !ForwardServiceSystem.TryGetProviderForMember(world, unit, out EntityId provider)) return false;
        EnsureState(world, unit);
        ref MissionRefitState state = ref world.Entities.MissionRefitState.Get(unit);
        if ((target != MissionConfiguration.T3Escort && target != MissionConfiguration.T3Survey) || target == state.CurrentConfiguration ||
            state.ConfigurationLockTicks > 0 || (target == MissionConfiguration.T3Survey && !state.SurveyUnlocked)) return false;

        bool ownsTarget = (state.OwnedConfigurationMask & ConfigurationBit(target)) != 0;
        ushort ore = ownsTarget ? LaterSwapOre : FirstSurveyInstallOre;
        ushort energy = ownsTarget ? LaterSwapEnergy : FirstSurveyInstallEnergy;
        ushort ticks = ownsTarget ? LaterSwapTicks : FirstSurveyInstallTicks;
        if (!TryFindOreBank(world, playerSlot, ore, out EntityId bank) ||
            !EnergyDomainSystem.TryResolveForEntity(world, provider, playerSlot, out EntityId energyRoot) ||
            !EnergyDomainSystem.CanSpend(world, energyRoot, energy)) return false;

        ref ResourceBank resourceBank = ref world.Entities.ResourceBank.Get(bank);
        resourceBank.ProcessedAmount -= ore;
        if (!EnergyDomainSystem.TrySpend(world, energyRoot, energy))
        {
            resourceBank.ProcessedAmount += ore;
            return false;
        }
        if (world.Entities.Navigation.Has(unit) && world.Entities.Movement.Has(unit))
        {
            ref NavigationAgent nav = ref world.Entities.Navigation.Get(unit);
            ref Movement movement = ref world.Entities.Movement.Get(unit);
            CommandExecutionSystem.StopMovement(world, unit, ref nav, ref movement);
            world.GetQueue(unit).Clear();
        }
        world.Entities.MissionRefitJob.Set(unit, new MissionRefitJob
        {
            Provider = provider, FundingBank = bank, EnergyDomainRoot = energyRoot,
            OldConfiguration = state.CurrentConfiguration, NewConfiguration = target,
            TotalTicks = ticks, RemainingTicks = ticks, CommittedOre = ore, CommittedEnergy = energy
        });
        return true;
    }

    public static void EnsureState(SimulationWorld world, EntityId unit)
    {
        if (!world.Entities.MissionRefitState.Has(unit))
            world.Entities.MissionRefitState.Set(unit, new MissionRefitState
            {
                CurrentConfiguration = MissionConfiguration.T3Escort,
                OwnedConfigurationMask = ConfigurationBit(MissionConfiguration.T3Escort)
            });
    }

    private static void ApplyT3Configuration(SimulationWorld world, EntityId unit, MissionConfiguration configuration)
    {
        if (world.Entities.Movement.Has(unit))
            world.Entities.Movement.Get(unit).MaxSpeed = configuration == MissionConfiguration.T3Survey ? Fix32.FromRatio(185, 100) : Fix32.FromRatio(170, 100);
        if (world.Entities.Vision.Has(unit))
            world.Entities.Vision.Get(unit).RadiusBuildCells = configuration == MissionConfiguration.T3Survey ? (byte)13 : (byte)9;
    }

    private static bool TryFindOreBank(SimulationWorld world, byte playerSlot, int amount, out EntityId bank)
    {
        IReadOnlyList<EntityId> alive = world.Entities.Alive;
        for (int i = 0; i < alive.Count; i++)
        {
            EntityId id = alive[i];
            if (world.Entities.Ownership.TryGet(id, out Ownership ownership) && ownership.PlayerSlot == playerSlot &&
                world.Entities.ResourceBank.TryGet(id, out ResourceBank candidate) && candidate.Type == ResourceType.Ore && candidate.ProcessedAmount >= amount)
            { bank = id; return true; }
        }
        bank = EntityId.None;
        return false;
    }

    private static bool IsT3(SimulationWorld world, EntityId id)
        => world.Entities.Selectable.TryGet(id, out Selectable selectable) && selectable.ContentType == T3TrikeType;

    private static byte ConfigurationBit(MissionConfiguration configuration) => configuration switch
    {
        MissionConfiguration.T3Escort => 1,
        MissionConfiguration.T3Survey => 2,
        _ => 0
    };
}
}
