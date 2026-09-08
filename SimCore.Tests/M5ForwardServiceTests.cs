using LegoSpaceRTS.SimCore;
using NUnit.Framework;

namespace LegoSpaceRTS.SimCore.Tests
{
public sealed class M5ForwardServiceTests
{
    private static readonly ContentId ServiceHub = StableId.FromKey("building.ast.service_refit_hub");
    private static readonly ContentId SolarExplorer = StableId.FromKey(CanonicalRosterReferences.SolarExplorer);
    private static readonly ContentId T3Trike = StableId.FromKey(CanonicalRosterReferences.T3Trike);

    [Test]
    public void CanonicalProvidersExposeEighteenAndTenCellRadiiOnlyWhenActive()
    {
        SimulationWorld world = NewWorld();
        EntityId hub = AddHub(world, 0, FixVec2.FromInts(40, 40), BuildingState.Completed);
        EntityId solar = AddSolarExplorer(world, 0, FixVec2.FromInts(80, 40), DeploymentState.Mobile);
        ForwardServiceSystem system = new();

        system.Step(world);
        Assert.Multiple(() =>
        {
            ForwardServiceProvider hubProvider = world.Entities.ForwardServiceProvider.Get(hub);
            ForwardServiceProvider solarProvider = world.Entities.ForwardServiceProvider.Get(solar);
            Assert.That(hubProvider.RadiusBuildCells, Is.EqualTo(18));
            Assert.That(hubProvider.IsActive, Is.True);
            Assert.That(solarProvider.RadiusBuildCells, Is.EqualTo(10));
            Assert.That(solarProvider.IsActive, Is.False);
        });

        world.Entities.Deployment.Get(solar).State = DeploymentState.Deployed;
        system.Step(world);
        Assert.That(world.Entities.ForwardServiceProvider.Get(solar).IsActive, Is.True);
    }

    [Test]
    public void MembershipUsesExactRadiusAndOwner()
    {
        SimulationWorld world = NewWorld();
        EntityId hub = AddHub(world, 0, FixVec2.FromInts(40, 40), BuildingState.Completed);
        EntityId boundary = AddT3(world, 0, FixVec2.FromInts(58, 40));
        EntityId outside = AddT3(world, 0, new FixVec2(Fix32.FromRatio(117, 2), Fix32.FromInt(40)));
        EntityId enemyInside = AddT3(world, 1, FixVec2.FromInts(42, 40));

        ForwardServiceSystem system = new();
        system.Step(world);

        Assert.Multiple(() =>
        {
            Assert.That(ForwardServiceSystem.TryGetProviderForMember(world, boundary, out EntityId boundaryProvider), Is.True);
            Assert.That(boundaryProvider, Is.EqualTo(hub));
            Assert.That(ForwardServiceSystem.TryGetProviderForMember(world, outside, out _), Is.False);
            Assert.That(ForwardServiceSystem.TryGetProviderForMember(world, enemyInside, out _), Is.False);
        });

        world.Entities.Ownership.Get(boundary).PlayerSlot = 1;
        system.Step(world);
        Assert.That(world.Entities.ForwardServiceMember.Get(boundary).Provider, Is.EqualTo(EntityId.None), "Member ownership changes invalidate cached service even inside the same query cell.");
    }

    [Test]
    public void MemberUpdatesWhenItCrossesProviderQueryCells()
    {
        SimulationWorld world = NewWorld();
        AddHub(world, 0, FixVec2.FromInts(40, 40), BuildingState.Completed);
        EntityId trike = AddT3(world, 0, FixVec2.FromInts(58, 40));
        ForwardServiceSystem system = new();
        system.Step(world);
        Assert.That(ForwardServiceSystem.TryGetProviderForMember(world, trike, out _), Is.True);

        world.Entities.Transform.Get(trike).Position = FixVec2.FromInts(59, 40);
        system.Step(world);

        Assert.Multiple(() =>
        {
            Assert.That(ForwardServiceSystem.TryGetProviderForMember(world, trike, out _), Is.False);
            Assert.That(world.Entities.ForwardServiceMember.Get(trike).QueryCellX, Is.EqualTo(59));
        });
    }

    [Test]
    public void ProviderDeploymentDestructionAndBrownoutInvalidateMembership()
    {
        SimulationWorld world = NewWorld();
        EntityId hub = AddHub(world, 0, FixVec2.FromInts(40, 40), BuildingState.Completed);
        EntityId solar = AddSolarExplorer(world, 0, FixVec2.FromInts(48, 40), DeploymentState.Deployed);
        EntityId trike = AddT3(world, 0, FixVec2.FromInts(45, 40));
        ForwardServiceSystem system = new();
        system.Step(world);
        Assert.That(world.Entities.ForwardServiceMember.Get(trike).Provider, Is.EqualTo(hub), "Overlapping coverage uses stable Entity ID order.");

        Assert.That(world.Entities.Destroy(hub), Is.True);
        system.Step(world);
        Assert.That(world.Entities.ForwardServiceMember.Get(trike).Provider, Is.EqualTo(solar));

        world.Entities.Deployment.Get(solar).State = DeploymentState.Undeploying;
        system.Step(world);
        Assert.That(ForwardServiceSystem.TryGetProviderForMember(world, trike, out _), Is.False);

        EntityId replacementHub = AddHub(world, 0, FixVec2.FromInts(40, 40), BuildingState.Completed);
        world.Entities.PowerState.Set(replacementHub, new PowerState { Priority = EnergyPriority.Normal, IsPowered = false });
        system.Step(world);
        Assert.That(world.Entities.ForwardServiceProvider.Get(replacementHub).IsActive, Is.False, "Brownout-disabled service infrastructure cannot repair or refit.");
    }

    [Test]
    public void ConstructionSiteDoesNotProvideForwardService()
    {
        SimulationWorld world = NewWorld();
        EntityId hub = AddHub(world, 0, FixVec2.FromInts(40, 40), BuildingState.ConstructionSite);
        EntityId trike = AddT3(world, 0, FixVec2.FromInts(42, 40));

        new ForwardServiceSystem().Step(world);

        Assert.Multiple(() =>
        {
            Assert.That(world.Entities.ForwardServiceProvider.Get(hub).IsActive, Is.False);
            Assert.That(ForwardServiceSystem.TryGetProviderForMember(world, trike, out _), Is.False);
        });
    }

    [Test]
    public void SnapshotPreservesMembershipAndDeterministicContinuation()
    {
        SimulationWorld world = NewWorld();
        AddAstCommand(world, 0, FixVec2.FromInts(25, 40));
        AddHub(world, 0, FixVec2.FromInts(40, 40), BuildingState.Completed);
        AddSolarExplorer(world, 0, FixVec2.FromInts(70, 40), DeploymentState.Deployed);
        EntityId trike = AddT3(world, 0, FixVec2.FromInts(45, 40));
        SimulationRunner original = new(world);
        original.StepTicks(5);
        ulong before = StateHasher.Hash(world);

        SimulationWorld restoredWorld = SnapshotSerializer.Deserialize(SnapshotSerializer.Serialize(world));
        Assert.Multiple(() =>
        {
            Assert.That(StateHasher.Hash(restoredWorld), Is.EqualTo(before));
            Assert.That(ForwardServiceSystem.TryGetProviderForMember(restoredWorld, trike, out _), Is.True);
        });

        SimulationRunner restored = new(restoredWorld);
        original.StepTicks(40);
        restored.StepTicks(40);
        Assert.That(StateHasher.Hash(restoredWorld), Is.EqualTo(StateHasher.Hash(world)));
    }

    private static SimulationWorld NewWorld()
    {
        SimulationWorld world = new(DevMapFactory.Create(), 2);
        ExcavationTopologySystem.InitializeFeatures(world);
        return world;
    }

    private static EntityId AddHub(SimulationWorld world, byte player, FixVec2 position, BuildingState state)
    {
        EntityId id = AddSelectable(world, player, position, ServiceHub, SelectableKind.Building);
        world.Entities.Building.Set(id, new Building
        {
            Type = ServiceHub, AnchorX = checked((short)(position.X.FloorToInt() - 3)), AnchorY = checked((short)(position.Y.FloorToInt() - 3)),
            FootprintWidth = 7, FootprintHeight = 7, State = state
        });
        return id;
    }

    private static EntityId AddAstCommand(SimulationWorld world, byte player, FixVec2 position)
    {
        ContentId type = StableId.FromKey("building.ast.mb01_eagle_command_base");
        EntityId id = AddSelectable(world, player, position, type, SelectableKind.Building);
        world.Entities.Building.Set(id, new Building { Type = type, AnchorX = 21, AnchorY = 36, FootprintWidth = 8, FootprintHeight = 8, State = BuildingState.Completed });
        return id;
    }

    private static EntityId AddSolarExplorer(SimulationWorld world, byte player, FixVec2 position, DeploymentState state)
    {
        EntityId id = AddSelectable(world, player, position, SolarExplorer, SelectableKind.CombatSupport);
        world.Entities.Deployment.Set(id, new Deployment { State = state });
        return id;
    }

    private static EntityId AddT3(SimulationWorld world, byte player, FixVec2 position)
        => AddSelectable(world, player, position, T3Trike, SelectableKind.CombatSupport);

    private static EntityId AddSelectable(SimulationWorld world, byte player, FixVec2 position, ContentId type, SelectableKind kind)
    {
        EntityId id = world.Entities.Create();
        world.Entities.Ownership.Set(id, new Ownership { PlayerSlot = player });
        world.Entities.Transform.Set(id, new SimTransform { Position = position, Orientation = Angle16.Zero });
        world.Entities.Selectable.Set(id, new Selectable { IsSelectable = true, ContentType = type, Kind = kind });
        return id;
    }
}
}
