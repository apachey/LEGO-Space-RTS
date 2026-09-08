using System;
using System.Collections.Generic;
using System.IO;

namespace LegoSpaceRTS.SimCore
{
public static class SnapshotSerializer
{
    public const uint Magic = 0x53525453; // STRS
    public const ushort FormatVersion = 21;
    public const ushort SimulationProtocolVersion = 19;

    [Flags]
    private enum EntityComponents : ulong
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
        Destruction = 1 << 24,
        Passenger = 1 << 25,
        Transport = 1 << 26,
        Transformation = 1 << 27,
        WorksiteNode = 1UL << 28,
        WorksiteMember = 1UL << 29,
        WorksiteComponent = 1UL << 30,
        Excavatable = 1UL << 31,
        Deployment = 1UL << 32,
        ForwardServiceProvider = 1UL << 33,
        ForwardServiceMember = 1UL << 34,
        MissionRefitState = 1UL << 35,
        MissionRefitJob = 1UL << 36,
        ResonanceCore = 1UL << 37,
        SurgeZone = 1UL << 38,
        SurgeReceiver = 1UL << 39,
        All = Ownership | Transform | Movement | Navigation | Selectable | Vision | ResourceNode | CommandQueue | RouteCorridor | Worker | ResourceCarrier | ResourceReceiver | ResourceBank | Building | ConstructionSite | Builder | Production | EnergyDomain | EnergyDomainMember | PowerState | Targetable | Targeting | Weapon | Health | Destruction | Passenger | Transport | Transformation | WorksiteNode | WorksiteMember | WorksiteComponent | Excavatable | Deployment | ForwardServiceProvider | ForwardServiceMember | MissionRefitState | MissionRefitJob | ResonanceCore | SurgeZone | SurgeReceiver
    }

    public static byte[] Serialize(SimulationWorld world)
    {
        using MemoryStream ms = new(); using BinaryWriter w = new(ms);
        w.Write(Magic); w.Write(FormatVersion); w.Write(SimulationProtocolVersion); w.Write(world.Tick.Value);
        world.Map.Serialize(w);
        w.Write(world.Entities.NextEntityValue);
        IReadOnlyList<EntityId> alive = world.Entities.Alive; w.Write(alive.Count);
        for (int i = 0; i < alive.Count; i++) WriteEntity(w, world, alive[i]);
        w.Write(world.PlayerCount);
        for (byte player = 0; player < world.PlayerCount; player++)
        {
            AlienChargeState charge = world.GetAlienCharge(player);
            w.Write(charge.CurrentMillicharge); w.Write(charge.ResonanceInitiationUnlocked);
        }
        WriteTubeGraph(w, world);
        WriteTubeTransfers(w, world);
        WriteStability(w, world);
        WriteResearch(w, world);
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
        bool supportedLegacy = ((format == 2 || format == 3) && protocol == 1) || (format == 4 && protocol == 2) || (format == 5 && protocol == 3) || (format == 6 && protocol == 4) || (format == 7 && protocol == 5) || (format == 8 && protocol == 6) || (format == 9 && protocol == 7) || (format == 10 && protocol == 8) || (format == 11 && protocol == 9) || (format == 12 && protocol == 10) || (format == 13 && protocol == 11) || (format == 14 && protocol == 12) || (format == 15 && protocol == 13) || (format == 16 && protocol == 14) || (format == 17 && protocol == 15) || (format == 18 && protocol == 16) || (format == 19 && protocol == 17) || (format == 20 && protocol == 18);
        if (!supportedLegacy && (format != FormatVersion || protocol != SimulationProtocolVersion)) throw new InvalidDataException($"Unsupported snapshot {format}/{protocol}.");
        SimTick tick = new(r.ReadInt32()); MapGrid map = MapGrid.Deserialize(r, includeExcavatableMetadata: format >= 20); uint nextEntity = r.ReadUInt32();
        EntityStore entities = new(); int entityCount = r.ReadInt32(); if (entityCount < 0 || entityCount > 10000) throw new InvalidDataException("Invalid entity count.");
        SimulationWorld temp = new(map, entities, new FogState(2), tick, content);
        for (int i = 0; i < entityCount; i++)
        {
            if (format == 2) ReadEntityV2(r, temp);
            else ReadEntity(r, temp, format);
        }
        NormalizeDestroyedCollision(temp);
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
        if (format < 18) AddLegacyTransportComponents(temp);
        ValidateTransportLinks(temp);
        if (format < 19) AddLegacyTransformationComponents(temp);
        ValidateTransformationLinks(temp);
        entities.RestoreNextEntityValue(nextEntity);
        if (format >= 20)
        {
            int chargePlayers = r.ReadInt32();
            if (chargePlayers != temp.PlayerCount) throw new InvalidDataException("Alien Charge player count mismatch.");
            for (byte player = 0; player < chargePlayers; player++)
            {
                int currentMillicharge = r.ReadInt32();
                if (currentMillicharge < 0) throw new InvalidDataException("Invalid Alien Charge amount.");
                temp.SetAlienCharge(player, new AlienChargeState { CurrentMillicharge = currentMillicharge, ResonanceInitiationUnlocked = r.ReadBoolean() });
            }
        }
        if (format >= 20) ReadTubeGraph(r, temp, format);
        if (format >= 20) ReadTubeTransfers(r, temp);
        if (format >= 20) ReadStability(r, temp);
        if (format >= 21) ReadResearch(r, temp);
        temp.Commands.Deserialize(r, includeBuildFields: format >= 6, includeEnergyPriority: format >= 10, includeMissionRefit: format >= 20, includeResonanceCommitment: format >= 20);
        temp.Fog = FogState.Deserialize(r);
        if (format >= 13) ReadProjectileState(r, temp);
        if (ms.Position != ms.Length) throw new InvalidDataException("Trailing snapshot bytes.");
        ExcavationTopologySystem.InitializeFeatures(temp);
        if (format < 9) InitializeLegacyEnergy(temp);
        else if (format < 11) MigrateLegacyWorksites(temp);
        else EnergyDomainSystem.RecalculateAll(temp);
        AlienChargeSystem.RecalculateAll(temp);
        OperationsCapacitySystem.Recalculate(temp);
        temp.Spatial.Rebuild(temp.Entities);
        return temp;
    }

    private static void WriteResearch(BinaryWriter w, SimulationWorld world)
    {
        List<ulong> completed = new(world.CompletedResearch); completed.Sort();
        w.Write(completed.Count); for (int i = 0; i < completed.Count; i++) w.Write(completed[i]);
        List<uint> providers = new(world.ResearchJobs.Keys); providers.Sort();
        w.Write(providers.Count);
        for (int i = 0; i < providers.Count; i++)
        {
            ResearchJob job = world.ResearchJobs[providers[i]];
            w.Write(providers[i]); w.Write(job.ResearchType.Value); w.Write(job.FundingBank.Value); w.Write(job.CrystalFundingBank.Value); w.Write(job.EnergyDomainRoot.Value);
            w.Write(job.OreCost); w.Write(job.EnergyCost); w.Write(job.CrystalCost); w.Write(job.TotalTicks); w.Write(job.RemainingTicks);
        }
        List<uint> excavators = new(world.ExcavationJobs.Keys); excavators.Sort();
        w.Write(excavators.Count);
        for (int i = 0; i < excavators.Count; i++)
        {
            ExcavationJob job = world.ExcavationJobs[excavators[i]];
            w.Write(excavators[i]); w.Write(job.Feature.Value); w.Write(job.TotalTicks); w.Write(job.RemainingTicks);
        }
        List<uint> defenseNodes = new(world.DefenseNodeStates.Keys); defenseNodes.Sort();
        w.Write(defenseNodes.Count);
        for (int i = 0; i < defenseNodes.Count; i++)
        {
            DefenseNodeState state = world.DefenseNodeStates[defenseNodes[i]];
            w.Write(defenseNodes[i]); w.Write((byte)state.CurrentMode); w.Write((byte)state.TargetMode); w.Write(state.RemainingTicks);
            w.Write(state.LastHostileCombatTick); w.Write(state.IsReconfiguring); w.Write(state.ShuntRemainingTicks);
        }
    }

    private static void ReadResearch(BinaryReader r, SimulationWorld world)
    {
        int completedCount = r.ReadInt32(); if (completedCount < 0 || completedCount > 10000) throw new InvalidDataException("Invalid completed research count.");
        for (int i = 0; i < completedCount; i++) if (!world.CompletedResearch.Add(r.ReadUInt64())) throw new InvalidDataException("Duplicate completed research.");
        int jobCount = r.ReadInt32(); if (jobCount < 0 || jobCount > 10000) throw new InvalidDataException("Invalid research job count.");
        for (int i = 0; i < jobCount; i++)
        {
            uint provider = r.ReadUInt32(); ResearchJob job = new()
            {
                ResearchType = new ContentId(r.ReadUInt32()), FundingBank = new EntityId(r.ReadUInt32()), CrystalFundingBank = new EntityId(r.ReadUInt32()), EnergyDomainRoot = new EntityId(r.ReadUInt32()),
                OreCost = r.ReadUInt16(), EnergyCost = r.ReadUInt16(), CrystalCost = r.ReadByte(), TotalTicks = r.ReadUInt16(), RemainingTicks = r.ReadUInt16()
            };
            if (!world.Entities.Exists(new EntityId(provider)) || job.ResearchType.Value == 0 || job.TotalTicks == 0 || job.RemainingTicks > job.TotalTicks ||
                !world.ResearchJobs.TryAdd(provider, job)) throw new InvalidDataException("Invalid research job.");
        }
        int excavationCount = r.ReadInt32(); if (excavationCount < 0 || excavationCount > 10000) throw new InvalidDataException("Invalid excavation job count.");
        for (int i = 0; i < excavationCount; i++)
        {
            uint excavator = r.ReadUInt32(); ExcavationJob job = new() { Feature = new EntityId(r.ReadUInt32()), TotalTicks = r.ReadUInt16(), RemainingTicks = r.ReadUInt16() };
            if (!world.Entities.Exists(new EntityId(excavator)) || !world.Entities.Excavatable.Has(job.Feature) || job.TotalTicks == 0 || job.RemainingTicks > job.TotalTicks ||
                !world.ExcavationJobs.TryAdd(excavator, job)) throw new InvalidDataException("Invalid excavation job.");
        }
        int defenseCount = r.ReadInt32(); if (defenseCount < 0 || defenseCount > 10000) throw new InvalidDataException("Invalid Defense Node state count.");
        for (int i = 0; i < defenseCount; i++)
        {
            uint node = r.ReadUInt32(); DefenseNodeState state = new()
            {
                CurrentMode = (DefenseNodeMode)r.ReadByte(), TargetMode = (DefenseNodeMode)r.ReadByte(), RemainingTicks = r.ReadUInt16(),
                LastHostileCombatTick = r.ReadInt32(), IsReconfiguring = r.ReadBoolean(), ShuntRemainingTicks = r.ReadUInt16()
            };
            if (!world.Entities.Exists(new EntityId(node)) || state.CurrentMode > DefenseNodeMode.AirLance || state.TargetMode > DefenseNodeMode.AirLance ||
                (state.IsReconfiguring ? state.RemainingTicks == 0 || state.RemainingTicks > DefenseNodeSystem.ReconfigurationTicks : state.RemainingTicks != 0) ||
                !world.DefenseNodeStates.TryAdd(node, state)) throw new InvalidDataException("Invalid Defense Node state.");
        }
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

    private static void WriteTubeGraph(BinaryWriter w, SimulationWorld world)
    {
        w.Write(world.TubeTopologyRevision); w.Write(world.TubeSegmentationRevision);
        List<EntityId> stations = new(), links = new(), components = new();
        IReadOnlyList<EntityId> alive = world.Entities.Alive;
        for (int i = 0; i < alive.Count; i++)
        {
            EntityId id = alive[i];
            if (world.Entities.TubeStation.Has(id)) stations.Add(id);
            if (world.Entities.TubeLink.Has(id)) links.Add(id);
            if (world.Entities.TubeComponent.Has(id)) components.Add(id);
        }
        w.Write(stations.Count);
        for (int i = 0; i < stations.Count; i++)
        {
            EntityId id = stations[i]; TubeStation station = world.Entities.TubeStation.Get(id);
            w.Write(id.Value); w.Write(station.ComponentRoot.Value); w.Write(station.ConnectionLimit); w.Write(station.ConnectionCount); w.Write(station.RedundantRoutingUnlocked); w.Write(station.HypersledThroughputUnlocked);
        }
        w.Write(links.Count);
        for (int i = 0; i < links.Count; i++)
        {
            EntityId id = links[i]; TubeLink link = world.Entities.TubeLink.Get(id);
            if (!world.TubeRoutes.TryGetValue(id.Value, out TubeRoute route)) throw new InvalidDataException("Tube Link has no route.");
            w.Write(id.Value); w.Write(link.EndpointA.Value); w.Write(link.EndpointB.Value); w.Write(link.ComponentRoot.Value);
            w.Write(link.LengthBuildCells); w.Write(link.EnergyDemandPerSecond); w.Write(link.IsOperational);
            w.Write(route.Cells.Count);
            for (int c = 0; c < route.Cells.Count; c++) { w.Write(route.Cells[c].X); w.Write(route.Cells[c].Y); }
        }
        w.Write(components.Count);
        for (int i = 0; i < components.Count; i++)
        {
            EntityId id = components[i]; TubeComponent component = world.Entities.TubeComponent.Get(id);
            w.Write(id.Value); w.Write(component.StationCount); w.Write(component.OperationalLinkCount); w.Write(component.TopologyRevision);
        }
    }

    private static void ReadTubeGraph(BinaryReader r, SimulationWorld world, ushort format)
    {
        world.TubeTopologyRevision = r.ReadUInt32(); world.TubeSegmentationRevision = r.ReadUInt32();
        int stationCount = r.ReadInt32();
        if (stationCount < 0 || stationCount > 1000) throw new InvalidDataException("Invalid Tube Station count.");
        for (int i = 0; i < stationCount; i++)
        {
            EntityId id = new(r.ReadUInt32()), root = new(r.ReadUInt32());
            TubeStation station = new() { ComponentRoot = root, ConnectionLimit = r.ReadByte(), ConnectionCount = r.ReadByte(), RedundantRoutingUnlocked = r.ReadBoolean(), HypersledThroughputUnlocked = format >= 18 && r.ReadBoolean() };
            if (!world.Entities.Exists(id) || world.Entities.TubeStation.Has(id) || root == EntityId.None || station.ConnectionCount > station.ConnectionLimit)
                throw new InvalidDataException("Invalid Tube Station state.");
            world.Entities.TubeStation.Set(id, station);
        }
        int linkCount = r.ReadInt32();
        if (linkCount < 0 || linkCount > 4000) throw new InvalidDataException("Invalid Tube Link count.");
        for (int i = 0; i < linkCount; i++)
        {
            EntityId id = new(r.ReadUInt32());
            TubeLink link = new()
            {
                EndpointA = new EntityId(r.ReadUInt32()), EndpointB = new EntityId(r.ReadUInt32()), ComponentRoot = new EntityId(r.ReadUInt32()),
                LengthBuildCells = r.ReadUInt16(), EnergyDemandPerSecond = r.ReadByte(), IsOperational = r.ReadBoolean()
            };
            int routeCount = r.ReadInt32();
            if (!world.Entities.Exists(id) || world.Entities.TubeLink.Has(id) || routeCount <= 0 || routeCount > ushort.MaxValue || routeCount != link.LengthBuildCells)
                throw new InvalidDataException("Invalid Tube Link state.");
            TubeRoute route = new();
            for (int c = 0; c < routeCount; c++) route.Cells.Add(new TubeBuildCell(r.ReadInt16(), r.ReadInt16()));
            if (!TubeGraphSystem.RouteIsStructurallyValid(route)) throw new InvalidDataException("Invalid Tube route.");
            world.Entities.TubeLink.Set(id, link); world.TubeRoutes.Add(id.Value, route);
        }
        int componentCount = r.ReadInt32();
        if (componentCount < 0 || componentCount > stationCount) throw new InvalidDataException("Invalid Tube component count.");
        for (int i = 0; i < componentCount; i++)
        {
            EntityId id = new(r.ReadUInt32());
            TubeComponent component = new() { StationCount = r.ReadUInt16(), OperationalLinkCount = r.ReadUInt16(), TopologyRevision = r.ReadUInt32() };
            if (!world.Entities.Exists(id) || world.Entities.TubeComponent.Has(id) || component.StationCount == 0 || component.TopologyRevision != world.TubeTopologyRevision)
                throw new InvalidDataException("Invalid Tube component state.");
            world.Entities.TubeComponent.Set(id, component);
        }
        if (TubeGraphSystem.Rebuild(world)) throw new InvalidDataException("Inconsistent cached Tube topology.");
    }

    private static void WriteTubeTransfers(BinaryWriter w, SimulationWorld world)
    {
        List<EntityId> passengers = new(); IReadOnlyList<EntityId> alive = world.Entities.Alive;
        for (int i = 0; i < alive.Count; i++) if (world.Entities.TubeTransfer.Has(alive[i])) passengers.Add(alive[i]);
        w.Write(passengers.Count);
        for (int i = 0; i < passengers.Count; i++)
        {
            EntityId id = passengers[i]; TubeTransfer transfer = world.Entities.TubeTransfer.Get(id);
            if (!world.TubeTransitRoutes.TryGetValue(id.Value, out TubeTransitRoute route) || route.Links.Count == 0) throw new InvalidDataException("Tube transfer has no selected route.");
            w.Write(id.Value); w.Write(transfer.Origin.Value); w.Write(transfer.Destination.Value); w.Write(transfer.RequestedTick); w.Write(transfer.DepartureTick);
            w.Write(transfer.TotalTravelTicks); w.Write(transfer.RemainingTicks); w.Write(transfer.CurrentEdgeIndex); w.Write(transfer.ExitWaitTicks);
            w.Write((byte)transfer.State); w.Write(transfer.HasArrivalMoveOrder); w.Write(transfer.ArrivalMoveTarget.X.Raw); w.Write(transfer.ArrivalMoveTarget.Y.Raw);
            w.Write(route.Links.Count); for (int l = 0; l < route.Links.Count; l++) w.Write(route.Links[l].Value);
        }
    }

    private static void ReadTubeTransfers(BinaryReader r, SimulationWorld world)
    {
        int count = r.ReadInt32(); if (count < 0 || count > 10000) throw new InvalidDataException("Invalid Tube transfer count.");
        for (int i = 0; i < count; i++)
        {
            EntityId id = new(r.ReadUInt32()); TubeTransfer transfer = new()
            {
                Origin = new EntityId(r.ReadUInt32()), Destination = new EntityId(r.ReadUInt32()), RequestedTick = r.ReadInt32(), DepartureTick = r.ReadInt32(),
                TotalTravelTicks = r.ReadInt32(), RemainingTicks = r.ReadInt32(), CurrentEdgeIndex = r.ReadUInt16(), ExitWaitTicks = r.ReadUInt16(),
                State = (TubeTransferState)r.ReadByte(), HasArrivalMoveOrder = r.ReadBoolean(),
                ArrivalMoveTarget = new FixVec2(Fix32.FromRaw(r.ReadInt32()), Fix32.FromRaw(r.ReadInt32()))
            };
            int linkCount = r.ReadInt32();
            if (!world.Entities.Exists(id) || world.Entities.TubeTransfer.Has(id) || !world.Entities.TubeStation.Has(transfer.Origin) || !world.Entities.TubeStation.Has(transfer.Destination) ||
                (byte)transfer.State > (byte)TubeTransferState.ArrivalRecovery || transfer.RemainingTicks < 0 || transfer.TotalTravelTicks < 0 || linkCount <= 0 || linkCount > 1000)
                throw new InvalidDataException("Invalid Tube transfer state.");
            TubeTransitRoute route = new(); for (int l = 0; l < linkCount; l++) route.Links.Add(new EntityId(r.ReadUInt32()));
            if (transfer.CurrentEdgeIndex >= route.Links.Count) throw new InvalidDataException("Invalid Tube transfer edge.");
            bool shouldHaveTransform = transfer.State == TubeTransferState.Approaching || transfer.State == TubeTransferState.Queued || transfer.State == TubeTransferState.Loading || transfer.State == TubeTransferState.ArrivalRecovery;
            if (world.Entities.Transform.Has(id) != shouldHaveTransform) throw new InvalidDataException("Invalid Tube passenger spatial state.");
            world.Entities.TubeTransfer.Set(id, transfer); world.TubeTransitRoutes.Add(id.Value, route);
        }
    }

    private static void WriteStability(BinaryWriter w, SimulationWorld world)
    {
        List<EntityId> stable = new(); IReadOnlyList<EntityId> alive = world.Entities.Alive;
        for (int i = 0; i < alive.Count; i++) if (world.Entities.Stability.Has(alive[i])) stable.Add(alive[i]);
        w.Write(stable.Count);
        for (int i = 0; i < stable.Count; i++) { w.Write(stable[i].Value); w.Write(world.Entities.Stability.Get(stable[i]).UntilTick); }
    }

    private static void ReadStability(BinaryReader r, SimulationWorld world)
    {
        int count = r.ReadInt32(); if (count < 0 || count > 10000) throw new InvalidDataException("Invalid Stability count.");
        for (int i = 0; i < count; i++)
        {
            EntityId id = new(r.ReadUInt32()); int untilTick = r.ReadInt32();
            if (!world.Entities.Exists(id) || world.Entities.Stability.Has(id) || untilTick < 0) throw new InvalidDataException("Invalid Stability state.");
            world.Entities.Stability.Set(id, new Stability { UntilTick = untilTick });
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
        if (world.Entities.Passenger.Has(id)) components |= EntityComponents.Passenger;
        if (world.Entities.Transport.Has(id)) components |= EntityComponents.Transport;
        if (world.Entities.Transformation.Has(id)) components |= EntityComponents.Transformation;
        if (world.Entities.ResourceCarrier.Has(id)) components |= EntityComponents.ResourceCarrier;
        if (world.Entities.ResourceReceiver.Has(id)) components |= EntityComponents.ResourceReceiver;
        if (world.Entities.ResourceBank.Has(id)) components |= EntityComponents.ResourceBank;
        if (world.Entities.Building.Has(id)) components |= EntityComponents.Building;
        if (world.Entities.EnergyDomain.Has(id)) components |= EntityComponents.EnergyDomain;
        if (world.Entities.EnergyDomainMember.Has(id)) components |= EntityComponents.EnergyDomainMember;
        if (world.Entities.PowerState.Has(id)) components |= EntityComponents.PowerState;
        if (world.Entities.WorksiteNode.Has(id)) components |= EntityComponents.WorksiteNode;
        if (world.Entities.WorksiteMember.Has(id)) components |= EntityComponents.WorksiteMember;
        if (world.Entities.WorksiteComponent.Has(id)) components |= EntityComponents.WorksiteComponent;
        if (world.Entities.Excavatable.Has(id)) components |= EntityComponents.Excavatable;
        if (world.Entities.Deployment.Has(id)) components |= EntityComponents.Deployment;
        if (world.Entities.ForwardServiceProvider.Has(id)) components |= EntityComponents.ForwardServiceProvider;
        if (world.Entities.ForwardServiceMember.Has(id)) components |= EntityComponents.ForwardServiceMember;
        if (world.Entities.MissionRefitState.Has(id)) components |= EntityComponents.MissionRefitState;
        if (world.Entities.MissionRefitJob.Has(id)) components |= EntityComponents.MissionRefitJob;
        if (world.Entities.ResonanceCore.Has(id)) components |= EntityComponents.ResonanceCore;
        if (world.Entities.SurgeZone.Has(id)) components |= EntityComponents.SurgeZone;
        if (world.Entities.SurgeReceiver.Has(id)) components |= EntityComponents.SurgeReceiver;
        if (world.Entities.ConstructionSite.Has(id)) components |= EntityComponents.ConstructionSite;
        if (world.Entities.Production.Has(id)) components |= EntityComponents.Production;
        if (world.Entities.Targetable.Has(id)) components |= EntityComponents.Targetable;
        if (world.Entities.Targeting.Has(id)) components |= EntityComponents.Targeting;
        if (world.Entities.Weapon.Has(id)) components |= EntityComponents.Weapon;
        if (world.Entities.Health.Has(id)) components |= EntityComponents.Health;
        if (world.Entities.Destruction.Has(id)) components |= EntityComponents.Destruction;
        if (world.TryGetQueue(id, out _)) components |= EntityComponents.CommandQueue;
        if (world.Corridors.ContainsKey(id.Value)) components |= EntityComponents.RouteCorridor;
        w.Write((ulong)components);

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
        if ((components & EntityComponents.Passenger) != 0) WritePassenger(w, world.Entities.Passenger.Get(id));
        if ((components & EntityComponents.Transport) != 0) WriteTransport(w, world.Entities.Transport.Get(id));
        if ((components & EntityComponents.Transformation) != 0) WriteTransformation(w, world.Entities.Transformation.Get(id));
        if ((components & EntityComponents.ResourceCarrier) != 0) WriteResourceCarrier(w, world.Entities.ResourceCarrier.Get(id));
        if ((components & EntityComponents.ResourceReceiver) != 0) WriteResourceReceiver(w, world.Entities.ResourceReceiver.Get(id));
        if ((components & EntityComponents.ResourceBank) != 0) WriteResourceBank(w, world.Entities.ResourceBank.Get(id));
        if ((components & EntityComponents.Building) != 0) WriteBuilding(w, world.Entities.Building.Get(id));
        if ((components & EntityComponents.EnergyDomain) != 0) WriteEnergyDomain(w, world.Entities.EnergyDomain.Get(id));
        if ((components & EntityComponents.EnergyDomainMember) != 0) w.Write(world.Entities.EnergyDomainMember.Get(id).DomainRoot.Value);
        if ((components & EntityComponents.PowerState) != 0) { PowerState state = world.Entities.PowerState.Get(id); w.Write((byte)state.Priority); w.Write(state.IsPowered); }
        if ((components & EntityComponents.WorksiteNode) != 0) { WorksiteNode node = world.Entities.WorksiteNode.Get(id); w.Write(node.ServiceRadius); w.Write(node.ComponentRoot.Value); }
        if ((components & EntityComponents.WorksiteMember) != 0) w.Write(world.Entities.WorksiteMember.Get(id).ComponentRoot.Value);
        if ((components & EntityComponents.WorksiteComponent) != 0) { WorksiteComponent component = world.Entities.WorksiteComponent.Get(id); w.Write(component.NodeCount); w.Write(component.MemberCount); w.Write(component.TopologyRevision); }
        if ((components & EntityComponents.Excavatable) != 0)
        {
            Excavatable feature = world.Entities.Excavatable.Get(id);
            w.Write(feature.MapFeatureId); w.Write(feature.StableId.Value); w.Write((byte)feature.TerrainClass);
            w.Write((byte)feature.State); w.Write(feature.RequiredEnergy); w.Write(feature.VisualProfile.Value);
        }
        if ((components & EntityComponents.Deployment) != 0) w.Write((byte)world.Entities.Deployment.Get(id).State);
        if ((components & EntityComponents.ForwardServiceProvider) != 0)
        {
            ForwardServiceProvider provider = world.Entities.ForwardServiceProvider.Get(id);
            w.Write(provider.RadiusBuildCells); w.Write(provider.IsActive);
        }
        if ((components & EntityComponents.ForwardServiceMember) != 0)
        {
            ForwardServiceMember member = world.Entities.ForwardServiceMember.Get(id);
            w.Write(member.Provider.Value); w.Write(member.QueryOwner); w.Write(member.QueryCellX); w.Write(member.QueryCellY);
        }
        if ((components & EntityComponents.MissionRefitState) != 0)
        {
            MissionRefitState state = world.Entities.MissionRefitState.Get(id);
            w.Write((byte)state.CurrentConfiguration); w.Write(state.OwnedConfigurationMask); w.Write(state.ConfigurationLockTicks); w.Write(state.SurveyUnlocked);
        }
        if ((components & EntityComponents.MissionRefitJob) != 0)
        {
            MissionRefitJob job = world.Entities.MissionRefitJob.Get(id);
            w.Write(job.Provider.Value); w.Write(job.FundingBank.Value); w.Write(job.EnergyDomainRoot.Value);
            w.Write((byte)job.OldConfiguration); w.Write((byte)job.NewConfiguration); w.Write(job.TotalTicks); w.Write(job.RemainingTicks);
            w.Write(job.CommittedOre); w.Write(job.CommittedEnergy);
        }
        if ((components & EntityComponents.ResonanceCore) != 0)
        {
            ResonanceCore core = world.Entities.ResonanceCore.Get(id);
            w.Write(core.CommandCore.Value); w.Write(core.TransitionBank.Value); w.Write(core.CommittedSlotMask); w.Write(core.DesiredCommittedCrystals);
            w.Write(core.TransitionSlot); w.Write((byte)core.TransitionKind); w.Write(core.TransitionTotalTicks); w.Write(core.TransitionRemainingTicks); w.Write(core.ExpandedLatticeUnlocked);
        }
        if ((components & EntityComponents.SurgeZone) != 0)
        {
            SurgeZone zone = world.Entities.SurgeZone.Get(id);
            w.Write(zone.RadiusBuildCells); w.Write(zone.BuildupRemainingTicks); w.Write(zone.ActiveRemainingTicks);
        }
        if ((components & EntityComponents.SurgeReceiver) != 0) w.Write(world.Entities.SurgeReceiver.Get(id).ActiveZone.Value);
        if ((components & EntityComponents.ConstructionSite) != 0) WriteConstructionSite(w, world.Entities.ConstructionSite.Get(id));
        if ((components & EntityComponents.Production) != 0) WriteProduction(w, world.Entities.Production.Get(id));
        if ((components & EntityComponents.Targetable) != 0) WriteTargetable(w, world.Entities.Targetable.Get(id));
        if ((components & EntityComponents.Targeting) != 0) WriteTargeting(w, world.Entities.Targeting.Get(id));
        if ((components & EntityComponents.Weapon) != 0) WriteWeapon(w, world.Entities.Weapon.Get(id));
        if ((components & EntityComponents.Health) != 0) WriteHealth(w, world.Entities.Health.Get(id));
        if ((components & EntityComponents.Destruction) != 0) WriteDestruction(w, world.Entities.Destruction.Get(id));
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
        w.Write(builder.RepairTarget.Value); w.Write(builder.RepairOreRemainder.Raw);
    }

    private static void WritePassenger(BinaryWriter w, Passenger passenger)
    {
        w.Write(passenger.Transport.Value); w.Write((byte)passenger.State); w.Write(passenger.SizePoints);
        w.Write(passenger.AttackLockedUntilTick); w.Write(passenger.MovementPenaltyUntilTick);
    }

    private static void WriteTransport(BinaryWriter w, Transport transport)
    {
        w.Write(transport.CapacityPoints); w.Write(transport.OccupiedPoints); w.Write(transport.PassengerCount);
        w.Write((byte)transport.JobState); w.Write(transport.ActivePassenger.Value); w.Write(transport.PhaseTicks);
        w.Write(transport.UnloadTarget.X.Raw); w.Write(transport.UnloadTarget.Y.Raw); w.Write(transport.UnloadBlocked); w.Write(transport.LoadingSettled);
        for (int i = 0; i < Transport.MaximumPassengerSlots; i++) w.Write(transport.GetPassenger(i).Value);
    }

    private static void WriteTransformation(BinaryWriter w, Transformation transformation)
    {
        w.Write(transformation.Definition.Value); w.Write(transformation.CurrentState.Value); w.Write(transformation.SourceState.Value); w.Write(transformation.DestinationState.Value);
        w.Write((byte)transformation.Phase); w.Write(transformation.ProgressTicks); w.Write(transformation.TotalTicks); w.Write(transformation.RollbackTicksRemaining);
        w.Write(transformation.ReversalLockedUntilTick); w.Write(transformation.QueuedToggle);
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
        w.Write(site.AssignedBuilder.Value); w.Write(site.FundingBank.Value); w.Write(site.CrystalFundingBank.Value);
        w.Write(site.ReservedOre); w.Write(site.ConsumedOre); w.Write(site.RequiredCrystals); w.Write(site.RequiredEnergy);
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
            w.Write(item.UnitType.Value); w.Write(item.FundingBank.Value); w.Write(item.CrystalFundingBank.Value); w.Write(item.EnergyDomainRoot.Value); w.Write(item.ReservedOre); w.Write(item.RequiredEnergy);
            w.Write(item.RequiredCrystals); w.Write(item.ReservedOperationsCapacity); w.Write(item.TotalTicks); w.Write(item.RemainingTicks); w.Write(item.RapidFabricationUsed);
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
        w.Write(targeting.PursuitOrigin.X.Raw); w.Write(targeting.PursuitOrigin.Y.Raw); w.Write(targeting.ApproachSlotIndex);
        w.Write(targeting.HasPursuitOrigin); w.Write(targeting.HasApproachSlot); w.Write(targeting.HasCombatMove);
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

    private static void WriteDestruction(BinaryWriter w, DestructionState destruction)
    {
        w.Write((byte)destruction.Kind); w.Write(destruction.StartedTick); w.Write(destruction.BlockingUntilTick); w.Write(destruction.VisualUntilTick);
        w.Write(destruction.Owner); w.Write(destruction.ContentType.Value); w.Write((byte)destruction.SelectableKind); w.Write((byte)destruction.Footprint);
        w.Write(destruction.BuildingType.Value); w.Write(destruction.BuildingAnchorX); w.Write(destruction.BuildingAnchorY); w.Write(destruction.BuildingOrientation);
        w.Write(destruction.BuildingWidth); w.Write(destruction.BuildingHeight);
    }

    private static void WriteCorridor(BinaryWriter w, RouteCorridor path)
    {
        w.Write(path.TopologyVersion); w.Write(path.Cells.Count);
        for (int p = 0; p < path.Cells.Count; p++) { w.Write(path.Cells[p].X); w.Write(path.Cells[p].Y); }
    }

    private static void ReadEntity(BinaryReader r, SimulationWorld world, ushort format)
    {
        EntityId id = world.Entities.CreateRestored(r.ReadUInt32());
        EntityComponents components = format >= 20 ? (EntityComponents)r.ReadUInt64() : (EntityComponents)(format >= 8 ? r.ReadUInt32() : r.ReadUInt16());
        if ((components & ~EntityComponents.All) != 0) throw new InvalidDataException("Snapshot entity has unknown component bits.");
        if ((components & EntityComponents.Ownership) != 0) world.Entities.Ownership.Set(id, new Ownership { PlayerSlot = r.ReadByte() });
        if ((components & EntityComponents.Transform) != 0) world.Entities.Transform.Set(id, ReadTransform(r));
        if ((components & EntityComponents.Movement) != 0) world.Entities.Movement.Set(id, ReadMovement(r));
        if ((components & EntityComponents.Navigation) != 0) world.Entities.Navigation.Set(id, ReadNavigation(r));
        if ((components & EntityComponents.Selectable) != 0) world.Entities.Selectable.Set(id, new Selectable { IsSelectable = r.ReadBoolean(), ContentType = new ContentId(r.ReadUInt32()), Kind=(SelectableKind)r.ReadByte() });
        if ((components & EntityComponents.Vision) != 0) world.Entities.Vision.Set(id, new Vision { RadiusBuildCells = r.ReadByte(), IsAirVision = r.ReadBoolean(), LastFogX = r.ReadInt32(), LastFogY = r.ReadInt32() });
        if ((components & EntityComponents.ResourceNode) != 0) world.Entities.ResourceNode.Set(id, ReadResourceNode(r));
        if ((components & EntityComponents.Worker) != 0) world.Entities.Worker.Set(id, ReadWorker(r));
        if ((components & EntityComponents.Builder) != 0) world.Entities.Builder.Set(id, ReadBuilder(r, format));
        if ((components & EntityComponents.Passenger) != 0) world.Entities.Passenger.Set(id, ReadPassenger(r));
        if ((components & EntityComponents.Transport) != 0) world.Entities.Transport.Set(id, ReadTransport(r));
        if ((components & EntityComponents.Transformation) != 0) world.Entities.Transformation.Set(id, ReadTransformation(r));
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
        if ((components & EntityComponents.WorksiteNode) != 0)
            world.Entities.WorksiteNode.Set(id, new WorksiteNode { ServiceRadius = r.ReadByte(), ComponentRoot = new EntityId(r.ReadUInt32()) });
        if ((components & EntityComponents.WorksiteMember) != 0)
            world.Entities.WorksiteMember.Set(id, new WorksiteMember { ComponentRoot = new EntityId(r.ReadUInt32()) });
        if ((components & EntityComponents.WorksiteComponent) != 0)
            world.Entities.WorksiteComponent.Set(id, new WorksiteComponent { NodeCount = r.ReadUInt16(), MemberCount = r.ReadUInt16(), TopologyRevision = r.ReadUInt32() });
        if ((components & EntityComponents.Excavatable) != 0)
        {
            Excavatable feature = new()
            {
                MapFeatureId = r.ReadUInt16(), StableId = new ContentId(r.ReadUInt32()), TerrainClass = (ExcavatableTerrainClass)r.ReadByte(),
                State = (ExcavatableFeatureState)r.ReadByte(), RequiredEnergy = r.ReadUInt16(), VisualProfile = new ContentId(r.ReadUInt32())
            };
            if (feature.MapFeatureId == 0 || feature.StableId.Value == 0 || feature.TerrainClass < ExcavatableTerrainClass.LooseRubbleBlockage ||
                feature.TerrainClass > ExcavatableTerrainClass.ReinforcedBedrockBarrier || feature.State < ExcavatableFeatureState.Blocked ||
                feature.State > ExcavatableFeatureState.Open || feature.VisualProfile.Value == 0)
                throw new InvalidDataException("Invalid authoritative Excavatable Feature.");
            world.Entities.Excavatable.Set(id, feature);
        }
        if ((components & EntityComponents.Deployment) != 0)
        {
            DeploymentState state = (DeploymentState)r.ReadByte();
            if (state < DeploymentState.Mobile || state > DeploymentState.Undeploying) throw new InvalidDataException("Invalid deployment state.");
            world.Entities.Deployment.Set(id, new Deployment { State = state });
        }
        if ((components & EntityComponents.ForwardServiceProvider) != 0)
        {
            byte radius = r.ReadByte(); bool active = r.ReadBoolean();
            if (radius == 0) throw new InvalidDataException("Invalid Forward Service provider radius.");
            world.Entities.ForwardServiceProvider.Set(id, new ForwardServiceProvider { RadiusBuildCells = radius, IsActive = active });
        }
        if ((components & EntityComponents.ForwardServiceMember) != 0)
            world.Entities.ForwardServiceMember.Set(id, new ForwardServiceMember { Provider = new EntityId(r.ReadUInt32()), QueryOwner = r.ReadByte(), QueryCellX = r.ReadInt16(), QueryCellY = r.ReadInt16() });
        if ((components & EntityComponents.MissionRefitState) != 0)
        {
            MissionRefitState state = new() { CurrentConfiguration = (MissionConfiguration)r.ReadByte(), OwnedConfigurationMask = r.ReadByte(), ConfigurationLockTicks = r.ReadUInt16(), SurveyUnlocked = r.ReadBoolean() };
            if (state.CurrentConfiguration < MissionConfiguration.T3Escort || state.CurrentConfiguration > MissionConfiguration.T3Survey || (state.OwnedConfigurationMask & 1) == 0)
                throw new InvalidDataException("Invalid Mission Refit state.");
            world.Entities.MissionRefitState.Set(id, state);
        }
        if ((components & EntityComponents.MissionRefitJob) != 0)
        {
            MissionRefitJob job = new()
            {
                Provider = new EntityId(r.ReadUInt32()), FundingBank = new EntityId(r.ReadUInt32()), EnergyDomainRoot = new EntityId(r.ReadUInt32()),
                OldConfiguration = (MissionConfiguration)r.ReadByte(), NewConfiguration = (MissionConfiguration)r.ReadByte(),
                TotalTicks = r.ReadUInt16(), RemainingTicks = r.ReadUInt16(), CommittedOre = r.ReadUInt16(), CommittedEnergy = r.ReadUInt16()
            };
            if (job.Provider == EntityId.None || job.FundingBank == EntityId.None || job.EnergyDomainRoot == EntityId.None || job.TotalTicks == 0 || job.RemainingTicks == 0 || job.RemainingTicks > job.TotalTicks ||
                job.OldConfiguration < MissionConfiguration.T3Escort || job.OldConfiguration > MissionConfiguration.T3Survey || job.NewConfiguration < MissionConfiguration.T3Escort || job.NewConfiguration > MissionConfiguration.T3Survey || job.OldConfiguration == job.NewConfiguration)
                throw new InvalidDataException("Invalid Mission Refit job.");
            world.Entities.MissionRefitJob.Set(id, job);
        }
        if ((components & EntityComponents.ResonanceCore) != 0)
        {
            ResonanceCore core = new()
            {
                CommandCore = new EntityId(r.ReadUInt32()), TransitionBank = new EntityId(r.ReadUInt32()), CommittedSlotMask = r.ReadByte(), DesiredCommittedCrystals = r.ReadByte(),
                TransitionSlot = r.ReadByte(), TransitionKind = (ResonanceTransitionKind)r.ReadByte(), TransitionTotalTicks = r.ReadUInt16(), TransitionRemainingTicks = r.ReadUInt16(), ExpandedLatticeUnlocked = r.ReadBoolean()
            };
            byte maximum = ResonanceCoreSystem.MaximumSlots(core);
            bool idle = core.TransitionKind == ResonanceTransitionKind.None && core.TransitionBank == EntityId.None && core.TransitionTotalTicks == 0 && core.TransitionRemainingTicks == 0;
            bool transitioning = (core.TransitionKind == ResonanceTransitionKind.Commit || core.TransitionKind == ResonanceTransitionKind.Withdraw) && core.TransitionBank != EntityId.None &&
                core.TransitionSlot < maximum && (core.CommittedSlotMask & (1 << core.TransitionSlot)) == 0 &&
                core.TransitionTotalTicks == (core.TransitionKind == ResonanceTransitionKind.Commit ? ResonanceCoreSystem.CommitTicks : ResonanceCoreSystem.WithdrawTicks) &&
                core.TransitionRemainingTicks > 0 && core.TransitionRemainingTicks <= core.TransitionTotalTicks;
            int legalSlotMask = (1 << maximum) - 1;
            if ((core.CommittedSlotMask & ~legalSlotMask) != 0 || ResonanceCoreSystem.CountCommitted(core) > maximum || core.DesiredCommittedCrystals > maximum || (!idle && !transitioning))
                throw new InvalidDataException("Invalid Resonance Core state.");
            world.Entities.ResonanceCore.Set(id, core);
        }
        if ((components & EntityComponents.SurgeZone) != 0)
        {
            SurgeZone zone = new() { RadiusBuildCells = r.ReadByte(), BuildupRemainingTicks = r.ReadUInt16(), ActiveRemainingTicks = r.ReadUInt16() };
            bool building = zone.BuildupRemainingTicks > 0 && zone.BuildupRemainingTicks <= AlienChargeSystem.SurgeBuildupTicks && zone.ActiveRemainingTicks == 0;
            bool active = zone.BuildupRemainingTicks == 0 && zone.ActiveRemainingTicks > 0 && zone.ActiveRemainingTicks <= AlienChargeSystem.SurgeActiveTicks;
            if ((zone.RadiusBuildCells != AlienChargeSystem.ResonanceCoreSurgeRadius && zone.RadiusBuildCells != AlienChargeSystem.MothershipRelaySurgeRadius) || (!building && !active))
                throw new InvalidDataException("Invalid Surge zone state.");
            world.Entities.SurgeZone.Set(id, zone);
        }
        if ((components & EntityComponents.SurgeReceiver) != 0) world.Entities.SurgeReceiver.Set(id, new SurgeReceiver { ActiveZone = new EntityId(r.ReadUInt32()) });
        if ((components & EntityComponents.ConstructionSite) != 0) world.Entities.ConstructionSite.Set(id, ReadConstructionSite(r, format));
        if ((components & EntityComponents.Production) != 0) world.Entities.Production.Set(id, ReadProduction(r, format));
        if ((components & EntityComponents.Targetable) != 0) world.Entities.Targetable.Set(id, ReadTargetable(r));
        if ((components & EntityComponents.Targeting) != 0) world.Entities.Targeting.Set(id, ReadTargeting(r, format));
        if ((components & EntityComponents.Weapon) != 0)
        {
            WeaponState weapon = ReadWeapon(r);
            if (!world.Content.TryGetWeapon(weapon.WeaponProfile, out WeaponDefinition definition) || weapon.CooldownRemainingTicks > definition.CooldownTicks)
                throw new InvalidDataException("Invalid weapon state.");
            world.Entities.Weapon.Set(id, weapon);
        }
        if ((components & EntityComponents.Health) != 0) world.Entities.Health.Set(id, ReadHealth(r, world.Tick.Value));
        if ((components & EntityComponents.Destruction) != 0) world.Entities.Destruction.Set(id, ReadDestruction(r, world));
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

    private static Builder ReadBuilder(BinaryReader r, ushort format)
    {
        Builder builder = new() { ConstructionTarget = new EntityId(r.ReadUInt32()), JobState = (BuilderJobState)r.ReadByte() };
        if (format >= 17)
        {
            builder.RepairTarget = new EntityId(r.ReadUInt32());
            builder.RepairOreRemainder = Fix32.FromRaw(r.ReadInt32());
        }
        if (builder.JobState < BuilderJobState.Idle || builder.JobState > BuilderJobState.Repairing ||
            builder.RepairOreRemainder < Fix32.Zero || builder.RepairOreRemainder >= Fix32.One ||
            ((builder.JobState == BuilderJobState.MovingToRepair || builder.JobState == BuilderJobState.Repairing) && builder.RepairTarget == EntityId.None))
            throw new InvalidDataException("Invalid builder job state.");
        return builder;
    }

    private static Passenger ReadPassenger(BinaryReader r)
    {
        Passenger passenger = new()
        {
            Transport = new EntityId(r.ReadUInt32()), State = (PassengerState)r.ReadByte(), SizePoints = r.ReadByte(),
            AttackLockedUntilTick = r.ReadInt32(), MovementPenaltyUntilTick = r.ReadInt32()
        };
        if (passenger.State < PassengerState.Grounded || passenger.State > PassengerState.Loaded || passenger.SizePoints < 1 || passenger.SizePoints > 6 ||
            passenger.AttackLockedUntilTick < 0 || passenger.MovementPenaltyUntilTick < 0 ||
            (passenger.State == PassengerState.Grounded && passenger.Transport != EntityId.None) ||
            (passenger.State != PassengerState.Grounded && passenger.Transport == EntityId.None)) throw new InvalidDataException("Invalid passenger state.");
        return passenger;
    }

    private static Transport ReadTransport(BinaryReader r)
    {
        Transport transport = new()
        {
            CapacityPoints = r.ReadByte(), OccupiedPoints = r.ReadByte(), PassengerCount = r.ReadByte(), JobState = (TransportJobState)r.ReadByte(),
            ActivePassenger = new EntityId(r.ReadUInt32()), PhaseTicks = r.ReadUInt16(),
            UnloadTarget = new FixVec2(Fix32.FromRaw(r.ReadInt32()), Fix32.FromRaw(r.ReadInt32())),
            UnloadBlocked = r.ReadBoolean(), LoadingSettled = r.ReadBoolean()
        };
        for (int i = 0; i < Transport.MaximumPassengerSlots; i++) transport.SetPassenger(i, new EntityId(r.ReadUInt32()));
        bool slotsValid = true;
        for (int i = 0; i < Transport.MaximumPassengerSlots; i++)
            if ((i < transport.PassengerCount) != (transport.GetPassenger(i) != EntityId.None)) slotsValid = false;
        if (transport.CapacityPoints == 0 || transport.CapacityPoints > 10 || transport.OccupiedPoints > transport.CapacityPoints ||
            transport.PassengerCount > Transport.MaximumPassengerSlots || transport.JobState < TransportJobState.Idle || transport.JobState > TransportJobState.UnloadBlocked ||
            !slotsValid || (transport.JobState == TransportJobState.Idle && transport.ActivePassenger != EntityId.None) ||
            ((transport.JobState == TransportJobState.LoadingDocking || transport.JobState == TransportJobState.LoadingPassenger) && transport.ActivePassenger == EntityId.None))
            throw new InvalidDataException("Invalid transport state.");
        return transport;
    }

    private static Transformation ReadTransformation(BinaryReader r)
    {
        Transformation transformation = new()
        {
            Definition = new ContentId(r.ReadUInt32()), CurrentState = new ContentId(r.ReadUInt32()), SourceState = new ContentId(r.ReadUInt32()),
            DestinationState = new ContentId(r.ReadUInt32()), Phase = (TransformationPhase)r.ReadByte(), ProgressTicks = r.ReadUInt16(),
            TotalTicks = r.ReadUInt16(), RollbackTicksRemaining = r.ReadUInt16(), ReversalLockedUntilTick = r.ReadInt32(), QueuedToggle = r.ReadBoolean()
        };
        if (transformation.Definition.Value == 0 || transformation.CurrentState.Value == 0 || transformation.SourceState.Value == 0 || transformation.DestinationState.Value == 0 ||
            transformation.Phase < TransformationPhase.Idle || transformation.Phase > TransformationPhase.RollingBack || transformation.ReversalLockedUntilTick < 0)
            throw new InvalidDataException("Invalid transformation state.");
        return transformation;
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
            AssignedBuilder = new EntityId(r.ReadUInt32()), FundingBank = new EntityId(r.ReadUInt32())
        };
        if (format >= 21) site.CrystalFundingBank = new EntityId(r.ReadUInt32());
        site.ReservedOre = r.ReadInt32();
        site.ConsumedOre = format >= 7 ? r.ReadInt32() : 0;
        if (format >= 21)
            site.RequiredCrystals = r.ReadByte();
        site.RequiredEnergy = r.ReadInt32();
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

    private static Production ReadProduction(BinaryReader r, ushort format)
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
                UnitType = new ContentId(r.ReadUInt32()), FundingBank = new EntityId(r.ReadUInt32()),
                CrystalFundingBank = format >= 21 ? new EntityId(r.ReadUInt32()) : EntityId.None,
                EnergyDomainRoot = format >= 21 ? new EntityId(r.ReadUInt32()) : EntityId.None,
                ReservedOre = r.ReadUInt16(), RequiredEnergy = r.ReadUInt16(),
                RequiredCrystals = r.ReadByte(), ReservedOperationsCapacity = r.ReadByte(), TotalTicks = r.ReadUInt16(), RemainingTicks = r.ReadUInt16(),
                RapidFabricationUsed = format >= 21 && r.ReadBoolean()
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

    private static Targeting ReadTargeting(BinaryReader r, ushort format)
    {
        Targeting targeting = new()
        {
            CurrentTarget = new EntityId(r.ReadUInt32()), AcquisitionRadius = Fix32.FromRaw(r.ReadInt32()), LegalLayers = (TargetLayerMask)r.ReadByte(),
            LegalClasses = (TargetClassMask)r.ReadByte(), PriorityProfile = (TargetPriorityProfile)r.ReadByte(), SelectionKind = (TargetSelectionKind)r.ReadByte()
        };
        if (format >= 15)
        {
            targeting.PursuitOrigin = new FixVec2(Fix32.FromRaw(r.ReadInt32()), Fix32.FromRaw(r.ReadInt32()));
            targeting.ApproachSlotIndex = r.ReadByte(); targeting.HasPursuitOrigin = r.ReadBoolean(); targeting.HasApproachSlot = r.ReadBoolean(); targeting.HasCombatMove = r.ReadBoolean();
        }
        if (targeting.AcquisitionRadius <= Fix32.Zero || targeting.LegalLayers == TargetLayerMask.None || (targeting.LegalLayers & ~TargetLayerMask.All) != 0 ||
            targeting.LegalClasses == TargetClassMask.None || (targeting.LegalClasses & ~TargetClassMask.All) != 0 ||
            targeting.PriorityProfile < TargetPriorityProfile.AntiLight || targeting.PriorityProfile > TargetPriorityProfile.Control ||
            targeting.SelectionKind < TargetSelectionKind.None || targeting.SelectionKind > TargetSelectionKind.DirectOrder ||
            (targeting.SelectionKind == TargetSelectionKind.None && targeting.CurrentTarget != EntityId.None) ||
            (targeting.SelectionKind != TargetSelectionKind.None && targeting.CurrentTarget == EntityId.None) ||
            targeting.ApproachSlotIndex >= 16 || (targeting.SelectionKind == TargetSelectionKind.None && (targeting.HasPursuitOrigin || targeting.HasApproachSlot || targeting.HasCombatMove))) throw new InvalidDataException("Invalid targeting state.");
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

    private static DestructionState ReadDestruction(BinaryReader r, SimulationWorld world)
    {
        DestructionState destruction = new()
        {
            Kind = (DestructionKind)r.ReadByte(), StartedTick = r.ReadInt32(), BlockingUntilTick = r.ReadInt32(), VisualUntilTick = r.ReadInt32(),
            Owner = r.ReadByte(), ContentType = new ContentId(r.ReadUInt32()), SelectableKind = (SelectableKind)r.ReadByte(), Footprint = (FootprintClass)r.ReadByte(),
            BuildingType = new ContentId(r.ReadUInt32()), BuildingAnchorX = r.ReadInt16(), BuildingAnchorY = r.ReadInt16(), BuildingOrientation = r.ReadByte(),
            BuildingWidth = r.ReadByte(), BuildingHeight = r.ReadByte()
        };
        bool structureFieldsValid = destruction.Kind != DestructionKind.Structure ||
            (destruction.BuildingType.Value != 0 && destruction.BuildingWidth > 0 && destruction.BuildingHeight > 0 &&
             world.Content.TryGetBuilding(destruction.BuildingType, out _));
        bool unitFieldsValid = destruction.Kind != DestructionKind.Unit || destruction.BuildingType.Value == 0;
        bool activeLifetime = destruction.VisualUntilTick > world.Tick.Value;
        if (destruction.Kind < DestructionKind.Unit || destruction.Kind > DestructionKind.Structure || destruction.StartedTick < 0 ||
            destruction.StartedTick > world.Tick.Value || destruction.BlockingUntilTick < destruction.StartedTick ||
            destruction.VisualUntilTick < destruction.BlockingUntilTick || !activeLifetime ||
            destruction.Owner >= world.PlayerCount || destruction.ContentType.Value == 0 || destruction.SelectableKind < SelectableKind.CombatSupport ||
            destruction.SelectableKind > SelectableKind.ResourceNode || destruction.Footprint < FootprintClass.Tiny || destruction.Footprint > FootprintClass.Huge ||
            !structureFieldsValid || !unitFieldsValid) throw new InvalidDataException("Invalid destruction state.");
        return destruction;
    }

    private static void NormalizeDestroyedCollision(SimulationWorld world)
    {
        IReadOnlyList<EntityId> alive = world.Entities.Alive;
        for (int i = 0; i < alive.Count; i++)
        {
            EntityId id = alive[i];
            if (!world.Entities.Destruction.TryGet(id, out DestructionState destruction)) continue;
            ref DestructionState stored = ref world.Entities.Destruction.Get(id);
            stored.BlockingUntilTick = stored.StartedTick;
            if (stored.Kind == DestructionKind.Structure) world.ClearDestroyedStructureFootprint(stored);
            world.RemoveRuntimeState(id);
            world.Entities.Movement.Remove(id);
            world.Entities.Navigation.Remove(id);
        }
    }

    private static void AddLegacyProduction(SimulationWorld world)
    {
        IReadOnlyList<EntityId> alive = world.Entities.Alive;
        for (int i = 0; i < alive.Count; i++)
        {
            EntityId id = alive[i];
            if (world.Entities.Building.TryGet(id, out Building building) && building.State == BuildingState.Completed &&
                ProductionSystem.IsRuntimeEnabledProducer(world.Content, building.Type))
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

    private static void AddLegacyTransportComponents(SimulationWorld world)
    {
        IReadOnlyList<EntityId> alive = world.Entities.Alive;
        for (int i = 0; i < alive.Count; i++)
        {
            EntityId id = alive[i];
            if (!world.Entities.Selectable.TryGet(id, out Selectable selectable) ||
                !world.Content.TryGetEntity(selectable.ContentType, out PrototypeEntityDefinition definition)) continue;
            ScenarioFactory.AddTransportComponents(world, id, definition);
        }
    }

    private static void AddLegacyTransformationComponents(SimulationWorld world)
    {
        IReadOnlyList<EntityId> alive = world.Entities.Alive;
        for (int i = 0; i < alive.Count; i++)
        {
            EntityId id = alive[i];
            if (!world.Entities.Selectable.TryGet(id, out Selectable selectable) ||
                !world.Content.TryGetEntity(selectable.ContentType, out PrototypeEntityDefinition definition)) continue;
            ScenarioFactory.AddTransformationComponents(world, id, definition);
        }
    }

    private static void ValidateTransformationLinks(SimulationWorld world)
    {
        IReadOnlyList<EntityId> alive = world.Entities.Alive;
        for (int i = 0; i < alive.Count; i++)
        {
            EntityId id = alive[i];
            if (!world.Entities.Transformation.TryGet(id, out Transformation state)) continue;
            if (!world.Entities.Selectable.TryGet(id, out Selectable selectable) || !world.Content.TryGetTransformation(selectable.ContentType, out TransformationDefinition definition) ||
                state.Definition != definition.Id || (state.CurrentState != definition.ModeA.StateId && state.CurrentState != definition.ModeB.StateId) ||
                (state.SourceState != definition.ModeA.StateId && state.SourceState != definition.ModeB.StateId) ||
                (state.DestinationState != definition.ModeA.StateId && state.DestinationState != definition.ModeB.StateId))
                throw new InvalidDataException("Transformation state does not match its content definition.");
            if (state.Phase == TransformationPhase.Idle)
            {
                if (state.ProgressTicks != 0 || state.TotalTicks != 0 || state.RollbackTicksRemaining != 0 || state.SourceState != state.CurrentState || state.DestinationState != state.CurrentState)
                    throw new InvalidDataException("Invalid idle transformation state.");
            }
            else
            {
                if (state.TotalTicks == 0 || state.ProgressTicks > state.TotalTicks || state.SourceState != state.CurrentState || state.DestinationState == state.CurrentState)
                    throw new InvalidDataException("Invalid active transformation progress.");
                if ((state.Phase == TransformationPhase.Transitioning && state.RollbackTicksRemaining != 0) ||
                    (state.Phase == TransformationPhase.RollingBack && (state.RollbackTicksRemaining == 0 || state.RollbackTicksRemaining > state.TotalTicks)))
                    throw new InvalidDataException("Invalid transformation rollback state.");
            }
        }
    }

    private static void ValidateTransportLinks(SimulationWorld world)
    {
        HashSet<uint> loaded = new();
        IReadOnlyList<EntityId> alive = world.Entities.Alive;
        for (int i = 0; i < alive.Count; i++)
        {
            EntityId carrier = alive[i];
            if (!world.Entities.Transport.TryGet(carrier, out Transport transport)) continue;
            int occupied = 0;
            for (int p = 0; p < transport.PassengerCount; p++)
            {
                EntityId passengerId = transport.GetPassenger(p);
                if (!loaded.Add(passengerId.Value) || !world.Entities.Passenger.TryGet(passengerId, out Passenger passenger) ||
                    passenger.State != PassengerState.Loaded || passenger.Transport != carrier || world.Entities.Transform.Has(passengerId))
                    throw new InvalidDataException("Invalid loaded passenger link.");
                occupied += passenger.SizePoints;
            }
            if (occupied != transport.OccupiedPoints) throw new InvalidDataException("Transport occupied points do not match passenger links.");
            if (transport.ActivePassenger != EntityId.None &&
                (!world.Entities.Passenger.TryGet(transport.ActivePassenger, out Passenger active) || active.Transport != carrier || active.State == PassengerState.Loaded))
                throw new InvalidDataException("Invalid active transport passenger.");
        }
        for (int i = 0; i < alive.Count; i++)
        {
            EntityId passengerId = alive[i];
            if (!world.Entities.Passenger.TryGet(passengerId, out Passenger passenger)) continue;
            if (passenger.State == PassengerState.Loaded)
            {
                if (!loaded.Contains(passengerId.Value)) throw new InvalidDataException("Loaded passenger is absent from its transport.");
            }
            else if (!world.Entities.Transform.Has(passengerId) ||
                (passenger.State != PassengerState.Grounded && !world.Entities.Transport.Has(passenger.Transport)))
                throw new InvalidDataException("Ground passenger has invalid spatial or transport state.");
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

    private static void MigrateLegacyWorksites(SimulationWorld world)
    {
        IReadOnlyList<EntityId> alive = world.Entities.Alive;
        for (int i = 0; i < alive.Count; i++)
        {
            EntityId id = alive[i];
            if (world.Entities.EnergyDomainMember.TryGet(id, out EnergyDomainMember member))
                world.Entities.WorksiteMember.Set(id, new WorksiteMember { ComponentRoot = member.DomainRoot });
        }
        WorksiteGraphSystem.Rebuild(world);
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
