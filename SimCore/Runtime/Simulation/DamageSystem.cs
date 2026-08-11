namespace LegoSpaceRTS.SimCore
{
/// <summary>Delivery-independent input for the authoritative damage resolver.</summary>
public readonly struct DamageRequest
{
    public readonly EntityId Source;
    public readonly EntityId Target;
    public readonly ContentId WeaponProfile;
    public readonly ushort BaseDamage;
    public readonly DamageType DamageType;

    public DamageRequest(EntityId source, EntityId target, ContentId weaponProfile, ushort baseDamage, DamageType damageType)
    {
        Source = source; Target = target; WeaponProfile = weaponProfile; BaseDamage = baseDamage; DamageType = damageType;
    }
}

/// <summary>Applies the canonical class matrix and Armor Rating to committed hits.</summary>
public sealed class DamageSystem : ISimSystem
{
    public void Step(SimulationWorld world)
    {
        for (int i = 0; i < world.ProjectileImpacts.Count; i++)
        {
            ProjectileImpactRecord impact = world.ProjectileImpacts[i];
            Resolve(world, new DamageRequest(impact.Source, impact.Target, impact.WeaponProfile, impact.BaseDamage, impact.DamageType));
        }
    }

    public static Fix32 Resolve(SimulationWorld world, DamageRequest request)
    {
        if (request.Target == EntityId.None || request.BaseDamage == 0 ||
            request.DamageType < DamageType.Light || request.DamageType > DamageType.Control ||
            !world.Entities.Exists(request.Target) ||
            !world.Entities.Targetable.TryGet(request.Target, out Targetable targetable) ||
            !world.Entities.Health.Has(request.Target)) return Fix32.Zero;

        ref Health health = ref world.Entities.Health.Get(request.Target);
        if (health.IsDepleted) return Fix32.Zero;
        Fix32 damage = CalculateDamage(request.BaseDamage, request.DamageType, targetable.Class, health.ArmorRating);
        health.Current = Fix32.Max(Fix32.Zero, health.Current - damage);
        health.LastDamageTick = world.Tick.Value;
        return damage;
    }

    public static Fix32 CalculateDamage(ushort baseDamage, DamageType damageType, CombatTargetClass targetClass, byte armorRating)
    {
        if (baseDamage == 0) throw new System.ArgumentOutOfRangeException(nameof(baseDamage));
        if (damageType < DamageType.Light || damageType > DamageType.Control) throw new System.ArgumentOutOfRangeException(nameof(damageType));
        if (targetClass < CombatTargetClass.Personnel || targetClass > CombatTargetClass.FortifiedStructure) throw new System.ArgumentOutOfRangeException(nameof(targetClass));
        if (armorRating > 5) throw new System.ArgumentOutOfRangeException(nameof(armorRating));

        Fix32 classAdjusted = Fix32.FromInt(baseDamage) * TypeMultiplier(damageType, targetClass);
        Fix32 armored = classAdjusted * ArmorMultiplier(armorRating);
        // Armor by itself can never push an otherwise whole point of damage below one.
        return classAdjusted >= Fix32.One ? Fix32.Max(Fix32.One, armored) : armored;
    }

    public static Fix32 TypeMultiplier(DamageType damageType, CombatTargetClass targetClass)
    {
        int numerator = damageType switch
        {
            DamageType.Light => targetClass switch { CombatTargetClass.Personnel => 125, CombatTargetClass.LightMachine => 130, CombatTargetClass.MediumMachine => 90, CombatTargetClass.HeavyMachine => 70, CombatTargetClass.MassiveMachine => 60, CombatTargetClass.Structure => 60, _ => 50 },
            DamageType.General => targetClass switch { CombatTargetClass.Personnel => 110, CombatTargetClass.LightMachine => 105, CombatTargetClass.MediumMachine => 100, CombatTargetClass.HeavyMachine => 90, CombatTargetClass.MassiveMachine => 80, CombatTargetClass.Structure => 80, _ => 70 },
            DamageType.Breach => targetClass switch { CombatTargetClass.Personnel => 75, CombatTargetClass.LightMachine => 85, CombatTargetClass.MediumMachine => 110, CombatTargetClass.HeavyMachine => 155, CombatTargetClass.MassiveMachine => 145, CombatTargetClass.Structure => 110, _ => 100 },
            DamageType.Siege => targetClass switch { CombatTargetClass.Personnel => 55, CombatTargetClass.LightMachine => 65, CombatTargetClass.MediumMachine => 80, CombatTargetClass.HeavyMachine => 100, CombatTargetClass.MassiveMachine => 115, CombatTargetClass.Structure => 160, _ => 175 },
            DamageType.AntiAir => targetClass switch { CombatTargetClass.Personnel => 90, CombatTargetClass.LightMachine => 140, CombatTargetClass.MediumMachine => 125, CombatTargetClass.HeavyMachine => 110, CombatTargetClass.MassiveMachine => 95, CombatTargetClass.Structure => 30, _ => 25 },
            DamageType.Control => targetClass switch { CombatTargetClass.Personnel => 90, CombatTargetClass.LightMachine => 100, CombatTargetClass.MediumMachine => 85, CombatTargetClass.HeavyMachine => 70, CombatTargetClass.MassiveMachine => 50, CombatTargetClass.Structure => 35, _ => 25 },
            _ => throw new System.ArgumentOutOfRangeException(nameof(damageType))
        };
        return Fix32.FromRatio(numerator, 100);
    }

    public static Fix32 ArmorMultiplier(byte armorRating) => armorRating switch
    {
        0 => Fix32.One,
        1 => Fix32.FromRatio(96, 100),
        2 => Fix32.FromRatio(92, 100),
        3 => Fix32.FromRatio(88, 100),
        4 => Fix32.FromRatio(84, 100),
        5 => Fix32.FromRatio(80, 100),
        _ => throw new System.ArgumentOutOfRangeException(nameof(armorRating))
    };
}
}
