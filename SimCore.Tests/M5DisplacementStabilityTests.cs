using LegoSpaceRTS.SimCore;
using NUnit.Framework;

namespace LegoSpaceRTS.SimCore.Tests
{
public class M5DisplacementStabilityTests
{
    [Test]
    public void CanonicalProfilesScaleByFootprintDeploymentAndStability()
    {
        Assert.Multiple(() =>
        {
            Assert.That(DisplacementSystem.GetCanonicalDistance(DisplacementEffect.DeflectorArm, DisplacementRelation.Hostile, FootprintClass.Small, false), Is.EqualTo(Fix32.FromRatio(5, 2)));
            Assert.That(DisplacementSystem.GetCanonicalDistance(DisplacementEffect.DeflectorArm, DisplacementRelation.Hostile, FootprintClass.Medium, false), Is.EqualTo(Fix32.FromRatio(3, 2)));
            Assert.That(DisplacementSystem.GetCanonicalDistance(DisplacementEffect.GuardSweep, DisplacementRelation.Hostile, FootprintClass.Large, false), Is.EqualTo(Fix32.Half));
            Assert.That(DisplacementSystem.GetCanonicalDistance(DisplacementEffect.ExcavationClamp, DisplacementRelation.Hostile, FootprintClass.Large, true), Is.EqualTo(Fix32.Half));
            Assert.That(DisplacementSystem.GetCanonicalDistance(DisplacementEffect.ExcavationClamp, DisplacementRelation.FriendlyTow, FootprintClass.Medium, false), Is.EqualTo(Fix32.FromInt(3)));
            Assert.That(DisplacementSystem.GetCanonicalDistance(DisplacementEffect.ExcavationClamp, DisplacementRelation.Hostile, FootprintClass.Huge, false), Is.EqualTo(Fix32.Zero));
            Assert.That(DisplacementSystem.StabilityTicks, Is.EqualTo(160));
        });

        SimulationWorld world = CreateWorld(); EntityId source = AddSource(world, 0); EntityId target = AddUnit(world, 1, 20, 20, FootprintClass.Small);
        Assert.That(DisplacementSystem.TryApply(world, source, target, DisplacementEffect.DeflectorArm, DisplacementRelation.Hostile, new FixVec2(Fix32.One, Fix32.Zero), out FixVec2 first), Is.True);
        Assert.That(first.X, Is.EqualTo(Fix32.FromRatio(45, 2)));
        Assert.That(DisplacementSystem.IsStable(world, target), Is.True);
        Assert.That(DisplacementSystem.TryApply(world, source, target, DisplacementEffect.DeflectorArm, DisplacementRelation.Hostile, new FixVec2(Fix32.One, Fix32.Zero), out FixVec2 second), Is.True);
        Assert.That(second.X - first.X, Is.EqualTo(Fix32.FromRatio(5, 8)), "Stability scales the next hostile displacement to exactly 25%.");
    }

    [Test]
    public void ResolverClipsBeforeImpassableGeometry()
    {
        SimulationWorld world = CreateWorld(); world.Map.SetFlagsRect(new IntRect(44, 0, 2, 320), MapCellFlags.Impassable, MapCellFlags.Ground);
        EntityId source = AddSource(world, 0); EntityId target = AddUnit(world, 1, 20, 20, FootprintClass.Tiny);
        Assert.That(DisplacementSystem.TryApply(world, source, target, DisplacementEffect.ExcavationClamp, DisplacementRelation.Hostile, new FixVec2(Fix32.One, Fix32.Zero), out FixVec2 resolved), Is.True);
        Assert.Multiple(() =>
        {
            Assert.That(resolved.X, Is.GreaterThan(Fix32.FromInt(20)));
            Assert.That(resolved.X, Is.LessThan(Fix32.FromInt(23)));
            Assert.That(world.Pathfinder.IsPassable(MapGrid.BuildToNav(resolved), FootprintClass.Tiny), Is.True);
        });
    }

    [Test]
    public void ResolverStopsBeforeOccupiedSpace()
    {
        SimulationWorld world = CreateWorld(); EntityId source = AddSource(world, 0);
        EntityId target = AddUnit(world, 1, 20, 20, FootprintClass.Tiny); EntityId blocker = AddUnit(world, 1, 22, 20, FootprintClass.Tiny);
        Assert.That(DisplacementSystem.TryApply(world, source, target, DisplacementEffect.ExcavationClamp, DisplacementRelation.Hostile, new FixVec2(Fix32.One, Fix32.Zero), out FixVec2 resolved), Is.True);
        Fix32 required = FootprintRules.CollisionRadiusBuild(FootprintClass.Tiny) * Fix32.FromInt(2);
        Assert.That(FixVec2.Distance(resolved, world.Entities.Transform.Get(blocker).Position), Is.GreaterThanOrEqualTo(required));
    }

    [Test]
    public void DisplacementPreservesOrdersAndDeploymentWhileFriendlyTowAddsNoStability()
    {
        SimulationWorld world = CreateWorld(); EntityId hostileSource = AddSource(world, 0); EntityId target = AddUnit(world, 1, 30, 30, FootprintClass.Large);
        world.Entities.Deployment.Set(target, new Deployment { State = DeploymentState.Deployed });
        ref NavigationAgent navigation = ref world.Entities.Navigation.Get(target); navigation.HasTarget = true; navigation.Target = new FixVec2(Fix32.FromInt(60), Fix32.FromInt(60));
        FixVec2 retainedTarget = navigation.Target;
        Assert.That(DisplacementSystem.TryApply(world, hostileSource, target, DisplacementEffect.ExcavationClamp, DisplacementRelation.Hostile, new FixVec2(Fix32.One, Fix32.Zero), out FixVec2 hostileResult), Is.True);
        Assert.Multiple(() =>
        {
            Assert.That(hostileResult.X, Is.EqualTo(Fix32.FromRatio(61, 2)), "Deployed Heavy receives half of the normal 1-cell Clamp.");
            Assert.That(world.Entities.Deployment.Get(target).State, Is.EqualTo(DeploymentState.Deployed));
            Assert.That(world.Entities.Navigation.Get(target).Target, Is.EqualTo(retainedTarget));
            Assert.That(world.Entities.Navigation.Get(target).PathDirty, Is.True);
            Assert.That(world.Entities.Stability.Get(target).UntilTick, Is.EqualTo(160));
        });

        EntityId friendlySource = AddSource(world, 1); EntityId friendly = AddUnit(world, 1, 40, 40, FootprintClass.Medium);
        Assert.That(DisplacementSystem.TryApply(world, friendlySource, friendly, DisplacementEffect.ExcavationClamp, DisplacementRelation.FriendlyTow, new FixVec2(Fix32.One, Fix32.Zero), out _), Is.True);
        Assert.That(world.Entities.Stability.Has(friendly), Is.False);
    }

    [Test]
    public void SnapshotPreservesStabilityAndDeterministicContinuation()
    {
        SimulationWorld world = CreateWorld(); EntityId source = AddSource(world, 0); EntityId target = AddUnit(world, 1, 25, 25, FootprintClass.Medium);
        Assert.That(DisplacementSystem.TryApply(world, source, target, DisplacementEffect.GuardSweep, DisplacementRelation.Hostile, new FixVec2(Fix32.One, Fix32.Zero), out _), Is.True);
        new SimulationRunner(world).StepTicks(37); SimulationWorld restored = SnapshotSerializer.Deserialize(SnapshotSerializer.Serialize(world));
        Assert.Multiple(() =>
        {
            Assert.That(StateHasher.Hash(restored), Is.EqualTo(StateHasher.Hash(world)));
            Assert.That(DisplacementSystem.RemainingStabilityTicks(restored, target), Is.EqualTo(123));
        });
        SimulationRunner originalRunner = new(world), restoredRunner = new(restored); originalRunner.StepTicks(200); restoredRunner.StepTicks(200);
        Assert.That(StateHasher.Hash(restored), Is.EqualTo(StateHasher.Hash(world)));
    }

    private static SimulationWorld CreateWorld() => new(new MapGrid("map.test.m5_displacement"), 2);

    private static EntityId AddSource(SimulationWorld world, byte owner)
    {
        EntityId id = world.Entities.Create(); world.Entities.Ownership.Set(id, new Ownership { PlayerSlot = owner }); return id;
    }

    private static EntityId AddUnit(SimulationWorld world, byte owner, int x, int y, FootprintClass footprint)
    {
        EntityId id = world.Entities.Create(); world.Entities.Ownership.Set(id, new Ownership { PlayerSlot = owner });
        FixVec2 position = new(Fix32.FromInt(x), Fix32.FromInt(y)); world.Entities.Transform.Set(id, new SimTransform { Position = position, Orientation = Angle16.Zero });
        world.Entities.Navigation.Set(id, new NavigationAgent { Footprint = footprint, Layer = MovementLayer.Ground });
        world.Entities.Movement.Set(id, new Movement { MaxSpeed = Fix32.FromInt(4), Acceleration = Fix32.FromInt(4), Deceleration = Fix32.FromInt(4), LastPosition = position });
        return id;
    }
}
}
