using Godot;

namespace LegoSpaceRTS.Presentation;

public readonly record struct M85AssetPipelineValidationReport(
    int CloseTriangles,
    int CombatTriangles,
    int StrategicTriangles,
    int MeshCount,
    int PivotCount,
    int SocketCount);

public static class M85AssetPipelineContract
{
    public const string RuntimePath = "res://Assets/M85/PipelineReference/pipeline_reference_vehicle.glb";
    public const string RootName = "Asset_PipelineReferenceVehicle";
    public const float WorldUnitsPerBuildCell = 2f;

    private static readonly string[] RequiredPivots =
    {
        "Pivot_Suspension", "Pivot_ToolPrimary",
        "Pivot_Wheel_Left_Front", "Pivot_Wheel_Left_Rear",
        "Pivot_Wheel_Right_Front", "Pivot_Wheel_Right_Rear"
    };

    private static readonly string[] RequiredSockets =
    {
        "Socket_Selection", "Socket_Health", "Socket_Weapon_Primary",
        "Socket_VFX_Exhaust", "Socket_Audio_Engine", "Socket_Cargo"
    };

    public static bool ValidateImportedScene(Node3D imported,
        out M85AssetPipelineValidationReport report, out string error)
    {
        report = default;
        error = string.Empty;
        Node3D? root = imported.Name == RootName ? imported : imported.FindChild(RootName, true, false) as Node3D;
        if (root is null) return Fail("model root was not preserved", out error);
        if (!root.Position.IsEqualApprox(Vector3.Zero) || !root.Rotation.IsEqualApprox(Vector3.Zero) ||
            !root.Scale.IsEqualApprox(Vector3.One))
            return Fail("root transform is not identity", out error);

        Node3D? close = root.FindChild("LOD_Close", true, false) as Node3D;
        Node3D? combat = root.FindChild("LOD_Combat", true, false) as Node3D;
        Node3D? strategic = root.FindChild("LOD_Strategic", true, false) as Node3D;
        if (close is null || combat is null || strategic is null)
            return Fail("one or more authored LOD roots are missing", out error);

        int closeTriangles = CountTriangles(close, out int closeMeshes);
        int combatTriangles = CountTriangles(combat, out int combatMeshes);
        int strategicTriangles = CountTriangles(strategic, out int strategicMeshes);
        if (closeTriangles != 868 || combatTriangles != 332 || strategicTriangles != 168)
            return Fail($"imported triangle counts changed ({closeTriangles}/{combatTriangles}/{strategicTriangles})", out error);
        if (!(closeTriangles > combatTriangles && combatTriangles > strategicTriangles))
            return Fail("LOD triangle counts do not strictly decrease", out error);
        if (!ValidateGeometryNames(close, "Close", allowSupport: true, allowMicro: true, out error) ||
            !ValidateGeometryNames(combat, "Combat", allowSupport: true, allowMicro: false, out error) ||
            !ValidateGeometryNames(strategic, "Strategic", allowSupport: false, allowMicro: false, out error))
            return false;

        for (int i = 0; i < RequiredPivots.Length; i++)
            if (root.FindChild(RequiredPivots[i], true, false) is not Node3D)
                return Fail($"required pivot missing: {RequiredPivots[i]}", out error);
        foreach ((string name, Vector3 expected) in new[]
        {
            ("Pivot_Wheel_Left_Front", new Vector3(-1.60f, 0.54f, -0.95f)),
            ("Pivot_Wheel_Left_Rear", new Vector3(-1.60f, 0.54f, 0.75f)),
            ("Pivot_Wheel_Right_Front", new Vector3(1.60f, 0.54f, -0.95f)),
            ("Pivot_Wheel_Right_Rear", new Vector3(1.60f, 0.54f, 0.75f))
        })
        {
            Node3D pivot = (Node3D)root.FindChild(name, true, false)!;
            if (!pivot.Position.IsEqualApprox(expected))
                return Fail($"wheel pivot is not centred on its wheel: {name} at {pivot.Position}", out error);
        }
        for (int i = 0; i < RequiredSockets.Length; i++)
            if (root.FindChild(RequiredSockets[i], true, false) is not Node3D)
                return Fail($"required socket missing: {RequiredSockets[i]}", out error);

        Node3D selection = (Node3D)root.FindChild("Socket_Selection", true, false)!;
        Node3D health = (Node3D)root.FindChild("Socket_Health", true, false)!;
        Node3D weapon = (Node3D)root.FindChild("Socket_Weapon_Primary", true, false)!;
        if (!selection.Position.IsEqualApprox(Vector3.Zero))
            return Fail("selection socket is not at ground-centre origin", out error);
        if (health.Position.Y < 2.3f)
            return Fail("health socket is not above the readable silhouette", out error);
        if (weapon.Position.Z > -2.5f || weapon.Position.Y < 0.8f)
            return Fail($"weapon socket did not import at the forward tool tip ({weapon.Position})", out error);

        report = new M85AssetPipelineValidationReport(
            closeTriangles, combatTriangles, strategicTriangles,
            closeMeshes + combatMeshes + strategicMeshes,
            RequiredPivots.Length, RequiredSockets.Length);
        return true;
    }

    public static void ShowOnlyLod(Node3D root, string lodName)
    {
        foreach (string candidate in new[] { "Close", "Combat", "Strategic" })
            if (root.FindChild($"LOD_{candidate}", true, false) is Node3D lod)
                lod.Visible = candidate == lodName;
    }

    private static int CountTriangles(Node root, out int meshes)
    {
        int triangles = 0;
        meshes = 0;
        CountTrianglesRecursive(root, ref triangles, ref meshes);
        return triangles;
    }

    private static void CountTrianglesRecursive(Node root, ref int triangles, ref int meshes)
    {
        foreach (Node child in root.GetChildren())
        {
            if (child is MeshInstance3D { Mesh: Mesh source })
            {
                meshes++;
                triangles += source.GetFaces().Length / 3;
            }
            CountTrianglesRecursive(child, ref triangles, ref meshes);
        }
    }

    private static bool ValidateGeometryNames(Node root, string lod, bool allowSupport,
        bool allowMicro, out string error)
    {
        foreach (Node child in root.GetChildren())
        {
            if (child is MeshInstance3D)
            {
                string name = child.Name.ToString();
                string[] parts = name.Split('_');
                if (parts.Length < 4 || parts[^1] != lod ||
                    parts[0] is not ("Body" or "Accent" or "Neutral" or "Tool" or "Rubber" or "Glass" or "Lamp") ||
                    parts[1] is not ("Hero" or "Support" or "Micro"))
                    return Fail($"geometry naming contract changed: {name}", out error);
                if (!allowSupport && parts[1] == "Support")
                    return Fail($"{lod} contains Support geometry: {name}", out error);
                if (!allowMicro && parts[1] == "Micro")
                    return Fail($"{lod} contains Micro geometry: {name}", out error);
            }
            if (!ValidateGeometryNames(child, lod, allowSupport, allowMicro, out error)) return false;
        }
        error = string.Empty;
        return true;
    }

    private static bool Fail(string message, out string error)
    {
        error = message;
        return false;
    }
}
