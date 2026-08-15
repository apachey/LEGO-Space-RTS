using System.Linq;
using LegoSpaceRTS.SimCore;
using NUnit.Framework;

namespace LegoSpaceRTS.SimCore.Tests
{
public sealed class M5WorksiteGraphTests
{
    private static readonly ContentId Hq = StableId.FromKey("building.rock_raiders.hq");
    private static readonly ContentId ProcessingPlant = StableId.FromKey("building.rock_raiders.ore_processing_plant");
    private static readonly ContentId ServiceBay = StableId.FromKey("building.rock_raiders.vehicle_service_bay");
    private static readonly ContentId HoverScout = StableId.FromKey("unit.rock_raiders.hover_scout");

    [Test]
    public void CanonicalNodesExposeEighteenAndTwelveCellServiceRadii()
    {
        PrototypeContentCatalog content = PrototypeContentFactory.CreateM2Catalog();
        Assert.Multiple(() =>
        {
            Assert.That(content.Buildings.Single(x => x.Id == Hq).WorksiteServiceRadius, Is.EqualTo(18));
            Assert.That(content.Buildings.Single(x => x.Id == ServiceBay).WorksiteServiceRadius, Is.EqualTo(12));
            Assert.That(content.Buildings.Single(x => x.Id == ProcessingPlant).WorksiteServiceRadius, Is.Zero);
        });
    }

    [Test]
    public void ServiceMembershipUsesOperatingCenterAndOwner()
    {
        SimulationWorld world = ScenarioFactory.CreateCanonicalOpening();
        EntityId playerHq = PlayerHq(world, 0);
        EntityId inside = AddCompletedBuilding(world, 0, ProcessingPlant, 22, 70);
        EntityId outside = AddCompletedBuilding(world, 0, ProcessingPlant, 35, 70);
        EntityId enemyInside = AddCompletedBuilding(world, 1, ProcessingPlant, 22, 70);

        WorksiteGraphSystem.Rebuild(world);

        Assert.Multiple(() =>
        {
            Assert.That(WorksiteGraphSystem.TryGetComponentForEntity(world, playerHq, out EntityId root), Is.True);
            Assert.That(WorksiteGraphSystem.TryGetComponentForEntity(world, inside, out EntityId insideRoot), Is.True);
            Assert.That(insideRoot, Is.EqualTo(root));
            Assert.That(WorksiteGraphSystem.TryGetComponentForEntity(world, outside, out _), Is.False);
            Assert.That(WorksiteGraphSystem.TryGetComponentForEntity(world, enemyInside, out _), Is.False,
                "An enemy structure inside the Raider service geometry must not become a member.");
        });
    }

    [Test]
    public void BridgeNodeMergesAndRemovalSplitsComponentsWithoutLosingOreOrEnergy()
    {
        SimulationWorld world = ScenarioFactory.CreateCanonicalOpening();
        EntityId startingHq = PlayerHq(world, 0);
        EntityId remoteHq = AddCompletedBuilding(world, 0, Hq, 52, 70, ore: 300);
        WorksiteGraphSystem.Rebuild(world);
        EntityId startingRoot = world.Entities.WorksiteMember.Get(startingHq).ComponentRoot;
        EntityId remoteRoot = world.Entities.WorksiteMember.Get(remoteHq).ComponentRoot;
        world.Entities.EnergyDomain.Get(startingRoot).Reserve = Fix32.FromInt(120);
        world.Entities.EnergyDomain.Get(remoteRoot).Reserve = Fix32.FromInt(80);
        EntityId remoteSite = AddCompletedBuilding(world, 0, ProcessingPlant, 58, 72);
        world.Entities.Building.Get(remoteSite).State = BuildingState.ConstructionSite;
        world.Entities.ConstructionSite.Set(remoteSite, new ConstructionSite
        {
            FundingBank = remoteHq, ReservedEnergy = 10, RequiredEnergy = 10, EnergyDomainRoot = remoteRoot,
            RequiredTicks = 100
        });
        WorksiteGraphSystem.Rebuild(world);
        int oreBefore = world.GetProcessedResourceTotal(0, ResourceType.Ore);

        EntityId bridge = AddCompletedBuilding(world, 0, ServiceBay, 29, 71);
        Assert.That(WorksiteGraphSystem.Rebuild(world), Is.True);
        EntityId mergedRoot = world.Entities.WorksiteMember.Get(startingHq).ComponentRoot;
        Assert.Multiple(() =>
        {
            Assert.That(world.Entities.WorksiteMember.Get(remoteHq).ComponentRoot, Is.EqualTo(mergedRoot));
            Assert.That(world.Entities.WorksiteMember.Get(bridge).ComponentRoot, Is.EqualTo(mergedRoot));
            Assert.That(world.Entities.WorksiteComponent.Get(mergedRoot).NodeCount, Is.EqualTo(3));
            Assert.That(world.Entities.EnergyDomain.Get(mergedRoot).Reserve, Is.EqualTo(Fix32.FromInt(200)));
            Assert.That(WorksiteGraphSystem.GetProcessedResourceTotal(world, mergedRoot, ResourceType.Ore), Is.EqualTo(oreBefore));
            Assert.That(world.Entities.ConstructionSite.Get(remoteSite).EnergyDomainRoot, Is.EqualTo(mergedRoot),
                "A pending refund follows its funding bank when the old domain root disappears.");
        });
        Assert.That(ConstructionPlacement.TryCancelUnstarted(world, 0, remoteSite), Is.True);
        Assert.That(world.Entities.EnergyDomain.Get(mergedRoot).Reserve, Is.EqualTo(Fix32.FromInt(210)));

        Assert.That(world.Entities.Destroy(bridge), Is.True);
        Assert.That(WorksiteGraphSystem.Rebuild(world), Is.True);
        EntityId splitStartingRoot = world.Entities.WorksiteMember.Get(startingHq).ComponentRoot;
        EntityId splitRemoteRoot = world.Entities.WorksiteMember.Get(remoteHq).ComponentRoot;
        Assert.Multiple(() =>
        {
            Assert.That(splitRemoteRoot, Is.Not.EqualTo(splitStartingRoot));
            Assert.That(world.Entities.EnergyDomain.Get(splitStartingRoot).Reserve + world.Entities.EnergyDomain.Get(splitRemoteRoot).Reserve, Is.EqualTo(Fix32.FromInt(210)));
            Assert.That(world.Entities.ResourceBank.Get(startingHq).ProcessedAmount, Is.EqualTo(500));
            Assert.That(world.Entities.ResourceBank.Get(remoteHq).ProcessedAmount, Is.EqualTo(300));
            Assert.That(world.GetProcessedResourceTotal(0, ResourceType.Ore), Is.EqualTo(oreBefore));
        });
    }

    [Test]
    public void ConnectedProductionCanUseRemoteBankButSplitImmediatelyRemovesAccess()
    {
        SimulationWorld world = ScenarioFactory.CreateCanonicalOpening();
        EntityId startingHq = PlayerHq(world, 0);
        EntityId remoteHq = AddCompletedBuilding(world, 0, Hq, 52, 70, ore: 0);
        EntityId remoteProducer = AddCompletedBuilding(world, 0, ServiceBay, 56, 78, producer: true);
        EntityId bridge = AddCompletedBuilding(world, 0, ServiceBay, 29, 71);
        WorksiteGraphSystem.Rebuild(world);
        EntityId mergedRoot = world.Entities.WorksiteMember.Get(startingHq).ComponentRoot;
        world.Entities.EnergyDomain.Get(mergedRoot).Reserve = Fix32.FromInt(100);
        world.Entities.ResourceBank.Get(startingHq).ProcessedAmount = 50;
        world.Entities.ResourceBank.Get(remoteHq).ProcessedAmount = 30;

        Assert.That(ProductionSystem.TryQueue(world, 0, remoteProducer, HoverScout), Is.True);
        Assert.That(WorksiteGraphSystem.GetProcessedResourceTotal(world, mergedRoot, ResourceType.Ore), Is.EqualTo(5),
            "A connected component can fund one order from multiple local banks without creating Ore.");

        Assert.That(world.Entities.Destroy(bridge), Is.True);
        WorksiteGraphSystem.Rebuild(world);
        Assert.Multiple(() =>
        {
            Assert.That(world.Entities.WorksiteMember.Get(remoteHq).ComponentRoot, Is.Not.EqualTo(world.Entities.WorksiteMember.Get(startingHq).ComponentRoot));
            Assert.That(ProductionSystem.TryQueue(world, 0, remoteProducer, HoverScout), Is.False);
            Assert.That(world.Entities.Production.Get(remoteProducer).Count, Is.EqualTo(1), "The already supplied queue remains valid after disconnection.");
        });
    }

    [Test]
    public void SnapshotPreservesTopologyAndDeterministicContinuation()
    {
        SimulationWorld world = ScenarioFactory.CreateCanonicalOpening();
        AddCompletedBuilding(world, 0, Hq, 52, 70, ore: 125);
        AddCompletedBuilding(world, 0, ServiceBay, 29, 71);
        WorksiteGraphSystem.Rebuild(world);
        new SimulationRunner(world).StepTicks(7);
        ulong before = StateHasher.Hash(world);

        SimulationWorld restored = SnapshotSerializer.Deserialize(SnapshotSerializer.Serialize(world));
        Assert.That(StateHasher.Hash(restored), Is.EqualTo(before));
        Assert.That(WorksiteGraphSystem.Rebuild(restored), Is.False, "A restored cached graph must already be current.");
        SimulationRunner originalRunner = new(world), restoredRunner = new(restored);
        originalRunner.StepTicks(40); restoredRunner.StepTicks(40);
        Assert.That(StateHasher.Hash(restored), Is.EqualTo(StateHasher.Hash(world)));
    }

    private static EntityId PlayerHq(SimulationWorld world, byte player)
        => world.Entities.Alive.Single(id => world.Entities.Building.TryGet(id, out Building building) && building.Type == Hq &&
            world.Entities.Ownership.TryGet(id, out Ownership ownership) && ownership.PlayerSlot == player);

    private static EntityId AddCompletedBuilding(SimulationWorld world, byte player, ContentId type, short anchorX, short anchorY,
        int? ore = null, bool producer = false)
    {
        BuildingDefinition definition = world.Content.Buildings.Single(x => x.Id == type);
        EntityId id = world.Entities.Create();
        FixVec2 center = new(Fix32.FromRatio(anchorX * 2 + definition.FootprintWidth, 2), Fix32.FromRatio(anchorY * 2 + definition.FootprintHeight, 2));
        world.Entities.Ownership.Set(id, new Ownership { PlayerSlot = player });
        world.Entities.Transform.Set(id, new SimTransform { Position = center, Orientation = Angle16.Zero });
        world.Entities.Selectable.Set(id, new Selectable { IsSelectable = true, ContentType = type, Kind = SelectableKind.Building });
        world.Entities.Building.Set(id, new Building
        {
            Type = type, AnchorX = anchorX, AnchorY = anchorY, FootprintWidth = definition.FootprintWidth,
            FootprintHeight = definition.FootprintHeight, State = BuildingState.Completed
        });
        if (ore.HasValue)
        {
            world.Entities.ResourceReceiver.Set(id, new ResourceReceiver { AcceptedType = ResourceType.Ore, IsHqEmergencyReceiver = true });
            world.Entities.ResourceBank.Set(id, new ResourceBank { Type = ResourceType.Ore, ProcessedAmount = ore.Value });
        }
        if (producer) world.Entities.Production.Set(id, new Production());
        return id;
    }
}
}
