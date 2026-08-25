using LegoSpaceRTS.SimCore;
using LegoSpaceRTS.UI;

namespace LegoSpaceRTS.Presentation;

/// <summary>
/// Builds a bounded minimap frame from the same recipient-legal presentation
/// snapshot and fog knowledge used by the world view. Retained enemy knowledge
/// is bounded to last-observed static structures/resources and observed tube segments.
/// </summary>
public sealed class MinimapPresentationSource
{
    private readonly Dictionary<uint, HudMinimapMarkerFrame> _rememberedStatic = new();
    private readonly Dictionary<ulong, HudMinimapLineFrame> _rememberedEnemyTubeSegments = new();
    private byte[] _terrain = Array.Empty<byte>();
    private int _terrainRevision = int.MinValue;

    public int RememberedStaticCount => _rememberedStatic.Count;

    public HudMinimapFrame Capture(SimulationWorld world, PresentationSnapshot snapshot,
        IReadOnlyList<EntityId> selected, byte viewerPlayer = 0)
    {
        if (_terrainRevision != world.Map.TopologyVersion || _terrain.Length != HudMinimapFrame.CellCount)
        {
            _terrain = BuildTerrain(world.Map);
            _terrainRevision = world.Map.TopologyVersion;
        }

        byte[] knowledge = CaptureKnowledge(world.Fog, viewerPlayer);
        HashSet<uint> selectedIds = new(selected.Count);
        for (int i = 0; i < selected.Count; i++) selectedIds.Add(selected[i].Value);
        HashSet<uint> currentIds = new(snapshot.Entities.Count);
        List<HudMinimapMarkerFrame> markers = new(snapshot.Entities.Count + _rememberedStatic.Count);

        for (int i = 0; i < snapshot.Entities.Count; i++)
        {
            PresentationEntity entity = snapshot.Entities[i];
            currentIds.Add(entity.EntityId.Value);
            if (entity.IsDestroyed)
            {
                _rememberedStatic.Remove(entity.EntityId.Value);
                continue;
            }

            HudMinimapMarkerFrame marker = Marker(entity, viewerPlayer, selectedIds.Contains(entity.EntityId.Value));
            markers.Add(marker);
            if ((marker.Relation == HudMinimapRelation.Enemy && marker.Kind == HudMinimapMarkerKind.Structure) ||
                marker.Kind == HudMinimapMarkerKind.Resource)
            {
                RememberStatic(marker);
            }
        }

        List<uint> confirmedAbsent = new();
        foreach (KeyValuePair<uint, HudMinimapMarkerFrame> pair in _rememberedStatic)
        {
            if (currentIds.Contains(pair.Key)) continue;
            HudMinimapMarkerFrame previous = pair.Value;
            int x = Math.Clamp((int)previous.BuildX, 0, HudMinimapFrame.Width - 1);
            int y = Math.Clamp((int)previous.BuildY, 0, HudMinimapFrame.Height - 1);
            byte state = knowledge[y * HudMinimapFrame.Width + x];
            if (state == (byte)VisibilityState.Visible)
            {
                confirmedAbsent.Add(pair.Key);
                continue;
            }
            if (state == (byte)VisibilityState.Explored) markers.Add(CloneMarker(previous, remembered: true));
        }
        for (int i = 0; i < confirmedAbsent.Count; i++) _rememberedStatic.Remove(confirmedAbsent[i]);

        markers.Sort(static (left, right) =>
        {
            int current = left.Remembered.CompareTo(right.Remembered);
            if (current != 0) return current;
            current = RelationPriority(left.Relation).CompareTo(RelationPriority(right.Relation));
            return current != 0 ? current : left.StableId.CompareTo(right.StableId);
        });
        if (markers.Count > HudMinimapFrame.MarkerCapacity)
            markers.RemoveRange(HudMinimapFrame.MarkerCapacity, markers.Count - HudMinimapFrame.MarkerCapacity);

        return new HudMinimapFrame
        {
            Revision = snapshot.Tick.Value,
            TerrainRevision = _terrainRevision,
            Terrain = _terrain,
            Knowledge = knowledge,
            Markers = markers,
            Lines = CaptureLegalNetworkLines(world, viewerPlayer, knowledge),
            Pings = CaptureLegalPings(world, snapshot, viewerPlayer)
        };
    }

    private static byte[] BuildTerrain(MapGrid map)
    {
        byte[] terrain = new byte[HudMinimapFrame.CellCount];
        for (int y = 0; y < HudMinimapFrame.Height; y++)
        for (int x = 0; x < HudMinimapFrame.Width; x++)
        {
            MapCellFlags combined = MapCellFlags.None;
            for (int offsetY = 0; offsetY < MapGrid.NavPerBuild; offsetY++)
            for (int offsetX = 0; offsetX < MapGrid.NavPerBuild; offsetX++)
                combined |= map.GetFlags(x * MapGrid.NavPerBuild + offsetX, y * MapGrid.NavPerBuild + offsetY);
            HudMinimapTerrain kind = (combined & MapCellFlags.Excavatable) != 0 ? HudMinimapTerrain.Excavatable :
                (combined & MapCellFlags.Impassable) != 0 ? HudMinimapTerrain.Blocked :
                (combined & MapCellFlags.Rough) != 0 ? HudMinimapTerrain.Rough : HudMinimapTerrain.Ground;
            terrain[y * HudMinimapFrame.Width + x] = (byte)kind;
        }
        return terrain;
    }

    private static byte[] CaptureKnowledge(FogState fog, byte viewerPlayer)
    {
        byte[] knowledge = new byte[HudMinimapFrame.CellCount];
        for (int y = 0; y < HudMinimapFrame.Height; y++)
        for (int x = 0; x < HudMinimapFrame.Width; x++)
            knowledge[y * HudMinimapFrame.Width + x] = (byte)fog.Get(viewerPlayer, x, y);
        return knowledge;
    }

    private static HudMinimapMarkerFrame Marker(PresentationEntity entity, byte viewerPlayer, bool selected)
    {
        HudMinimapMarkerKind kind = entity.SelectableKind == SelectableKind.ResourceNode ? HudMinimapMarkerKind.Resource :
            entity.IsTrueAir ? HudMinimapMarkerKind.TrueAir :
            entity.SelectableKind == SelectableKind.Building ? HudMinimapMarkerKind.Structure : HudMinimapMarkerKind.GroundMobile;
        HudMinimapRelation relation = entity.Owner == viewerPlayer ? HudMinimapRelation.Owned :
            entity.Owner == byte.MaxValue ? HudMinimapRelation.Neutral : HudMinimapRelation.Enemy;
        return new HudMinimapMarkerFrame
        {
            StableId = entity.EntityId.Value,
            Owner = entity.Owner,
            BuildX = entity.Position.X.ToPresentationFloat(),
            BuildY = entity.Position.Y.ToPresentationFloat(),
            Kind = kind,
            Relation = relation,
            Selected = selected
        };
    }

    private static HudMinimapMarkerFrame CloneMarker(HudMinimapMarkerFrame marker, bool remembered) => new()
    {
        StableId = marker.StableId,
        Owner = marker.Owner,
        BuildX = marker.BuildX,
        BuildY = marker.BuildY,
        Kind = marker.Kind,
        Relation = marker.Relation,
        Remembered = remembered,
        Selected = false
    };

    private void RememberStatic(HudMinimapMarkerFrame marker)
    {
        if (!_rememberedStatic.ContainsKey(marker.StableId) && _rememberedStatic.Count >= HudMinimapFrame.MarkerCapacity)
        {
            uint eviction = uint.MaxValue;
            foreach (uint stableId in _rememberedStatic.Keys) if (stableId < eviction) eviction = stableId;
            if (eviction != uint.MaxValue) _rememberedStatic.Remove(eviction);
        }
        _rememberedStatic[marker.StableId] = CloneMarker(marker, remembered: false);
    }

    private static int RelationPriority(HudMinimapRelation relation) => relation switch
    {
        HudMinimapRelation.Owned => 0,
        HudMinimapRelation.Allied => 1,
        HudMinimapRelation.Enemy => 2,
        _ => 3
    };

    private List<HudMinimapLineFrame> CaptureLegalNetworkLines(SimulationWorld world, byte viewerPlayer, byte[] knowledge)
    {
        List<HudMinimapLineFrame> lines = new();
        HashSet<ulong> confirmedVisibleSegments = new();
        IReadOnlyList<EntityId> alive = world.Entities.Alive;
        for (int i = 0; i < alive.Count; i++)
        {
            EntityId id = alive[i];
            if (!world.Entities.TubeLink.TryGet(id, out TubeLink link) ||
                !world.Entities.Ownership.TryGet(id, out Ownership owner)) continue;
            if (owner.PlayerSlot == viewerPlayer)
            {
                if (lines.Count >= HudMinimapFrame.LineCapacity ||
                    !world.Entities.Transform.TryGet(link.EndpointA, out SimTransform from) ||
                    !world.Entities.Transform.TryGet(link.EndpointB, out SimTransform to)) continue;
                lines.Add(new HudMinimapLineFrame
                {
                    StableId = id.Value,
                    FromBuildX = from.Position.X.ToPresentationFloat(), FromBuildY = from.Position.Y.ToPresentationFloat(),
                    ToBuildX = to.Position.X.ToPresentationFloat(), ToBuildY = to.Position.Y.ToPresentationFloat(),
                    Kind = HudMinimapLineKind.OwnedNetwork, Operational = link.IsOperational
                });
                continue;
            }
            TubeRoute? route = world.GetTubeRoute(id);
            if (route is null) continue;
            for (int segment = 1; segment < route.Cells.Count; segment++)
            {
                TubeBuildCell from = route.Cells[segment - 1], to = route.Cells[segment];
                if (!MapCellInBounds(from.X, from.Y) || !MapCellInBounds(to.X, to.Y)) continue;
                byte fromKnowledge = knowledge[from.Y * HudMinimapFrame.Width + from.X];
                byte toKnowledge = knowledge[to.Y * HudMinimapFrame.Width + to.X];
                ulong key = ((ulong)id.Value << 32) | (uint)segment;
                if (fromKnowledge == (byte)VisibilityState.Visible || toKnowledge == (byte)VisibilityState.Visible)
                    confirmedVisibleSegments.Add(key);
                if (fromKnowledge != (byte)VisibilityState.Visible || toKnowledge != (byte)VisibilityState.Visible) continue;
                RememberEnemyTubeSegment(key, new HudMinimapLineFrame
                {
                    StableId = unchecked(id.Value * 397u + (uint)segment),
                    FromBuildX = from.X + 0.5f, FromBuildY = from.Y + 0.5f,
                    ToBuildX = to.X + 0.5f, ToBuildY = to.Y + 0.5f,
                    Kind = HudMinimapLineKind.KnownEnemyNetwork, Operational = link.IsOperational
                });
            }
        }

        List<ulong> disproved = new();
        foreach (KeyValuePair<ulong, HudMinimapLineFrame> pair in _rememberedEnemyTubeSegments)
        {
            HudMinimapLineFrame segment = pair.Value;
            bool visible = KnowledgeAt(knowledge, segment.FromBuildX, segment.FromBuildY) == (byte)VisibilityState.Visible ||
                KnowledgeAt(knowledge, segment.ToBuildX, segment.ToBuildY) == (byte)VisibilityState.Visible;
            if (visible && !confirmedVisibleSegments.Contains(pair.Key)) disproved.Add(pair.Key);
        }
        for (int i = 0; i < disproved.Count; i++) _rememberedEnemyTubeSegments.Remove(disproved[i]);

        List<ulong> retained = new(_rememberedEnemyTubeSegments.Keys);
        retained.Sort();
        for (int i = 0; i < retained.Count && lines.Count < HudMinimapFrame.LineCapacity; i++)
        {
            HudMinimapLineFrame segment = _rememberedEnemyTubeSegments[retained[i]];
            if (KnowledgeAt(knowledge, segment.FromBuildX, segment.FromBuildY) != (byte)VisibilityState.Unseen &&
                KnowledgeAt(knowledge, segment.ToBuildX, segment.ToBuildY) != (byte)VisibilityState.Unseen)
                lines.Add(segment);
        }
        return lines;
    }

    private void RememberEnemyTubeSegment(ulong key, HudMinimapLineFrame segment)
    {
        if (!_rememberedEnemyTubeSegments.ContainsKey(key) && _rememberedEnemyTubeSegments.Count >= HudMinimapFrame.LineCapacity)
        {
            ulong eviction = ulong.MaxValue;
            foreach (ulong candidate in _rememberedEnemyTubeSegments.Keys) if (candidate < eviction) eviction = candidate;
            if (eviction != ulong.MaxValue) _rememberedEnemyTubeSegments.Remove(eviction);
        }
        _rememberedEnemyTubeSegments[key] = segment;
    }

    private static bool MapCellInBounds(int x, int y) => (uint)x < HudMinimapFrame.Width && (uint)y < HudMinimapFrame.Height;

    private static byte KnowledgeAt(byte[] knowledge, float x, float y)
    {
        int cellX = Math.Clamp((int)x, 0, HudMinimapFrame.Width - 1);
        int cellY = Math.Clamp((int)y, 0, HudMinimapFrame.Height - 1);
        return knowledge[cellY * HudMinimapFrame.Width + cellX];
    }

    private static List<HudMinimapPingFrame> CaptureLegalPings(SimulationWorld world, PresentationSnapshot snapshot, byte viewerPlayer)
    {
        List<HudMinimapPingFrame> pings = new();
        HashSet<uint> used = new();
        for (int i = 0; i < snapshot.Entities.Count && pings.Count < HudMinimapFrame.PingCapacity; i++)
        {
            PresentationEntity entity = snapshot.Entities[i];
            if (entity.IsDestroyed) continue;
            if (world.Entities.SurgeZone.TryGet(entity.EntityId, out SurgeZone surge) &&
                surge.ActiveRemainingTicks > 0 && used.Add(entity.EntityId.Value))
            {
                pings.Add(new HudMinimapPingFrame
                {
                    StableId = entity.EntityId.Value,
                    BuildX = entity.Position.X.ToPresentationFloat(), BuildY = entity.Position.Y.ToPresentationFloat(),
                    Priority = entity.Owner == viewerPlayer ? HudAlertPriority.Normal : HudAlertPriority.High,
                    Phase = (snapshot.Tick.Value % SimClock.TicksPerSecond) / (float)SimClock.TicksPerSecond
                });
            }
            if (entity.Owner != viewerPlayer || entity.LastDamageTick < 0 ||
                snapshot.Tick.Value - entity.LastDamageTick > SimClock.TicksPerSecond * 2 || !used.Add(entity.EntityId.Value)) continue;
            int healthPercent = entity.MaximumHitPointsRaw <= 0 ? 100 :
                checked((int)((long)entity.CurrentHitPointsRaw * 100 / entity.MaximumHitPointsRaw));
            pings.Add(new HudMinimapPingFrame
            {
                StableId = entity.EntityId.Value,
                BuildX = entity.Position.X.ToPresentationFloat(), BuildY = entity.Position.Y.ToPresentationFloat(),
                Priority = healthPercent < 20 ? HudAlertPriority.Critical : HudAlertPriority.High,
                Phase = (snapshot.Tick.Value % SimClock.TicksPerSecond) / (float)SimClock.TicksPerSecond
            });
        }
        IReadOnlyList<EntityId> alive = world.Entities.Alive;
        for (int i = 0; i < alive.Count && pings.Count < HudMinimapFrame.PingCapacity; i++)
        {
            EntityId id = alive[i];
            if (!world.Entities.Ownership.TryGet(id, out Ownership owner) || owner.PlayerSlot != viewerPlayer ||
                !world.Entities.EnergyDomain.TryGet(id, out EnergyDomain domain) || !domain.IsBrownout ||
                !world.Entities.Transform.TryGet(id, out SimTransform transform) || !used.Add(id.Value)) continue;
            pings.Add(new HudMinimapPingFrame
            {
                StableId = id.Value,
                BuildX = transform.Position.X.ToPresentationFloat(), BuildY = transform.Position.Y.ToPresentationFloat(),
                Priority = HudAlertPriority.High,
                Phase = (snapshot.Tick.Value % SimClock.TicksPerSecond) / (float)SimClock.TicksPerSecond
            });
        }
        return pings;
    }
}
