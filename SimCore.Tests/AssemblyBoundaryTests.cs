using NUnit.Framework;

public class AssemblyBoundaryTests
{
    [Test]
    public void SimCoreDoesNotReferenceAnyGameEngineAssembly()
    {
        string[] refs = typeof(LegoSpaceRTS.SimCore.Fix32).Assembly.GetReferencedAssemblies().Select(x => x.Name ?? string.Empty).ToArray();
        Assert.That(refs.Any(x => x.StartsWith("UnityEngine", StringComparison.Ordinal)), Is.False);
        Assert.That(refs.Any(x => x.StartsWith("GodotSharp", StringComparison.Ordinal) || x.Equals("Godot", StringComparison.Ordinal)), Is.False);
    }
}
