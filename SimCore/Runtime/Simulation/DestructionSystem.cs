using System.Collections.Generic;

namespace LegoSpaceRTS.SimCore
{
public sealed class DestructionSystem : ISimSystem
{
    public const int StandardUnitBlockingTicks = 0;
    public const int MassiveUnitBlockingTicks = 0;
    public const int StructureBlockingTicks = 0;
    public const int StructureVisualTicks = 1;
    public const int StandardUnitVisualTicks = 160;
    public const int MassiveUnitVisualTicks = 240;

    private readonly List<EntityId> _expired = new(32);

    public void Step(SimulationWorld world)
    {
        _expired.Clear();
        bool functionsChanged = false;
        IReadOnlyList<EntityId> alive = world.Entities.Alive;
        for (int i = 0; i < alive.Count; i++)
        {
            EntityId id = alive[i];
            if (world.Entities.Destruction.TryGet(id, out DestructionState destruction))
            {
                if (world.Tick.Value >= destruction.VisualUntilTick) _expired.Add(id);
                continue;
            }
            if (!world.Entities.Health.TryGet(id, out Health health) || !health.IsDepleted) continue;
            Begin(world, id);
            functionsChanged = true;
        }

        for (int i = 0; i < _expired.Count; i++)
        {
            EntityId id = _expired[i];
            if (!world.Entities.Destruction.TryGet(id, out DestructionState destruction)) continue;
            if (destruction.Kind == DestructionKind.Structure) world.ClearDestroyedStructureFootprint(destruction);
            world.RemoveRuntimeState(id);
            world.Entities.Destroy(id);
            functionsChanged = true;
        }

        if (!functionsChanged) return;
        DisableOrphanedEnergyMembers(world);
        EnergyDomainSystem.RecalculateAll(world);
        OperationsCapacitySystem.Recalculate(world);
        world.Spatial.Rebuild(world.Entities);
    }

    private static void Begin(SimulationWorld world, EntityId id)
    {
        TransportSystem.EmergencyDeploy(world, id);
        Ownership ownership = world.Entities.Ownership.TryGet(id, out Ownership storedOwnership) ? storedOwnership : new Ownership { PlayerSlot = byte.MaxValue };
        Selectable selectable = world.Entities.Selectable.TryGet(id, out Selectable storedSelectable) ? storedSelectable : default;
        NavigationAgent navigation = world.Entities.Navigation.TryGet(id, out NavigationAgent storedNavigation) ? storedNavigation : default;
        Targetable targetable = world.Entities.Targetable.TryGet(id, out Targetable storedTargetable) ? storedTargetable : default;
        bool isStructure = world.Entities.Building.TryGet(id, out Building building);
        bool isMassive = !isStructure && (navigation.Footprint == FootprintClass.Huge || targetable.Class == CombatTargetClass.MassiveMachine);
        int blockingTicks = isStructure ? StructureBlockingTicks : isMassive ? MassiveUnitBlockingTicks : StandardUnitBlockingTicks;
        int visualTicks = isStructure ? StructureVisualTicks : isMassive ? MassiveUnitVisualTicks : StandardUnitVisualTicks;
        DestructionState destruction = new()
        {
            Kind = isStructure ? DestructionKind.Structure : DestructionKind.Unit,
            StartedTick = world.Tick.Value,
            BlockingUntilTick = checked(world.Tick.Value + blockingTicks),
            VisualUntilTick = checked(world.Tick.Value + visualTicks),
            Owner = ownership.PlayerSlot,
            ContentType = selectable.ContentType,
            SelectableKind = selectable.Kind,
            Footprint = isStructure ? FootprintClass.Huge : navigation.Footprint,
            BuildingType = isStructure ? building.Type : default,
            BuildingAnchorX = isStructure ? building.AnchorX : (short)0,
            BuildingAnchorY = isStructure ? building.AnchorY : (short)0,
            BuildingOrientation = isStructure ? building.Orientation : (byte)0,
            BuildingWidth = isStructure ? building.FootprintWidth : (byte)0,
            BuildingHeight = isStructure ? building.FootprintHeight : (byte)0
        };
        world.Entities.Destruction.Set(id, destruction);
        if (isStructure) world.ClearDestroyedStructureFootprint(destruction);

        if (world.Entities.Builder.Has(id)) ConstructionSystem.ReleaseBuilderAssignment(world, id);
        ReleaseBuildersTargeting(world, id);
        if (!isStructure && world.Entities.Navigation.Has(id) && world.Entities.Movement.Has(id))
        {
            ref NavigationAgent nav = ref world.Entities.Navigation.Get(id);
            ref Movement movement = ref world.Entities.Movement.Get(id);
            CommandExecutionSystem.StopMovement(world, id, ref nav, ref movement);
        }
        world.RemoveRuntimeState(id);

        world.Entities.Ownership.Remove(id);
        world.Entities.Selectable.Remove(id);
        world.Entities.Vision.Remove(id);
        world.Entities.ResourceNode.Remove(id);
        world.Entities.Worker.Remove(id);
        world.Entities.Builder.Remove(id);
        world.Entities.Passenger.Remove(id);
        world.Entities.Transport.Remove(id);
        world.Entities.ResourceCarrier.Remove(id);
        world.Entities.ResourceReceiver.Remove(id);
        world.Entities.ResourceBank.Remove(id);
        world.Entities.Building.Remove(id);
        world.Entities.EnergyDomain.Remove(id);
        world.Entities.EnergyDomainMember.Remove(id);
        world.Entities.PowerState.Remove(id);
        world.Entities.ConstructionSite.Remove(id);
        world.Entities.Production.Remove(id);
        world.Entities.Targetable.Remove(id);
        world.Entities.Targeting.Remove(id);
        world.Entities.Weapon.Remove(id);
        // Every depleted gameplay object becomes nonblocking in this same
        // authoritative tick. DestructionState/Transform remain only long
        // enough to publish the cosmetic debris transition.
        world.Entities.Movement.Remove(id);
        world.Entities.Navigation.Remove(id);
    }

    private static void ReleaseBuildersTargeting(SimulationWorld world, EntityId site)
    {
        IReadOnlyList<EntityId> alive = world.Entities.Alive;
        for (int i = 0; i < alive.Count; i++)
        {
            EntityId builderId = alive[i];
            if (builderId == site || !world.Entities.Builder.TryGet(builderId, out Builder builder) || builder.ConstructionTarget != site) continue;
            ConstructionSystem.ReleaseBuilderAssignment(world, builderId);
        }
    }

    private static void DisableOrphanedEnergyMembers(SimulationWorld world)
    {
        IReadOnlyList<EntityId> alive = world.Entities.Alive;
        for (int i = 0; i < alive.Count; i++)
        {
            EntityId id = alive[i];
            if (!world.Entities.EnergyDomainMember.TryGet(id, out EnergyDomainMember member) || world.Entities.EnergyDomain.Has(member.DomainRoot)) continue;
            world.Entities.EnergyDomainMember.Remove(id);
            if (!world.Entities.PowerState.Has(id)) continue;
            ref PowerState power = ref world.Entities.PowerState.Get(id);
            power.IsPowered = false;
        }
    }
}
}
