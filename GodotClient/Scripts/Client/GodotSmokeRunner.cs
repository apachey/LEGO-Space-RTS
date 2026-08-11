using Godot;
using LegoSpaceRTS.Presentation;
using LegoSpaceRTS.SimCore;

namespace LegoSpaceRTS.Client;

public partial class GodotSmokeRunner : Node
{
    private GodotSimBridge? _bridge;
    private SelectionController? _selection;
    private RtsCameraController? _camera;
    private RtsInputController? _input;
    private string? _capturePath;
    private bool _captureConstruction;
    private bool _constructionSeeded;
    private EntityId _captureFocus;
    private EntityId _damagedFocus;
    private EntityId _destroyedUnit;
    private EntityId _collapseUnit;
    private bool _finished;
    private int _frames;
    public void Configure(GodotSimBridge bridge, SelectionController selection, RtsCameraController camera, RtsInputController input, string[] commandLineArgs)
    {
        _bridge = bridge; _selection = selection; _camera = camera; _input = input;
        ProcessPriority = 1000;
        for (int i = 0; i + 1 < commandLineArgs.Length; i++)
        {
            if (commandLineArgs[i] == "--capture-path") _capturePath = commandLineArgs[i + 1];
        }
        for (int i = 0; i < commandLineArgs.Length; i++) if (commandLineArgs[i] == "--capture-construction") _captureConstruction = true;
    }
    public override void _Process(double delta)
    {
        if (_bridge is null || _input is null || _finished) return;
        _frames++;
        if (_capturePath is not null && _captureFocus != EntityId.None && _bridge.World.Entities.Transform.TryGet(_captureFocus, out SimTransform focusTransform))
            _camera?.CenterOn(focusTransform.Position.ToWorld());
        if (_frames == 2)
        {
            if (_captureConstruction) _input.DebugPrepareConstructionPlaytest();
            else _input.DebugPrepareDestructionPlaytest();
        }
        ResolvePreparedCaptureFocus();
        if (!_captureConstruction) SeedPreparedDestructionStates();
        if (_bridge.World.Tick.Value < 40 || _frames < 60) return;
        Node? hud = GetTree().Root.FindChild("BasicHUD", true, false);
        bool hudOk = hud is not null && hud.FindChild("ResourceStrip", true, false) is not null &&
            hud.FindChild("SelectionPanel", true, false) is not null && hud.FindChild("PortraitSlot", true, false) is not null &&
            hud.FindChild("ContextualSlot", true, false) is not null && hud.FindChild("ContextualActions", true, false) is not null;
        Node? focusedView = _captureFocus == EntityId.None ? null : GetTree().Root.FindChild($"SimEntity_{_captureFocus.Value}_*", true, false);
        Node? damagedView = _damagedFocus == EntityId.None ? null : GetTree().Root.FindChild($"SimEntity_{_damagedFocus.Value}_*", true, false);
        Node? constructionProgress = focusedView?.FindChild("ConstructionProgressBar", false, false);
        Node? constructionHealth = focusedView?.FindChild("HealthBar", false, false);
        Node? movingTargetControl = GetTree().Root.FindChild("MoveEnemyTest", true, false);
        Node? prepareConstructionControl = GetTree().Root.FindChild("PrepareConstructionTest", true, false);
        Node? prepareDestructionControl = GetTree().Root.FindChild("PrepareDestructionTest", true, false);
        Node? destroyCrewControl = GetTree().Root.FindChild("DestroyCrewTest", true, false);
        Node? destroyChromeControl = GetTree().Root.FindChild("DestroyChromeTest", true, false);
        Node? destroyBuildingControl = GetTree().Root.FindChild("DestroyBuildingTest", true, false);
        Node? debris = _destroyedUnit == EntityId.None ? null : GetTree().Root.FindChild($"Debris_{_destroyedUnit.Value}", true, false);
        Node? activeCollapse = _collapseUnit == EntityId.None ? null : GetTree().Root.FindChild($"SimEntity_{_collapseUnit.Value}_*", true, false);
        Node? healthBar = damagedView?.FindChild("HealthBar", false, false);
        Node? contactImpact = GetTree().Root.FindChild("ContactImpact", true, false);
        bool healthBarOk = HealthBarGeometryOk(healthBar);
        bool constructionOk = !_captureConstruction || (_constructionSeeded && constructionProgress is Node3D progressBar && progressBar.Visible &&
            constructionHealth is Node3D siteHealth && !siteHealth.Visible &&
            ConstructionProgressHeightOk(progressBar, focusedView) &&
            progressBar.FindChild("Background", false, false) is MeshInstance3D constructionBackground &&
            constructionBackground.Mesh is BoxMesh constructionBackgroundMesh &&
            constructionBackgroundMesh.Material is StandardMaterial3D constructionBackgroundMaterial &&
            constructionBackgroundMaterial.BillboardMode == BaseMaterial3D.BillboardModeEnum.Disabled &&
            progressBar.FindChild("Fill", false, false) is MeshInstance3D constructionFill && constructionFill.Mesh is BoxMesh constructionFillMesh &&
            constructionFillMesh.Material is StandardMaterial3D constructionFillMaterial &&
            constructionFillMaterial.BillboardMode == BaseMaterial3D.BillboardModeEnum.Disabled);
        bool destructionOk = _captureConstruction ||
            (_destroyedUnit != EntityId.None && !_bridge.World.Entities.Exists(_destroyedUnit) && debris is MeshInstance3D &&
             _collapseUnit != EntityId.None && _bridge.World.Entities.Destruction.Has(_collapseUnit) &&
             activeCollapse is MeshInstance3D collapseView && TryGetPresentation(_collapseUnit, out PresentationEntity collapseEntity) &&
             collapseView.Scale.IsEqualApprox(UnitViewManager.BaseVisualScale(collapseEntity)));
        int minimumAlive = _captureConstruction ? 15 : 17;
        bool controlsOk = movingTargetControl is Button && prepareConstructionControl is Button && prepareDestructionControl is Button &&
            destroyCrewControl is Button && destroyChromeControl is Button && destroyBuildingControl is Button;
        bool ok = _bridge.Current is not null && _bridge.World.Entities.Alive.Count >= minimumAlive && _bridge.GameplayContentHash != 0 &&
            hudOk && constructionOk && destructionOk && controlsOk && contactImpact is MeshInstance3D && healthBarOk;
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
            GD.PrintErr($"PHASE10 GODOT HEADLESS SMOKE DETAIL: hud={hudOk} construction={constructionOk} health={healthBarOk} controls={controlsOk} destruction={destructionOk} debris={debris is MeshInstance3D} collapseId={_collapseUnit.Value} collapseActive={_collapseUnit != EntityId.None && _bridge.World.Entities.Destruction.Has(_collapseUnit)} collapseView={activeCollapse is MeshInstance3D} contact={contactImpact is MeshInstance3D}");
        _finished = true;
        GD.Print(ok ? $"PHASE10 GODOT HEADLESS SMOKE: PASS tick={_bridge.World.Tick.Value} hash={_bridge.StateHashHex()}" : "PHASE10 GODOT HEADLESS SMOKE: FAIL");
        GetTree().Quit(ok ? 0 : 2);
    }

    private void ResolvePreparedCaptureFocus()
    {
        if (_bridge is null || _captureFocus != EntityId.None) return;
        IReadOnlyList<EntityId> alive = _bridge.World.Entities.Alive;
        if (_captureConstruction)
        {
            for (int i = 0; i < alive.Count; i++)
            {
                EntityId id = alive[i];
                if (!_bridge.World.Entities.ConstructionSite.Has(id) ||
                    !_bridge.World.Entities.Ownership.TryGet(id, out Ownership owner) || owner.PlayerSlot != 0) continue;
                _captureFocus = id;
                _constructionSeeded = true;
                break;
            }
            if (_captureFocus == EntityId.None) return;
            for (int i = 0; i < alive.Count; i++)
            {
                EntityId id = alive[i];
                if (!_bridge.World.Entities.Building.TryGet(id, out Building building) || building.State != BuildingState.Completed ||
                    !_bridge.World.Entities.Ownership.TryGet(id, out Ownership owner) || owner.PlayerSlot != 0 || !_bridge.World.Entities.Health.Has(id)) continue;
                ref Health health = ref _bridge.World.Entities.Health.Get(id);
                health.Current = health.Maximum * Fix32.FromRatio(75, 100);
                health.LastDamageTick = _bridge.World.Tick.Value;
                _damagedFocus = id;
                break;
            }
            return;
        }
        ContentId buildingType = StableId.FromKey(DebugPlaytestScenario.DestructionBuildingKey);
        for (int i = 0; i < alive.Count; i++)
        {
            EntityId id = alive[i];
            if (!_bridge.World.Entities.Selectable.TryGet(id, out Selectable selectable) || selectable.ContentType != buildingType ||
                !_bridge.World.Entities.Ownership.TryGet(id, out Ownership owner) || owner.PlayerSlot == 0 || !_bridge.World.Entities.Building.Has(id)) continue;
            _captureFocus = id;
            _damagedFocus = id;
        }
    }

    private void SeedPreparedDestructionStates()
    {
        if (_bridge is null) return;
        IReadOnlyList<EntityId> alive = _bridge.World.Entities.Alive;
        if (_destroyedUnit == EntityId.None && _bridge.World.Tick.Value >= 3)
        {
            ContentId crewType = StableId.FromKey(DebugPlaytestScenario.CrewKey);
            for (int i = 0; i < alive.Count; i++)
            {
                EntityId id = alive[i];
                if (!_bridge.World.Entities.Ownership.TryGet(id, out Ownership owner) || owner.PlayerSlot == 0 ||
                    !_bridge.World.Entities.Selectable.TryGet(id, out Selectable selectable) || selectable.ContentType != crewType ||
                    !_bridge.World.Entities.Health.Has(id) || !_bridge.World.Entities.Transform.TryGet(id, out SimTransform transform) ||
                    FixVec2.Distance(transform.Position, DebugPlaytestScenario.DestructionArenaCenter) > Fix32.FromInt(20)) continue;
                ref Health health = ref _bridge.World.Entities.Health.Get(id);
                health.Current = Fix32.Zero;
                health.LastDamageTick = _bridge.World.Tick.Value;
                _destroyedUnit = id;
                break;
            }
        }
        if (_collapseUnit == EntityId.None && _bridge.World.Tick.Value >= 20)
        {
            ContentId chromeType = StableId.FromKey(DebugPlaytestScenario.ChromeCrusherKey);
            for (int i = 0; i < alive.Count; i++)
            {
                EntityId id = alive[i];
                if (!_bridge.World.Entities.Ownership.TryGet(id, out Ownership owner) || owner.PlayerSlot == 0 ||
                    !_bridge.World.Entities.Selectable.TryGet(id, out Selectable selectable) || selectable.ContentType != chromeType ||
                    !_bridge.World.Entities.Health.Has(id) || !_bridge.World.Entities.Transform.TryGet(id, out SimTransform transform) ||
                    FixVec2.Distance(transform.Position, DebugPlaytestScenario.DestructionArenaCenter) > Fix32.FromInt(20)) continue;
                ref Health health = ref _bridge.World.Entities.Health.Get(id);
                health.Current = Fix32.Zero;
                health.LastDamageTick = _bridge.World.Tick.Value;
                _collapseUnit = id;
                break;
            }
        }
    }

    private bool TryGetPresentation(EntityId id, out PresentationEntity entity)
    {
        if (_bridge?.Current is not null)
            for (int i = 0; i < _bridge.Current.Entities.Count; i++)
                if (_bridge.Current.Entities[i].EntityId == id) { entity = _bridge.Current.Entities[i]; return true; }
        entity = default;
        return false;
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
