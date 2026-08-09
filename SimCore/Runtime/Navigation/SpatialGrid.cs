using System.Collections.Generic;

namespace LegoSpaceRTS.SimCore
{
/// <summary>Deterministic uniform spatial buckets, 4 build cells per bucket.</summary>
public sealed class SpatialGrid
{
    public const int BucketBuildCells = 4;
    private const int Width = MapGrid.BuildWidth / BucketBuildCells;
    private const int Height = MapGrid.BuildHeight / BucketBuildCells;
    private readonly List<EntityId>[] _buckets = new List<EntityId>[Width * Height];

    public SpatialGrid()
    {
        for (int i = 0; i < _buckets.Length; i++) _buckets[i] = new List<EntityId>(8);
    }

    public void Rebuild(EntityStore entities)
    {
        for (int i = 0; i < _buckets.Length; i++) _buckets[i].Clear();
        IReadOnlyList<EntityId> alive = entities.Alive;
        for (int i = 0; i < alive.Count; i++)
        {
            EntityId id = alive[i];
            if (!entities.Transform.TryGet(id, out SimTransform transform)) continue;
            int bx = transform.Position.X.FloorToInt() / BucketBuildCells;
            int by = transform.Position.Y.FloorToInt() / BucketBuildCells;
            if (bx < 0) bx = 0; else if (bx >= Width) bx = Width - 1;
            if (by < 0) by = 0; else if (by >= Height) by = Height - 1;
            _buckets[by * Width + bx].Add(id);
        }
    }

    public void Query(FixVec2 center, int radiusBuildCells, List<EntityId> output)
    {
        output.Clear();
        int minX = (center.X.FloorToInt() - radiusBuildCells) / BucketBuildCells;
        int minY = (center.Y.FloorToInt() - radiusBuildCells) / BucketBuildCells;
        int maxX = (center.X.FloorToInt() + radiusBuildCells) / BucketBuildCells;
        int maxY = (center.Y.FloorToInt() + radiusBuildCells) / BucketBuildCells;
        if (minX < 0) minX = 0; if (minY < 0) minY = 0;
        if (maxX >= Width) maxX = Width - 1; if (maxY >= Height) maxY = Height - 1;
        for (int by = minY; by <= maxY; by++)
            for (int bx = minX; bx <= maxX; bx++)
            {
                List<EntityId> bucket = _buckets[by * Width + bx];
                for (int i = 0; i < bucket.Count; i++) output.Add(bucket[i]);
            }
        output.Sort(EntityIdComparer.Instance);
    }
}
}
