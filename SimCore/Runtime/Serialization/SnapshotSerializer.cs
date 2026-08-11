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
        WorksiteNode = 1 << 20,
        WorksiteMember = 1 << 21,
        WorksiteComponent = 1 << 22,
        Excavatable = 1 << 23,
        Deployment = 1 << 24,
        ForwardServiceProvider = 1 << 25,
        ForwardServiceMember = 1 << 26,
        MissionRefitState = 1 << 27,
        MissionRefitJob = 1 << 28,
        All = Ownership | Transform | Movement | Navigation | Selectable | Vision | ResourceNode | CommandQueue | RouteCorridor | Worker | ResourceCarrier | ResourceReceiver | ResourceBank | Building | ConstructionSite | Builder | Production | EnergyDomain | EnergyDomainMember | PowerState | WorksiteNode | WorksiteMember | WorksiteComponent | Excavatable | Deployment | ForwardServiceProvider | ForwardServiceMember | MissionRefitState | MissionRefitJob
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
        w.Flush(); return ms.ToArray();
    }

    public static SimulationWorld Deserialize(byte[] bytes, PrototypeContentCatalog? content = null)
    {
        using MemoryStream ms = new(bytes, false); using BinaryReader r = new(ms);
        if (r.ReadUInt32() != Magic) throw new InvalidDataException("Snapshot magic mismatch.");
        ushort format = r.ReadUInt16(), protocol = r.ReadUInt16();
        bool supportedLegacy = ((format == 2 || format == 3) && protocol == 1) || (format == 4 && protocol == 2) || (format == 5 && protocol == 3) || (format == 6 && protocol == 4) || (format == 7 && protocol == 5) || (format == 8 && protocol == 6) || (format == 9 && protocol == 7) || (format == 10 && protocol == 8) || (format == 11 && protocol == 9) || (format == 12 && protocol == 10) || (format == 13 && protocol == 11);
        if (!supportedLegacy && (format != FormatVersion || protocol != SimulationProtocolVersion)) throw new InvalidDataException($"Unsupported snapshot {format}/{protocol}.");
        SimTick tick = new(r.ReadInt32()); MapGrid map = MapGrid.Deserialize(r, includeExcavatableMetadata: format >= 12); uint nextEntity = r.ReadUInt32();
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
        entities.RestoreNextEntityValue(nextEntity);
        temp.Commands.Deserialize(r, includeBuildFields: format >= 6, includeEnergyPriority: format >= 10, includeMissionRefit: format >= 14);
        temp.Fog = FogState.Deserialize(r);
        if (ms.Position != ms.Length) throw new InvalidDataException("Trailing snapshot bytes.");
        ExcavationTopologySystem.InitializeFeatures(temp);
        if (format < 9) InitializeLegacyEnergy(temp);
        else if (format < 11) MigrateLegacyWorksites(temp);
        else EnergyDomainSystem.RecalculateAll(temp);
        OperationsCapacitySystem.Recalculate(temp);
        temp.Spatial.Rebuild(temp.Entities);
        return temp;
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
        if (world.Entities.WorksiteNode.Has(id)) components |= EntityComponents.WorksiteNode;
        if (world.Entities.WorksiteMember.Has(id)) components |= EntityComponents.WorksiteMember;
        if (world.Entities.WorksiteComponent.Has(id)) components |= EntityComponents.WorksiteComponent;
        if (world.Entities.Excavatable.Has(id)) components |= EntityComponents.Excavatable;
        if (world.Entities.Deployment.Has(id)) components |= EntityComponents.Deployment;
        if (world.Entities.ForwardServiceProvider.Has(id)) components |= EntityComponents.ForwardServiceProvider;
        if (world.Entities.ForwardServiceMember.Has(id)) components |= EntityComponents.ForwardServiceMember;
        if (world.Entities.MissionRefitState.Has(id)) components |= EntityComponents.MissionRefitState;
        if (world.Entities.MissionRefitJob.Has(id)) components |= EntityComponents.MissionRefitJob;
        if (world.Entities.ConstructionSite.Has(id)) components |= EntityComponents.ConstructionSite;
        if (world.Entities.Production.Has(id)) components |= EntityComponents.Production;
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
        if ((components & EntityComponents.ConstructionSite) != 0) WriteConstructionSite(w, world.Entities.ConstructionSite.Get(id));
        if ((components & EntityComponents.Production) != 0) WriteProduction(w, world.Entities.Production.Get(id));
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
        if ((components & EntityComponents.ConstructionSite) != 0) world.Entities.ConstructionSite.Set(id, ReadConstructionSite(r, format));
        if ((components & EntityComponents.Production) != 0) world.Entities.Production.Set(id, ReadProduction(r));
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
