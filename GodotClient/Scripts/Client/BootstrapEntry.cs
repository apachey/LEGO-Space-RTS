using Godot;

namespace LegoSpaceRTS.Client;

public partial class BootstrapEntry : Node
{
    public override void _Ready()
    {
        // SceneTree is still attaching Bootstrap while _Ready runs. Deferring the
        // transition avoids mutating that tree during its own add-child callback.
        Callable.From(() => { GetTree().ChangeSceneToFile("res://Scenes/PrototypeRTS.tscn"); }).CallDeferred();
    }
}
