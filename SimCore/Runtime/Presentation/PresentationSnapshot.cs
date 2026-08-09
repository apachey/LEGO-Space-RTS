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
    public readonly ResourceVisualState ResourceState;
    public readonly bool Snap;
    public PresentationEntity(EntityId entityId, ContentId contentType, byte owner, FixVec2 position, Angle16 orientation, MovementState movement, VisibilityState visibility, FootprintClass footprint, SelectableKind selectableKind, bool snap = false, ResourceVisualState resourceState = ResourceVisualState.Full)
    { EntityId = entityId; ContentType = contentType; Owner = owner; Position = position; Orientation = orientation; Movement = movement; Visibility = visibility; Footprint = footprint; SelectableKind=selectableKind; ResourceState=resourceState; Snap = snap; }
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
            if (world.Entities.ResourceNode.TryGet(id, out ResourceNode resource) && world.Entities.Transform.TryGet(id, out SimTransform resourceTransform) && world.Entities.Selectable.TryGet(id, out Selectable resourceSelectable))
            {
                int rx = resourceTransform.Position.X.FloorToInt(), ry = resourceTransform.Position.Y.FloorToInt();
                VisibilityState resourceVisibility = world.Fog.Get(viewerPlayer, rx, ry);
                if (resourceVisibility != VisibilityState.Visible) continue;
                FootprintClass visualSize = resource.DepositSize switch
                {
                    ResourceDepositSize.Small => FootprintClass.Small,
                    ResourceDepositSize.Standard => FootprintClass.Medium,
                    ResourceDepositSize.Rich => FootprintClass.Large,
                    _ => FootprintClass.Huge
                };
                list.Add(new PresentationEntity(id, resourceSelectable.ContentType, byte.MaxValue, resourceTransform.Position, resourceTransform.Orientation,
                    MovementState.Idle, resourceVisibility, visualSize, SelectableKind.ResourceNode, resourceState: resource.VisualState));
                continue;
            }
            if (world.Entities.ResourceReceiver.Has(id) && world.Entities.Transform.TryGet(id, out SimTransform receiverTransform) &&
                world.Entities.Ownership.TryGet(id, out Ownership receiverOwner) && world.Entities.Selectable.TryGet(id, out Selectable receiverSelectable))
            {
                VisibilityState receiverVisibility = receiverOwner.PlayerSlot == viewerPlayer ? VisibilityState.Visible : world.Fog.Get(viewerPlayer, receiverTransform.Position.X.FloorToInt(), receiverTransform.Position.Y.FloorToInt());
                if (receiverOwner.PlayerSlot != viewerPlayer && receiverVisibility != VisibilityState.Visible) continue;
                list.Add(new PresentationEntity(id, receiverSelectable.ContentType, receiverOwner.PlayerSlot, receiverTransform.Position, receiverTransform.Orientation,
                    MovementState.Idle, receiverVisibility, FootprintClass.Huge, SelectableKind.Building));
                continue;
            }
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
