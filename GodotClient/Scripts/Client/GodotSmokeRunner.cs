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
    private bool _constructionSeeded;
    private EntityId _captureFocus;
    private EntityId _damagedFocus;
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
        for (int i = 0; i < commandLineArgs.Length; i++) if (commandLineArgs[i] == "--capture-construction") _captureConstruction = true;
    }
    public override void _Process(double delta)
    {
        if (_bridge is null || _finished) return;
        _frames++;
        if (_capturePath is not null && _captureFocus != EntityId.None && _bridge.World.Entities.Transform.TryGet(_captureFocus, out SimTransform focusTransform))
            _camera?.CenterOn(focusTransform.Position.ToWorld());
        if (_frames == 2 && _selection is not null)
        {
            if (_captureConstruction && TrySeedConstructionSite(out EntityId site))
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
            TrySeedDamagedHealth(_captureFocus);
        }
        if (_bridge.World.Tick.Value < 40 || _frames < 60) return;
        Node? hud = GetTree().Root.FindChild("BasicHUD", true, false);
        bool hudOk = hud is not null && hud.FindChild("ResourceStrip", true, false) is not null &&
            hud.FindChild("SelectionPanel", true, false) is not null && hud.FindChild("PortraitSlot", true, false) is not null &&
            hud.FindChild("ContextualSlot", true, false) is not null && hud.FindChild("ContextualActions", true, false) is not null;
        Node? focusedView = _captureFocus == EntityId.None ? null : GetTree().Root.FindChild($"SimEntity_{_captureFocus.Value}_*", true, false);
        Node? damagedView = _damagedFocus == EntityId.None ? null : GetTree().Root.FindChild($"SimEntity_{_damagedFocus.Value}_*", true, false);
        Node? constructionProgress = focusedView?.FindChild("ConstructionProgressBar", false, false);
        Node? movingTargetControl = GetTree().Root.FindChild("MoveEnemyTest", true, false);
        Node? healthBar = damagedView?.FindChild("HealthBar", false, false);
        Node? contactImpact = GetTree().Root.FindChild("ContactImpact", true, false);
        bool healthBarOk = HealthBarGeometryOk(healthBar);
        bool constructionOk = !_captureConstruction || (_constructionSeeded && constructionProgress is Node3D progressBar && progressBar.Visible &&
            ConstructionProgressHeightOk(progressBar, focusedView) &&
            progressBar.FindChild("Background", false, false) is MeshInstance3D constructionBackground &&
            constructionBackground.Mesh is BoxMesh constructionBackgroundMesh &&
            constructionBackgroundMesh.Material is StandardMaterial3D constructionBackgroundMaterial &&
            constructionBackgroundMaterial.BillboardMode == BaseMaterial3D.BillboardModeEnum.Disabled &&
            progressBar.FindChild("Fill", false, false) is MeshInstance3D constructionFill && constructionFill.Mesh is BoxMesh constructionFillMesh &&
            constructionFillMesh.Material is StandardMaterial3D constructionFillMaterial &&
            constructionFillMaterial.BillboardMode == BaseMaterial3D.BillboardModeEnum.Disabled);
        bool ok = _bridge.Current is not null && _bridge.World.Entities.Alive.Count >= 18 && _bridge.GameplayContentHash != 0 && hudOk && constructionOk &&
            movingTargetControl is Button && contactImpact is MeshInstance3D && healthBarOk;
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
        if (!ok)
            GD.PrintErr($"PHASE10 GODOT HEADLESS SMOKE DETAIL: hud={hudOk} construction={constructionOk} health={healthBarOk} movingTarget={movingTargetControl is Button} contact={contactImpact is MeshInstance3D}");
        _finished = true;
        GD.Print(ok ? $"PHASE10 GODOT HEADLESS SMOKE: PASS tick={_bridge.World.Tick.Value} hash={_bridge.StateHashHex()}" : "PHASE10 GODOT HEADLESS SMOKE: FAIL");
        GetTree().Quit(ok ? 0 : 2);
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

    private void TrySeedDamagedHealth(EntityId preferred)
    {
        if (_bridge is null) return;
        EntityId target = preferred;
        if (target == EntityId.None || !_bridge.World.Entities.Health.Has(target))
        {
            IReadOnlyList<EntityId> alive = _bridge.World.Entities.Alive;
            target = EntityId.None;
            for (int i = 0; i < alive.Count; i++)
            {
                EntityId candidate = alive[i];
                if (!_bridge.World.Entities.Health.Has(candidate) ||
                    !_bridge.World.Entities.Transform.TryGet(candidate, out SimTransform transform) ||
                    !_bridge.World.Fog.IsVisible(0, transform.Position.X.FloorToInt(), transform.Position.Y.FloorToInt())) continue;
                target = candidate;
                break;
            }
        }
        if (target == EntityId.None) return;
        ref Health health = ref _bridge.World.Entities.Health.Get(target);
        health.Current = health.Maximum * Fix32.FromRatio(75, 100);
        _damagedFocus = target;
    }

    private static bool HealthBarGeometryOk(Node? healthBar)
    {
        if (healthBar is not Node3D bar || !bar.TopLevel ||
            bar.FindChild("Background", false, false) is not MeshInstance3D background || background.Mesh is not QuadMesh backgroundMesh ||
            backgroundMesh.Material is not StandardMaterial3D barMaterial || barMaterial.BillboardMode != BaseMaterial3D.BillboardModeEnum.Enabled ||
            barMaterial.ShadingMode != BaseMaterial3D.ShadingModeEnum.Unshaded || !barMaterial.NoDepthTest || barMaterial.RenderPriority != 0 ||
            bar.FindChild("Fill", false, false) is not MeshInstance3D fill || fill.Mesh is not QuadMesh fillMesh ||
            fill.MaterialOverride is not StandardMaterial3D fillMaterial || fillMaterial.BillboardMode != BaseMaterial3D.BillboardModeEnum.Enabled ||
            fillMaterial.RenderPriority != 1) return false;
        int[] percentages = { 75, 55, 25 };
        for (int i = 0; i < percentages.Length; i++)
        {
            float ratio = percentages[i] / 100f;
            UnitViewManager.UpdateHealthBarFillGeometry(fill, ratio);
            float expectedWidth = 2.32f * ratio;
            float expectedCenter = -1.16f + expectedWidth * 0.5f;
            if (!Mathf.IsEqualApprox(fillMesh.Size.X, expectedWidth) || !Mathf.IsEqualApprox(fillMesh.CenterOffset.X, expectedCenter) ||
                fill.Position.DistanceSquaredTo(Vector3.Zero) >= 0.000001f ||
                fillMesh.CenterOffset.X - fillMesh.Size.X * 0.5f < -backgroundMesh.Size.X * 0.5f ||
                fillMesh.CenterOffset.X + fillMesh.Size.X * 0.5f > backgroundMesh.Size.X * 0.5f) return false;
        }
        UnitViewManager.UpdateHealthBarFillGeometry(fill, 0.75f);
        return true;
    }

    private static bool ConstructionProgressHeightOk(Node3D progressBar, Node? focusedView)
    {
        if (focusedView is not MeshInstance3D building ||
            !Mathf.IsEqualApprox(progressBar.Position.Y * building.Scale.Y, UnitViewManager.ConstructionProgressHeightWorld)) return false;
        float[] representativeHeights = { 0.35f, 1.20f, 2.40f };
        for (int i = 0; i < representativeHeights.Length; i++)
            if (!Mathf.IsEqualApprox(UnitViewManager.ConstructionProgressLocalY(representativeHeights[i]) * representativeHeights[i],
                    UnitViewManager.ConstructionProgressHeightWorld)) return false;
        return true;
    }
}
