using System.Collections.Generic;

namespace LegoSpaceRTS.SimCore
{
public readonly struct PresentationEntity
{
    public readonly EntityId EntityId; public readonly ContentId ContentType; public readonly byte Owner;
    public readonly FixVec2 Position; public readonly Angle16 Orientation; public readonly MovementState Movement;
    public readonly VisibilityState Visibility;
    public readonly FootprintClass Footprint;
    public readonly SelectableKind SelectableKind;
    public readonly bool Snap;
    public PresentationEntity(EntityId entityId, ContentId contentType, byte owner, FixVec2 position, Angle16 orientation, MovementState movement, VisibilityState visibility, FootprintClass footprint, SelectableKind selectableKind, bool snap = false)
    { EntityId = entityId; ContentType = contentType; Owner = owner; Position = position; Orientation = orientation; Movement = movement; Visibility = visibility; Footprint = footprint; SelectableKind=selectableKind; Snap = snap; }
}

public sealed class PresentationSnapshot
{
    public SimTick Tick { get; }
    public IReadOnlyList<PresentationEntity> Entities { get; }
    public PresentationSnapshot(SimTick tick, List<PresentationEntity> entities) { Tick = tick; Entities = entities; }

    public static PresentationSnapshot Capture(SimulationWorld world, byte viewerPlayer)
    {
        List<PresentationEntity> list = new(world.Entities.Alive.Count);
        IReadOnlyList<EntityId> alive = world.Entities.Alive;
        for (int i = 0; i < alive.Count; i++)
        {
            EntityId id = alive[i];
            if (!world.Entities.Transform.TryGet(id, out SimTransform t) || !world.Entities.Ownership.TryGet(id, out Ownership o) || !world.Entities.Selectable.TryGet(id, out Selectable s) || !world.Entities.Movement.TryGet(id, out Movement m) || !world.Entities.Navigation.TryGet(id, out NavigationAgent n)) continue;
            int fx = t.Position.X.FloorToInt(), fy = t.Position.Y.FloorToInt();
            VisibilityState v = o.PlayerSlot == viewerPlayer ? VisibilityState.Visible : world.Fog.Get(viewerPlayer, fx, fy);
            // M2 has no last-known enemy record yet. Never publish current hidden-enemy truth as an "Explored" entity.
            if (o.PlayerSlot != viewerPlayer && v != VisibilityState.Visible) continue;
            list.Add(new PresentationEntity(id, s.ContentType, o.PlayerSlot, t.Position, t.Orientation, m.State, v, n.Footprint, s.Kind));
        }
        return new PresentationSnapshot(world.Tick, list);
    }
}
}
