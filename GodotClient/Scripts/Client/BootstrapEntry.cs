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
            M6DedicatedServerTransportHost host = new() { Name = "M6DedicatedServerTransportHost" };
            AddChild(host);
            Error result = host.Listen(options);
            if (result == Error.Ok) return;
            GD.PrintErr($"M6 dedicated transport failed to listen: {result}");
        }
        catch (Exception exception)
        {
            GD.PrintErr($"M6 dedicated transport configuration failed: {exception.Message}");
        }
        GetTree().Quit(2);
    }
}
