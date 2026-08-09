using Godot;
using LegoSpaceRTS.Presentation;

namespace LegoSpaceRTS.Client;

public partial class GodotSmokeRunner : Node
{
    private GodotSimBridge? _bridge;
    private int _frames;
    public void Configure(GodotSimBridge bridge) { _bridge = bridge; ProcessPriority = 1000; }
    public override void _Process(double delta)
    {
        if (_bridge is null) return;
        _frames++;
        if (_bridge.World.Tick.Value < 40 || _frames < 60) return;
        bool ok = _bridge.Current is not null && _bridge.World.Entities.Alive.Count >= 18 && _bridge.GameplayContentHash != 0;
        GD.Print(ok ? $"PHASE10 GODOT HEADLESS SMOKE: PASS tick={_bridge.World.Tick.Value} hash={_bridge.StateHashHex()}" : "PHASE10 GODOT HEADLESS SMOKE: FAIL");
        GetTree().Quit(ok ? 0 : 2);
    }
}
