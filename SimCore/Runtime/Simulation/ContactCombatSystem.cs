using System;
using System.Collections.Generic;

namespace LegoSpaceRTS.SimCore
{
/// <summary>Deterministic footprint-aware distance and approach geometry shared by contact weapons.</summary>
public static class CombatGeometry
{
    public static Fix32 ContactGap(SimulationWorld world, EntityId source, EntityId target)
    {
        SimTransform sourceTransform = world.Entities.Transform.Get(source);
        SimTransform targetTransform = world.Entities.Transform.Get(target);
        Fix32 sourceRadius = world.Entities.Navigation.TryGet(source, out NavigationAgent sourceNav)
            ? FootprintRules.CollisionRadiusBuild(sourceNav.Footprint) : Fix32.Zero;
        Fix32 edgeDistance;
        if (world.Entities.Building.TryGet(target, out Building building))
        {
            Fix32 minX = Fix32.FromInt(building.AnchorX), maxX = Fix32.FromInt(building.AnchorX + building.FootprintWidth);
            Fix32 minY = Fix32.FromInt(building.AnchorY), maxY = Fix32.FromInt(building.AnchorY + building.FootprintHeight);
            Fix32 nearestX = Fix32.Clamp(sourceTransform.Position.X, minX, maxX);
            Fix32 nearestY = Fix32.Clamp(sourceTransform.Position.Y, minY, maxY);
            edgeDistance = FixVec2.Distance(sourceTransform.Position, new FixVec2(nearestX, nearestY));
        }
        else
        {
            Fix32 targetRadius = world.Entities.Navigation.TryGet(target, out NavigationAgent targetNav)
                ? FootprintRules.CollisionRadiusBuild(targetNav.Footprint) : Fix32.Zero;
            edgeDistance = FixVec2.Distance(sourceTransform.Position, targetTransform.Position) - targetRadius;
        }
        return Fix32.Max(Fix32.Zero, edgeDistance - sourceRadius);
    }

    public static FixVec2 ContactSlot(SimulationWorld world, EntityId source, EntityId target, WeaponDefinition weapon, byte slotIndex)
    {
        FixVec2 direction = ContactApproachSystem.SlotDirections[slotIndex];
        SimTransform targetTransform = world.Entities.Transform.Get(target);
        Fix32 targetExtent;
        if (world.Entities.Building.TryGet(target, out Building building))
        {
            Fix32 halfWidth = Fix32.FromRatio(building.FootprintWidth, 2);
            Fix32 halfHeight = Fix32.FromRatio(building.FootprintHeight, 2);
            Fix32 tx = direction.X.Raw == 0 ? Fix32.MaxValue : halfWidth / Fix32.Abs(direction.X);
            Fix32 ty = direction.Y.Raw == 0 ? Fix32.MaxValue : halfHeight / Fix32.Abs(direction.Y);
            targetExtent = Fix32.Min(tx, ty);
        }
        else if (world.Entities.Navigation.TryGet(target, out NavigationAgent targetNav))
            targetExtent = FootprintRules.CollisionRadiusBuild(targetNav.Footprint);
        else targetExtent = Fix32.Zero;

        Fix32 sourceRadius = world.Entities.Navigation.TryGet(source, out NavigationAgent sourceNav)
            ? FootprintRules.CollisionRadiusBuild(sourceNav.Footprint) : Fix32.Zero;
        Fix32 standOff = Fix32.Min(Fix32.FromRatio(1, 4), weapon.Range / Fix32.FromInt(2));
        return targetTransform.Position + direction * (targetExtent + sourceRadius + standOff);
    }
}

/// <summary>Reserves legal ring positions and turns out-of-range direct attacks into bounded pursuit movement.</summary>
public sealed class ContactApproachSystem : ISimSystem
{
    public const byte SlotCount = 16;
    internal static readonly FixVec2[] SlotDirections =
    {
        Raw(65536,0),Raw(60547,25080),Raw(46341,46341),Raw(25080,60547),
        Raw(0,65536),Raw(-25080,60547),Raw(-46341,46341),Raw(-60547,25080),
        Raw(-65536,0),Raw(-60547,-25080),Raw(-46341,-46341),Raw(-25080,-60547),
        Raw(0,-65536),Raw(25080,-60547),Raw(46341,-46341),Raw(60547,-25080)
    };
    private static readonly Fix32 DirectChaseLeash = Fix32.FromInt(12);
    private static readonly Fix32 RetargetDistance = Fix32.FromRatio(1, 4);
    private static readonly Fix32 ArrivalDistance = Fix32.FromRatio(2, 5);
    private readonly List<EntityId> _contactAttackers = new(32);
    private readonly Dictionary<uint, ushort> _reservedByTarget = new();

    public void Step(SimulationWorld world)
    {
        _contactAttackers.Clear(); _reservedByTarget.Clear();
        IReadOnlyList<EntityId> alive = world.Entities.Alive;
        for (int i = 0; i < alive.Count; i++)
        {
            EntityId source = alive[i];
            if (!TryActiveMobileAttacker(world, source, out Targeting targeting, out WeaponDefinition weapon)) continue;
            if (weapon.DeliveryKind == WeaponDeliveryKind.Contact) _contactAttackers.Add(source);
            else UpdateRangedPursuit(world, source, weapon);
        }

        // Retained reservations win in stable EntityId order, preventing slot churn as units move.
        for (int i = 0; i < _contactAttackers.Count; i++)
        {
            EntityId source = _contactAttackers[i]; ref Targeting targeting = ref world.Entities.Targeting.Get(source);
            if (!targeting.HasApproachSlot) continue;
            ushort used = _reservedByTarget.TryGetValue(targeting.CurrentTarget.Value, out ushort mask) ? mask : (ushort)0;
            ushort bit = checked((ushort)(1 << targeting.ApproachSlotIndex));
            world.Content.TryGetWeapon(world.Entities.Weapon.Get(source).WeaponProfile, out WeaponDefinition retainedWeapon);
            FixVec2 candidate = CombatGeometry.ContactSlot(world, source, targeting.CurrentTarget, retainedWeapon, targeting.ApproachSlotIndex);
            if ((used & bit) != 0 || !IsPassable(world, source, candidate)) { targeting.HasApproachSlot = false; continue; }
            _reservedByTarget[targeting.CurrentTarget.Value] = checked((ushort)(used | bit));
        }

        for (int i = 0; i < _contactAttackers.Count; i++)
        {
            EntityId source = _contactAttackers[i]; ref Targeting targeting = ref world.Entities.Targeting.Get(source);
            world.Content.TryGetWeapon(world.Entities.Weapon.Get(source).WeaponProfile, out WeaponDefinition weapon);
            if (!targeting.HasApproachSlot && !AssignClosestFreeSlot(world, source, targeting.CurrentTarget, weapon, ref targeting))
            {
                StopCombatMove(world, source, ref targeting); continue;
            }
            FixVec2 slot = CombatGeometry.ContactSlot(world, source, targeting.CurrentTarget, weapon, targeting.ApproachSlotIndex);
            UpdateContactPursuit(world, source, weapon, slot);
        }
    }

    private static bool TryActiveMobileAttacker(SimulationWorld world, EntityId source, out Targeting targeting, out WeaponDefinition weapon)
    {
        targeting = default; weapon = default;
        if (!world.Entities.Targeting.TryGet(source, out targeting) || targeting.CurrentTarget == EntityId.None ||
            !world.Entities.Weapon.TryGet(source, out WeaponState state) || !world.Content.TryGetWeapon(state.WeaponProfile, out weapon) ||
            !world.Entities.Transform.Has(source) || !world.Entities.Transform.Has(targeting.CurrentTarget) ||
            !world.Entities.Navigation.Has(source) || !world.Entities.Movement.TryGet(source, out Movement movement) || movement.State == MovementState.Holding ||
            !TargetingSystem.IsLegalTarget(world, source, targeting.CurrentTarget, requireVisible: true)) return false;
        // Automatic acquisition must not hijack an explicit move/harvest/construction route.
        return targeting.SelectionKind == TargetSelectionKind.DirectOrder || !world.Entities.Navigation.Get(source).HasTarget || targeting.HasCombatMove;
    }

    private bool AssignClosestFreeSlot(SimulationWorld world, EntityId source, EntityId target, WeaponDefinition weapon, ref Targeting targeting)
    {
        ushort used = _reservedByTarget.TryGetValue(target.Value, out ushort mask) ? mask : (ushort)0;
        SimTransform sourceTransform = world.Entities.Transform.Get(source);
        int best = -1; Fix32 bestDistance = Fix32.MaxValue;
        for (byte slot = 0; slot < SlotCount; slot++)
        {
            ushort bit = checked((ushort)(1 << slot)); if ((used & bit) != 0) continue;
            FixVec2 candidate = CombatGeometry.ContactSlot(world, source, target, weapon, slot);
            if (!IsPassable(world, source, candidate)) continue;
            Fix32 distance = FixVec2.DistanceSquared(sourceTransform.Position, candidate);
            if (best < 0 || distance < bestDistance) { best = slot; bestDistance = distance; }
        }
        if (best < 0) return false;
        targeting.ApproachSlotIndex = checked((byte)best); targeting.HasApproachSlot = true;
        _reservedByTarget[target.Value] = checked((ushort)(used | (1 << best)));
        return true;
    }

    private static bool IsPassable(SimulationWorld world, EntityId source, FixVec2 candidate)
    {
        NavigationAgent nav = world.Entities.Navigation.Get(source);
        return world.Pathfinder.IsPassable(MapGrid.BuildToNav(candidate), nav.Footprint);
    }

    private static void UpdateContactPursuit(SimulationWorld world, EntityId source, WeaponDefinition weapon, FixVec2 slot)
    {
        ref Targeting targeting = ref world.Entities.Targeting.Get(source);
        SimTransform transform = world.Entities.Transform.Get(source);
        EnsurePursuitOrigin(ref targeting, transform.Position);
        bool inRange = CombatGeometry.ContactGap(world, source, targeting.CurrentTarget) <= weapon.Range;
        if (FixVec2.Distance(transform.Position, targeting.PursuitOrigin) >= DirectChaseLeash && !inRange)
        {
            StopCombatMove(world, source, ref targeting); TargetingSystem.ClearTarget(world, source); return;
        }
        slot = ClampToLeash(targeting.PursuitOrigin, slot, out bool leashLimited);
        if (FixVec2.Distance(transform.Position, slot) <= ArrivalDistance)
        {
            StopCombatMove(world, source, ref targeting);
            if (leashLimited && !inRange) TargetingSystem.ClearTarget(world, source);
            return;
        }
        SetCombatMove(world, source, slot, ref targeting);
    }

    private static void UpdateRangedPursuit(SimulationWorld world, EntityId source, WeaponDefinition weapon)
    {
        ref Targeting targeting = ref world.Entities.Targeting.Get(source);
        SimTransform transform = world.Entities.Transform.Get(source), target = world.Entities.Transform.Get(targeting.CurrentTarget);
        EnsurePursuitOrigin(ref targeting, transform.Position);
        Fix32 distance = FixVec2.Distance(transform.Position, target.Position);
        if (distance <= weapon.Range)
        {
            StopCombatMove(world, source, ref targeting); return;
        }
        if (FixVec2.Distance(transform.Position, targeting.PursuitOrigin) >= DirectChaseLeash)
        {
            StopCombatMove(world, source, ref targeting); TargetingSystem.ClearTarget(world, source); return;
        }
        FixVec2 away = (transform.Position - target.Position).NormalizeSafe();
        if (away.Equals(FixVec2.Zero)) away = SlotDirections[source.Value % SlotCount];
        FixVec2 desired = target.Position + away * (weapon.Range * Fix32.FromRatio(9, 10));
        desired = ClampToLeash(targeting.PursuitOrigin, desired, out bool leashLimited);
        if (leashLimited && FixVec2.Distance(transform.Position, desired) <= ArrivalDistance)
        {
            StopCombatMove(world, source, ref targeting); TargetingSystem.ClearTarget(world, source); return;
        }
        if (!IsPassable(world, source, desired)) desired = FormationPlanner.ResolvePassableSlot(world, desired, world.Entities.Navigation.Get(source).Footprint);
        SetCombatMove(world, source, desired, ref targeting);
    }

    private static FixVec2 ClampToLeash(FixVec2 origin, FixVec2 desired, out bool limited)
    {
        FixVec2 delta = desired - origin;
        limited = delta.Length() > DirectChaseLeash;
        return limited ? origin + delta.NormalizeSafe() * DirectChaseLeash : desired;
    }

    private static void EnsurePursuitOrigin(ref Targeting targeting, FixVec2 fallback)
    {
        if (targeting.HasPursuitOrigin) return;
        targeting.PursuitOrigin = fallback; targeting.HasPursuitOrigin = true;
    }

    private static void SetCombatMove(SimulationWorld world, EntityId source, FixVec2 target, ref Targeting targeting)
    {
        NavigationAgent nav = world.Entities.Navigation.Get(source);
        if (!nav.HasTarget || !targeting.HasCombatMove || FixVec2.Distance(nav.Target, target) > RetargetDistance)
            CommandExecutionSystem.SetMove(world, source, target);
        targeting.HasCombatMove = true;
    }

    private static void StopCombatMove(SimulationWorld world, EntityId source, ref Targeting targeting)
    {
        if (!targeting.HasCombatMove || !world.Entities.Navigation.Has(source) || !world.Entities.Movement.Has(source)) return;
        ref NavigationAgent nav = ref world.Entities.Navigation.Get(source); ref Movement movement = ref world.Entities.Movement.Get(source);
        CommandExecutionSystem.StopMovement(world, source, ref nav, ref movement); targeting.HasCombatMove = false;
    }

    private static FixVec2 Raw(int x, int y) => new(Fix32.FromRaw(x), Fix32.FromRaw(y));
}

/// <summary>Turns contact attackers toward their target without locking either participant's movement.</summary>
public sealed class ContactFacingSystem : ISimSystem
{
    public void Step(SimulationWorld world)
    {
        IReadOnlyList<EntityId> alive = world.Entities.Alive;
        for (int i = 0; i < alive.Count; i++)
        {
            EntityId source = alive[i];
            if (!world.Entities.Weapon.TryGet(source, out WeaponState state) || !world.Content.TryGetWeapon(state.WeaponProfile, out WeaponDefinition weapon) || weapon.DeliveryKind != WeaponDeliveryKind.Contact ||
                !world.Entities.Targeting.TryGet(source, out Targeting targeting) || targeting.CurrentTarget == EntityId.None ||
                !world.Entities.Transform.Has(source) || !world.Entities.Transform.TryGet(targeting.CurrentTarget, out SimTransform target)) continue;
            if (CombatGeometry.ContactGap(world, source, targeting.CurrentTarget) > weapon.Range + Fix32.FromRatio(1, 2)) continue;
            ref SimTransform transform = ref world.Entities.Transform.Get(source);
            Angle16 desired = Angle16.FromDirection(target.Position - transform.Position);
            ushort turn = world.Entities.Movement.TryGet(source, out Movement movement) ? movement.TurnRatePerTick : ushort.MaxValue;
            transform.Orientation = Angle16.TurnToward(transform.Orientation, desired, turn);
        }
    }
}

/// <summary>Routes contact firing events through the same class/armor resolver as projectile impacts.</summary>
public sealed class ContactDamageSystem : ISimSystem
{
    public void Step(SimulationWorld world)
    {
        IReadOnlyList<EntityId> alive = world.Entities.Alive;
        for (int i = 0; i < alive.Count; i++)
        {
            EntityId source = alive[i];
            if (!world.Entities.Weapon.TryGet(source, out WeaponState state) || state.LastFiredTick != world.Tick.Value ||
                !world.Content.TryGetWeapon(state.WeaponProfile, out WeaponDefinition weapon) || weapon.DeliveryKind != WeaponDeliveryKind.Contact) continue;
            DamageSystem.Resolve(world, new DamageRequest(source, state.LastFiredTarget, state.WeaponProfile, weapon.BaseDamage, weapon.DamageType));
        }
    }
}
}
