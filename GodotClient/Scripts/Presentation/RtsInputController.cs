using Godot;
using LegoSpaceRTS.SimCore;

namespace LegoSpaceRTS.Presentation;

public partial class RtsInputController : Node
{
    private GodotSimBridge? _bridge;
    private SelectionController? _selection;
    private RtsCameraController? _camera;
    private readonly ControlGroups _groups = new();
    private readonly double[] _lastGroupTap = new double[10];
    private readonly List<Node3D> _movePreviewMarkers = new(16);
    private EntityId[] _previewEntities = Array.Empty<EntityId>();
    private static readonly string[] BuildKeys =
    {
        "building.rock_raiders.ore_processing_plant",
        "building.rock_raiders.power_station",
        "building.rock_raiders.vehicle_service_bay",
        "building.rock_raiders.hq"
    };
    private bool _buildMode;
    private int _buildIndex;
    private byte _buildOrientation;
    private MeshInstance3D? _buildGhost;
    private StandardMaterial3D? _buildGhostMaterial;
    private string _buildStatus = "Off";
    private uint _sequence = 1;
    private readonly Dictionary<uint, uint> _productionRoundRobin = new();

    public ControlGroups Groups => _groups;
    public bool BuildModeActive => _buildMode;
    public string BuildStatus => _buildStatus;

    public void Configure(GodotSimBridge bridge, SelectionController selection, RtsCameraController camera)
    {
        _bridge = bridge; _selection = selection; _camera = camera;
        ProcessPriority = 10;
    }

    public override void _Process(double delta)
    {
        if (_bridge is null || _selection is null || _camera is null) return;
        double now = Time.GetTicksMsec() / 1000.0;
        if (_movePreviewMarkers.Count > 0 && PreviewGroupFinished()) ClearMovePreviews();

        if (Input.IsActionJustPressed("build_toggle")) { if (_buildMode) ExitBuildMode(); else EnterBuildMode(); }
        if (_buildMode && Input.IsActionJustPressed("build_cycle")) CycleBuilding();
        if (_buildMode && Input.IsActionJustPressed("build_rotate")) RotateBuilding();
        if (Input.IsActionJustPressed("build_cancel_recent") && Input.IsKeyPressed(Key.Ctrl)) CancelMostRecentSite();
        if (_buildMode) UpdateBuildPreview();

        if (Input.IsActionJustPressed("command_move") && _selection.Selected.Count > 0 && _camera.TryProjectToGround(GetViewport().GetMousePosition(), out Vector3 movePoint)) IssueMove(movePoint);
        if (Input.IsActionJustPressed("command_stop") && _selection.Selected.Count > 0 && !Input.IsKeyPressed(Key.Ctrl)) { IssueSimple(SimCommandType.Stop); ClearMovePreviews(); }
        if (Input.IsActionJustPressed("command_hold") && _selection.Selected.Count > 0) { IssueSimple(SimCommandType.HoldPosition); ClearMovePreviews(); }
        if (Input.IsActionJustPressed("debug_open_excavatable"))
            _bridge.Enqueue(new CommandEnvelope(_bridge.World.Tick.Next(), 0, _sequence++, SimCommandType.DebugOpenExcavatable, Array.Empty<EntityId>(), FixVec2.Zero, debugFeatureId: DevMapFactory.ExcavatableFeatureId));

        if (_buildMode) return;
        for (int i = 0; i < 10; i++)
        {
            if (!Input.IsActionJustPressed($"group_{i}")) continue;
            if (Input.IsKeyPressed(Key.Ctrl)) _groups.Assign(i, _selection.Selected);
            else if (Input.IsKeyPressed(Key.Shift)) _groups.Add(i, _selection.Selected);
            else if (Input.IsKeyPressed(Key.Alt)) _groups.Remove(i, _selection.Selected);
            else
            {
                IReadOnlyList<EntityId> recalled = _groups.Recall(i, _bridge.World);
                _selection.SetSelection(recalled);
                if (now - _lastGroupTap[i] <= 0.35 && recalled.Count > 0)
                {
                    FixVec2 sum = FixVec2.Zero; int count = 0;
                    for (int e = 0; e < recalled.Count; e++) if (_bridge.World.Entities.Transform.TryGet(recalled[e], out SimTransform t)) { sum += t.Position; count++; }
                    if (count > 0) _camera.CenterOn((sum / Fix32.FromInt(count)).ToWorld());
                }
                _lastGroupTap[i] = now;
            }
        }
    }

    public override void _UnhandledInput(InputEvent @event)
    {
        if (_bridge is null || _selection is null || _camera is null) return;
        if (_buildMode && @event is InputEventMouseButton buildMouse && buildMouse.Pressed)
        {
            if (buildMouse.ButtonIndex == MouseButton.Left) ConfirmBuildPlacement(buildMouse.Position);
            else if (buildMouse.ButtonIndex == MouseButton.Right) ExitBuildMode();
            else return;
            GetViewport().SetInputAsHandled();
            return;
        }
        if (_buildMode && @event is InputEventKey buildKey && buildKey.Pressed && buildKey.Keycode == Key.Escape)
        {
            ExitBuildMode(); GetViewport().SetInputAsHandled(); return;
        }
        if (@event is InputEventMouseButton mouse && mouse.Pressed && mouse.ButtonIndex == MouseButton.Right && _selection.Selected.Count > 0)
        {
            EntityId constructionSite = _selection.FindConstructionSiteAtScreen(mouse.Position);
            EntityId resource = _selection.FindResourceAtScreen(mouse.Position);
            if (HasSelectedProduction() && _camera.TryProjectToGround(mouse.Position, out Vector3 rallyPoint)) IssueRally(rallyPoint, resource);
            else if (constructionSite != EntityId.None) IssueAssistConstruction(constructionSite);
            else if (resource != EntityId.None) IssueHarvest(resource);
            else if (_camera.TryProjectToGround(mouse.Position, out Vector3 point)) IssueMove(point);
            GetViewport().SetInputAsHandled();
        }
    }

    public bool CanQueueProduction(string unitKey)
    {
        if (_bridge is null || _selection is null || !_bridge.World.Content.TryGetEntity(unitKey, out PrototypeEntityDefinition unit) ||
            !_bridge.World.Content.TryGetProduction(unit.Id, out UnitProductionDefinition definition)) return false;
        OperationsCapacityState capacity = _bridge.World.GetOperationsCapacity(0);
        if (capacity.Used + definition.OperationsCapacity > capacity.Maximum) return false;
        for (int i = 0; i < _selection.Selected.Count; i++)
        {
            EntityId id = _selection.Selected[i];
            if (_bridge.World.Entities.Production.TryGet(id, out Production production) && production.Count < Production.Capacity &&
                _bridge.World.Entities.Building.TryGet(id, out Building building) && building.State == BuildingState.Completed && building.Type == definition.ProducerType &&
                EnergyDomainSystem.CanSpendForEntity(_bridge.World, id, 0, definition.EnergyCost)) return true;
        }
        return false;
    }

    public void QueueProduction(string unitKey, bool fiveCopies = false)
    {
        if (_bridge is null || _selection is null || !_bridge.World.Content.TryGetEntity(unitKey, out PrototypeEntityDefinition unit) ||
            !_bridge.World.Content.TryGetProduction(unit.Id, out UnitProductionDefinition definition)) return;
        Dictionary<uint, int> projected = new();
        Dictionary<uint, int> queuedNow = new();
        List<EntityId> candidates = new();
        for (int i = 0; i < _selection.Selected.Count; i++)
        {
            EntityId id = _selection.Selected[i];
            if (!_bridge.World.Entities.Production.TryGet(id, out Production production) || production.Count >= Production.Capacity ||
                !_bridge.World.Entities.Building.TryGet(id, out Building building) || building.State != BuildingState.Completed || building.Type != definition.ProducerType) continue;
            candidates.Add(id); projected[id.Value] = production.ProjectedTicks; queuedNow[id.Value] = 0;
        }
        candidates.Sort(static (a, b) => a.Value.CompareTo(b.Value));
        int copies = fiveCopies ? 5 : 1;
        for (int copy = 0; copy < copies; copy++)
        {
            EntityId chosen = EntityId.None; int bestTicks = int.MaxValue;
            for (int i = 0; i < candidates.Count; i++)
            {
                EntityId candidate = candidates[i];
                Production production = _bridge.World.Entities.Production.Get(candidate);
                if (production.Count + queuedNow[candidate.Value] >= Production.Capacity) continue;
                int ticks = projected[candidate.Value];
                if (ticks < bestTicks) { bestTicks = ticks; chosen = candidate; }
                else if (ticks == bestTicks && PreferRoundRobin(unit.Id, candidate, chosen)) chosen = candidate;
            }
            if (chosen == EntityId.None) break;
            _bridge.Enqueue(new CommandEnvelope(_bridge.World.Tick.Next(), 0, _sequence++, SimCommandType.QueueProduction,
                Array.Empty<EntityId>(), FixVec2.Zero, targetEntity: chosen, contentType: unit.Id));
            projected[chosen.Value] = checked(projected[chosen.Value] + definition.BuildTicks);
            queuedNow[chosen.Value]++;
            _productionRoundRobin[unit.Id.Value] = chosen.Value;
        }
    }

    private bool PreferRoundRobin(ContentId unitType, EntityId candidate, EntityId current)
    {
        if (current == EntityId.None) return true;
        if (!_productionRoundRobin.TryGetValue(unitType.Value, out uint last)) return candidate.Value < current.Value;
        bool candidateAfter = candidate.Value > last, currentAfter = current.Value > last;
        return candidateAfter != currentAfter ? candidateAfter : candidate.Value < current.Value;
    }

    private bool HasSelectedProduction()
    {
        if (_bridge is null || _selection is null) return false;
        for (int i = 0; i < _selection.Selected.Count; i++) if (_bridge.World.Entities.Production.Has(_selection.Selected[i])) return true;
        return false;
    }

    private void IssueRally(Vector3 worldPoint, EntityId resource)
    {
        if (_bridge is null || _selection is null) return;
        List<EntityId> facilities = new();
        for (int i = 0; i < _selection.Selected.Count; i++) if (_bridge.World.Entities.Production.Has(_selection.Selected[i])) facilities.Add(_selection.Selected[i]);
        if (facilities.Count == 0) return;
        FixVec2 target = resource != EntityId.None && _bridge.World.Entities.Transform.TryGet(resource, out SimTransform resourceTransform)
            ? resourceTransform.Position : worldPoint.ToFixedBuild();
        _bridge.Enqueue(new CommandEnvelope(_bridge.World.Tick.Next(), 0, _sequence++, SimCommandType.SetRallyPoint,
            facilities.ToArray(), target, targetEntity: resource));
        ClearMovePreviews();
    }

    private void IssueAssistConstruction(EntityId site)
    {
        if (_bridge is null || _selection is null) return;
        List<EntityId> builders = new(_selection.Selected.Count);
        for (int i = 0; i < _selection.Selected.Count; i++)
        {
            EntityId id = _selection.Selected[i];
            if (_bridge.World.Entities.Builder.Has(id)) builders.Add(id);
        }
        if (builders.Count == 0) return;
        CommandModifiers modifiers = Input.IsKeyPressed(Key.Shift) ? CommandModifiers.Queue : CommandModifiers.None;
        _bridge.Enqueue(new CommandEnvelope(_bridge.World.Tick.Next(), 0, _sequence++, SimCommandType.AssistConstruction,
            builders.ToArray(), FixVec2.Zero, modifiers, site));
        if (modifiers == CommandModifiers.None) ClearMovePreviews();
    }

    private void IssueHarvest(EntityId resource)
    {
        if (_bridge is null || _selection is null) return;
        List<EntityId> workers = new(_selection.Selected.Count);
        for (int i = 0; i < _selection.Selected.Count; i++)
        {
            EntityId id = _selection.Selected[i];
            if (_bridge.World.Entities.Worker.Has(id)) workers.Add(id);
        }
        if (workers.Count == 0) return;
        CommandModifiers modifiers = Input.IsKeyPressed(Key.Shift) ? CommandModifiers.Queue : CommandModifiers.None;
        _bridge.Enqueue(new CommandEnvelope(_bridge.World.Tick.Next(), 0, _sequence++, SimCommandType.Harvest, workers.ToArray(), FixVec2.Zero, modifiers, resource));
        if (modifiers == CommandModifiers.None) ClearMovePreviews();
    }

    private void IssueMove(Vector3 world)
    {
        if (_bridge is null || _selection is null) return;
        FixVec2 target = world.ToFixedBuild();
        if (target.X < Fix32.Zero || target.Y < Fix32.Zero || target.X >= Fix32.FromInt(MapGrid.BuildWidth) || target.Y >= Fix32.FromInt(MapGrid.BuildHeight)) return;
        EntityId[] ids = SelectionArray();
        FootprintClass largest = FootprintClass.Tiny; bool hasGround = false;
        for (int i = 0; i < ids.Length; i++)
            if (_bridge.World.Entities.Navigation.TryGet(ids[i], out NavigationAgent nav) && nav.Layer != MovementLayer.TrueAir)
            { hasGround = true; if (nav.Footprint > largest) largest = nav.Footprint; }
        if (hasGround && !_bridge.World.Pathfinder.IsPassable(MapGrid.BuildToNav(target), largest)) return;
        bool queued = Input.IsKeyPressed(Key.Shift);
        CommandModifiers modifiers = queued ? CommandModifiers.Queue : CommandModifiers.None;
        _bridge.Enqueue(new CommandEnvelope(_bridge.World.Tick.Next(), 0, _sequence++, SimCommandType.Move, ids, target, modifiers));

        if (!queued)
        {
            ClearMovePreviews();
            _previewEntities = ids;
        }
        else if (_movePreviewMarkers.Count == 0)
        {
            // Queueing onto a pre-existing order that predates the visual preview still gets a clear first marker.
            _previewEntities = ids;
        }
        AddMovePreview(target, _movePreviewMarkers.Count + 1, queued);
    }

    private EntityId[] SelectionArray()
    {
        if (_selection is null) return Array.Empty<EntityId>();
        EntityId[] ids = new EntityId[_selection.Selected.Count];
        for (int i = 0; i < ids.Length; i++) ids[i] = _selection.Selected[i];
        return ids;
    }

    private void IssueSimple(SimCommandType type) => _bridge!.Enqueue(new CommandEnvelope(_bridge.World.Tick.Next(), 0, _sequence++, type, SelectionArray(), FixVec2.Zero));

    private void EnterBuildMode()
    {
        _buildMode = true; _buildIndex = 0; _buildOrientation = 0; ClearMovePreviews();
        if (_buildGhost is null)
        {
            _buildGhostMaterial = new StandardMaterial3D
            {
                AlbedoColor = new Color(0.18f, 0.92f, 0.55f, 0.55f),
                Transparency = BaseMaterial3D.TransparencyEnum.Alpha,
                ShadingMode = BaseMaterial3D.ShadingModeEnum.Unshaded,
                NoDepthTest = true
            };
            BoxMesh mesh = new() { Size = Vector3.One, Material = _buildGhostMaterial };
            _buildGhost = new MeshInstance3D { Name = "ConstructionGhost", Mesh = mesh };
            AddChild(_buildGhost);
        }
        _buildGhost.Visible = true;
    }

    private void ExitBuildMode()
    {
        _buildMode = false; _buildStatus = "Off";
        if (_buildGhost is not null) _buildGhost.Visible = false;
    }

    private void CycleBuilding()
    {
        _buildIndex = (_buildIndex + 1) % BuildKeys.Length;
        _buildOrientation = 0;
    }

    private void RotateBuilding()
    {
        if (_bridge is null || !_bridge.World.Content.TryGetBuilding(BuildKeys[_buildIndex], out BuildingDefinition definition) || !definition.Rotatable) return;
        _buildOrientation = (byte)((_buildOrientation + 1) & 3);
    }

    private void UpdateBuildPreview()
    {
        if (_bridge is null || _camera is null || _buildGhost is null || _buildGhostMaterial is null ||
            !_bridge.World.Content.TryGetBuilding(BuildKeys[_buildIndex], out BuildingDefinition definition) ||
            !_camera.TryProjectToGround(GetViewport().GetMousePosition(), out Vector3 point))
        {
            if (_buildGhost is not null) _buildGhost.Visible = false;
            return;
        }
        GetBuildAnchor(point, definition, out short anchorX, out short anchorY);
        byte width = definition.RotatedWidth(_buildOrientation), height = definition.RotatedHeight(_buildOrientation);
        FixVec2 center = new(Fix32.FromRatio(anchorX * 2 + width, 2), Fix32.FromRatio(anchorY * 2 + height, 2));
        _buildGhost.Visible = true;
        _buildGhost.GlobalPosition = center.ToWorld(0.22f);
        _buildGhost.Scale = new Vector3(width * GodotConversions.WorldUnitsPerBuildCell, 0.35f, height * GodotConversions.WorldUnitsPerBuildCell);
        PlacementValidation validation = ConstructionPlacement.Validate(_bridge.World, 0, SelectionArray(), definition.Id, anchorX, anchorY, _buildOrientation);
        _buildGhostMaterial.AlbedoColor = validation.IsValid ? new Color(0.18f, 0.92f, 0.55f, 0.55f) : new Color(1f, 0.20f, 0.12f, 0.58f);
        _buildStatus = $"{DisplayName(definition.StableKey)} — {definition.OreCost} Ore + {definition.EnergyCost} Energy / {PlacementFailureText(validation.Failure)}";
    }

    private void ConfirmBuildPlacement(Vector2 mousePosition)
    {
        if (_bridge is null || _camera is null || !_bridge.World.Content.TryGetBuilding(BuildKeys[_buildIndex], out BuildingDefinition definition) ||
            !_camera.TryProjectToGround(mousePosition, out Vector3 point)) return;
        GetBuildAnchor(point, definition, out short anchorX, out short anchorY);
        EntityId[] builders = SelectionArray();
        PlacementValidation validation = ConstructionPlacement.Validate(_bridge.World, 0, builders, definition.Id, anchorX, anchorY, _buildOrientation);
        if (!validation.IsValid) { _buildStatus = $"{DisplayName(definition.StableKey)} — {PlacementFailureText(validation.Failure)}"; return; }
        CommandModifiers modifiers = Input.IsKeyPressed(Key.Shift) ? CommandModifiers.Queue : CommandModifiers.None;
        _bridge.Enqueue(new CommandEnvelope(_bridge.World.Tick.Next(), 0, _sequence++, SimCommandType.Build, builders,
            FixVec2.FromInts(anchorX, anchorY), modifiers, contentType: definition.Id, orientation: _buildOrientation));
        if (!Input.IsKeyPressed(Key.Shift)) ExitBuildMode();
    }

    private void CancelMostRecentSite()
    {
        if (_bridge is null) return;
        IReadOnlyList<EntityId> alive = _bridge.World.Entities.Alive;
        for (int i = alive.Count - 1; i >= 0; i--)
        {
            EntityId id = alive[i];
            if (!_bridge.World.Entities.ConstructionSite.TryGet(id, out ConstructionSite site) || site.ProgressTicks != 0 ||
                !_bridge.World.Entities.Ownership.TryGet(id, out Ownership ownership) || ownership.PlayerSlot != 0) continue;
            _bridge.Enqueue(new CommandEnvelope(_bridge.World.Tick.Next(), 0, _sequence++, SimCommandType.CancelConstruction, Array.Empty<EntityId>(), FixVec2.Zero, targetEntity: id));
            return;
        }
    }

    private void GetBuildAnchor(Vector3 point, BuildingDefinition definition, out short anchorX, out short anchorY)
    {
        Vector2 build = point.ToBuildXZ();
        int width = definition.RotatedWidth(_buildOrientation), height = definition.RotatedHeight(_buildOrientation);
        anchorX = checked((short)(Mathf.FloorToInt(build.X) - width / 2));
        anchorY = checked((short)(Mathf.FloorToInt(build.Y) - height / 2));
    }

    private static string DisplayName(string key) => key switch
    {
        "building.rock_raiders.hq" => "Rock Raiders HQ",
        "building.rock_raiders.ore_processing_plant" => "Ore Processing Plant",
        "building.rock_raiders.power_station" => "Power Station",
        "building.rock_raiders.vehicle_service_bay" => "Vehicle Service Bay",
        _ => key
    };

    private static string PlacementFailureText(PlacementFailure failure) => failure switch
    {
        PlacementFailure.None => "Valid — left click to place",
        PlacementFailure.NoEligibleBuilder => "Select a Crew builder",
        PlacementFailure.MissingPrerequisite => "Requires a completed HQ",
        PlacementFailure.OutsideMap => "Outside map",
        PlacementFailure.FootprintOccupied => "Footprint occupied",
        PlacementFailure.ResourceAccessBlocked => "Resource access blocked",
        PlacementFailure.NonBuildableTerrain => "Non-buildable terrain",
        PlacementFailure.TerrainFeature => "Terrain feature prevents construction",
        PlacementFailure.NoLegalProductionExit => "No legal production exit",
        PlacementFailure.InsufficientOre => "Insufficient processed Ore",
        PlacementFailure.NoEnergyDomain => "No connected Energy Domain",
        PlacementFailure.InsufficientEnergy => "Insufficient Energy reserve",
        _ => "Invalid placement"
    };

    private void AddMovePreview(FixVec2 target, int number, bool queued)
    {
        Node3D marker = CreateMoveMarker(number, queued);
        marker.GlobalPosition = target.ToWorld(0.09f);
        AddChild(marker);
        _movePreviewMarkers.Add(marker);
    }

    private void ClearMovePreviews()
    {
        for (int i = 0; i < _movePreviewMarkers.Count; i++) _movePreviewMarkers[i].QueueFree();
        _movePreviewMarkers.Clear();
        _previewEntities = Array.Empty<EntityId>();
    }

    private bool PreviewGroupFinished()
    {
        if (_bridge is null || _previewEntities.Length == 0) return true;
        for (int i = 0; i < _previewEntities.Length; i++)
        {
            EntityId id = _previewEntities[i];
            if (!_bridge.World.Entities.Exists(id)) continue;
            if (_bridge.World.Entities.Navigation.TryGet(id, out NavigationAgent nav) && nav.HasTarget) return false;
            if (_bridge.World.GetQueue(id).Count > 0) return false;
        }
        return true;
    }

    private static Node3D CreateMoveMarker(int number, bool queued)
    {
        Node3D root = new() { Name = $"MoveOrderPreview_{number}" };
        CylinderMesh mesh = new() { TopRadius = 1.15f, BottomRadius = 1.15f, Height = 0.045f, RadialSegments = 32 };
        StandardMaterial3D material = new()
        {
            AlbedoColor = queued ? new Color(0.30f, 0.72f, 1f, 0.88f) : new Color(0.20f, 1f, 0.80f, 0.92f),
            ShadingMode = BaseMaterial3D.ShadingModeEnum.Unshaded,
            NoDepthTest = true
        };
        mesh.Material = material;
        root.AddChild(new MeshInstance3D { Name = "MarkerDisc", Mesh = mesh });

        Label3D label = new()
        {
            Name = "OrderNumber",
            Text = number.ToString(),
            FontSize = 36,
            OutlineSize = 12,
            Modulate = new Color(1f, 1f, 1f),
            OutlineModulate = new Color(0.02f, 0.02f, 0.02f, 0.95f),
            Billboard = BaseMaterial3D.BillboardModeEnum.Enabled,
            FixedSize = true,
            NoDepthTest = true,
            Position = new Vector3(0f, 0.25f, 0f)
        };
        root.AddChild(label);
        return root;
    }
}
