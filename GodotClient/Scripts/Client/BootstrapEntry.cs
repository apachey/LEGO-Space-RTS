using Godot;
using LegoSpaceRTS.Networking;

namespace LegoSpaceRTS.Client;

public partial class BootstrapEntry : Node
{
    public override void _Ready()
    {
        string[] arguments = OS.GetCmdlineUserArgs();
        if (arguments.Contains("--m6-transport-smoke"))
        {
            AddChild(new M6TransportSmokeRunner { Name = "M6TransportSmokeRunner" });
            return;
        }
        if (arguments.Contains("--m6-command-smoke"))
        {
            AddChild(new M6CommandAuthoritySmokeRunner { Name = "M6CommandAuthoritySmokeRunner" });
            return;
        }
        if (arguments.Contains("--m6-reconnect-smoke"))
        {
            AddChild(new M6ReconnectSmokeRunner { Name = "M6ReconnectSmokeRunner" });
            return;
        }
        if (arguments.Contains("--dedicated-server"))
        {
            StartDedicatedServer(arguments);
            return;
        }

        // SceneTree is still attaching Bootstrap while _Ready runs. Deferring the
        // transition avoids mutating that tree during its own add-child callback.
        Callable.From(() => { GetTree().ChangeSceneToFile("res://Scenes/PrototypeRTS.tscn"); }).CallDeferred();
    }

    private void StartDedicatedServer(string[] arguments)
    {
        try
        {
            M6TransportHostOptions options = M6TransportHostOptions.Parse(arguments);
            LoadedScenario scenario = RuntimeScenarioLoader.LoadCanonicalOpening();
            M6DedicatedServerCommandHost host = new() { Name = "M6DedicatedServerCommandHost" };
            AddChild(host);
            Error result = host.Listen(options, scenario.World, scenario.GameplayContentHash);
            if (result == Error.Ok) return;
            GD.PrintErr($"M6 dedicated command authority failed to listen: {result}");
        }
        catch (Exception exception)
        {
            GD.PrintErr($"M6 dedicated command authority configuration failed: {exception.Message}");
        }
        GetTree().Quit(2);
    }
}
