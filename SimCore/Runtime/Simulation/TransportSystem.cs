using System.Collections.Generic;

namespace LegoSpaceRTS.SimCore
{
/// <summary>Deterministic M4 T047 Rapid Rider passenger loading, unloading and emergency deployment.</summary>
public sealed class TransportSystem : ISimSystem
{
    public static readonly Fix32 InteractionRange = Fix32.FromRatio(3, 2);
    public const int DockingTicks = 30;
    public const int RapidRiderLoadTicks = 15;
    public const int UnloadSettlingTicks = 20;
    public const int RapidRiderUnloadTicks = 10;
    public const int DamagePauseTicks = 15;
    public const int EmergencyAttackLockTicks = 30;
    public const int EmergencyMovementPenaltyTicks = 50;
    private static readonly Fix32 EmergencyMovementMultiplier = Fix32.FromRatio(70, 100);

    public void Step(SimulationWorld world)
    {
        UpdatePassengerApproaches(world);
        IReadOnlyList<EntityId> alive = world.Entities.Alive;
        for (int i = 0; i < alive.Count; i++)
        {
            EntityId carrier = alive[i];
            if (!world.Entities.Transport.TryGet(carrier, out Transport transport) ||
                (world.Entities.Health.TryGet(carrier, out Health health) && health.IsDepleted)) continue;
            if (transport.JobState == TransportJobState.MovingToUnload) UpdateUnloadApproach(world, carrier);
            else if (transport.JobState == TransportJobState.UnloadSettling || transport.JobState == TransportJobState.Unloading || transport.JobState == TransportJobState.UnloadBlocked)
                UpdateUnload(world, carrier);
            else UpdateLoading(world, carrier);
        }
    }

    public static bool TryStartLoad(SimulationWorld world, EntityId passengerId, EntityId carrier, bool clearQueue)
    {
        if (!IsEligibleLoad(world, passengerId, carrier) || ReservedPoints(world, carrier) + world.Entities.Passenger.Get(passengerId).SizePoints > world.Entities.Transport.Get(carrier).CapacityPoints)
            return false;
        ref Transport transport = ref world.Entities.Transport.Get(carrier);
        if (transport.JobState == TransportJobState.MovingToUnload || transport.JobState == TransportJobState.UnloadSettling ||
            transport.JobState == TransportJobState.Unloading || transport.JobState == TransportJobState.UnloadBlocked) return false;

        if (clearQueue) world.GetQueue(passengerId).Clear();
        ConstructionSystem.ReleaseBuilderAssignment(world, passengerId);
        CommandExecutionSystem.CancelHarvest(world, passengerId);
        TargetingSystem.ClearTarget(world, passengerId);
        ref Passenger passenger = ref world.Entities.Passenger.Get(passengerId);
        passenger.Transport = carrier;
        passenger.State = PassengerState.MovingToLoad;
        SetPassengerApproach(world, passengerId, carrier);
        world.GetQueue(carrier).Clear();
        StopCarrier(world, carrier);
        return true;
    }

    public static bool TryStartUnload(SimulationWorld world, EntityId carrier, FixVec2 target)
    {
        if (!world.Entities.Transport.TryGet(carrier, out Transport snapshot) || snapshot.PassengerCount == 0 ||
            !world.Entities.Navigation.TryGet(carrier, out NavigationAgent navigation) || !world.Entities.Movement.Has(carrier) ||
            !world.Entities.Transform.Has(carrier)) return false;
        CancelPendingLoadsTo(world, carrier);
        ref Transport transport = ref world.Entities.Transport.Get(carrier);
        transport.JobState = TransportJobState.MovingToUnload;
        transport.ActivePassenger = EntityId.None;
        transport.PhaseTicks = 0;
        transport.UnloadBlocked = false;
        transport.LoadingSettled = false;
        transport.UnloadTarget = FormationPlanner.ResolvePassableSlot(world, target, navigation.Footprint);
        world.GetQueue(carrier).Clear();
        TargetingSystem.ClearTarget(world, carrier);
        CommandExecutionSystem.SetMove(world, carrier, transport.UnloadTarget);
        return true;
    }

    public static void EmergencyDeploy(SimulationWorld world, EntityId carrier)
    {
        if (!world.Entities.Transport.TryGet(carrier, out Transport transport) || !world.Entities.Transform.TryGet(carrier, out SimTransform carrierTransform)) return;
        CancelPendingLoadsTo(world, carrier);
        List<DeploymentReservation> reservations = new(transport.PassengerCount);
        for (int i = 0; i < transport.PassengerCount; i++)
        {
            EntityId passenger = transport.GetPassenger(i);
            if (!world.Entities.Passenger.TryGet(passenger, out Passenger state) || state.State != PassengerState.Loaded || state.Transport != carrier) continue;
            FixVec2 position = FindLegalDeployment(world, carrier, passenger, carrierTransform.Position, reservations, allowCarrierPositionFallback: true);
            Deploy(world, passenger, position, carrierTransform.Orientation, emergency: true);
            reservations.Add(new DeploymentReservation(position, FootprintRules.CollisionRadiusBuild(world.Entities.Navigation.Get(passenger).Footprint)));
        }
        ref Transport stored = ref world.Entities.Transport.Get(carrier);
        stored.PassengerCount = 0;
        stored.OccupiedPoints = 0;
        for (int i = 0; i < Transport.MaximumPassengerSlots; i++) stored.SetPassenger(i, EntityId.None);
    }

    public static bool IsLoadedPassenger(SimulationWorld world, EntityId id)
        => world.Entities.Passenger.TryGet(id, out Passenger passenger) && passenger.State == PassengerState.Loaded;

    public static bool IsPassengerBusy(SimulationWorld world, EntityId id)
        => world.Entities.Passenger.TryGet(id, out Passenger passenger) && passenger.State != PassengerState.Grounded;

    public static Fix32 MovementMultiplier(SimulationWorld world, EntityId id)
        => world.Entities.Passenger.TryGet(id, out Passenger passenger) && passenger.MovementPenaltyUntilTick > world.Tick.Value
            ? EmergencyMovementMultiplier : Fix32.One;

    public static bool IsAttackLocked(SimulationWorld world, EntityId id)
        => world.Entities.Passenger.TryGet(id, out Passenger passenger) && passenger.AttackLockedUntilTick > world.Tick.Value;

    public static void CancelForDirectOrder(SimulationWorld world, EntityId id)
    {
        if (world.Entities.Passenger.TryGet(id, out Passenger passenger) &&
            (passenger.State == PassengerState.MovingToLoad || passenger.State == PassengerState.WaitingToLoad))
            CancelPassengerLoad(world, id, startNextOrder: false);
        if (!world.Entities.Transport.TryGet(id, out Transport transport) || transport.JobState == TransportJobState.Idle) return;
        CancelPendingLoadsTo(world, id);
        ref Transport stored = ref world.Entities.Transport.Get(id);
        ResetTransportJob(ref stored);
    }

    private static void UpdatePassengerApproaches(SimulationWorld world)
    {
        IReadOnlyList<EntityId> alive = world.Entities.Alive;
        for (int i = 0; i < alive.Count; i++)
        {
            EntityId passengerId = alive[i];
            if (!world.Entities.Passenger.TryGet(passengerId, out Passenger passenger) ||
                (passenger.State != PassengerState.MovingToLoad && passenger.State != PassengerState.WaitingToLoad)) continue;
            if (!IsPendingLoadValid(world, passengerId, passenger.Transport)) { CancelPassengerLoad(world, passengerId); continue; }
            Fix32 gap = CombatGeometry.ContactGap(world, passengerId, passenger.Transport);
            if (gap <= InteractionRange)
            {
                if (passenger.State != PassengerState.WaitingToLoad)
                {
                    ref NavigationAgent nav = ref world.Entities.Navigation.Get(passengerId);
                    ref Movement movement = ref world.Entities.Movement.Get(passengerId);
                    CommandExecutionSystem.StopMovement(world, passengerId, ref nav, ref movement);
                    ref Passenger stored = ref world.Entities.Passenger.Get(passengerId);
                    stored.State = PassengerState.WaitingToLoad;
                }
                continue;
            }
            if (passenger.State != PassengerState.MovingToLoad || !world.Entities.Navigation.Get(passengerId).HasTarget)
                SetPassengerApproach(world, passengerId, passenger.Transport);
        }
    }

    private static void UpdateLoading(SimulationWorld world, EntityId carrier)
    {
        ref Transport transport = ref world.Entities.Transport.Get(carrier);
        if (transport.JobState == TransportJobState.Idle)
        {
            EntityId waiting = FindWaitingPassenger(world, carrier);
            if (waiting == EntityId.None)
            {
                if (!HasPendingLoad(world, carrier)) transport.LoadingSettled = false;
                return;
            }
            StopCarrier(world, carrier);
            transport.ActivePassenger = waiting;
            transport.JobState = transport.LoadingSettled ? TransportJobState.LoadingPassenger : TransportJobState.LoadingDocking;
            transport.PhaseTicks = checked((ushort)(transport.LoadingSettled ? RapidRiderLoadTicks : DockingTicks));
            return;
        }
        if (transport.JobState != TransportJobState.LoadingDocking && transport.JobState != TransportJobState.LoadingPassenger) return;
        EntityId active = transport.ActivePassenger;
        if (!IsPendingLoadValid(world, active, carrier) || world.Entities.Passenger.Get(active).State != PassengerState.WaitingToLoad ||
            CombatGeometry.ContactGap(world, active, carrier) > InteractionRange)
        {
            transport.ActivePassenger = EntityId.None;
            transport.JobState = TransportJobState.Idle;
            transport.PhaseTicks = 0;
            return;
        }
        if (WasRecentlyDamaged(world, carrier) || WasRecentlyDamaged(world, active)) return;
        if (transport.PhaseTicks > 0) transport.PhaseTicks--;
        if (transport.PhaseTicks > 0) return;
        if (transport.JobState == TransportJobState.LoadingDocking)
        {
            transport.JobState = TransportJobState.LoadingPassenger;
            transport.LoadingSettled = true;
            transport.PhaseTicks = RapidRiderLoadTicks;
            return;
        }
        Board(world, carrier, active);
    }

    private static void Board(SimulationWorld world, EntityId carrier, EntityId passengerId)
    {
        ref Transport transport = ref world.Entities.Transport.Get(carrier);
        Passenger passenger = world.Entities.Passenger.Get(passengerId);
        if (!transport.TryAddPassenger(passengerId, passenger.SizePoints)) { CancelPassengerLoad(world, passengerId); }
        else
        {
            ref NavigationAgent nav = ref world.Entities.Navigation.Get(passengerId);
            ref Movement movement = ref world.Entities.Movement.Get(passengerId);
            CommandExecutionSystem.StopMovement(world, passengerId, ref nav, ref movement);
            world.RemoveRuntimeState(passengerId);
            world.Entities.Transform.Remove(passengerId);
            ref Passenger storedPassenger = ref world.Entities.Passenger.Get(passengerId);
            storedPassenger.State = PassengerState.Loaded;
            storedPassenger.Transport = carrier;
        }
        transport.ActivePassenger = EntityId.None;
        transport.JobState = TransportJobState.Idle;
        transport.PhaseTicks = 0;
        if (!HasPendingLoad(world, carrier)) transport.LoadingSettled = false;
    }

    private static void UpdateUnloadApproach(SimulationWorld world, EntityId carrier)
    {
        ref Transport transport = ref world.Entities.Transport.Get(carrier);
        if (transport.PassengerCount == 0) { ResetTransportJob(ref transport); return; }
        SimTransform transform = world.Entities.Transform.Get(carrier);
        if (FixVec2.Distance(transform.Position, transport.UnloadTarget) > Fix32.FromRatio(3, 5))
        {
            if (!world.Entities.Navigation.Get(carrier).HasTarget) CommandExecutionSystem.SetMove(world, carrier, transport.UnloadTarget);
            return;
        }
        StopCarrier(world, carrier);
        transport.JobState = TransportJobState.UnloadSettling;
        transport.PhaseTicks = UnloadSettlingTicks;
    }

    private static void UpdateUnload(SimulationWorld world, EntityId carrier)
    {
        ref Transport transport = ref world.Entities.Transport.Get(carrier);
        if (transport.PassengerCount == 0) { ResetTransportJob(ref transport); return; }
        if (transport.JobState == TransportJobState.UnloadSettling || transport.JobState == TransportJobState.Unloading)
        {
            if (transport.PhaseTicks > 0) transport.PhaseTicks--;
            if (transport.PhaseTicks > 0) return;
        }
        EntityId passenger = transport.GetPassenger(0);
        if (!world.Entities.Passenger.TryGet(passenger, out Passenger passengerState))
        {
            transport.RemoveFirstPassenger(0);
            return;
        }
        SimTransform carrierTransform = world.Entities.Transform.Get(carrier);
        FixVec2 position = FindLegalDeployment(world, carrier, passenger, transport.UnloadTarget, new List<DeploymentReservation>(0), allowCarrierPositionFallback: false);
        if (position.Equals(InvalidPosition))
        {
            transport.JobState = TransportJobState.UnloadBlocked;
            transport.UnloadBlocked = true;
            return;
        }
        transport.RemoveFirstPassenger(passengerState.SizePoints);
        Deploy(world, passenger, position, carrierTransform.Orientation, emergency: false);
        transport.UnloadBlocked = false;
        if (transport.PassengerCount == 0) ResetTransportJob(ref transport);
        else { transport.JobState = TransportJobState.Unloading; transport.PhaseTicks = RapidRiderUnloadTicks; }
    }

    private static readonly FixVec2 InvalidPosition = new(Fix32.MinValue, Fix32.MinValue);

    private static FixVec2 FindLegalDeployment(SimulationWorld world, EntityId carrier, EntityId passenger, FixVec2 desired,
        List<DeploymentReservation> reservations, bool allowCarrierPositionFallback)
    {
        NavigationAgent navigation = world.Entities.Navigation.Get(passenger);
        NavCell origin = MapGrid.BuildToNav(desired);
        for (int radius = 0; radius <= 32; radius++)
        {
            if (radius == 0)
            {
                if (TryDeploymentCell(world, carrier, passenger, origin.X, origin.Y, navigation.Footprint, reservations, allowCarrierPositionFallback, out FixVec2 center)) return center;
                continue;
            }
            for (int dx = -radius; dx <= radius; dx++)
            {
                if (TryDeploymentCell(world, carrier, passenger, origin.X + dx, origin.Y - radius, navigation.Footprint, reservations, allowCarrierPositionFallback, out FixVec2 top)) return top;
                if (TryDeploymentCell(world, carrier, passenger, origin.X + dx, origin.Y + radius, navigation.Footprint, reservations, allowCarrierPositionFallback, out FixVec2 bottom)) return bottom;
            }
            for (int dy = -radius + 1; dy <= radius - 1; dy++)
            {
                if (TryDeploymentCell(world, carrier, passenger, origin.X - radius, origin.Y + dy, navigation.Footprint, reservations, allowCarrierPositionFallback, out FixVec2 left)) return left;
                if (TryDeploymentCell(world, carrier, passenger, origin.X + radius, origin.Y + dy, navigation.Footprint, reservations, allowCarrierPositionFallback, out FixVec2 right)) return right;
            }
        }
        return allowCarrierPositionFallback && world.Entities.Transform.TryGet(carrier, out SimTransform transform) ? transform.Position : InvalidPosition;
    }

    private static bool TryDeploymentCell(SimulationWorld world, EntityId carrier, EntityId passenger, int x, int y, FootprintClass footprint,
        List<DeploymentReservation> reservations, bool ignoreCarrier, out FixVec2 position)
    {
        position = default;
        if ((uint)x >= MapGrid.NavWidth || (uint)y >= MapGrid.NavHeight) return false;
        NavCell cell = new(checked((short)x), checked((short)y));
        if (!world.Pathfinder.IsPassable(cell, footprint)) return false;
        position = MapGrid.NavCellCenterToBuild(cell);
        Fix32 radius = FootprintRules.CollisionRadiusBuild(footprint);
        IReadOnlyList<EntityId> alive = world.Entities.Alive;
        for (int i = 0; i < alive.Count; i++)
        {
            EntityId other = alive[i];
            if (other == passenger || (ignoreCarrier && other == carrier) || !world.Entities.Transform.TryGet(other, out SimTransform otherTransform) ||
                !world.Entities.Navigation.TryGet(other, out NavigationAgent otherNavigation)) continue;
            if (FixVec2.Distance(position, otherTransform.Position) < radius + FootprintRules.CollisionRadiusBuild(otherNavigation.Footprint)) return false;
        }
        for (int i = 0; i < reservations.Count; i++)
            if (FixVec2.Distance(position, reservations[i].Position) < radius + reservations[i].Radius) return false;
        return true;
    }

    private static void Deploy(SimulationWorld world, EntityId passengerId, FixVec2 position, Angle16 orientation, bool emergency)
    {
        world.Entities.Transform.Set(passengerId, new SimTransform { Position = position, Orientation = orientation });
        ref NavigationAgent navigation = ref world.Entities.Navigation.Get(passengerId);
        navigation.Target = position; navigation.HasTarget = false; navigation.PathDirty = false; navigation.Formation = default;
        navigation.PathTopologyVersion = world.Map.TopologyVersion; navigation.RequestAge = 0;
        ref Movement movement = ref world.Entities.Movement.Get(passengerId);
        movement.CurrentSpeed = Fix32.Zero; movement.CurrentVelocity = FixVec2.Zero; movement.DesiredMovement = FixVec2.Zero;
        movement.PathIndex = 0; movement.State = MovementState.Idle; movement.StuckTicks = 0; movement.CompressionTicks = 0; movement.LastPosition = position;
        ref Passenger passenger = ref world.Entities.Passenger.Get(passengerId);
        passenger.Transport = EntityId.None; passenger.State = PassengerState.Grounded;
        if (emergency)
        {
            ref Health health = ref world.Entities.Health.Get(passengerId);
            health.Current = health.Maximum * Fix32.FromInt(2) / Fix32.FromInt(5);
            passenger.AttackLockedUntilTick = checked(world.Tick.Value + EmergencyAttackLockTicks);
            passenger.MovementPenaltyUntilTick = checked(world.Tick.Value + EmergencyMovementPenaltyTicks);
        }
    }

    private static void SetPassengerApproach(SimulationWorld world, EntityId passenger, EntityId carrier)
    {
        FixVec2 source = world.Entities.Transform.Get(passenger).Position;
        FixVec2 destination = world.Entities.Transform.Get(carrier).Position;
        FixVec2 direction = (source - destination).NormalizeSafe();
        if (direction.Equals(FixVec2.Zero)) direction = new FixVec2(-Fix32.One, Fix32.Zero);
        FixVec2 desired = CombatGeometry.RangedApproachPoint(world, passenger, carrier, direction, InteractionRange * Fix32.FromRatio(3, 4));
        desired = FormationPlanner.ResolvePassableSlot(world, desired, world.Entities.Navigation.Get(passenger).Footprint);
        CommandExecutionSystem.SetMove(world, passenger, desired);
        ref Passenger stored = ref world.Entities.Passenger.Get(passenger);
        stored.State = PassengerState.MovingToLoad;
    }

    private static void StopCarrier(SimulationWorld world, EntityId carrier)
    {
        if (!world.Entities.Navigation.Has(carrier) || !world.Entities.Movement.Has(carrier)) return;
        ref NavigationAgent navigation = ref world.Entities.Navigation.Get(carrier);
        ref Movement movement = ref world.Entities.Movement.Get(carrier);
        CommandExecutionSystem.StopMovement(world, carrier, ref navigation, ref movement);
    }

    private static bool IsEligibleLoad(SimulationWorld world, EntityId passenger, EntityId carrier)
        => IsPendingLoadValid(world, passenger, carrier) && world.Entities.Passenger.Get(passenger).State == PassengerState.Grounded;

    private static bool IsPendingLoadValid(SimulationWorld world, EntityId passenger, EntityId carrier)
        => passenger != EntityId.None && carrier != EntityId.None && passenger != carrier && world.Entities.Exists(passenger) && world.Entities.Exists(carrier) &&
           world.Entities.Passenger.Has(passenger) && world.Entities.Transport.Has(carrier) && world.Entities.Transform.Has(passenger) && world.Entities.Transform.Has(carrier) &&
           world.Entities.Navigation.Has(passenger) && world.Entities.Movement.Has(passenger) &&
           world.Entities.Ownership.TryGet(passenger, out Ownership passengerOwner) && world.Entities.Ownership.TryGet(carrier, out Ownership carrierOwner) &&
           passengerOwner.PlayerSlot == carrierOwner.PlayerSlot &&
           (!world.Entities.Health.TryGet(passenger, out Health passengerHealth) || !passengerHealth.IsDepleted) &&
           (!world.Entities.Health.TryGet(carrier, out Health carrierHealth) || !carrierHealth.IsDepleted);

    private static int ReservedPoints(SimulationWorld world, EntityId carrier)
    {
        int points = world.Entities.Transport.Get(carrier).OccupiedPoints;
        IReadOnlyList<EntityId> alive = world.Entities.Alive;
        for (int i = 0; i < alive.Count; i++)
        {
            EntityId id = alive[i];
            if (!world.Entities.Passenger.TryGet(id, out Passenger passenger) || passenger.Transport != carrier || passenger.State == PassengerState.Grounded || passenger.State == PassengerState.Loaded) continue;
            points += passenger.SizePoints;
        }
        return points;
    }

    private static bool HasPendingLoad(SimulationWorld world, EntityId carrier)
    {
        IReadOnlyList<EntityId> alive = world.Entities.Alive;
        for (int i = 0; i < alive.Count; i++)
            if (world.Entities.Passenger.TryGet(alive[i], out Passenger passenger) && passenger.Transport == carrier &&
                (passenger.State == PassengerState.MovingToLoad || passenger.State == PassengerState.WaitingToLoad)) return true;
        return false;
    }

    private static EntityId FindWaitingPassenger(SimulationWorld world, EntityId carrier)
    {
        IReadOnlyList<EntityId> alive = world.Entities.Alive;
        for (int i = 0; i < alive.Count; i++)
            if (world.Entities.Passenger.TryGet(alive[i], out Passenger passenger) && passenger.Transport == carrier && passenger.State == PassengerState.WaitingToLoad)
                return alive[i];
        return EntityId.None;
    }

    private static bool WasRecentlyDamaged(SimulationWorld world, EntityId id)
        => world.Entities.Health.TryGet(id, out Health health) && health.LastDamageTick >= 0 && world.Tick.Value - health.LastDamageTick <= DamagePauseTicks;

    private static void CancelPendingLoadsTo(SimulationWorld world, EntityId carrier)
    {
        IReadOnlyList<EntityId> alive = world.Entities.Alive;
        for (int i = 0; i < alive.Count; i++)
        {
            EntityId id = alive[i];
            if (world.Entities.Passenger.TryGet(id, out Passenger passenger) && passenger.Transport == carrier && passenger.State != PassengerState.Loaded)
                CancelPassengerLoad(world, id);
        }
    }

    private static void CancelPassengerLoad(SimulationWorld world, EntityId passengerId, bool startNextOrder = true)
    {
        if (!world.Entities.Passenger.Has(passengerId)) return;
        ref Passenger passenger = ref world.Entities.Passenger.Get(passengerId);
        passenger.Transport = EntityId.None; passenger.State = PassengerState.Grounded;
        if (world.Entities.Navigation.Has(passengerId) && world.Entities.Movement.Has(passengerId))
        {
            ref NavigationAgent navigation = ref world.Entities.Navigation.Get(passengerId);
            ref Movement movement = ref world.Entities.Movement.Get(passengerId);
            CommandExecutionSystem.StopMovement(world, passengerId, ref navigation, ref movement);
        }
        if (startNextOrder) CommandExecutionSystem.TryStartNextOrder(world, passengerId);
    }

    private static void ResetTransportJob(ref Transport transport)
    {
        transport.JobState = TransportJobState.Idle; transport.ActivePassenger = EntityId.None; transport.PhaseTicks = 0;
        transport.UnloadBlocked = false; transport.LoadingSettled = false;
    }

    private readonly struct DeploymentReservation
    {
        public readonly FixVec2 Position;
        public readonly Fix32 Radius;
        public DeploymentReservation(FixVec2 position, Fix32 radius) { Position = position; Radius = radius; }
    }
}
}
