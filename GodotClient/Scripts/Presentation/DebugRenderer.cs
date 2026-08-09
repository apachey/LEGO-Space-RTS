using Godot;
using LegoSpaceRTS.SimCore;

namespace LegoSpaceRTS.Presentation;

public partial class DebugRenderer : MeshInstance3D
{
    private GodotSimBridge? _bridge;
    private readonly ImmediateMesh _mesh = new();
    private readonly StandardMaterial3D _material = new()
    {
        ShadingMode = BaseMaterial3D.ShadingModeEnum.Unshaded,
        VertexColorUseAsAlbedo = true,
        Transparency = BaseMaterial3D.TransparencyEnum.Alpha,
        NoDepthTest = true
    };
    private SimTick _lastTick = new(-1);
    private int _lastTopology = -1;

    public bool DrawNavigation { get; set; } = true;
    public bool DrawClusters { get; set; } = true;
    public bool DrawPortals { get; set; }
    public bool DrawPaths { get; set; } = true;
    public bool DrawReservations { get; set; }
    public bool DrawSpatialBuckets { get; set; }
    public bool DrawVision { get; set; }
    public bool DrawExcavatable { get; set; } = true;

    public void Configure(GodotSimBridge bridge)
    {
        _bridge = bridge;
        Name = "DebugVisualization";
        Mesh = _mesh;
        MaterialOverride = _material;
        CastShadow = GeometryInstance3D.ShadowCastingSetting.Off;
        ProcessPriority = 130;
        Redraw();
    }

    public override void _Process(double delta)
    {
        if (_bridge is null) return;
        if (!_bridge.World.Tick.Equals(_lastTick) || _bridge.World.Map.TopologyVersion != _lastTopology) Redraw();
    }

    public void Invalidate() => _lastTick = new SimTick(-1);

    private void Redraw()
    {
        if (_bridge is null) return;
        _mesh.ClearSurfaces();
        _mesh.SurfaceBegin(Mesh.PrimitiveType.Lines);
        if (DrawNavigation) DrawNavigationOverlay();
        if (DrawClusters) DrawClusterOverlay();
        if (DrawPortals) DrawPortalOverlay();
        if (DrawPaths) DrawPathOverlay();
        if (DrawReservations) DrawReservationOverlay();
        if (DrawSpatialBuckets) DrawSpatialOverlay();
        if (DrawVision) DrawVisionOverlay();
        if (DrawExcavatable) DrawExcavatableOverlay();
        _mesh.SurfaceEnd();
        _lastTick = _bridge.World.Tick;
        _lastTopology = _bridge.World.Map.TopologyVersion;
    }

    private void Line(Vector3 a, Vector3 b, Color color)
    {
        _mesh.SurfaceSetColor(color); _mesh.SurfaceAddVertex(a); _mesh.SurfaceAddVertex(b);
    }

    private void WireCircle(Vector3 center, float radius, Color color, int segments = 20)
    {
        Vector3 prev = center + new Vector3(radius, 0f, 0f);
        for (int i = 1; i <= segments; i++)
        {
            float a = Mathf.Tau * i / segments;
            Vector3 next = center + new Vector3(Mathf.Cos(a) * radius, 0f, Mathf.Sin(a) * radius);
            Line(prev, next, color); prev = next;
        }
    }

    private void DrawNavigationOverlay()
    {
        Color grid = new(1f, 1f, 1f, 0.08f);
        for (int i = 0; i <= MapGrid.BuildWidth; i += 8)
        {
            float p = i * GodotConversions.WorldUnitsPerBuildCell;
            Line(new Vector3(p, 0.03f, 0f), new Vector3(p, 0.03f, 320f), grid);
            Line(new Vector3(0f, 0.03f, p), new Vector3(320f, 0.03f, p), grid);
        }
        Color blocked = new(1f, 0.25f, 0.15f, 0.20f);
        for (int y = 0; y < MapGrid.NavHeight; y += 8) for (int x = 0; x < MapGrid.NavWidth; x += 8)
        {
            if ((_bridge!.World.Map.GetFlags(x, y) & MapCellFlags.Impassable) == 0) continue;
            Vector3 p = MapGrid.NavCellCenterToBuild(new NavCell((short)x, (short)y)).ToWorld(0.08f);
            Line(p + new Vector3(-0.6f, 0f, -0.6f), p + new Vector3(0.6f, 0f, 0.6f), blocked);
            Line(p + new Vector3(-0.6f, 0f, 0.6f), p + new Vector3(0.6f, 0f, -0.6f), blocked);
        }
    }

    private void DrawClusterOverlay()
    {
        Color c = new(0.9f, 0.6f, 0.15f, 0.24f);
        float step = HierarchicalPathfinder.ClusterSize * 0.5f * GodotConversions.WorldUnitsPerBuildCell;
        for (int i = 0; i <= HierarchicalPathfinder.ClusterWidth; i++) { float p = i * step; Line(new Vector3(p, 0.06f, 0f), new Vector3(p, 0.06f, 320f), c); }
        for (int i = 0; i <= HierarchicalPathfinder.ClusterHeight; i++) { float p = i * step; Line(new Vector3(0f, 0.06f, p), new Vector3(320f, 0.06f, p), c); }
    }

    private void DrawPortalOverlay()
    {
        Color c = new(0.1f, 1f, 0.5f, 0.9f);
        for (int cy = 0; cy < HierarchicalPathfinder.ClusterHeight; cy++) for (int cx = 0; cx < HierarchicalPathfinder.ClusterWidth; cx++) for (int dir = 0; dir < 2; dir++)
        {
            Portal? portal = _bridge!.World.Pathfinder.GetPortal(FootprintClass.Tiny, cx, cy, dir);
            if (portal.HasValue) WireCircle(MapGrid.NavCellCenterToBuild(portal.Value.Cell).ToWorld(0.12f), 0.16f, c, 8);
        }
    }

    private void DrawPathOverlay()
    {
        Color c = new(0.2f, 0.9f, 1f, 0.92f);
        IReadOnlyList<EntityId> alive = _bridge!.World.Entities.Alive;
        for (int i = 0; i < alive.Count; i++)
        {
            NavPath? path = _bridge.World.GetPath(alive[i]);
            if (path is null || path.Cells.Count < 2) continue;
            for (int n = 1; n < path.Cells.Count; n++)
                Line(MapGrid.NavCellCenterToBuild(path.Cells[n - 1]).ToWorld(0.12f), MapGrid.NavCellCenterToBuild(path.Cells[n]).ToWorld(0.12f), c);
        }
    }

    private void DrawReservationOverlay()
    {
        Color c = new(0.7f, 0.2f, 1f, 0.8f);
        IReadOnlyList<EntityId> alive = _bridge!.World.Entities.Alive;
        for (int i = 0; i < alive.Count; i++) if (_bridge.World.HasReservationPermit(alive[i]) && _bridge.World.Entities.Transform.TryGet(alive[i], out SimTransform t)) WireCircle(t.Position.ToWorld(0.2f), 0.35f, c, 12);
    }

    private void DrawSpatialOverlay()
    {
        Color c = new(0.4f, 0.7f, 1f, 0.2f);
        float step = SpatialGrid.BucketBuildCells * GodotConversions.WorldUnitsPerBuildCell;
        for (float p = 0; p <= 320f; p += step)
        {
            Line(new Vector3(p, 0.08f, 0f), new Vector3(p, 0.08f, 320f), c);
            Line(new Vector3(0f, 0.08f, p), new Vector3(320f, 0.08f, p), c);
        }
    }

    private void DrawVisionOverlay()
    {
        Color c = new(1f, 1f, 0.2f, 0.28f);
        IReadOnlyList<EntityId> alive = _bridge!.World.Entities.Alive;
        for (int i = 0; i < alive.Count; i++) if (_bridge.World.Entities.Transform.TryGet(alive[i], out SimTransform t) && _bridge.World.Entities.Vision.TryGet(alive[i], out Vision v)) WireCircle(t.Position.ToWorld(0.1f), v.RadiusBuildCells * GodotConversions.WorldUnitsPerBuildCell, c, 28);
    }

    private void DrawExcavatableOverlay()
    {
        Color c = new(1f, 0.45f, 0.05f, 0.85f);
        IReadOnlyList<ExcavatableFeature> features = _bridge!.World.Map.Features;
        for (int i = 0; i < features.Count; i++)
        {
            IntRect r = features[i].NavRect;
            float x0 = r.X; float x1 = r.X + r.Width; float z0 = r.Y; float z1 = r.Y + r.Height;
            float y = 0.18f;
            Line(new Vector3(x0, y, z0), new Vector3(x1, y, z0), c);
            Line(new Vector3(x1, y, z0), new Vector3(x1, y, z1), c);
            Line(new Vector3(x1, y, z1), new Vector3(x0, y, z1), c);
            Line(new Vector3(x0, y, z1), new Vector3(x0, y, z0), c);
        }
    }
}
