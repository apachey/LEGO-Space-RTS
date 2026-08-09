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
    private uint _sequence = 1;

    public ControlGroups Groups => _groups;

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

        if (Input.IsActionJustPressed("command_move") && _selection.Selected.Count > 0 && _camera.TryProjectToGround(GetViewport().GetMousePosition(), out Vector3 movePoint)) IssueMove(movePoint);
        if (Input.IsActionJustPressed("command_stop") && _selection.Selected.Count > 0 && !Input.IsKeyPressed(Key.Ctrl)) { IssueSimple(SimCommandType.Stop); ClearMovePreviews(); }
        if (Input.IsActionJustPressed("command_hold") && _selection.Selected.Count > 0) { IssueSimple(SimCommandType.HoldPosition); ClearMovePreviews(); }
        if (Input.IsActionJustPressed("debug_open_excavatable"))
            _bridge.Enqueue(new CommandEnvelope(_bridge.World.Tick.Next(), 0, _sequence++, SimCommandType.DebugOpenExcavatable, Array.Empty<EntityId>(), FixVec2.Zero, debugFeatureId: DevMapFactory.ExcavatableFeatureId));

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
        if (@event is InputEventMouseButton mouse && mouse.Pressed && mouse.ButtonIndex == MouseButton.Right && _selection.Selected.Count > 0)
        {
            EntityId resource = _selection.FindResourceAtScreen(mouse.Position);
            if (resource != EntityId.None) IssueHarvest(resource);
            else if (_camera.TryProjectToGround(mouse.Position, out Vector3 point)) IssueMove(point);
            GetViewport().SetInputAsHandled();
        }
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
