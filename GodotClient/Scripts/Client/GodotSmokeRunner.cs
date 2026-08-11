using Godot;
using LegoSpaceRTS.Presentation;
using LegoSpaceRTS.SimCore;

namespace LegoSpaceRTS.Client;

public partial class GodotSmokeRunner : Node
{
    private GodotSimBridge? _bridge;
    private SelectionController? _selection;
    private RtsCameraController? _camera;
    private string? _capturePath;
    private bool _captureConstruction;
    private bool _captureExcavation;
    private bool _constructionSeeded;
    private EntityId _captureFocus;
    private bool _finished;
    private int _frames;
    public void Configure(GodotSimBridge bridge, SelectionController selection, RtsCameraController camera, string[] commandLineArgs)
    {
        _bridge = bridge; _selection = selection; _camera = camera;
        ProcessPriority = 1000;
        for (int i = 0; i + 1 < commandLineArgs.Length; i++)
        {
            if (commandLineArgs[i] == "--capture-path") _capturePath = commandLineArgs[i + 1];
        }
        for (int i = 0; i < commandLineArgs.Length; i++)
        {
            if (commandLineArgs[i] == "--capture-construction") _captureConstruction = true;
            if (commandLineArgs[i] == "--capture-excavation") _captureExcavation = true;
        }
    }
    public override void _Process(double delta)
    {
        if (_bridge is null || _finished) return;
        _frames++;
        if (_capturePath is not null && _captureFocus != EntityId.None && _bridge.World.Entities.Transform.TryGet(_captureFocus, out SimTransform focusTransform))
            _camera?.CenterOn(focusTransform.Position.ToWorld());
        if (_frames == 2 && _selection is not null)
        {
            if (_captureExcavation && TryOpenExcavatable())
            {
                _selection.SetSelection(Array.Empty<EntityId>());
            }
            else if (_captureConstruction && TrySeedConstructionSite(out EntityId site))
            {
                _constructionSeeded = true;
                _captureFocus = site;
                _selection.SetSelection(new[] { site });
                if (_bridge.World.Entities.Transform.TryGet(site, out SimTransform siteTransform)) _camera?.CenterOn(siteTransform.Position.ToWorld());
            }
            else
            {
                IReadOnlyList<EntityId> alive = _bridge.World.Entities.Alive;
                for (int i = 0; i < alive.Count; i++)
                {
                    EntityId id = alive[i];
                    if (_bridge.World.Entities.Production.Has(id) && _bridge.World.Entities.Ownership.TryGet(id, out Ownership ownership) && ownership.PlayerSlot == 0)
                    { _selection.SetSelection(new[] { id }); _captureFocus = id; break; }
                }
            }
        }
        if (_bridge.World.Tick.Value < 40 || _frames < 60) return;
        Node? hud = GetTree().Root.FindChild("BasicHUD", true, false);
        bool hudOk = hud is not null && hud.FindChild("ResourceStrip", true, false) is not null &&
            hud.FindChild("SelectionPanel", true, false) is not null && hud.FindChild("PortraitSlot", true, false) is not null &&
            hud.FindChild("ContextualSlot", true, false) is not null && hud.FindChild("ContextualActions", true, false) is not null;
        Node? constructionProgress = GetTree().Root.FindChild("ConstructionProgressBar", true, false);
        bool constructionOk = !_captureConstruction || (_constructionSeeded && constructionProgress is Node3D progressBar && progressBar.Visible);
        bool excavationOk = !_captureExcavation || ExcavationIsOpen();
        bool ok = _bridge.Current is not null && _bridge.World.Entities.Alive.Count >= 18 && _bridge.GameplayContentHash != 0 && hudOk && constructionOk && excavationOk;
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

    private bool TryOpenExcavatable()
    {
        if (_bridge is null || _bridge.World.Map.Features.Count == 0) return false;
        ExcavatableFeature feature = _bridge.World.Map.Features[0];
        _bridge.Enqueue(new CommandEnvelope(_bridge.World.Tick.Next(), 0, uint.MaxValue - 1,
            SimCommandType.DebugOpenExcavatable, Array.Empty<EntityId>(), FixVec2.Zero, debugFeatureId: feature.FeatureId));
        float centerX = feature.NavRect.X + feature.NavRect.Width * 0.5f;
        float centerZ = feature.NavRect.Y + feature.NavRect.Height * 0.5f;
        _camera?.CenterOn(new Vector3(centerX, 0f, centerZ));
        if (GetTree().Root.FindChild("DebugRenderer", true, false) is DebugRenderer debug) debug.DrawExcavatable = true;
        return true;
    }

    private bool ExcavationIsOpen()
    {
        if (_bridge is null || _bridge.World.Map.Features.Count == 0) return false;
        ExcavatableFeature feature = _bridge.World.Map.Features[0];
        return feature.State == ExcavatableFeatureState.Open &&
            ExcavationTopologySystem.TryGetFeatureEntity(_bridge.World, feature.FeatureId, out EntityId entity) &&
            _bridge.World.Entities.Excavatable.Get(entity).State == ExcavatableFeatureState.Open;
    }

    private bool TrySeedConstructionSite(out EntityId site)
    {
        site = EntityId.None;
        if (_bridge is null) return false;
        List<EntityId> builders = new();
        IReadOnlyList<EntityId> alive = _bridge.World.Entities.Alive;
        for (int i = 0; i < alive.Count; i++)
            if (_bridge.World.Entities.Builder.Has(alive[i]) && _bridge.World.Entities.Ownership.TryGet(alive[i], out Ownership ownership) && ownership.PlayerSlot == 0)
            { builders.Add(alive[i]); break; }
        if (builders.Count == 0) return false;
        ContentId buildingType = StableId.FromKey("building.rock_raiders.ore_processing_plant");
        const int preferredAnchorX = 21;
        const int preferredAnchorY = 71;
        for (int radius = 0; radius <= 28; radius++) for (int dy = -radius; dy <= radius; dy++) for (int dx = -radius; dx <= radius; dx++)
        {
            if (Math.Max(Math.Abs(dx), Math.Abs(dy)) != radius) continue;
            short x = checked((short)(preferredAnchorX + dx));
            short y = checked((short)(preferredAnchorY + dy));
            if (!ConstructionPlacement.TryPlace(_bridge.World, 0, builders, buildingType, x, y, 0, out site, out _)) continue;
            ref ConstructionSite construction = ref _bridge.World.Entities.ConstructionSite.Get(site);
            construction.ProgressTicks = checked((ushort)Math.Max(1, construction.RequiredTicks * 14 / 100));
            return true;
        }
        return false;
    }
}
