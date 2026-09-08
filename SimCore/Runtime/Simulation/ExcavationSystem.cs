using System.Collections.Generic;

namespace LegoSpaceRTS.SimCore
{
public struct ExcavationJob
{
    public EntityId Feature;
    public ushort TotalTicks;
    public ushort RemainingTicks;
}

public sealed class ExcavationSystem : ISimSystem
{
    public const ushort DrillCraftStandardTicks = 400;
    public const ushort DrillCraftReinforcedTicks = 800;
    public const ushort ChromeCrusherStandardTicks = 500;
    public const ushort ChromeCrusherReinforcedTicks = 900;
    public const ushort GraniteGrinderStandardTicks = 600;
    public const ushort GraniteGrinderReinforcedTicks = 1100;
    public const ushort StandardEnergy = 25;
    public const ushort ReinforcedEnergy = 50;
    private static readonly ContentId DrillCraft = StableId.FromKey("unit.rock_raiders.drill_craft");
    private static readonly ContentId ChromeCrusher = StableId.FromKey("unit.rock_raiders.chrome_crusher");
    private static readonly ContentId GraniteGrinder = StableId.FromKey("unit.rock_raiders.granite_grinder");
    private static readonly ContentId ReinforcedResearch = StableId.FromKey("research.rr.reinforced_drilling_assemblies");

    public void Step(SimulationWorld world)
    {
        if (world.ExcavationJobs.Count == 0) return;
        List<uint> excavators = new(world.ExcavationJobs.Keys); excavators.Sort();
        for (int i = 0; i < excavators.Count; i++)
        {
            uint key = excavators[i];
            if (!world.Entities.Exists(new EntityId(key))) { world.ExcavationJobs.Remove(key); continue; }
            ExcavationJob job = world.ExcavationJobs[key];
            if (!world.Entities.Excavatable.TryGet(job.Feature, out Excavatable feature) || feature.State == ExcavatableFeatureState.Open)
            { world.ExcavationJobs.Remove(key); continue; }
            if (job.RemainingTicks > 0) job.RemainingTicks--;
            if (job.RemainingTicks > 0) { world.ExcavationJobs[key] = job; continue; }
            world.OpenExcavatable(feature.MapFeatureId);
            world.ExcavationJobs.Remove(key);
        }
    }

    public static bool TryStart(SimulationWorld world, byte playerSlot, EntityId excavator, EntityId featureEntity)
    {
        if (world.ExcavationJobs.ContainsKey(excavator.Value) || !world.Entities.Ownership.TryGet(excavator, out Ownership owner) || owner.PlayerSlot != playerSlot ||
            !world.Entities.Selectable.TryGet(excavator, out Selectable selectable) ||
            !world.Entities.Excavatable.TryGet(featureEntity, out Excavatable feature) || feature.State != ExcavatableFeatureState.Blocked ||
            !TryDuration(selectable.ContentType, IsReinforced(feature.TerrainClass), out ushort ticks)) return false;
        if (selectable.ContentType == DrillCraft && IsReinforced(feature.TerrainClass) && !ResearchSystem.HasCompleted(world, playerSlot, ReinforcedResearch)) return false;
        ushort energy = IsReinforced(feature.TerrainClass) ? ReinforcedEnergy : StandardEnergy;
        if (feature.RequiredEnergy != energy || !EnergyDomainSystem.TryResolveForEntity(world, excavator, playerSlot, out EntityId root) ||
            !EnergyDomainSystem.TrySpend(world, root, energy)) return false;
        if (world.Entities.Navigation.Has(excavator) && world.Entities.Movement.Has(excavator))
        {
            ref NavigationAgent nav = ref world.Entities.Navigation.Get(excavator); ref Movement movement = ref world.Entities.Movement.Get(excavator);
            CommandExecutionSystem.StopMovement(world, excavator, ref nav, ref movement); world.GetQueue(excavator).Clear();
        }
        world.Entities.Excavatable.Get(featureEntity).State = ExcavatableFeatureState.ActiveExcavation;
        world.Map.SetFeatureState(feature.MapFeatureId, ExcavatableFeatureState.ActiveExcavation);
        world.ExcavationJobs.Add(excavator.Value, new ExcavationJob { Feature = featureEntity, TotalTicks = ticks, RemainingTicks = ticks });
        return true;
    }

    public static bool TryDuration(ContentId unitType, bool reinforced, out ushort ticks)
    {
        ticks = unitType == DrillCraft ? (reinforced ? DrillCraftReinforcedTicks : DrillCraftStandardTicks) :
            unitType == ChromeCrusher ? (reinforced ? ChromeCrusherReinforcedTicks : ChromeCrusherStandardTicks) :
            unitType == GraniteGrinder ? (reinforced ? GraniteGrinderReinforcedTicks : GraniteGrinderStandardTicks) : (ushort)0;
        return ticks != 0;
    }

    private static bool IsReinforced(ExcavatableTerrainClass terrainClass) => terrainClass == ExcavatableTerrainClass.ReinforcedBedrockBarrier;
}
}
