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
    private UnitViewManager? _views;
    private string? _capturePath;
    private bool _captureConstruction;
    private bool _captureRepair;
    private bool _captureTransport;
    private bool _captureTransformation;
    private bool _captureTransformationRollback;
    private bool _captureExcavation;
    private bool _m5Acceptance;
    private bool _constructionSeeded;
    private EntityId _captureFocus;
    private EntityId _damagedFocus;
    private EntityId _destroyedUnit;
    private EntityId _collapseUnit;
    private EntityId _scoutDamageTarget;
    private int _scoutDamageInitialRaw;
    private bool _scoutDamageSeeded;
    private bool _preparedEdgePickObserved;
    private EntityId _repairer;
    private int _repairInitialRaw;
    private bool _repairSeeded;
    private bool _transportSeeded;
    private bool _transformationSeeded;
    private bool _transformationCancelIssued;
    private int _rollbackObservedFrames;
    private bool _finished;
    private int _frames;
    public void Configure(GodotSimBridge bridge, SelectionController selection, RtsCameraController camera, RtsInputController input,
        UnitViewManager views, string[] commandLineArgs)
    {
        _bridge = bridge; _selection = selection; _camera = camera; _input = input; _views = views;
        ProcessPriority = 1000;
        for (int i = 0; i + 1 < commandLineArgs.Length; i++)
        {
            if (commandLineArgs[i] == "--capture-path") _capturePath = commandLineArgs[i + 1];
        }
        for (int i = 0; i < commandLineArgs.Length; i++)
        {
            if (commandLineArgs[i] == "--capture-construction") _captureConstruction = true;
            if (commandLineArgs[i] == "--capture-repair") _captureRepair = true;
            if (commandLineArgs[i] == "--capture-transport") _captureTransport = true;
            if (commandLineArgs[i] == "--capture-transformation") _captureTransformation = true;
            if (commandLineArgs[i] == "--capture-transformation-rollback") { _captureTransformation = true; _captureTransformationRollback = true; }
            if (commandLineArgs[i] == "--capture-excavation") _captureExcavation = true;
            if (commandLineArgs[i] == "--m5-playtest") _m5Acceptance = true;
        }
    }
    public override void _Process(double delta)
    {
        if (_bridge is null || _input is null || _finished) return;
        _frames++;
        if (_capturePath is not null && _captureFocus != EntityId.None && _bridge.World.Entities.Transform.TryGet(_captureFocus, out SimTransform focusTransform))
            _camera?.CenterOn(focusTransform.Position.ToWorld());
        if (_frames == 2)
        {
            if (_m5Acceptance && M5AcceptanceScenarioFactory.TryFindFirst(_bridge.World, M5AcceptanceScenarioFactory.ResonanceCoreKey, out EntityId m5Core))
            {
                _selection?.SetSelection(new[] { m5Core });
                _captureFocus = m5Core;
                if (_bridge.World.Entities.Transform.TryGet(m5Core, out SimTransform coreTransform)) _camera?.CenterOn(coreTransform.Position.ToWorld());
            }
            else if (_captureExcavation && TryOpenExcavatable())
            {
                _selection?.SetSelection(Array.Empty<EntityId>());
            }
            else if (_captureTransformation) _input.DebugPrepareTransformationPlaytest();
            else if (_captureTransport) _input.DebugPrepareTransportPlaytest();
            else if (_captureRepair) _input.DebugPrepareRepairPlaytest();
            else if (_captureConstruction) _input.DebugPrepareConstructionPlaytest();
            else _input.DebugPrepareDestructionPlaytest();
        }
        ResolvePreparedCaptureFocus();
        if (!_captureConstruction && !_captureRepair && !_captureTransport && !_captureTransformation && !_captureExcavation && !_m5Acceptance && !_preparedEdgePickObserved && _captureFocus != EntityId.None)
            _preparedEdgePickObserved = PreparedBuildingEdgePick() == _captureFocus;
        if (_captureTransformation) SeedPreparedTransformation();
        else if (_captureTransport) SeedPreparedTransport();
        else if (_captureRepair) SeedPreparedRepair();
        else if (!_captureConstruction && !_captureExcavation && !_m5Acceptance) { SeedPreparedScoutDamage(); SeedPreparedDestructionStates(); }
        if (_captureTransformationRollback)
        {
            if (!_bridge.World.Entities.Transformation.TryGet(_captureFocus, out Transformation rollbackState) ||
                !_transformationCancelIssued || rollbackState.Phase != TransformationPhase.RollingBack) return;
            _rollbackObservedFrames++;
            if (_rollbackObservedFrames < 2) return;
        }
        int requiredTick = _captureTransport ? 170 : _captureTransformation && !_captureTransformationRollback ? 80 : 120;
        int requiredFrames = _captureTransport ? 190 : _captureTransformation && !_captureTransformationRollback ? 100 : 140;
        if (_captureTransformationRollback) { requiredTick = 0; requiredFrames = 0; }
        if (_bridge.World.Tick.Value < requiredTick || _frames < requiredFrames) return;
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
        Node? prepareRepairControl = GetTree().Root.FindChild("PrepareRepairTest", true, false);
        Node? prepareTransportControl = GetTree().Root.FindChild("PrepareTransportTest", true, false);
        Node? destroyTransportControl = GetTree().Root.FindChild("DestroyTransportTest", true, false);
        Node? prepareTransformationControl = GetTree().Root.FindChild("PrepareTransformationTest", true, false);
        Node? destroyCrewControl = GetTree().Root.FindChild("DestroyCrewTest", true, false);
        Node? destroyChromeControl = GetTree().Root.FindChild("DestroyChromeTest", true, false);
        Node? destroyBuildingControl = GetTree().Root.FindChild("DestroyBuildingTest", true, false);
        MeshInstance3D? standardWreck = _views is not null && _views.TryGetEntityView(_destroyedUnit, out MeshInstance3D standard) ? standard : null;
        MeshInstance3D? activeCollapse = _views is not null && _views.TryGetEntityView(_collapseUnit, out MeshInstance3D collapse) ? collapse : null;
        Node? healthBar = damagedView?.FindChild("HealthBar", false, false);
        Node? contactImpact = GetTree().Root.FindChild("ContactImpact", true, false);
        Label? selectionTitle = GetTree().Root.FindChild("SelectionTitle", true, false) as Label;
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
        bool destructionOk = _captureConstruction || _captureRepair || _captureTransport || _captureTransformation ||
            (_destroyedUnit != EntityId.None && _bridge.World.Entities.Destruction.Has(_destroyedUnit) &&
             !_bridge.World.Entities.Navigation.Has(_destroyedUnit) && standardWreck is MeshInstance3D standardView &&
             TryGetPresentation(_destroyedUnit, out PresentationEntity standardEntity) &&
             standardView.Scale.IsEqualApprox(UnitViewManager.DebrisScale(standardEntity)) &&
             _collapseUnit != EntityId.None && _bridge.World.Entities.Destruction.Has(_collapseUnit) &&
             !_bridge.World.Entities.Navigation.Has(_collapseUnit) && activeCollapse is MeshInstance3D collapseView &&
             TryGetPresentation(_collapseUnit, out PresentationEntity collapseEntity) &&
             collapseView.Scale.IsEqualApprox(UnitViewManager.DebrisScale(collapseEntity)));
        bool preparedTitleOk = _captureConstruction || _captureRepair || _captureTransport || _captureTransformation || selectionTitle?.Text == "Chrome Crusher";
        bool preparedEdgePickOk = _captureConstruction || _captureRepair || _captureTransport || _captureTransformation || _preparedEdgePickObserved;
        bool preparedUiOk = preparedTitleOk && preparedEdgePickOk;
        bool scoutDamageOk = _captureConstruction || _captureRepair || _captureTransport || _captureTransformation || (_scoutDamageSeeded &&
            _bridge.World.Entities.Health.TryGet(_scoutDamageTarget, out Health damagedByScout) &&
            damagedByScout.Current.Raw < _scoutDamageInitialRaw);
        bool repairOk = !_captureRepair || (_repairSeeded && _bridge.World.Entities.Health.TryGet(_captureFocus, out Health repaired) &&
            repaired.Current.Raw > _repairInitialRaw && _bridge.World.Entities.Builder.TryGet(_repairer, out Builder repairBuilder) &&
            repairBuilder.JobState == BuilderJobState.Repairing && _views is not null && _views.TryGetEntityView(_repairer, out MeshInstance3D repairView) &&
            repairView.GetNodeOrNull<Node3D>("RepairEffect") is Node3D repairEffect && repairEffect.Visible);
        bool transportOk = !_captureTransport || (_transportSeeded && _bridge.World.Entities.Transport.TryGet(_captureFocus, out Transport transport) &&
            transport.PassengerCount == 4 && transport.OccupiedPoints == 4 && selectionTitle?.Text == "Rapid Rider" &&
            focusedView?.FindChild("TransportLabel", false, false) is Label3D cargoLabel && cargoLabel.Visible);
        bool transformationCompleteOk = !_captureTransformationRollback && _transformationSeeded && _bridge.World.Entities.Transformation.TryGet(_captureFocus, out Transformation transformation) &&
            _bridge.World.Content.TryGetTransformation(StableId.FromKey(DebugPlaytestScenario.Mx41Key), out TransformationDefinition transformationDefinition) &&
            transformation.CurrentState == transformationDefinition.ModeB.StateId && transformation.Phase == TransformationPhase.Idle &&
            _bridge.World.Entities.Navigation.Get(_captureFocus).Layer == MovementLayer.TrueAir && selectionTitle?.Text == "MX-41 Switch Fighter" &&
            focusedView?.FindChild("TransformationLabel", false, false) is Label3D transformationLabel && transformationLabel.Visible;
        bool transformationRollbackOk = _captureTransformationRollback && _transformationCancelIssued &&
            _bridge.World.Entities.Transformation.TryGet(_captureFocus, out Transformation rollback) && rollback.Phase == TransformationPhase.RollingBack &&
            TransformationSystem.ProgressBasisPoints(rollback) is > 0 and < 4_000 && focusedView is MeshInstance3D rollbackView &&
            rollbackView.Position.Y is > 0.03f and < 1.57f &&
            focusedView.FindChild("TransformationLabel", false, false) is Label3D rollbackLabel && rollbackLabel.Visible && rollbackLabel.Text.StartsWith("CANCELLING") &&
            focusedView.FindChild("TransformationProgressBar", false, false) is Node3D rollbackBar && rollbackBar.Visible;
        bool transformationOk = !_captureTransformation || transformationCompleteOk || transformationRollbackOk;
        int minimumAlive = _m5Acceptance ? 18 : _captureConstruction ? 15 : _captureRepair ? 16 : _captureTransport ? 16 : _captureTransformation ? 16 : 17;
        bool controlsOk = movingTargetControl is Button && prepareConstructionControl is Button && prepareDestructionControl is Button && prepareRepairControl is Button &&
            prepareTransportControl is Button && destroyTransportControl is Button && prepareTransformationControl is Button &&
            destroyCrewControl is Button && destroyChromeControl is Button && destroyBuildingControl is Button;
        bool excavationOk = !_captureExcavation || ExcavationIsOpen();
        string m5Reason = string.Empty;
        bool m5Ok = !_m5Acceptance || (M5AcceptanceScenarioFactory.IsFreshHandoffReady(_bridge.World, out m5Reason) &&
            GetTree().Root.FindChild("M5AcceptancePanel", true, false) is not null && GetTree().Root.FindChild("M5AcceptanceStatus", true, false) is not null);
        if (!m5Ok && _m5Acceptance) GD.PrintErr($"M5 ACCEPTANCE SMOKE: FAIL {m5Reason}");
        bool m4VisualOk = _captureExcavation || _m5Acceptance || (destructionOk && preparedUiOk && scoutDamageOk && repairOk && transportOk && transformationOk && contactImpact is MeshInstance3D && healthBarOk);
        bool ok = _bridge.Current is not null && _bridge.World.Entities.Alive.Count >= minimumAlive && _bridge.GameplayContentHash != 0 &&
            hudOk && constructionOk && controlsOk && excavationOk && m5Ok && m4VisualOk;
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
            GD.PrintErr($"PHASE10 GODOT HEADLESS SMOKE DETAIL: hud={hudOk} construction={constructionOk} health={healthBarOk} controls={controlsOk} destruction={destructionOk} preparedTitle={preparedTitleOk} preparedEdgePick={preparedEdgePickOk} scoutDamage={scoutDamageOk} repair={repairOk} transport={transportOk} transformation={transformationOk} excavation={excavationOk} m5={m5Ok} standardWreck={standardWreck is MeshInstance3D} collapseId={_collapseUnit.Value} collapseActive={_collapseUnit != EntityId.None && _bridge.World.Entities.Destruction.Has(_collapseUnit)} collapseView={activeCollapse is MeshInstance3D} contact={contactImpact is MeshInstance3D}");
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

    private void ResolvePreparedCaptureFocus()
    {
        if (_bridge is null || _captureFocus != EntityId.None) return;
        IReadOnlyList<EntityId> alive = _bridge.World.Entities.Alive;
        if (_captureTransformation)
        {
            ContentId mx41Type = StableId.FromKey(DebugPlaytestScenario.Mx41Key);
            for (int i = 0; i < alive.Count; i++)
            {
                EntityId id = alive[i];
                if (_bridge.World.Entities.Selectable.TryGet(id, out Selectable selectable) && selectable.ContentType == mx41Type &&
                    _bridge.World.Entities.Ownership.TryGet(id, out Ownership owner) && owner.PlayerSlot == 0 && _bridge.World.Entities.Transformation.Has(id))
                { _captureFocus = id; _damagedFocus = id; break; }
            }
            return;
        }
        if (_captureTransport)
        {
            ContentId riderType = StableId.FromKey(DebugPlaytestScenario.RapidRiderKey);
            for (int i = 0; i < alive.Count; i++)
            {
                EntityId id = alive[i];
                if (_bridge.World.Entities.Selectable.TryGet(id, out Selectable selectable) && selectable.ContentType == riderType &&
                    _bridge.World.Entities.Ownership.TryGet(id, out Ownership owner) && owner.PlayerSlot == 0 && _bridge.World.Entities.Transport.Has(id))
                { _captureFocus = id; _damagedFocus = id; break; }
            }
            return;
        }
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
        if (_captureRepair)
        {
            ContentId hoverType = StableId.FromKey(DebugPlaytestScenario.HoverScoutKey);
            for (int i = 0; i < alive.Count; i++)
            {
                EntityId id = alive[i];
                if (_bridge.World.Entities.Selectable.TryGet(id, out Selectable selectable) && selectable.ContentType == hoverType &&
                    _bridge.World.Entities.Ownership.TryGet(id, out Ownership owner) && owner.PlayerSlot == 0 &&
                    _bridge.World.Entities.Health.TryGet(id, out Health health) && health.Current < health.Maximum)
                { _captureFocus = id; _damagedFocus = id; break; }
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

    private void SeedPreparedTransformation()
    {
        if (_bridge is null || _input is null || _captureFocus == EntityId.None ||
            !_bridge.World.Entities.Transformation.TryGet(_captureFocus, out Transformation state)) return;
        if (!_transformationSeeded && state.Phase == TransformationPhase.Idle)
        {
            _selection?.SetSelection(new[] { _captureFocus });
            _input.StateChangeSelected();
            _transformationSeeded = true;
            return;
        }
        if (_captureTransformationRollback && !_transformationCancelIssued && state.Phase == TransformationPhase.Transitioning && state.ProgressTicks >= 10)
        {
            _input.StopSelected();
            _transformationCancelIssued = true;
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

    private void SeedPreparedScoutDamage()
    {
        if (_bridge is null || _scoutDamageSeeded || _captureFocus == EntityId.None || _bridge.World.Tick.Value < 3 ||
            !_bridge.World.Entities.Health.TryGet(_captureFocus, out Health targetHealth)) return;
        ContentId hoverType = StableId.FromKey(DebugPlaytestScenario.HoverScoutKey);
        IReadOnlyList<EntityId> alive = _bridge.World.Entities.Alive;
        for (int i = 0; i < alive.Count; i++)
        {
            EntityId id = alive[i];
            if (!_bridge.World.Entities.Selectable.TryGet(id, out Selectable selectable) || selectable.ContentType != hoverType ||
                !_bridge.World.Entities.Ownership.TryGet(id, out Ownership owner) || owner.PlayerSlot != 0) continue;
            _scoutDamageTarget = _captureFocus;
            _scoutDamageInitialRaw = targetHealth.Current.Raw;
            _bridge.Enqueue(new CommandEnvelope(_bridge.World.Tick.Next(), 0, uint.MaxValue - 1, SimCommandType.Attack,
                new[] { id }, FixVec2.Zero, targetEntity: _captureFocus));
            _scoutDamageSeeded = true;
            return;
        }
    }

    private void SeedPreparedRepair()
    {
        if (_bridge is null || _repairSeeded || _captureFocus == EntityId.None || _bridge.World.Tick.Value < 3 ||
            !_bridge.World.Entities.Health.TryGet(_captureFocus, out Health health)) return;
        ContentId crewType = StableId.FromKey(DebugPlaytestScenario.CrewKey);
        IReadOnlyList<EntityId> alive = _bridge.World.Entities.Alive;
        for (int i = 0; i < alive.Count; i++)
        {
            EntityId id = alive[i];
            if (!_bridge.World.Entities.Selectable.TryGet(id, out Selectable selectable) || selectable.ContentType != crewType ||
                !_bridge.World.Entities.Ownership.TryGet(id, out Ownership owner) || owner.PlayerSlot != 0) continue;
            _repairer = id; _repairInitialRaw = health.Current.Raw;
            _bridge.Enqueue(new CommandEnvelope(_bridge.World.Tick.Next(), 0, uint.MaxValue - 2, SimCommandType.Repair,
                new[] { id }, FixVec2.Zero, targetEntity: _captureFocus));
            _repairSeeded = true; return;
        }
    }

    private void SeedPreparedTransport()
    {
        if (_bridge is null || _selection is null || _transportSeeded || _captureFocus == EntityId.None || _bridge.World.Tick.Value < 3) return;
        List<EntityId> crews = new();
        IReadOnlyList<EntityId> alive = _bridge.World.Entities.Alive;
        for (int i = 0; i < alive.Count; i++)
        {
            EntityId id = alive[i];
            if (_bridge.World.Entities.Passenger.TryGet(id, out Passenger passenger) && passenger.State == PassengerState.Grounded &&
                _bridge.World.Entities.Ownership.TryGet(id, out Ownership owner) && owner.PlayerSlot == 0 &&
                _bridge.World.Entities.Transform.TryGet(id, out SimTransform transform) &&
                FixVec2.Distance(transform.Position, DebugPlaytestScenario.TransportArenaCenter) <= Fix32.FromInt(20)) crews.Add(id);
        }
        crews.Sort(static (a, b) => a.Value.CompareTo(b.Value));
        if (crews.Count < 4) return;
        _bridge.Enqueue(new CommandEnvelope(_bridge.World.Tick.Next(), 0, uint.MaxValue - 3, SimCommandType.Load,
            crews.Take(4).ToArray(), FixVec2.Zero, targetEntity: _captureFocus));
        _selection.SetSelection(new[] { _captureFocus });
        _transportSeeded = true;
    }

    private bool TryGetPresentation(EntityId id, out PresentationEntity entity)
    {
        if (_bridge?.Current is not null)
            for (int i = 0; i < _bridge.Current.Entities.Count; i++)
                if (_bridge.Current.Entities[i].EntityId == id) { entity = _bridge.Current.Entities[i]; return true; }
        entity = default;
        return false;
    }

    private EntityId PreparedBuildingEdgePick()
    {
        if (_selection is null || _camera is null || !TryGetPresentation(_captureFocus, out PresentationEntity building) ||
            building.SelectableKind != SelectableKind.Building) return EntityId.None;
        FixVec2 nearEdge = building.Position + new FixVec2(Fix32.FromRatio(building.BuildingWidth * 9, 20), Fix32.Zero);
        Vector2 screen = _camera.UnprojectPosition(nearEdge.ToWorld(0.5f));
        return _selection.FindVisibleEnemyAtScreen(screen);
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
