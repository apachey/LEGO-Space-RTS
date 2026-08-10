using System;
using System.Collections.Generic;

namespace LegoSpaceRTS.SimCore
{
public readonly struct ProjectileId : IEquatable<ProjectileId>
{
    public readonly uint Value;
    public ProjectileId(uint value) => Value = value;
    public bool Equals(ProjectileId other) => Value == other.Value;
    public override bool Equals(object? obj) => obj is ProjectileId other && Equals(other);
    public override int GetHashCode() => unchecked((int)Value);
    public static bool operator ==(ProjectileId a, ProjectileId b) => a.Value == b.Value;
    public static bool operator !=(ProjectileId a, ProjectileId b) => a.Value != b.Value;
}

public enum ProjectileGuidance : byte
{
    Ordinary = 0
}

/// <summary>Compact authoritative projectile state. It is simulation data, never a physics object.</summary>
public struct ProjectileRecord
{
    public ProjectileId Id;
    public byte Owner;
    public EntityId Source;
    public EntityId Target;
    public ContentId WeaponProfile;
    public FixVec2 Position;
    public FixVec2 Velocity;
    public FixVec2 CommittedImpactPosition;
    public ushort BaseDamage;
    public DamageType DamageType;
    public ushort LifetimeRemainingTicks;
    public ProjectileGuidance Guidance;
}

/// <summary>Ordered impact output consumed by the later T043 damage system.</summary>
public readonly struct ProjectileImpactRecord
{
    public readonly ProjectileId ProjectileId;
    public readonly byte Owner;
    public readonly EntityId Source;
    public readonly EntityId Target;
    public readonly ContentId WeaponProfile;
    public readonly FixVec2 Position;
    public readonly ushort BaseDamage;
    public readonly DamageType DamageType;
    public readonly int ImpactTick;

    public ProjectileImpactRecord(ProjectileRecord projectile, int impactTick)
    {
        ProjectileId = projectile.Id; Owner = projectile.Owner; Source = projectile.Source; Target = projectile.Target;
        WeaponProfile = projectile.WeaponProfile; Position = projectile.CommittedImpactPosition;
        BaseDamage = projectile.BaseDamage; DamageType = projectile.DamageType; ImpactTick = impactTick;
    }

    internal ProjectileImpactRecord(ProjectileId projectileId, byte owner, EntityId source, EntityId target, ContentId weaponProfile,
        FixVec2 position, ushort baseDamage, DamageType damageType, int impactTick)
    {
        ProjectileId = projectileId; Owner = owner; Source = source; Target = target; WeaponProfile = weaponProfile;
        Position = position; BaseDamage = baseDamage; DamageType = damageType; ImpactTick = impactTick;
    }
}

/// <summary>Creates, advances and resolves deterministic projectiles after weapon firing.</summary>
public sealed class ProjectileSystem : ISimSystem
{
    public const int MaximumProjectileRecords = 8192;

    public void Step(SimulationWorld world)
    {
        world.ProjectileImpactsInternal.Clear();
        CreateNewProjectiles(world);

        List<ProjectileRecord> projectiles = world.ProjectilesInternal;
        int originalCount = projectiles.Count;
        int writeIndex = 0;
        for (int readIndex = 0; readIndex < originalCount; readIndex++)
        {
            ProjectileRecord projectile = projectiles[readIndex];
            FixVec2 remaining = projectile.CommittedImpactPosition - projectile.Position;
            FixVec2 step = projectile.Velocity * SimClock.TickSeconds;
            if (remaining.LengthSquared() <= step.LengthSquared() || projectile.LifetimeRemainingTicks <= 1)
            {
                projectile.Position = projectile.CommittedImpactPosition;
                world.ProjectileImpactsInternal.Add(new ProjectileImpactRecord(projectile, world.Tick.Value));
                continue;
            }

            projectile.Position += step;
            projectile.LifetimeRemainingTicks--;
            projectiles[writeIndex++] = projectile;
        }
        if (writeIndex < originalCount) projectiles.RemoveRange(writeIndex, originalCount - writeIndex);
    }

    private static void CreateNewProjectiles(SimulationWorld world)
    {
        IReadOnlyList<EntityId> alive = world.Entities.Alive;
        for (int i = 0; i < alive.Count; i++)
        {
            EntityId source = alive[i];
            if (!world.Entities.Weapon.TryGet(source, out WeaponState state) || state.LastFiredTick != world.Tick.Value ||
                !world.Content.TryGetWeapon(state.WeaponProfile, out WeaponDefinition weapon) || weapon.DeliveryKind != WeaponDeliveryKind.Projectile ||
                !world.Entities.Transform.TryGet(source, out SimTransform sourceTransform) ||
                !world.Entities.Transform.TryGet(state.LastFiredTarget, out SimTransform targetTransform) ||
                !world.Entities.Ownership.TryGet(source, out Ownership ownership)) continue;

            FixVec2 delta = targetTransform.Position - sourceTransform.Position;
            Fix32 distance = delta.Length();
            FixVec2 velocity = delta.NormalizeSafe() * weapon.ProjectileSpeed;
            Fix32 distancePerTick = weapon.ProjectileSpeed * SimClock.TickSeconds;
            int lifetime = Math.Max(1, checked((distance.Raw + distancePerTick.Raw - 1) / distancePerTick.Raw));
            world.AddProjectile(new ProjectileRecord
            {
                Owner = ownership.PlayerSlot,
                Source = source,
                Target = state.LastFiredTarget,
                WeaponProfile = weapon.Id,
                Position = sourceTransform.Position,
                Velocity = velocity,
                CommittedImpactPosition = targetTransform.Position,
                BaseDamage = weapon.BaseDamage,
                DamageType = weapon.DamageType,
                LifetimeRemainingTicks = checked((ushort)lifetime),
                Guidance = ProjectileGuidance.Ordinary
            });
        }
    }
}
}
