using Godot;
using LegoSpaceRTS.SimCore;

namespace LegoSpaceRTS.Presentation;

public partial class MapPresenter : Node3D
{
    private SimulationWorld? _world;
    private MultiMeshInstance3D? _blocked;
    private MultiMeshInstance3D? _rough;
    private int _lastTopology = -1;

    public void Configure(SimulationWorld world)
    {
        _world = world;
        BuildGround();
        RebuildTerrainDebug();
        ProcessPriority = 50;
    }

    public override void _Process(double delta)
    {
        if (_world is not null && _world.Map.TopologyVersion != _lastTopology) RebuildTerrainDebug();
    }

    private void BuildGround()
    {
        BoxMesh mesh = new() { Size = new Vector3(MapGrid.BuildWidth * 2f, 0.2f, MapGrid.BuildHeight * 2f) };
        mesh.Material = new StandardMaterial3D { AlbedoColor = new Color(0.24f, 0.23f, 0.21f), Roughness = 0.92f };
        MeshInstance3D ground = new() { Name = "PrototypeGround", Mesh = mesh, Position = new Vector3(160f, -0.12f, 160f) };
        AddChild(ground);
    }

    private void RebuildTerrainDebug()
    {
        if (_world is null) return;
        FreeInstance(ref _blocked);
        FreeInstance(ref _rough);

        List<Transform3D> blocked = new();
        List<Transform3D> rough = new();
        for (int by = 0; by < MapGrid.BuildHeight; by++)
        for (int bx = 0; bx < MapGrid.BuildWidth; bx++)
        {
            bool isBlocked = false;
            bool isRough = false;
            for (int oy = 0; oy < 2; oy++)
            for (int ox = 0; ox < 2; ox++)
            {
                MapCellFlags flags = _world.Map.GetFlags(bx * 2 + ox, by * 2 + oy);
                isBlocked |= (flags & MapCellFlags.Impassable) != 0;
                isRough |= (flags & MapCellFlags.Rough) != 0;
            }

            Vector3 center = new(bx * 2f + 1f, 0f, by * 2f + 1f);
            if (isBlocked) blocked.Add(new Transform3D(Basis.Identity, center + new Vector3(0f, 1.0f, 0f)));
            else if (isRough) rough.Add(new Transform3D(Basis.Identity, center + new Vector3(0f, 0.03f, 0f)));
        }

        _blocked = BuildMultiMesh("ImpassableTerrain", blocked,
            new BoxMesh { Size = new Vector3(1.90f, 2.0f, 1.90f) },
            new Color(0.20f, 0.18f, 0.17f));
        _rough = BuildMultiMesh("RoughTerrain", rough,
            new BoxMesh { Size = new Vector3(1.92f, 0.08f, 1.92f) },
            new Color(0.40f, 0.31f, 0.20f));

        if (_blocked is not null) AddChild(_blocked);
        if (_rough is not null) AddChild(_rough);
        _lastTopology = _world.Map.TopologyVersion;
    }

    private static MultiMeshInstance3D? BuildMultiMesh(string name, List<Transform3D> transforms, PrimitiveMesh mesh, Color color)
    {
        if (transforms.Count == 0) return null;
        mesh.Material = new StandardMaterial3D { AlbedoColor = color, Roughness = 0.95f };
        MultiMesh mm = new() { TransformFormat = MultiMesh.TransformFormatEnum.Transform3D, Mesh = mesh, InstanceCount = transforms.Count };
        for (int i = 0; i < transforms.Count; i++) mm.SetInstanceTransform(i, transforms[i]);
        return new MultiMeshInstance3D { Name = name, Multimesh = mm };
    }

    private static void FreeInstance(ref MultiMeshInstance3D? instance)
    {
        if (instance is null) return;
        instance.QueueFree();
        instance = null;
    }
}
