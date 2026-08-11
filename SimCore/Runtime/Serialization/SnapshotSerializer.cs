using System;
using System.Collections.Generic;
using System.IO;

namespace LegoSpaceRTS.SimCore
{
public static class SnapshotSerializer
{
    public const uint Magic = 0x53525453; // STRS
    public const ushort FormatVersion = 14;
    public const ushort SimulationProtocolVersion = 12;

    [Flags]
    private enum EntityComponents : uint
    {
        None = 0,
        Ownership = 1 << 0,
        Transform = 1 << 1,
        Movement = 1 << 2,
        Navigation = 1 << 3,
        Selectable = 1 << 4,
        Vision = 1 << 5,
        ResourceNode = 1 << 6,
        CommandQueue = 1 << 7,
        RouteCorridor = 1 << 8,
        Worker = 1 << 9,
        ResourceCarrier = 1 << 10,
        ResourceReceiver = 1 << 11,
        ResourceBank = 1 << 12,
        Building = 1 << 13,
        ConstructionSite = 1 << 14,
        Builder = 1 << 15,
        Production = 1 << 16,
        EnergyDomain = 1 << 17,
        EnergyDomainMember = 1 << 18,
        PowerState = 1 << 19,
        Targetable = 1 << 20,
        Targeting = 1 << 21,
        Weapon = 1 << 22,
        Health = 1 << 23,
        All = Ownership | Transform | Movement | Navigation | Selectable | Vision | ResourceNode | CommandQueue | RouteCorridor | Worker | ResourceCarrier | ResourceReceiver | ResourceBank | Building | ConstructionSite | Builder | Production | EnergyDomain | EnergyDomainMember | PowerState | Targetable | Targeting | Weapon | Health
    }

    public static byte[] Serialize(SimulationWorld world)
    {
        using MemoryStream ms = new(); using BinaryWriter w = new(ms);
        w.Write(Magic); w.Write(FormatVersion); w.Write(SimulationProtocolVersion); w.Write(world.Tick.Value);
        world.Map.Serialize(w);
        w.Write(world.Entities.NextEntityValue);
        IReadOnlyList<EntityId> alive = world.Entities.Alive; w.Write(alive.Count);
        for (int i = 0; i < alive.Count; i++) WriteEntity(w, world, alive[i]);
        world.Commands.Serialize(w);
        world.Fog.Serialize(w);
        WriteProjectileState(w, world);
        w.Flush(); return ms.ToArray();
    }

    public static SimulationWorld Deserialize(byte[] bytes, PrototypeContentCatalog? content = null)
    {
        using MemoryStream ms = new(bytes, false); using BinaryReader r = new(ms);
        if (r.ReadUInt32() != Magic) throw new InvalidDataException("Snapshot magic mismatch.");
        ushort format = r.ReadUInt16(), protocol = r.ReadUInt16();
        bool supportedLegacy = ((format == 2 || format == 3) && protocol == 1) || (format == 4 && protocol == 2) || (format == 5 && protocol == 3) || (format == 6 && protocol == 4) || (format == 7 && protocol == 5) || (format == 8 && protocol == 6) || (format == 9 && protocol == 7) || (format == 10 && protocol == 8) || (format == 11 && protocol == 9) || (format == 12 && protocol == 10) || (format == 13 && protocol == 11);
        if (!supportedLegacy && (format != FormatVersion || protocol != SimulationProtocolVersion)) throw new InvalidDataException($"Unsupported snapshot {format}/{protocol}.");
        SimTick tick = new(r.ReadInt32()); MapGrid map = MapGrid.Deserialize(r); uint nextEntity = r.ReadUInt32();
        EntityStore entities = new(); int entityCount = r.ReadInt32(); if (entityCount < 0 || entityCount > 10000) throw new InvalidDataException("Invalid entity count.");
        SimulationWorld temp = new(map, entities, new FogState(2), tick, content);
        for (int i = 0; i < entityCount; i++)
        {
            if (format == 2) ReadEntityV2(r, temp);
            else ReadEntity(r, temp, format);
        }
        if (format < 5) AddLegacyResourceBanks(entities);
        if (format < 6) AddLegacyBuildings(temp);
        if (format < 7) AddLegacyBuilders(temp);
        if (format < 8) AddLegacyProduction(temp);
        if (format < 11) AddLegacyCombatComponents(temp);
        else
        {
            if (format < 12) AddLegacyWeaponComponents(temp);
            if (format < 14) AddLegacyHealthComponents(temp);
        }
        entities.RestoreNextEntityValue(nextEntity);
        temp.Commands.Deserialize(r, includeBuildFields: format >= 6, includeEnergyPriority: format >= 10);
        temp.Fog = FogState.Deserialize(r);
        if (format >= 13) ReadProjectileState(r, temp);
        if (ms.Position != ms.Length) throw new InvalidDataException("Trailing snapshot bytes.");
        if (format < 9) InitializeLegacyEnergy(temp);
        else EnergyDomainSystem.RecalculateAll(temp);
        OperationsCapacitySystem.Recalculate(temp);
        temp.Spatial.Rebuild(temp.Entities);
        return temp;
    }

    private static void WriteProjectileState(BinaryWriter w, SimulationWorld world)
    {
        w.Write(world.NextProjectileValue);
        w.Write(world.Projectiles.Count);
        for (int i = 0; i < world.Projectiles.Count; i++)
        {
            ProjectileRecord projectile = world.Projectiles[i];
            w.Write(projectile.Id.Value); w.Write(projectile.Owner); w.Write(projectile.Source.Value); w.Write(projectile.Target.Value); w.Write(projectile.WeaponProfile.Value);
            w.Write(projectile.Position.X.Raw); w.Write(projectile.Position.Y.Raw); w.Write(projectile.Velocity.X.Raw); w.Write(projectile.Velocity.Y.Raw);
            w.Write(projectile.CommittedImpactPosition.X.Raw); w.Write(projectile.CommittedImpactPosition.Y.Raw);
            w.Write(projectile.BaseDamage); w.Write((byte)projectile.DamageType); w.Write(projectile.LifetimeRemainingTicks); w.Write((byte)projectile.Guidance);
        }
        w.Write(world.ProjectileImpacts.Count);
        for (int i = 0; i < world.ProjectileImpacts.Count; i++)
        {
            ProjectileImpactRecord impact = world.ProjectileImpacts[i];
            w.Write(impact.ProjectileId.Value); w.Write(impact.Owner); w.Write(impact.Source.Value); w.Write(impact.Target.Value); w.Write(impact.WeaponProfile.Value);
            w.Write(impact.Position.X.Raw); w.Write(impact.Position.Y.Raw); w.Write(impact.BaseDamage); w.Write((byte)impact.DamageType); w.Write(impact.ImpactTick);
        }
    }

    private static void ReadProjectileState(BinaryReader r, SimulationWorld world)
    {
        uint nextProjectile = r.ReadUInt32();
        int projectileCount = r.ReadInt32();
        if (nextProjectile == 0 || projectileCount < 0 || projectileCount > ProjectileSystem.MaximumProjectileRecords) throw new InvalidDataException("Invalid projectile state header.");
        for (int i = 0; i < projectileCount; i++)
        {
            ProjectileRecord projectile = new()
            {
                Id = new ProjectileId(r.ReadUInt32()), Owner = r.ReadByte(), Source = new EntityId(r.ReadUInt32()), Target = new EntityId(r.ReadUInt32()), WeaponProfile = new ContentId(r.ReadUInt32()),
                Position = new FixVec2(Fix32.FromRaw(r.ReadInt32()), Fix32.FromRaw(r.ReadInt32())), Velocity = new FixVec2(Fix32.FromRaw(r.ReadInt32()), Fix32.FromRaw(r.ReadInt32())),
                CommittedImpactPosition = new FixVec2(Fix32.FromRaw(r.ReadInt32()), Fix32.FromRaw(r.ReadInt32())), BaseDamage = r.ReadUInt16(), DamageType = (DamageType)r.ReadByte(),
                LifetimeRemainingTicks = r.ReadUInt16(), Guidance = (ProjectileGuidance)r.ReadByte()
            };
            if (projectile.Id.Value >= nextProjectile || projectile.Owner >= world.PlayerCount || projectile.Source == EntityId.None || projectile.Target == EntityId.None ||
                projectile.WeaponProfile.Value == 0 || projectile.BaseDamage == 0 || projectile.LifetimeRemainingTicks == 0 || projectile.Guidance != ProjectileGuidance.Ordinary ||
                projectile.DamageType < DamageType.Light || projectile.DamageType > DamageType.Control || projectile.Velocity.Equals(FixVec2.Zero))
                throw new InvalidDataException("Invalid projectile record.");
            world.AddRestoredProjectile(projectile);
        }
        world.NextProjectileValue = nextProjectile;

        int impactCount = r.ReadInt32();
        if (impactCount < 0 || impactCount > ProjectileSystem.MaximumProjectileRecords) throw new InvalidDataException("Invalid projectile impact count.");
        uint previousImpact = 0;
        for (int i = 0; i < impactCount; i++)
        {
            ProjectileId id = new(r.ReadUInt32()); byte owner = r.ReadByte(); EntityId source = new(r.ReadUInt32()); EntityId target = new(r.ReadUInt32()); ContentId weapon = new(r.ReadUInt32());
            FixVec2 position = new(Fix32.FromRaw(r.ReadInt32()), Fix32.FromRaw(r.ReadInt32())); ushort damage = r.ReadUInt16(); DamageType type = (DamageType)r.ReadByte(); int tick = r.ReadInt32();
            if (id.Value == 0 || id.Value >= nextProjectile || id.Value <= previousImpact || owner >= world.PlayerCount || source == EntityId.None || target == EntityId.None ||
                weapon.Value == 0 || damage == 0 || type < DamageType.Light || type > DamageType.Control || tick != world.Tick.Value) throw new InvalidDataException("Invalid projectile impact record.");
            world.ProjectileImpactsInternal.Add(new ProjectileImpactRecord(id, owner, source, target, weapon, position, damage, type, tick));
            previousImpact = id.Value;
        }
    }

    private static void WriteEntity(BinaryWriter w, SimulationWorld world, EntityId id)
    {
        w.Write(id.Value);
        EntityComponents components = EntityComponents.None;
        if (world.Entities.Ownership.Has(id)) components |= EntityComponents.Ownership;
        if (world.Entities.Transform.Has(id)) components |= EntityComponents.Transform;
        if (world.Entities.Movement.Has(id)) components |= EntityComponents.Movement;
        if (world.Entities.Navigation.Has(id)) components |= EntityComponents.Navigation;
        if (world.Entities.Selectable.Has(id)) components |= EntityComponents.Selectable;
        if (world.Entities.Vision.Has(id)) components |= EntityComponents.Vision;
        if (world.Entities.ResourceNode.Has(id)) components |= EntityComponents.ResourceNode;
        if (world.Entities.Worker.Has(id)) components |= EntityComponents.Worker;
        if (world.Entities.Builder.Has(id)) components |= EntityComponents.Builder;
        if (world.Entities.ResourceCarrier.Has(id)) components |= EntityComponents.ResourceCarrier;
        if (world.Entities.ResourceReceiver.Has(id)) components |= EntityComponents.ResourceReceiver;
        if (world.Entities.ResourceBank.Has(id)) components |= EntityComponents.ResourceBank;
        if (world.Entities.Building.Has(id)) components |= EntityComponents.Building;
        if (world.Entities.EnergyDomain.Has(id)) components |= EntityComponents.EnergyDomain;
        if (world.Entities.EnergyDomainMember.Has(id)) components |= EntityComponents.EnergyDomainMember;
        if (world.Entities.PowerState.Has(id)) components |= EntityComponents.PowerState;
        if (world.Entities.ConstructionSite.Has(id)) components |= EntityComponents.ConstructionSite;
        if (world.Entities.Production.Has(id)) components |= EntityComponents.Production;
        if (world.Entities.Targetable.Has(id)) components |= EntityComponents.Targetable;
        if (world.Entities.Targeting.Has(id)) components |= EntityComponents.Targeting;
        if (world.Entities.Weapon.Has(id)) components |= EntityComponents.Weapon;
        if (world.Entities.Health.Has(id)) components |= EntityComponents.Health;
        if (world.TryGetQueue(id, out _)) components |= EntityComponents.CommandQueue;
        if (world.Corridors.ContainsKey(id.Value)) components |= EntityComponents.RouteCorridor;
        w.Write((uint)components);

        if ((components & EntityComponents.Ownership) != 0) w.Write(world.Entities.Ownership.Get(id).PlayerSlot);
        if ((components & EntityComponents.Transform) != 0) WriteTransform(w, world.Entities.Transform.Get(id));
        if ((components & EntityComponents.Movement) != 0) WriteMovement(w, world.Entities.Movement.Get(id));
        if ((components & EntityComponents.Navigation) != 0) WriteNavigation(w, world.Entities.Navigation.Get(id));
        if ((components & EntityComponents.Selectable) != 0)
        {
            Selectable s = world.Entities.Selectable.Get(id); w.Write(s.IsSelectable); w.Write(s.ContentType.Value); w.Write((byte)s.Kind);
        }
        if ((components & EntityComponents.Vision) != 0)
        {
            Vision v = world.Entities.Vision.Get(id); w.Write(v.RadiusBuildCells); w.Write(v.IsAirVision); w.Write(v.LastFogX); w.Write(v.LastFogY);
        }
        if ((components & EntityComponents.ResourceNode) != 0) WriteResourceNode(w, world.Entities.ResourceNode.Get(id));
        if ((components & EntityComponents.Worker) != 0) WriteWorker(w, world.Entities.Worker.Get(id));
        if ((components & EntityComponents.Builder) != 0) WriteBuilder(w, world.Entities.Builder.Get(id));
        if ((components & EntityComponents.ResourceCarrier) != 0) WriteResourceCarrier(w, world.Entities.ResourceCarrier.Get(id));
        if ((components & EntityComponents.ResourceReceiver) != 0) WriteResourceReceiver(w, world.Entities.ResourceReceiver.Get(id));
        if ((components & EntityComponents.ResourceBank) != 0) WriteResourceBank(w, world.Entities.ResourceBank.Get(id));
        if ((components & EntityComponents.Building) != 0) WriteBuilding(w, world.Entities.Building.Get(id));
        if ((components & EntityComponents.EnergyDomain) != 0) WriteEnergyDomain(w, world.Entities.EnergyDomain.Get(id));
        if ((components & EntityComponents.EnergyDomainMember) != 0) w.Write(world.Entities.EnergyDomainMember.Get(id).DomainRoot.Value);
        if ((components & EntityComponents.PowerState) != 0) { PowerState state = world.Entities.PowerState.Get(id); w.Write((byte)state.Priority); w.Write(state.IsPowered); }
        if ((components & EntityComponents.ConstructionSite) != 0) WriteConstructionSite(w, world.Entities.ConstructionSite.Get(id));
        if ((components & EntityComponents.Production) != 0) WriteProduction(w, world.Entities.Production.Get(id));
        if ((components & EntityComponents.Targetable) != 0) WriteTargetable(w, world.Entities.Targetable.Get(id));
        if ((components & EntityComponents.Targeting) != 0) WriteTargeting(w, world.Entities.Targeting.Get(id));
        if ((components & EntityComponents.Weapon) != 0) WriteWeapon(w, world.Entities.Weapon.Get(id));
        if ((components & EntityComponents.Health) != 0) WriteHealth(w, world.Entities.Health.Get(id));
        if ((components & EntityComponents.CommandQueue) != 0) world.GetQueue(id).Serialize(w, includeTargetEntity: true);
        if ((components & EntityComponents.RouteCorridor) != 0) WriteCorridor(w, world.Corridors[id.Value]);
    }

    private static void WriteTransform(BinaryWriter w, SimTransform t)
    {
        w.Write(t.Position.X.Raw); w.Write(t.Position.Y.Raw); w.Write(t.Orientation.Raw);
    }

    private static void WriteMovement(BinaryWriter w, Movement m)
    {
        w.Write(m.MaxSpeed.Raw); w.Write(m.Acceleration.Raw); w.Write(m.Deceleration.Raw); w.Write(m.TurnRatePerTick); w.Write((byte)m.ReversePolicy);
        w.Write(m.CurrentSpeed.Raw); w.Write(m.CurrentVelocity.X.Raw); w.Write(m.CurrentVelocity.Y.Raw); w.Write(m.DesiredMovement.X.Raw); w.Write(m.DesiredMovement.Y.Raw);
        w.Write(m.PathIndex); w.Write((byte)m.State); w.Write(m.StuckTicks); w.Write(m.CompressionTicks); w.Write(m.LastPosition.X.Raw); w.Write(m.LastPosition.Y.Raw);
    }

    private static void WriteNavigation(BinaryWriter w, NavigationAgent n)
    {
        w.Write((byte)n.Footprint); w.Write((byte)n.Layer); w.Write(n.Target.X.Raw); w.Write(n.Target.Y.Raw); w.Write(n.HasTarget); w.Write(n.PathDirty); w.Write(n.PathTopologyVersion); w.Write(n.RequestAge); FormationIntentCodec.Write(w,n.Formation);
    }

    private static void WriteResourceNode(BinaryWriter w, ResourceNode n)
    {
        w.Write((byte)n.Type); w.Write((byte)n.DepositSize); w.Write((byte)n.HarvestInteraction); w.Write((byte)n.DepletionProfile);
        w.Write(n.Capacity); w.Write(n.Remaining); w.Write(n.ReducedThresholdBasisPoints); w.Write(n.LowThresholdBasisPoints); w.Write(n.CriticalThresholdBasisPoints);
    }

    private static void WriteWorker(BinaryWriter w, Worker worker)
    {
        w.Write(worker.ResourceTarget.Value); w.Write(worker.ReceiverTarget.Value); w.Write((byte)worker.TaskState); w.Write(worker.ExtractionTicks); w.Write(worker.TicksPerOre);
    }

    private static void WriteBuilder(BinaryWriter w, Builder builder)
    {
        w.Write(builder.ConstructionTarget.Value); w.Write((byte)builder.JobState);
    }

    private static void WriteResourceCarrier(BinaryWriter w, ResourceCarrier carrier)
    {
        w.Write((byte)carrier.Type); w.Write(carrier.Amount); w.Write(carrier.Capacity);
    }

    private static void WriteResourceReceiver(BinaryWriter w, ResourceReceiver receiver)
    {
        w.Write((byte)receiver.AcceptedType); w.Write(receiver.PendingHauledAmount); w.Write(receiver.IsHqEmergencyReceiver);
    }

    private static void WriteResourceBank(BinaryWriter w, ResourceBank bank)
    {
        w.Write((byte)bank.Type); w.Write(bank.ProcessedAmount);
    }

    private static void WriteBuilding(BinaryWriter w, Building building)
    {
        w.Write(building.Type.Value); w.Write(building.AnchorX); w.Write(building.AnchorY); w.Write(building.Orientation);
        w.Write(building.FootprintWidth); w.Write(building.FootprintHeight); w.Write((byte)building.State);
    }

    private static void WriteConstructionSite(BinaryWriter w, ConstructionSite site)
    {
        w.Write(site.AssignedBuilder.Value); w.Write(site.FundingBank.Value); w.Write(site.ReservedOre); w.Write(site.ConsumedOre); w.Write(site.RequiredEnergy);
        w.Write(site.ReservedEnergy); w.Write(site.ConsumedEnergy); w.Write(site.EnergyDomainRoot.Value);
        w.Write(site.RequiredTicks); w.Write(site.ProgressTicks);
    }

    private static void WriteEnergyDomain(BinaryWriter w, EnergyDomain domain)
    {
        w.Write(domain.Reserve.Raw); w.Write(domain.ReserveCapacity.Raw); w.Write(domain.GenerationPerSecond);
        w.Write(domain.ContinuousDemandPerSecond); w.Write(domain.PoweredDemandPerSecond); w.Write(domain.FlowRemainderRaw);
        w.Write(domain.BrownoutRevision); w.Write((byte)domain.LastBrownoutEvent); w.Write(domain.IsBrownout);
    }

    private static void WriteProduction(BinaryWriter w, Production production)
    {
        w.Write(production.Count); w.Write(production.SpawnBlocked); w.Write(production.HasRallyPoint);
        w.Write(production.RallyPoint.X.Raw); w.Write(production.RallyPoint.Y.Raw); w.Write(production.RallyTargetEntity.Value);
        for (int i = 0; i < production.Count; i++)
        {
            ProductionQueueItem item = production.Get(i);
            w.Write(item.UnitType.Value); w.Write(item.FundingBank.Value); w.Write(item.ReservedOre); w.Write(item.RequiredEnergy);
            w.Write(item.RequiredCrystals); w.Write(item.ReservedOperationsCapacity); w.Write(item.TotalTicks); w.Write(item.RemainingTicks);
        }
    }

    private static void WriteTargetable(BinaryWriter w, Targetable targetable)
    {
        w.Write((byte)targetable.Class); w.Write((byte)targetable.Layer); w.Write((ushort)targetable.Flags);
    }

    private static void WriteTargeting(BinaryWriter w, Targeting targeting)
    {
        w.Write(targeting.CurrentTarget.Value); w.Write(targeting.AcquisitionRadius.Raw); w.Write((byte)targeting.LegalLayers);
        w.Write((byte)targeting.LegalClasses); w.Write((byte)targeting.PriorityProfile); w.Write((byte)targeting.SelectionKind);
    }

    private static void WriteWeapon(BinaryWriter w, WeaponState weapon)
    {
        w.Write(weapon.WeaponProfile.Value); w.Write(weapon.CooldownRemainingTicks); w.Write(weapon.FireSequence);
        w.Write(weapon.LastFiredTarget.Value); w.Write(weapon.LastFiredTick);
    }

    private static void WriteHealth(BinaryWriter w, Health health)
    {
        w.Write(health.Maximum.Raw); w.Write(health.Current.Raw); w.Write(health.ArmorRating); w.Write(health.LastDamageTick);
    }

    private static void WriteCorridor(BinaryWriter w, RouteCorridor path)
    {
        w.Write(path.TopologyVersion); w.Write(path.Cells.Count);
        for (int p = 0; p < path.Cells.Count; p++) { w.Write(path.Cells[p].X); w.Write(path.Cells[p].Y); }
    }

    private static void ReadEntity(BinaryReader r, SimulationWorld world, ushort format)
    {
        EntityId id = world.Entities.CreateRestored(r.ReadUInt32());
        EntityComponents components = (EntityComponents)(format >= 8 ? r.ReadUInt32() : r.ReadUInt16());
        if ((components & ~EntityComponents.All) != 0) throw new InvalidDataException("Snapshot entity has unknown component bits.");
        if ((components & EntityComponents.Ownership) != 0) world.Entities.Ownership.Set(id, new Ownership { PlayerSlot = r.ReadByte() });
        if ((components & EntityComponents.Transform) != 0) world.Entities.Transform.Set(id, ReadTransform(r));
        if ((components & EntityComponents.Movement) != 0) world.Entities.Movement.Set(id, ReadMovement(r));
        if ((components & EntityComponents.Navigation) != 0) world.Entities.Navigation.Set(id, ReadNavigation(r));
        if ((components & EntityComponents.Selectable) != 0) world.Entities.Selectable.Set(id, new Selectable { IsSelectable = r.ReadBoolean(), ContentType = new ContentId(r.ReadUInt32()), Kind=(SelectableKind)r.ReadByte() });
        if ((components & EntityComponents.Vision) != 0) world.Entities.Vision.Set(id, new Vision { RadiusBuildCells = r.ReadByte(), IsAirVision = r.ReadBoolean(), LastFogX = r.ReadInt32(), LastFogY = r.ReadInt32() });
        if ((components & EntityComponents.ResourceNode) != 0) world.Entities.ResourceNode.Set(id, ReadResourceNode(r));
        if ((components & EntityComponents.Worker) != 0) world.Entities.Worker.Set(id, ReadWorker(r));
        if ((components & EntityComponents.Builder) != 0) world.Entities.Builder.Set(id, ReadBuilder(r));
        if ((components & EntityComponents.ResourceCarrier) != 0) world.Entities.ResourceCarrier.Set(id, ReadResourceCarrier(r));
        if ((components & EntityComponents.ResourceReceiver) != 0) world.Entities.ResourceReceiver.Set(id, ReadResourceReceiver(r));
        if ((components & EntityComponents.ResourceBank) != 0) world.Entities.ResourceBank.Set(id, ReadResourceBank(r));
        if ((components & EntityComponents.Building) != 0) world.Entities.Building.Set(id, ReadBuilding(r));
        if ((components & EntityComponents.EnergyDomain) != 0) world.Entities.EnergyDomain.Set(id, ReadEnergyDomain(r, format));
        if ((components & EntityComponents.EnergyDomainMember) != 0) world.Entities.EnergyDomainMember.Set(id, new EnergyDomainMember { DomainRoot = new EntityId(r.ReadUInt32()) });
        if ((components & EntityComponents.PowerState) != 0)
        {
            EnergyPriority priority = (EnergyPriority)r.ReadByte(); bool powered = r.ReadBoolean();
            if (priority < EnergyPriority.High || priority > EnergyPriority.Low) throw new InvalidDataException("Invalid Energy priority.");
            world.Entities.PowerState.Set(id, new PowerState { Priority = priority, IsPowered = powered });
        }
        if ((components & EntityComponents.ConstructionSite) != 0) world.Entities.ConstructionSite.Set(id, ReadConstructionSite(r, format));
        if ((components & EntityComponents.Production) != 0) world.Entities.Production.Set(id, ReadProduction(r));
        if ((components & EntityComponents.Targetable) != 0) world.Entities.Targetable.Set(id, ReadTargetable(r));
        if ((components & EntityComponents.Targeting) != 0) world.Entities.Targeting.Set(id, ReadTargeting(r));
        if ((components & EntityComponents.Weapon) != 0)
        {
            WeaponState weapon = ReadWeapon(r);
            if (!world.Content.TryGetWeapon(weapon.WeaponProfile, out WeaponDefinition definition) || weapon.CooldownRemainingTicks > definition.CooldownTicks)
                throw new InvalidDataException("Invalid weapon state.");
            world.Entities.Weapon.Set(id, weapon);
        }
        if ((components & EntityComponents.Health) != 0) world.Entities.Health.Set(id, ReadHealth(r, world.Tick.Value));
        if ((components & EntityComponents.CommandQueue) != 0) world.GetQueue(id).Deserialize(r, includeTargetEntity: format >= 4);
        if ((components & EntityComponents.RouteCorridor) != 0) world.Corridors[id.Value] = ReadCorridor(r);
    }

    private static SimTransform ReadTransform(BinaryReader r)
        => new() { Position = new FixVec2(Fix32.FromRaw(r.ReadInt32()), Fix32.FromRaw(r.ReadInt32())), Orientation = new Angle16(r.ReadUInt16()) };

    private static Movement ReadMovement(BinaryReader r)
        => new()
        {
            MaxSpeed=Fix32.FromRaw(r.ReadInt32()), Acceleration=Fix32.FromRaw(r.ReadInt32()), Deceleration=Fix32.FromRaw(r.ReadInt32()), TurnRatePerTick=r.ReadUInt16(), ReversePolicy=(ReversePolicy)r.ReadByte(),
            CurrentSpeed=Fix32.FromRaw(r.ReadInt32()), CurrentVelocity=new FixVec2(Fix32.FromRaw(r.ReadInt32()),Fix32.FromRaw(r.ReadInt32())), DesiredMovement=new FixVec2(Fix32.FromRaw(r.ReadInt32()),Fix32.FromRaw(r.ReadInt32())),
            PathIndex=r.ReadInt32(), State=(MovementState)r.ReadByte(), StuckTicks=r.ReadInt32(), CompressionTicks=r.ReadInt32(), LastPosition=new FixVec2(Fix32.FromRaw(r.ReadInt32()),Fix32.FromRaw(r.ReadInt32()))
        };

    private static NavigationAgent ReadNavigation(BinaryReader r)
        => new() { Footprint = (FootprintClass)r.ReadByte(), Layer=(MovementLayer)r.ReadByte(), Target = new FixVec2(Fix32.FromRaw(r.ReadInt32()), Fix32.FromRaw(r.ReadInt32())), HasTarget = r.ReadBoolean(), PathDirty = r.ReadBoolean(), PathTopologyVersion = r.ReadInt32(), RequestAge = r.ReadInt32(), Formation=FormationIntentCodec.Read(r) };

    private static ResourceNode ReadResourceNode(BinaryReader r)
        => new()
        {
            Type=(ResourceType)r.ReadByte(), DepositSize=(ResourceDepositSize)r.ReadByte(), HarvestInteraction=(HarvestInteraction)r.ReadByte(), DepletionProfile=(ResourceDepletionProfile)r.ReadByte(),
            Capacity=r.ReadInt32(), Remaining=r.ReadInt32(), ReducedThresholdBasisPoints=r.ReadUInt16(), LowThresholdBasisPoints=r.ReadUInt16(), CriticalThresholdBasisPoints=r.ReadUInt16()
        };

    private static Worker ReadWorker(BinaryReader r)
        => new() { ResourceTarget = new EntityId(r.ReadUInt32()), ReceiverTarget = new EntityId(r.ReadUInt32()), TaskState = (WorkerTaskState)r.ReadByte(), ExtractionTicks = r.ReadUInt16(), TicksPerOre = r.ReadUInt16() };

    private static Builder ReadBuilder(BinaryReader r)
    {
        Builder builder = new() { ConstructionTarget = new EntityId(r.ReadUInt32()), JobState = (BuilderJobState)r.ReadByte() };
        if (builder.JobState < BuilderJobState.Idle || builder.JobState > BuilderJobState.Constructing) throw new InvalidDataException("Invalid builder job state.");
        return builder;
    }

    private static ResourceCarrier ReadResourceCarrier(BinaryReader r)
        => new() { Type = (ResourceType)r.ReadByte(), Amount = r.ReadByte(), Capacity = r.ReadByte() };

    private static ResourceReceiver ReadResourceReceiver(BinaryReader r)
        => new() { AcceptedType = (ResourceType)r.ReadByte(), PendingHauledAmount = r.ReadInt32(), IsHqEmergencyReceiver = r.ReadBoolean() };

    private static ResourceBank ReadResourceBank(BinaryReader r)
        => new() { Type = (ResourceType)r.ReadByte(), ProcessedAmount = r.ReadInt32() };

    private static Building ReadBuilding(BinaryReader r)
        => new() { Type = new ContentId(r.ReadUInt32()), AnchorX = r.ReadInt16(), AnchorY = r.ReadInt16(), Orientation = r.ReadByte(), FootprintWidth = r.ReadByte(), FootprintHeight = r.ReadByte(), State = (BuildingState)r.ReadByte() };

    private static ConstructionSite ReadConstructionSite(BinaryReader r, ushort format)
    {
        ConstructionSite site = new()
        {
            AssignedBuilder = new EntityId(r.ReadUInt32()), FundingBank = new EntityId(r.ReadUInt32()), ReservedOre = r.ReadInt32(),
            ConsumedOre = format >= 7 ? r.ReadInt32() : 0, RequiredEnergy = r.ReadInt32()
        };
        if (format >= 9)
        {
            site.ReservedEnergy = r.ReadInt32(); site.ConsumedEnergy = r.ReadInt32(); site.EnergyDomainRoot = new EntityId(r.ReadUInt32());
        }
        site.RequiredTicks = r.ReadUInt16(); site.ProgressTicks = r.ReadUInt16();
        return site;
    }

    private static EnergyDomain ReadEnergyDomain(BinaryReader r, ushort format)
    {
        EnergyDomain domain = new()
        {
            Reserve = Fix32.FromRaw(r.ReadInt32()), ReserveCapacity = Fix32.FromRaw(r.ReadInt32()), GenerationPerSecond = r.ReadInt32(),
            ContinuousDemandPerSecond = r.ReadInt32()
        };
        if (format >= 10) domain.PoweredDemandPerSecond = r.ReadInt32();
        domain.FlowRemainderRaw = r.ReadInt32();
        if (format >= 10)
        {
            domain.BrownoutRevision = r.ReadUInt32(); domain.LastBrownoutEvent = (BrownoutEventKind)r.ReadByte(); domain.IsBrownout = r.ReadBoolean();
            if (domain.LastBrownoutEvent < BrownoutEventKind.None || domain.LastBrownoutEvent > BrownoutEventKind.Recovered) throw new InvalidDataException("Invalid brownout event.");
        }
        return domain;
    }

    private static Production ReadProduction(BinaryReader r)
    {
        Production production = new()
        {
            Count = r.ReadByte(), SpawnBlocked = r.ReadBoolean(), HasRallyPoint = r.ReadBoolean(),
            RallyPoint = new FixVec2(Fix32.FromRaw(r.ReadInt32()), Fix32.FromRaw(r.ReadInt32())), RallyTargetEntity = new EntityId(r.ReadUInt32())
        };
        if (production.Count > Production.Capacity) throw new InvalidDataException("Invalid production queue count.");
        byte count = production.Count; production.Count = 0;
        for (int i = 0; i < count; i++)
        {
            ProductionQueueItem item = new()
            {
                UnitType = new ContentId(r.ReadUInt32()), FundingBank = new EntityId(r.ReadUInt32()), ReservedOre = r.ReadUInt16(), RequiredEnergy = r.ReadUInt16(),
                RequiredCrystals = r.ReadByte(), ReservedOperationsCapacity = r.ReadByte(), TotalTicks = r.ReadUInt16(), RemainingTicks = r.ReadUInt16()
            };
            if (item.UnitType.Value == 0 || item.TotalTicks == 0 || item.RemainingTicks > item.TotalTicks || !production.TryEnqueue(item)) throw new InvalidDataException("Invalid production queue item.");
        }
        return production;
    }

    private static Targetable ReadTargetable(BinaryReader r)
    {
        Targetable targetable = new() { Class = (CombatTargetClass)r.ReadByte(), Layer = (CombatTargetLayer)r.ReadByte(), Flags = (CombatTargetFlags)r.ReadUInt16() };
        if (targetable.Class < CombatTargetClass.Personnel || targetable.Class > CombatTargetClass.FortifiedStructure ||
            targetable.Layer < CombatTargetLayer.Ground || targetable.Layer > CombatTargetLayer.TrueAir) throw new InvalidDataException("Invalid combat target metadata.");
        return targetable;
    }

    private static Targeting ReadTargeting(BinaryReader r)
    {
        Targeting targeting = new()
        {
            CurrentTarget = new EntityId(r.ReadUInt32()), AcquisitionRadius = Fix32.FromRaw(r.ReadInt32()), LegalLayers = (TargetLayerMask)r.ReadByte(),
            LegalClasses = (TargetClassMask)r.ReadByte(), PriorityProfile = (TargetPriorityProfile)r.ReadByte(), SelectionKind = (TargetSelectionKind)r.ReadByte()
        };
        if (targeting.AcquisitionRadius <= Fix32.Zero || targeting.LegalLayers == TargetLayerMask.None || (targeting.LegalLayers & ~TargetLayerMask.All) != 0 ||
            targeting.LegalClasses == TargetClassMask.None || (targeting.LegalClasses & ~TargetClassMask.All) != 0 ||
            targeting.PriorityProfile < TargetPriorityProfile.AntiLight || targeting.PriorityProfile > TargetPriorityProfile.Control ||
            targeting.SelectionKind < TargetSelectionKind.None || targeting.SelectionKind > TargetSelectionKind.DirectOrder ||
            (targeting.SelectionKind == TargetSelectionKind.None && targeting.CurrentTarget != EntityId.None) ||
            (targeting.SelectionKind != TargetSelectionKind.None && targeting.CurrentTarget == EntityId.None)) throw new InvalidDataException("Invalid targeting state.");
        return targeting;
    }

    private static WeaponState ReadWeapon(BinaryReader r)
    {
        WeaponState weapon = new()
        {
            WeaponProfile = new ContentId(r.ReadUInt32()), CooldownRemainingTicks = r.ReadUInt16(), FireSequence = r.ReadUInt32(),
            LastFiredTarget = new EntityId(r.ReadUInt32()), LastFiredTick = r.ReadInt32()
        };
        if (weapon.WeaponProfile.Value == 0 || weapon.LastFiredTick < -1 ||
            (weapon.FireSequence == 0 && (weapon.LastFiredTarget != EntityId.None || weapon.LastFiredTick != -1)) ||
            (weapon.FireSequence > 0 && (weapon.LastFiredTarget == EntityId.None || weapon.LastFiredTick < 0)))
            throw new InvalidDataException("Invalid weapon firing record.");
        return weapon;
    }

    private static Health ReadHealth(BinaryReader r, int currentTick)
    {
        Health health = new()
        {
            Maximum = Fix32.FromRaw(r.ReadInt32()), Current = Fix32.FromRaw(r.ReadInt32()), ArmorRating = r.ReadByte(), LastDamageTick = r.ReadInt32()
        };
        if (health.Maximum <= Fix32.Zero || health.Current < Fix32.Zero || health.Current > health.Maximum || health.ArmorRating > 5 ||
            health.LastDamageTick < -1 || health.LastDamageTick > currentTick) throw new InvalidDataException("Invalid health state.");
        return health;
    }

    private static void AddLegacyProduction(SimulationWorld world)
    {
        IReadOnlyList<EntityId> alive = world.Entities.Alive;
        for (int i = 0; i < alive.Count; i++)
        {
            EntityId id = alive[i];
            if (world.Entities.Building.TryGet(id, out Building building) && building.State == BuildingState.Completed && world.Content.IsProducer(building.Type))
                world.Entities.Production.Set(id, new Production());
        }
    }

    private static void AddLegacyCombatComponents(SimulationWorld world)
    {
        IReadOnlyList<EntityId> alive = world.Entities.Alive;
        for (int i = 0; i < alive.Count; i++)
        {
            EntityId id = alive[i];
            if (!world.Entities.Selectable.TryGet(id, out Selectable selectable) ||
                !world.Content.TryGetEntity(selectable.ContentType, out PrototypeEntityDefinition definition)) continue;
            ScenarioFactory.AddCombatComponents(world, id, definition);
        }
    }

    private static void AddLegacyWeaponComponents(SimulationWorld world)
    {
        IReadOnlyList<EntityId> alive = world.Entities.Alive;
        for (int i = 0; i < alive.Count; i++)
        {
            EntityId id = alive[i];
            if (!world.Entities.Selectable.TryGet(id, out Selectable selectable) ||
                !world.Content.TryGetEntity(selectable.ContentType, out PrototypeEntityDefinition definition)) continue;
            ScenarioFactory.AddWeaponComponent(world, id, definition);
        }
    }

    private static void AddLegacyHealthComponents(SimulationWorld world)
    {
        IReadOnlyList<EntityId> alive = world.Entities.Alive;
        for (int i = 0; i < alive.Count; i++)
        {
            EntityId id = alive[i];
            if (!world.Entities.Targetable.Has(id) || !world.Entities.Selectable.TryGet(id, out Selectable selectable) ||
                !world.Content.TryGetEntity(selectable.ContentType, out PrototypeEntityDefinition definition) || !definition.Combat.IsTargetable) continue;
            Fix32 maximum = Fix32.FromInt(definition.Combat.MaximumHitPoints);
            world.Entities.Health.Set(id, new Health { Maximum = maximum, Current = maximum, ArmorRating = definition.Combat.ArmorRating, LastDamageTick = -1 });
        }
    }

    private static void InitializeLegacyEnergy(SimulationWorld world)
    {
        EnergyDomainSystem.InitializeOpeningDomains(world);
        IReadOnlyList<EntityId> alive = world.Entities.Alive;
        for (int i = 0; i < alive.Count; i++)
        {
            EntityId id = alive[i];
            if (!world.Entities.ConstructionSite.Has(id) && !world.Entities.Production.Has(id)) continue;
            if (!world.Entities.Ownership.TryGet(id, out Ownership ownership) || !EnergyDomainSystem.TryResolveForEntity(world, id, ownership.PlayerSlot, out EntityId root)) continue;
            if (world.Entities.ConstructionSite.Has(id))
            {
                ref ConstructionSite site = ref world.Entities.ConstructionSite.Get(id);
                int energy = site.RequiredEnergy;
                SpendLegacyEnergy(world, root, energy);
                site.ReservedEnergy = energy; site.ConsumedEnergy = 0; site.EnergyDomainRoot = root;
            }
            if (!world.Entities.Production.TryGet(id, out Production production)) continue;
            for (int q = 0; q < production.Count; q++) SpendLegacyEnergy(world, root, production.Get(q).RequiredEnergy);
        }
    }

    private static void SpendLegacyEnergy(SimulationWorld world, EntityId root, int amount)
    {
        if (amount <= 0) return;
        ref EnergyDomain domain = ref world.Entities.EnergyDomain.Get(root);
        domain.Reserve = Fix32.Max(Fix32.Zero, domain.Reserve - Fix32.FromInt(amount));
    }

    private static void AddLegacyBuilders(SimulationWorld world)
    {
        IReadOnlyList<EntityId> alive = world.Entities.Alive;
        for (int i = 0; i < alive.Count; i++)
        {
            EntityId id = alive[i];
            if (world.Entities.Worker.Has(id)) world.Entities.Builder.Set(id, new Builder { ConstructionTarget = EntityId.None, JobState = BuilderJobState.Idle });
        }
        for (int i = 0; i < alive.Count; i++)
        {
            EntityId siteId = alive[i];
            if (!world.Entities.ConstructionSite.TryGet(siteId, out ConstructionSite site) || !world.Entities.Builder.Has(site.AssignedBuilder)) continue;
            ConstructionSystem.StartBuilder(world, site.AssignedBuilder, siteId);
        }
    }

    private static void AddLegacyResourceBanks(EntityStore entities)
    {
        IReadOnlyList<EntityId> alive = entities.Alive;
        for (int i = 0; i < alive.Count; i++)
        {
            EntityId id = alive[i];
            if (!entities.ResourceReceiver.TryGet(id, out ResourceReceiver receiver)) continue;
            entities.ResourceBank.Set(id, new ResourceBank { Type = receiver.AcceptedType, ProcessedAmount = 0 });
        }
    }

    private static void AddLegacyBuildings(SimulationWorld world)
    {
        IReadOnlyList<EntityId> alive = world.Entities.Alive;
        for (int i = 0; i < alive.Count; i++)
        {
            EntityId id = alive[i];
            if (!world.Entities.ResourceReceiver.Has(id) || !world.Entities.Selectable.TryGet(id, out Selectable selectable) ||
                !world.Entities.Transform.TryGet(id, out SimTransform transform) || !world.Content.TryGetBuilding(selectable.ContentType, out BuildingDefinition definition)) continue;
            byte width = definition.RotatedWidth(0), height = definition.RotatedHeight(0);
            Building building = new()
            {
                Type = selectable.ContentType, AnchorX = checked((short)(transform.Position.X.FloorToInt() - width / 2)),
                AnchorY = checked((short)(transform.Position.Y.FloorToInt() - height / 2)), Orientation = 0,
                FootprintWidth = width, FootprintHeight = height, State = BuildingState.Completed
            };
            world.Entities.Building.Set(id, building);
            world.SetConstructionOccupied(building, true);
        }
    }

    private static RouteCorridor ReadCorridor(BinaryReader r)
    {
        RouteCorridor path = new() { TopologyVersion = r.ReadInt32() }; int count = r.ReadInt32(); if (count < 0 || count > 20000) throw new InvalidDataException("Invalid path count.");
        for (int i = 0; i < count; i++) path.Cells.Add(new NavCell(r.ReadInt16(), r.ReadInt16()));
        return path;
    }

    private static void ReadEntityV2(BinaryReader r, SimulationWorld world)
    {
        EntityId id = world.Entities.CreateRestored(r.ReadUInt32());
        world.Entities.Ownership.Set(id, new Ownership { PlayerSlot = r.ReadByte() });
        world.Entities.Transform.Set(id, ReadTransform(r));
        world.Entities.Movement.Set(id, ReadMovement(r));
        world.Entities.Navigation.Set(id, ReadNavigation(r));
        world.Entities.Selectable.Set(id, new Selectable { IsSelectable = r.ReadBoolean(), ContentType = new ContentId(r.ReadUInt32()), Kind=(SelectableKind)r.ReadByte() });
        world.Entities.Vision.Set(id, new Vision { RadiusBuildCells = r.ReadByte(), IsAirVision = r.ReadBoolean(), LastFogX = r.ReadInt32(), LastFogY = r.ReadInt32() });
        world.GetQueue(id).Deserialize(r, includeTargetEntity: false);
        if (r.ReadBoolean()) world.Corridors[id.Value] = ReadCorridor(r);
    }
}

public static class StateHasher
{
    public static ulong Hash(SimulationWorld world) => DeterministicHash.Fnv1A64(SnapshotSerializer.Serialize(world));
    public static string HashHex(SimulationWorld world) => Hash(world).ToString("X16");
}
}
