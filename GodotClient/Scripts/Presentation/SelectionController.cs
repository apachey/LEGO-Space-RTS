using Godot;
using LegoSpaceRTS.SimCore;

namespace LegoSpaceRTS.Presentation;

public partial class SelectionController : Node
{
    private GodotSimBridge? _bridge;
    private RtsCameraController? _camera;
    private Vector2 _dragStart;
    private bool _dragging;
    private readonly List<EntityId> _selected = new(128);
    private readonly List<PresentationEntity> _boxCandidates = new(128);
    private SelectionMarqueeOverlay? _marquee;

    public IReadOnlyList<EntityId> Selected => _selected;
    public EntityId Hovered { get; private set; } = EntityId.None;
    public bool IsDragging => _dragging;
    public Vector2 DragStart => _dragStart;
    public int LastFilteredWorkerCount { get; private set; }

    public EntityId FindResourceAtScreen(Vector2 screen)
    {
        if (_bridge?.Current is null || _camera is null) return EntityId.None;
        EntityId best = EntityId.None;
        float bestNormalized = 1f;
        for (int i = 0; i < _bridge.Current.Entities.Count; i++)
        {
            PresentationEntity entity = _bridge.Current.Entities[i];
            if (entity.SelectableKind != SelectableKind.ResourceNode) continue;
            Vector3 world = entity.Position.ToWorld(0.5f);
            if (_camera.IsPositionBehind(world)) continue;
            Vector2 projected = _camera.UnprojectPosition(world);
            float radius = ScreenPickRadius(entity.Footprint) * 1.35f;
            float normalized = (projected - screen).LengthSquared() / (radius * radius);
            if (normalized < bestNormalized) { bestNormalized = normalized; best = entity.EntityId; }
        }
        return best;
    }

    public EntityId FindConstructionSiteAtScreen(Vector2 screen)
    {
        if (_bridge?.Current is null || _camera is null) return EntityId.None;
        EntityId best = EntityId.None;
        float bestNormalized = 1f;
        for (int i = 0; i < _bridge.Current.Entities.Count; i++)
        {
            PresentationEntity entity = _bridge.Current.Entities[i];
            if (entity.Owner != 0 || !entity.IsConstructionSite) continue;
            Vector3 world = entity.Position.ToWorld(0.5f);
            if (_camera.IsPositionBehind(world)) continue;
            Vector2 projected = _camera.UnprojectPosition(world);
            float radius = Mathf.Max(28f, Mathf.Max(entity.BuildingWidth, entity.BuildingHeight) * 4f);
            float normalized = (projected - screen).LengthSquared() / (radius * radius);
            if (normalized < bestNormalized) { bestNormalized = normalized; best = entity.EntityId; }
        }
        return best;
    }

    public void Configure(GodotSimBridge bridge, RtsCameraController camera)
    {
        _bridge = bridge;
        _camera = camera;
        ProcessPriority = 0;

        CanvasLayer layer = new() { Name = "SelectionMarqueeLayer", Layer = 30 };
        _marquee = new SelectionMarqueeOverlay { Name = "SelectionMarquee" };
        layer.AddChild(_marquee);
        AddChild(layer);
    }

    public override void _Process(double delta)
    {
        if (_bridge is null || _camera is null) return;
        Vector2 pointer = GetViewport().GetMousePosition();
        Hovered = FindClosest(pointer);
        bool down = Input.IsMouseButtonPressed(MouseButton.Left);
        if (down && !_dragging)
        {
            // UI consumes its own clicks; do not start a world marquee underneath the debug HUD.
            if (GetViewport().GuiGetHoveredControl() is not null) return;
            _dragStart = pointer;
            _dragging = true;
            _marquee?.SetDrag(_dragStart, pointer);
        }
        else if (down && _dragging)
        {
            _marquee?.SetDrag(_dragStart, pointer);
        }
        else if (!down && _dragging)
        {
            Vector2 end = pointer;
            _dragging = false;
            _marquee?.HideDrag();
            bool shift = Input.IsKeyPressed(Key.Shift);
            bool ctrl = Input.IsKeyPressed(Key.Ctrl);
            bool subtract = Input.IsKeyPressed(Key.Alt);
            if ((end - _dragStart).LengthSquared() < 25f) ClickSelect(end, shift, ctrl, subtract);
            else BoxSelect(_dragStart, end, shift, ctrl, subtract);
        }
    }

    public void SetSelection(IEnumerable<EntityId> ids)
    {
        if (_bridge is null) return;
        _selected.Clear();
        LastFilteredWorkerCount = 0;
        foreach (EntityId id in ids)
            if (_selected.Count < 128 && _bridge.World.Entities.Exists(id)) _selected.Add(id);
        _selected.Sort(static (a, b) => a.Value.CompareTo(b.Value));
    }

    private EntityId FindClosest(Vector2 screen)
    {
        if (_bridge?.Current is null || _camera is null) return EntityId.None;
        EntityId best = EntityId.None;
        float bestNormalized = 1f;
        for (int i = 0; i < _bridge.Current.Entities.Count; i++)
        {
            PresentationEntity e = _bridge.Current.Entities[i];
            if (e.Owner != 0) continue;
            Vector3 world = e.Position.ToWorld(0.5f);
            if (_camera.IsPositionBehind(world)) continue;
            Vector2 sp = _camera.UnprojectPosition(world);
            float radius = EntityPickRadius(e);
            float normalized = (sp - screen).LengthSquared() / (radius * radius);
            if (normalized < bestNormalized) { bestNormalized = normalized; best = e.EntityId; }
        }
        return best;
    }

    private void ClickSelect(Vector2 screen, bool additive, bool sameType, bool subtract)
    {
        if (_bridge?.Current is null) return;
        LastFilteredWorkerCount = 0;
        EntityId best = FindClosest(screen);
        if (best == EntityId.None) { if (!additive && !subtract) _selected.Clear(); return; }
        if (sameType)
        {
            ContentId type = default;
            for (int i = 0; i < _bridge.Current.Entities.Count; i++) if (_bridge.Current.Entities[i].EntityId == best) { type = _bridge.Current.Entities[i].ContentType; break; }
            if (!additive && !subtract) _selected.Clear();
            for (int i = 0; i < _bridge.Current.Entities.Count; i++)
            {
                PresentationEntity e = _bridge.Current.Entities[i];
                if (e.Owner == 0 && e.ContentType == type) Apply(e.EntityId, subtract, false);
            }
        }
        else
        {
            if (!additive && !subtract) _selected.Clear();
            Apply(best, subtract, additive);
        }
        _selected.Sort(static (a, b) => a.Value.CompareTo(b.Value));
    }

    private void BoxSelect(Vector2 a, Vector2 b, bool additive, bool includeWorkers, bool subtract)
    {
        if (_bridge?.Current is null || _camera is null) return;
        Rect2 rect = new(new Vector2(Mathf.Min(a.X, b.X), Mathf.Min(a.Y, b.Y)), new Vector2(Mathf.Abs(b.X - a.X), Mathf.Abs(b.Y - a.Y)));
        _boxCandidates.Clear();
        LastFilteredWorkerCount = 0;
        bool hasCombat = false;
        for (int i = 0; i < _bridge.Current.Entities.Count; i++)
        {
            PresentationEntity e = _bridge.Current.Entities[i];
            if (e.Owner != 0 || e.SelectableKind == SelectableKind.Building) continue;
            Vector3 world = e.Position.ToWorld(0.5f);
            if (_camera.IsPositionBehind(world)) continue;
            Vector2 screen = _camera.UnprojectPosition(world);
            if (rect.Grow(ScreenPickRadius(e.Footprint) * 0.65f).HasPoint(screen))
            {
                _boxCandidates.Add(e);
                if (e.SelectableKind == SelectableKind.CombatSupport) hasCombat = true;
            }
        }
        if (!additive && !subtract) _selected.Clear();
        for (int i = 0; i < _boxCandidates.Count; i++)
        {
            PresentationEntity e = _boxCandidates[i];
            if (!includeWorkers && hasCombat && e.SelectableKind == SelectableKind.Worker)
            {
                LastFilteredWorkerCount++;
                continue;
            }
            Apply(e.EntityId, subtract, false);
        }
        _selected.Sort(static (x, y) => x.Value.CompareTo(y.Value));
    }

    private static float ScreenPickRadius(FootprintClass footprint) => footprint switch
    {
        FootprintClass.Tiny => 18f,
        FootprintClass.Small => 22f,
        FootprintClass.Medium => 26f,
        FootprintClass.Large => 31f,
        FootprintClass.Huge => 36f,
        _ => 22f
    };

    private static float EntityPickRadius(PresentationEntity entity)
        => entity.SelectableKind == SelectableKind.Building
            ? Mathf.Max(28f, Mathf.Max(entity.BuildingWidth, entity.BuildingHeight) * 4f)
            : ScreenPickRadius(entity.Footprint);

    private void Apply(EntityId id, bool subtract, bool toggle)
    {
        int index = _selected.IndexOf(id);
        if (subtract) { if (index >= 0) _selected.RemoveAt(index); return; }
        if (toggle && index >= 0) { _selected.RemoveAt(index); return; }
        if (index < 0 && _selected.Count < 128) _selected.Add(id);
    }
}
