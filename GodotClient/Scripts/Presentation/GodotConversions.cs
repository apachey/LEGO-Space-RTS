using Godot;
using LegoSpaceRTS.SimCore;

namespace LegoSpaceRTS.Presentation;

public static class GodotConversions
{
    public const float WorldUnitsPerBuildCell = 2f;
    public static float ToPresentationFloat(this Fix32 value) => value.Raw / 65536f;
    public static Vector3 ToWorld(this FixVec2 value, float y = 0f) => new(value.X.ToPresentationFloat() * WorldUnitsPerBuildCell, y, value.Y.ToPresentationFloat() * WorldUnitsPerBuildCell);
    public static Vector2 ToBuildXZ(this Vector3 world) => new(world.X / WorldUnitsPerBuildCell, world.Z / WorldUnitsPerBuildCell);
    public static FixVec2 ToFixedBuild(this Vector3 world)
    {
        Vector2 p = world.ToBuildXZ();
        return new FixVec2(Fix32.FromRatio((int)Mathf.Round(p.X * 1024f), 1024), Fix32.FromRatio((int)Mathf.Round(p.Y * 1024f), 1024));
    }
}
