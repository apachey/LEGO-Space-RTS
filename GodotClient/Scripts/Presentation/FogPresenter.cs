using Godot;
using LegoSpaceRTS.SimCore;

namespace LegoSpaceRTS.Presentation;

public partial class FogPresenter : MultiMeshInstance3D
{
    private GodotSimBridge? _bridge;
    private SimTick _lastTick = new(-1);
    public bool FogVisible { get; private set; } = true;

    public void Configure(GodotSimBridge bridge)
    {
        _bridge = bridge;
        Name = "CpuAuthoritativeFog";
        BuildInstances();
        ProcessPriority = 120;
    }

    public override void _Process(double delta)
    {
        if (_bridge is null || _bridge.World.Tick.Equals(_lastTick) || Multimesh is null) return;
        _lastTick = _bridge.World.Tick;
        for (int y = 0; y < MapGrid.BuildHeight; y++) for (int x = 0; x < MapGrid.BuildWidth; x++)
        {
            int i = y * MapGrid.BuildWidth + x;
            VisibilityState v = _bridge.World.Fog.Get(0, x, y);
            Color c = v switch
            {
                VisibilityState.Visible => new Color(0f, 0f, 0f, 0f),
                VisibilityState.Explored => new Color(0.025f, 0.025f, 0.03f, 0.40f),
                _ => new Color(0.008f, 0.008f, 0.012f, 0.82f)
            };
            Multimesh.SetInstanceColor(i, FogVisible ? c : new Color(0f, 0f, 0f, 0f));
        }
    }

    public void SetFogVisible(bool visible) { FogVisible = visible; _lastTick = new SimTick(-1); }

    private void BuildInstances()
    {
        PlaneMesh plane = new() { Size = new Vector2(2.02f, 2.02f) };
        StandardMaterial3D material = new()
        {
            AlbedoColor = Colors.White,
            VertexColorUseAsAlbedo = true,
            ShadingMode = BaseMaterial3D.ShadingModeEnum.Unshaded,
            Transparency = BaseMaterial3D.TransparencyEnum.Alpha,
            NoDepthTest = false
        };
        plane.Material = material;
        MultiMesh mm = new()
        {
            TransformFormat = MultiMesh.TransformFormatEnum.Transform3D,
            UseColors = true,
            Mesh = plane,
            InstanceCount = MapGrid.BuildWidth * MapGrid.BuildHeight
        };
        for (int y = 0; y < MapGrid.BuildHeight; y++) for (int x = 0; x < MapGrid.BuildWidth; x++)
        {
            int i = y * MapGrid.BuildWidth + x;
            mm.SetInstanceTransform(i, new Transform3D(Basis.Identity, new Vector3(x * 2f + 1f, 0.14f, y * 2f + 1f)));
        }
        Multimesh = mm;
    }
}
