using Godot;

namespace LegoSpaceRTS.Client;

public partial class BootstrapEntry : Node
{
    public override void _Ready()
    {
        string[] args = OS.GetCmdlineUserArgs();
        if (args.Contains("--smoke"))
        {
            GetTree().ChangeSceneToFile("res://Scenes/PrototypeRTS.tscn");
            return;
        }
        GetTree().ChangeSceneToFile("res://Scenes/PrototypeRTS.tscn");
    }
}
