using Godot;
using LegoSpaceRTS.Presentation;
using LegoSpaceRTS.SimCore;

namespace LegoSpaceRTS.Client;

public partial class GodotSmokeRunner : Node
{
    private GodotSimBridge? _bridge;
    private SelectionController? _selection;
    private string? _capturePath;
    private bool _finished;
    private int _frames;
    public void Configure(GodotSimBridge bridge, SelectionController selection, string[] commandLineArgs)
    {
        _bridge = bridge; _selection = selection;
        ProcessPriority = 1000;
        for (int i = 0; i + 1 < commandLineArgs.Length; i++)
        {
            if (commandLineArgs[i] == "--capture-path") _capturePath = commandLineArgs[i + 1];
        }
    }
    public override void _Process(double delta)
    {
        if (_bridge is null || _finished) return;
        _frames++;
        if (_frames == 2 && _selection is not null)
        {
            IReadOnlyList<EntityId> alive = _bridge.World.Entities.Alive;
            for (int i = 0; i < alive.Count; i++)
            {
                EntityId id = alive[i];
                if (_bridge.World.Entities.Production.Has(id) && _bridge.World.Entities.Ownership.TryGet(id, out Ownership ownership) && ownership.PlayerSlot == 0)
                { _selection.SetSelection(new[] { id }); break; }
            }
        }
        if (_bridge.World.Tick.Value < 40 || _frames < 60) return;
        Node? hud = GetTree().Root.FindChild("BasicHUD", true, false);
        bool hudOk = hud is not null && hud.FindChild("ResourceStrip", true, false) is not null && hud.FindChild("SelectionPanel", true, false) is not null && hud.FindChild("CommandPanel", true, false) is not null;
        bool ok = _bridge.Current is not null && _bridge.World.Entities.Alive.Count >= 18 && _bridge.GameplayContentHash != 0 && hudOk;
        if (ok && _capturePath is not null)
        {
            _finished = true;
            string? directory = Path.GetDirectoryName(_capturePath);
            if (!string.IsNullOrEmpty(directory)) Directory.CreateDirectory(directory);
            Image? image = GetViewport().GetTexture().GetImage();
            if (image is null)
            {
                GD.PrintErr("PHASE10 VISUAL SMOKE CAPTURE: SKIPPED renderer did not expose a viewport texture");
                GetTree().Quit(77);
                return;
            }
            Error captureResult = image.SavePng(_capturePath);
            if (captureResult != Error.Ok)
            {
                GD.PrintErr($"PHASE10 VISUAL SMOKE CAPTURE: FAIL error={captureResult} path={_capturePath}");
                GetTree().Quit(3);
                return;
            }
            GD.Print($"PHASE10 VISUAL SMOKE CAPTURE: PASS path={_capturePath}");
        }
        _finished = true;
        GD.Print(ok ? $"PHASE10 GODOT HEADLESS SMOKE: PASS tick={_bridge.World.Tick.Value} hash={_bridge.StateHashHex()}" : "PHASE10 GODOT HEADLESS SMOKE: FAIL");
        GetTree().Quit(ok ? 0 : 2);
    }
}
