using LegoSpaceRTS.SimCore;
using NUnit.Framework;

namespace LegoSpaceRTS.SimCore.Tests;

public class CompiledScenarioTests
{
    [Test]
    public void CompiledMapAndContentProduceSameInitialAuthoritativeStateAsFactories()
    {
        PrototypeContentCatalog content = PrototypeContentFactory.CreateM2Catalog();
        MapDefinition map = DevMapFactory.CreateDefinition();
        byte[] contentBytes = PrototypeContentCodec.Write(content);
        byte[] mapBytes = CompiledMapCodec.Write(map);

        SimulationWorld factoryWorld = ScenarioFactory.CreateFirstControllable(map, content, 18);
        SimulationWorld compiledWorld = ScenarioFactory.CreateFirstControllable(
            CompiledMapCodec.ReadDefinition(mapBytes),
            PrototypeContentCodec.Read(contentBytes),
            18);

        Assert.That(StateHasher.Hash(compiledWorld), Is.EqualTo(StateHasher.Hash(factoryWorld)));
    }
}
