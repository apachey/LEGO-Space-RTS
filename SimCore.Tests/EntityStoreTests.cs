using System.Linq;
using LegoSpaceRTS.SimCore;
using NUnit.Framework;

public class EntityStoreTests
{
    [Test] public void AllocationIsMonotonicAndDestroyedIdsAreNotReused()
    {
        EntityStore store = new(); EntityId a=store.Create(); EntityId b=store.Create(); store.Destroy(a); EntityId c=store.Create();
        Assert.That(a.Value, Is.EqualTo(1)); Assert.That(b.Value, Is.EqualTo(2)); Assert.That(c.Value, Is.EqualTo(3)); Assert.That(store.Exists(a), Is.False);
    }
    [Test] public void AliveIterationIsDeterministic()
    {
        EntityStore store = new(); EntityId a=store.Create(); EntityId b=store.Create(); EntityId c=store.Create(); store.Destroy(b);
        Assert.That(store.Alive.Select(x=>x.Value).ToArray(), Is.EqualTo(new[]{a.Value,c.Value}));
    }
}
