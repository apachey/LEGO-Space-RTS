using System;
using System.Collections.Generic;

namespace LegoSpaceRTS.SimCore
{
public interface ISimSystem { void Step(SimulationWorld world); }

public sealed class CommandExecutionSystem : ISimSystem
{
    private readonly FormationEntityComparer _formationComparer = new FormationEntityComparer();
    public void Step(SimulationWorld world)
    {
        world.Commands.DrainForTick(world.Tick, world.ScratchCommands);
        for (int c = 0; c < world.ScratchCommands.Count; c++) Execute(world, world.ScratchCommands[c]);
    }

    private void Execute(SimulationWorld world, CommandEnvelope command)
    {
        if (command.Type == SimCommandType.DebugOpenExcavatable)
        {
            world.OpenExcavatable(command.DebugFeatureId);
            return;
        }

        world.ScratchEntities.Clear();
        for (int i = 0; i < command.Entities.Length; i++)
        {
            EntityId id = command.Entities[i];
            if (!world.Entities.Exists(id) || !world.Entities.Ownership.TryGet(id, out Ownership owner) || owner.PlayerSlot != command.PlayerSlot || !world.Entities.Navigation.Has(id)) continue;
            world.ScratchEntities.Add(id);
        }
        world.ScratchEntities.Sort(EntityIdComparer.Instance);

        if (command.Type == SimCommandType.Harvest)
        {
            if (!world.Entities.ResourceNode.TryGet(command.TargetEntity, out ResourceNode node) || node.IsDepleted || !world.Entities.Transform.Has(command.TargetEntity)) return;
            bool queued = (command.Modifiers & CommandModifiers.Queue) != 0;
            for (int i = 0; i < world.ScratchEntities.Count; i++)
            {
                EntityId id = world.ScratchEntities[i];
                if (!world.Entities.Worker.Has(id) || !world.Entities.ResourceCarrier.TryGet(id, out ResourceCarrier carrier) || carrier.IsFull || carrier.Type != node.Type) continue;
                if (queued && IsBusy(world, id)) world.GetQueue(id).Enqueue(new UnitOrder(UnitOrderType.Harvest, FixVec2.Zero, targetEntity: command.TargetEntity));
                else
                {
                    if (!queued) world.GetQueue(id).Clear();
                    StartHarvest(world, id, command.TargetEntity);
                }
            }
            return;
        }

        if (command.Type == SimCommandType.Move)
        {
            FixVec2 centroid = FormationPlanner.ComputeCentroid(world, world.ScratchEntities);
            FixVec2 heading = command.TargetPosition - centroid;
            FootprintClass spacingClass = FormationPlanner.LargestFootprint(world, world.ScratchEntities);
            _formationComparer.World = world;
            world.ScratchEntities.Sort(_formationComparer);
            int columns = FormationPlanner.EstimateColumns(world, command.TargetPosition, world.ScratchEntities.Count, spacingClass);
            for (int i = 0; i < world.ScratchEntities.Count; i++)
            {
                EntityId id = world.ScratchEntities[i];
                FixVec2 desiredSlot = FormationPlanner.GetSlot(command.TargetPosition, i, world.ScratchEntities.Count, spacingClass, heading, columns);
                NavigationAgent slotNav = world.Entities.Navigation.Get(id);
                FixVec2 slotTarget = FormationPlanner.ResolvePassableSlot(world, desiredSlot, slotNav.Footprint);
                FormationIntent formation = world.ScratchEntities.Count > 1 ? new FormationIntent
                {
                    CohortId=command.Sequence,Anchor=command.TargetPosition,Heading=heading,SlotIndex=i,MemberCount=world.ScratchEntities.Count,
                    Columns=columns,SpacingFootprint=spacingClass,LastReflowTick=-1
                } : default;
                bool queued = (command.Modifiers & CommandModifiers.Queue) != 0;
                if (queued && IsBusy(world, id))
                {
                    world.GetQueue(id).Enqueue(new UnitOrder(UnitOrderType.Move, slotTarget, formation));
                }
                else
                {
                    if (!queued) world.GetQueue(id).Clear();
                    CancelHarvest(world, id);
                    SetMove(world, id, slotTarget, formation);
                }
            }
            return;
        }

        for (int i = 0; i < world.ScratchEntities.Count; i++)
        {
            EntityId id = world.ScratchEntities[i];
            ref NavigationAgent nav = ref world.Entities.Navigation.Get(id);
            ref Movement move = ref world.Entities.Movement.Get(id);
            world.GetQueue(id).Clear();
            CancelHarvest(world, id);
            StopMovement(world, id, ref nav, ref move);
            move.State = command.Type == SimCommandType.HoldPosition ? MovementState.Holding : MovementState.Idle;
        }
    }

    internal static bool IsBusy(SimulationWorld world, EntityId id)
    {
        if (world.Entities.Navigation.TryGet(id, out NavigationAgent nav) && nav.HasTarget) return true;
        return world.Entities.Worker.TryGet(id, out Worker worker) && (worker.TaskState == WorkerTaskState.MovingToResource || worker.TaskState == WorkerTaskState.Mining || worker.TaskState == WorkerTaskState.ReturningToReceiver);
    }

    internal static bool StartHarvest(SimulationWorld world, EntityId id, EntityId resourceId)
    {
        if (!world.Entities.Worker.Has(id) || !world.Entities.ResourceCarrier.TryGet(id, out ResourceCarrier carrier) || carrier.IsFull ||
            !world.Entities.ResourceNode.TryGet(resourceId, out ResourceNode node) || node.IsDepleted || carrier.Type != node.Type ||
            !world.Entities.Transform.TryGet(id, out SimTransform workerTransform) || !world.Entities.Transform.TryGet(resourceId, out SimTransform resourceTransform)) return false;

        ref Worker worker = ref world.Entities.Worker.Get(id);
        worker.ResourceTarget = resourceId;
        worker.ReceiverTarget = EntityId.None;
        worker.ExtractionTicks = 0;
        if (FixVec2.Distance(workerTransform.Position, resourceTransform.Position) <= HarvestSystem.InteractionRange)
        {
            ref NavigationAgent nearNav = ref world.Entities.Navigation.Get(id);
            ref Movement nearMove = ref world.Entities.Movement.Get(id);
            StopMovement(world, id, ref nearNav, ref nearMove);
            worker.TaskState = WorkerTaskState.Mining;
            return true;
        }

        FixVec2 direction = (workerTransform.Position - resourceTransform.Position).NormalizeSafe();
        if (direction.Equals(FixVec2.Zero)) direction = (id.Value & 1) == 0 ? new FixVec2(Fix32.One, Fix32.Zero) : new FixVec2(Fix32.Zero, Fix32.One);
        NavigationAgent nav = world.Entities.Navigation.Get(id);
        FixVec2 approach = FormationPlanner.ResolvePassableSlot(world, resourceTransform.Position + direction * Fix32.FromRatio(3, 2), nav.Footprint);
        worker.TaskState = WorkerTaskState.MovingToResource;
        SetMove(world, id, approach);
        return true;
    }

    internal static void CancelHarvest(SimulationWorld world, EntityId id)
    {
        if (!world.Entities.Worker.Has(id)) return;
        ref Worker worker = ref world.Entities.Worker.Get(id);
        worker.ResourceTarget = EntityId.None;
        worker.ReceiverTarget = EntityId.None;
        worker.ExtractionTicks = 0;
        worker.TaskState = world.Entities.ResourceCarrier.TryGet(id, out ResourceCarrier carrier) && carrier.Amount > 0
            ? WorkerTaskState.AwaitingDelivery : WorkerTaskState.Idle;
    }

    internal static bool StartDelivery(SimulationWorld world, EntityId id)
    {
        if (!world.Entities.Worker.Has(id) || !world.Entities.ResourceCarrier.TryGet(id, out ResourceCarrier carrier) || carrier.Amount == 0 ||
            !world.Entities.Ownership.TryGet(id, out Ownership owner) || !world.Entities.Transform.TryGet(id, out SimTransform workerTransform)) return false;
        EntityId receiverId = FindNearestReceiver(world, workerTransform.Position, owner.PlayerSlot, carrier.Type);
        if (receiverId == EntityId.None || !world.Entities.Transform.TryGet(receiverId, out SimTransform receiverTransform)) return false;
        ref Worker worker = ref world.Entities.Worker.Get(id);
        worker.ReceiverTarget = receiverId;
        worker.TaskState = WorkerTaskState.ReturningToReceiver;
        worker.ExtractionTicks = 0;
        FixVec2 direction = (workerTransform.Position - receiverTransform.Position).NormalizeSafe();
        if (direction.Equals(FixVec2.Zero)) direction = new FixVec2(Fix32.One, Fix32.Zero);
        NavigationAgent nav = world.Entities.Navigation.Get(id);
        FixVec2 approach = FormationPlanner.ResolvePassableSlot(world, receiverTransform.Position + direction * Fix32.FromRatio(5, 2), nav.Footprint);
        SetMove(world, id, approach);
        return true;
    }

    private static EntityId FindNearestReceiver(SimulationWorld world, FixVec2 origin, byte playerSlot, ResourceType type)
    {
        EntityId best = EntityId.None; Fix32 bestDistance = Fix32.MaxValue;
        IReadOnlyList<EntityId> alive = world.Entities.Alive;
        for (int i = 0; i < alive.Count; i++)
        {
            EntityId candidate = alive[i];
            if (!world.Entities.ResourceReceiver.TryGet(candidate, out ResourceReceiver receiver) || receiver.AcceptedType != type ||
                !world.Entities.Ownership.TryGet(candidate, out Ownership ownership) || ownership.PlayerSlot != playerSlot ||
                !world.Entities.Transform.TryGet(candidate, out SimTransform transform)) continue;
            Fix32 distance = FixVec2.Distance(origin, transform.Position);
            if (best == EntityId.None || distance < bestDistance || (distance == bestDistance && candidate.Value < best.Value)) { best = candidate; bestDistance = distance; }
        }
        return best;
    }

    internal static bool TryStartNextOrder(SimulationWorld world, EntityId id)
    {
        UnitCommandQueue queue = world.GetQueue(id);
        while (queue.TryPeek(out UnitOrder next))
        {
            queue.Dequeue();
            if (next.Type == UnitOrderType.Harvest)
            {
                if (StartHarvest(world, id, next.TargetEntity)) return true;
                continue;
            }
            if (next.Type == UnitOrderType.Move)
            {
                CancelHarvest(world, id);
                SetMove(world, id, next.Position, next.Formation);
                return true;
            }
        }
        return false;
    }

    internal static void StopMovement(SimulationWorld world, EntityId id, ref NavigationAgent nav, ref Movement move)
    {
        nav.HasTarget = false; nav.PathDirty = false; nav.Formation = default; move.PathIndex = 0;
        world.Corridors.Remove(id.Value);
        move.CurrentSpeed = Fix32.Zero; move.CurrentVelocity = FixVec2.Zero; move.DesiredMovement = FixVec2.Zero; move.State = MovementState.Idle;
    }

    internal static void SetMove(SimulationWorld world, EntityId id, FixVec2 target, FormationIntent formation = default)
    {
        ref NavigationAgent nav = ref world.Entities.Navigation.Get(id);
        ref Movement move = ref world.Entities.Movement.Get(id);
        nav.Target = target; nav.HasTarget = true; nav.PathDirty = true; nav.Formation=formation; move.PathIndex = 0; nav.RequestAge = 0;
        move.DesiredMovement = FixVec2.Zero; move.State = MovementState.WaitingForPath;
    }
}

public sealed class ResourceBankingSystem : ISimSystem
{
    public void Step(SimulationWorld world)
    {
        IReadOnlyList<EntityId> alive = world.Entities.Alive;
        for (int i = 0; i < alive.Count; i++)
        {
            EntityId id = alive[i];
            if (!world.Entities.ResourceReceiver.Has(id) || !world.Entities.ResourceBank.Has(id)) continue;
            ref ResourceReceiver receiver = ref world.Entities.ResourceReceiver.Get(id);
            ref ResourceBank bank = ref world.Entities.ResourceBank.Get(id);
            if (receiver.PendingHauledAmount < 0 || bank.ProcessedAmount < 0)
                throw new InvalidOperationException($"Resource inventory cannot be negative on entity {id.Value}.");
            if (receiver.AcceptedType != bank.Type || receiver.PendingHauledAmount == 0) continue;
            bank.ProcessedAmount = checked(bank.ProcessedAmount + receiver.PendingHauledAmount);
            receiver.PendingHauledAmount = 0;
        }
    }
}

public sealed class HarvestSystem : ISimSystem
{
    public static readonly Fix32 InteractionRange = Fix32.FromInt(2);
    public static readonly Fix32 ReceiverInteractionRange = Fix32.FromInt(3);

    public void Step(SimulationWorld world)
    {
        IReadOnlyList<EntityId> alive = world.Entities.Alive;
        for (int i = 0; i < alive.Count; i++)
        {
            EntityId id = alive[i];
            if (!world.Entities.Worker.Has(id) || !world.Entities.ResourceCarrier.Has(id) || !world.Entities.Transform.TryGet(id, out SimTransform workerTransform)) continue;
            ref Worker worker = ref world.Entities.Worker.Get(id);
            if (worker.TaskState == WorkerTaskState.ReturningToReceiver)
            {
                StepDelivery(world, id, workerTransform, ref worker);
                continue;
            }
            if (worker.TaskState != WorkerTaskState.MovingToResource && worker.TaskState != WorkerTaskState.Mining) continue;

            if (!world.Entities.ResourceNode.TryGet(worker.ResourceTarget, out ResourceNode resource) || resource.IsDepleted ||
                !world.Entities.Transform.TryGet(worker.ResourceTarget, out SimTransform resourceTransform))
            {
                FinishMining(world, id);
                continue;
            }

            Fix32 distance = FixVec2.Distance(workerTransform.Position, resourceTransform.Position);
            if (worker.TaskState == WorkerTaskState.MovingToResource)
            {
                if (distance > InteractionRange) continue;
                ref NavigationAgent nav = ref world.Entities.Navigation.Get(id);
                ref Movement move = ref world.Entities.Movement.Get(id);
                CommandExecutionSystem.StopMovement(world, id, ref nav, ref move);
                worker.TaskState = WorkerTaskState.Mining;
            }
            else if (distance > InteractionRange)
            {
                CommandExecutionSystem.StartHarvest(world, id, worker.ResourceTarget);
                continue;
            }

            worker.ExtractionTicks++;
            if (worker.ExtractionTicks < worker.TicksPerOre) continue;
            worker.ExtractionTicks = 0;
            if (!world.TryExtractResource(worker.ResourceTarget, 1, out int extracted) || extracted == 0)
            {
                FinishMining(world, id);
                continue;
            }
            ref ResourceCarrier carrier = ref world.Entities.ResourceCarrier.Get(id);
            carrier.Amount = checked((byte)(carrier.Amount + extracted));
            if (carrier.IsFull || world.Entities.ResourceNode.Get(worker.ResourceTarget).IsDepleted) FinishMining(world, id);
        }
    }

    private static void FinishMining(SimulationWorld world, EntityId id)
    {
        if (world.Entities.ResourceCarrier.TryGet(id, out ResourceCarrier carrier) && carrier.Amount > 0 && CommandExecutionSystem.StartDelivery(world, id)) return;
        FinishAssignment(world, id);
    }

    private static void StepDelivery(SimulationWorld world, EntityId id, SimTransform workerTransform, ref Worker worker)
    {
        if (!world.Entities.ResourceReceiver.Has(worker.ReceiverTarget) || !world.Entities.Transform.TryGet(worker.ReceiverTarget, out SimTransform receiverTransform))
        {
            worker.ReceiverTarget = EntityId.None;
            worker.TaskState = WorkerTaskState.AwaitingDelivery;
            return;
        }
        if (FixVec2.Distance(workerTransform.Position, receiverTransform.Position) > ReceiverInteractionRange) return;
        ref NavigationAgent nav = ref world.Entities.Navigation.Get(id);
        ref Movement move = ref world.Entities.Movement.Get(id);
        CommandExecutionSystem.StopMovement(world, id, ref nav, ref move);
        ref ResourceCarrier carrier = ref world.Entities.ResourceCarrier.Get(id);
        ref ResourceReceiver receiver = ref world.Entities.ResourceReceiver.Get(worker.ReceiverTarget);
        receiver.PendingHauledAmount = checked(receiver.PendingHauledAmount + carrier.Amount);
        carrier.Amount = 0;
        worker.ReceiverTarget = EntityId.None;
        if (world.Entities.ResourceNode.TryGet(worker.ResourceTarget, out ResourceNode resource) && !resource.IsDepleted && CommandExecutionSystem.StartHarvest(world, id, worker.ResourceTarget)) return;
        FinishAssignment(world, id);
    }

    private static void FinishAssignment(SimulationWorld world, EntityId id)
    {
        CommandExecutionSystem.CancelHarvest(world, id);
        if (CommandExecutionSystem.TryStartNextOrder(world, id)) return;
        ref NavigationAgent nav = ref world.Entities.Navigation.Get(id);
        ref Movement move = ref world.Entities.Movement.Get(id);
        CommandExecutionSystem.StopMovement(world, id, ref nav, ref move);
    }
}

internal sealed class EntityIdComparer : IComparer<EntityId>
{
    public static readonly EntityIdComparer Instance = new EntityIdComparer();
    private EntityIdComparer() { }
    public int Compare(EntityId a, EntityId b) => a.Value.CompareTo(b.Value);
}

public sealed class FormationEntityComparer : IComparer<EntityId>
{
    public SimulationWorld? World;
    public int Compare(EntityId a, EntityId b)
    {
        SimulationWorld world = World ?? throw new InvalidOperationException("Formation comparer is not bound to a world.");
        int ra = FormationPlanner.RoleRank(world, a), rb = FormationPlanner.RoleRank(world, b);
        int c = ra.CompareTo(rb); return c != 0 ? c : a.Value.CompareTo(b.Value);
    }
}

public static class FormationPlanner
{
    // M2 loose role-aware formation: heavy/frontline first, standards next, workers/support rear.
    public static int RoleRank(SimulationWorld world, EntityId id)
    {
        if (world.Entities.Selectable.TryGet(id, out Selectable selectable) && selectable.Kind == SelectableKind.Worker) return 2;
        if (world.Entities.Navigation.TryGet(id, out NavigationAgent nav) && (nav.Footprint == FootprintClass.Huge || nav.Footprint == FootprintClass.Large)) return 0;
        return 1;
    }

    public static FixVec2 ComputeCentroid(SimulationWorld world, IReadOnlyList<EntityId> ids)
    {
        if (ids.Count == 0) return FixVec2.Zero;
        long x = 0, y = 0; int count = 0;
        for (int i = 0; i < ids.Count; i++)
        {
            if (!world.Entities.Transform.TryGet(ids[i], out SimTransform t)) continue;
            x += t.Position.X.Raw; y += t.Position.Y.Raw; count++;
        }
        if (count == 0) return FixVec2.Zero;
        return new FixVec2(Fix32.FromRaw(checked((int)(x / count))), Fix32.FromRaw(checked((int)(y / count))));
    }

    public static FootprintClass LargestFootprint(SimulationWorld world, IReadOnlyList<EntityId> ids)
    {
        FootprintClass result = FootprintClass.Tiny;
        for (int i = 0; i < ids.Count; i++)
            if (world.Entities.Navigation.TryGet(ids[i], out NavigationAgent nav) && nav.Footprint > result) result = nav.Footprint;
        return result;
    }

    public static int EstimateColumns(SimulationWorld world, FixVec2 destination, int count, FootprintClass footprint)
    {
        int preferred = count <= 9 ? 3 : count <= 25 ? 5 : 8;
        NavCell center = MapGrid.BuildToNav(destination);
        int radius = FootprintRules.ClearanceNavCells(footprint);
        int clearX = 0;
        for (int x = center.X; x >= 0 && world.Pathfinder.IsPassable(new NavCell(checked((short)x), center.Y), footprint); x--) clearX++;
for (int x = center.X + 1; x < MapGrid.NavWidth && world.Pathfinder.IsPassable(new NavCell(checked((short)x), center.Y), footprint); x++) clearX++;

int clearY = 0;
for (int y = center.Y; y >= 0 && world.Pathfinder.IsPassable(new NavCell(center.X, checked((short)y)), footprint); y--) clearY++;
for (int y = center.Y + 1; y < MapGrid.NavHeight && world.Pathfinder.IsPassable(new NavCell(center.X, checked((short)y)), footprint); y++) clearY++;
        int usableNav = Math.Max(1, Math.Max(clearX, clearY) - radius * 2);
        int navSpacing = Math.Max(2, radius * 2 + 1);
        int available = Math.Max(1, usableNav / navSpacing);
        return Math.Max(1, Math.Min(preferred, available));
    }

    public static FixVec2 GetSlot(FixVec2 center, int index, int count, FootprintClass footprint, FixVec2 heading, int columns, bool spread = false)
    {
        if (count <= 1) return center;
        columns = Math.Max(1, Math.Min(columns, count));
        int row = index / columns;
        int col = index % columns;
        int rows = (count + columns - 1) / columns;
        Fix32 legacySpacing = Fix32.FromRatio(11 + (int)footprint * 4, 10);
        Fix32 collisionSpacing = FootprintRules.CollisionRadiusBuild(footprint) * Fix32.FromInt(2) + Fix32.FromRatio(35, 100);
        // Preserve any roomier legacy spacing while guaranteeing the canonical
        // collision diameter + 0.35 build-cell settling margin.
        Fix32 spacing = Fix32.Max(legacySpacing, collisionSpacing);
        if (spread) spacing *= Fix32.FromRatio(14,10); // Phase 06 +40% separation foundation.
        Fix32 lateral = Fix32.FromRatio((col * 2 - (columns - 1)), 2) * spacing;
        // First row is the front row; final row is the rear/support row.
        Fix32 longitudinal = Fix32.FromRatio(((rows - 1) - row * 2), 2) * spacing;
        FixVec2 forward = heading.NormalizeSafe();
        if (forward.Equals(FixVec2.Zero)) forward = new FixVec2(Fix32.One, Fix32.Zero);
        FixVec2 right = new FixVec2(-forward.Y, forward.X);
        return center + right * lateral + forward * longitudinal;
    }

    public static Fix32 SettlingRadius(int memberCount,FootprintClass footprint)
    {
        if(memberCount<=1)return FootprintRules.CollisionRadiusBuild(footprint)+Fix32.FromRatio(35,100);
        Fix32 legacySpacing=Fix32.FromRatio(11+(int)footprint*4,10);
        Fix32 collisionSpacing=FootprintRules.CollisionRadiusBuild(footprint)*Fix32.FromInt(2)+Fix32.FromRatio(35,100);
        Fix32 spacing=Fix32.Max(legacySpacing,collisionSpacing);
        return spacing*Fix32.FromRatio(memberCount-1,2)+FootprintRules.CollisionRadiusBuild(footprint)+Fix32.FromRatio(35,100);
    }

    public static FixVec2 ResolvePassableSlot(SimulationWorld world, FixVec2 desired, FootprintClass footprint)
    {
        NavCell origin = MapGrid.BuildToNav(desired);
        if (world.Pathfinder.IsPassable(origin, footprint)) return desired;

        const int MaxSearchRadiusNav = 16;
        for (int radius = 1; radius <= MaxSearchRadiusNav; radius++)
        {
            // Deterministic perimeter scan: top/bottom rows first, then left/right columns.
            for (int dx = -radius; dx <= radius; dx++)
            {
                int topX = origin.X + dx, topY = origin.Y - radius;
                if (TryPassable(world, topX, topY, footprint, out FixVec2 top)) return top;
                int bottomY = origin.Y + radius;
                if (TryPassable(world, topX, bottomY, footprint, out FixVec2 bottom)) return bottom;
            }
            for (int dy = -radius + 1; dy <= radius - 1; dy++)
            {
                int leftX = origin.X - radius, y = origin.Y + dy;
                if (TryPassable(world, leftX, y, footprint, out FixVec2 left)) return left;
                int rightX = origin.X + radius;
                if (TryPassable(world, rightX, y, footprint, out FixVec2 right)) return right;
            }
        }
        return desired;
    }

    public static bool ReflowCohort(SimulationWorld world, EntityId trigger)
    {
        if(!world.Entities.Navigation.TryGet(trigger,out NavigationAgent triggerNav)||!triggerNav.Formation.IsActive)return false;
        if(triggerNav.Formation.LastReflowTick>=0&&world.Tick.Value-triggerNav.Formation.LastReflowTick<SimClock.TicksPerSecond*3)return false;
        if(!world.Entities.Ownership.TryGet(trigger,out Ownership triggerOwner))return false;
        FormationIntent triggerIntent=triggerNav.Formation;
        world.ScratchEntities.Clear();
        IReadOnlyList<EntityId> alive=world.Entities.Alive;
        for(int i=0;i<alive.Count;i++)
        {
            EntityId id=alive[i];
            if(!world.Entities.Ownership.TryGet(id,out Ownership owner)||owner.PlayerSlot!=triggerOwner.PlayerSlot)continue;
            if(!world.Entities.Navigation.TryGet(id,out NavigationAgent nav)||!nav.HasTarget||!nav.Formation.IsActive||nav.Formation.CohortId!=triggerIntent.CohortId)continue;
            world.ScratchEntities.Add(id);
        }
        world.ScratchEntities.Sort(EntityIdComparer.Instance);
        if(world.ScratchEntities.Count==0)return false;
        int nextColumns=Math.Max(1,triggerIntent.Columns-1);
        bool release=triggerIntent.Columns<=1;
        for(int i=0;i<world.ScratchEntities.Count;i++)
        {
            EntityId id=world.ScratchEntities[i];ref NavigationAgent nav=ref world.Entities.Navigation.Get(id);ref Movement move=ref world.Entities.Movement.Get(id);
            FormationIntent intent=nav.Formation;intent.LastReflowTick=world.Tick.Value;
            if(release)
            {
                nav.Formation=default;
                SimTransform transform=world.Entities.Transform.Get(id);
                if(FixVec2.Distance(transform.Position,intent.Anchor)<=SettlingRadius(intent.MemberCount,intent.SpacingFootprint))nav.Target=transform.Position;
            }
            else
            {
                intent.Columns=nextColumns;
                FixVec2 desired=GetSlot(intent.Anchor,intent.SlotIndex,intent.MemberCount,intent.SpacingFootprint,intent.Heading,nextColumns);
                nav.Target=ResolvePassableSlot(world,desired,nav.Footprint);nav.Formation=intent;
            }
            nav.PathDirty=true;nav.RequestAge=Math.Max(nav.RequestAge,20);move.PathIndex=0;move.State=MovementState.StuckRecovery;
        }
        world.FormationReflowDiagnostics++;
        return true;
    }

    private static bool TryPassable(SimulationWorld world, int x, int y, FootprintClass footprint, out FixVec2 buildPosition)
    {
        if ((uint)x >= MapGrid.NavWidth || (uint)y >= MapGrid.NavHeight)
        {
            buildPosition = default;
            return false;
        }
        NavCell cell = new NavCell(checked((short)x), checked((short)y));
        if (!world.Pathfinder.IsPassable(cell, footprint))
        {
            buildPosition = default;
            return false;
        }
        buildPosition = MapGrid.NavCellCenterToBuild(cell);
        return true;
    }
}

public sealed class NavigationRequestSystem : ISimSystem
{
    public const int MaxRequestsPerTick = 24;
    private readonly RequestComparer _comparer = new RequestComparer();
    public void Step(SimulationWorld world)
    {
        world.ScratchEntities.Clear();
        IReadOnlyList<EntityId> alive = world.Entities.Alive;
        for (int i = 0; i < alive.Count; i++)
        {
            EntityId id = alive[i];
            if (!world.Entities.Navigation.Has(id)) continue;
            ref NavigationAgent nav = ref world.Entities.Navigation.Get(id);
            if (!nav.HasTarget) continue;
            if (nav.PathTopologyVersion != world.Map.TopologyVersion) nav.PathDirty = true;
            if (nav.PathDirty) { nav.RequestAge++; world.ScratchEntities.Add(id); }
        }
        _comparer.World = world;
        world.ScratchEntities.Sort(_comparer);
        int count = Math.Min(MaxRequestsPerTick, world.ScratchEntities.Count);
        for (int i = 0; i < count; i++)
        {
            EntityId id = world.ScratchEntities[i];
            ref NavigationAgent nav = ref world.Entities.Navigation.Get(id);
            SimTransform transform = world.Entities.Transform.Get(id);
            RouteCorridor corridor;
            if (!world.Corridors.TryGetValue(id.Value, out corridor)) { corridor = new RouteCorridor(); world.Corridors[id.Value] = corridor; }
            world.Pathfinder.FindCorridor(MapGrid.BuildToNav(transform.Position), MapGrid.BuildToNav(nav.Target), nav.Footprint, corridor, nav.Layer);
            world.PathRequestsProcessed++;
            nav.PathDirty = false; nav.PathTopologyVersion = world.Map.TopologyVersion; nav.RequestAge = 0;
            ref Movement move = ref world.Entities.Movement.Get(id);
            move.PathIndex = corridor.Cells.Count > 1 ? 1 : 0;
            move.State = corridor.IsValid ? MovementState.Moving : MovementState.StuckRecovery;
        }
    }

    private sealed class RequestComparer : IComparer<EntityId>
    {
        public SimulationWorld World = null!;
        public int Compare(EntityId a, EntityId b)
        {
            NavigationAgent na = World.Entities.Navigation.Get(a); NavigationAgent nb = World.Entities.Navigation.Get(b);
            Movement ma = World.Entities.Movement.Get(a); Movement mb = World.Entities.Movement.Get(b);
            bool aStuck = ma.StuckTicks >= 20, bStuck = mb.StuckTicks >= 20;
            if (aStuck != bStuck) return aStuck ? -1 : 1;
            int c = nb.RequestAge.CompareTo(na.RequestAge); if (c != 0) return c;
            c = FootprintRules.MovementPriority(nb.Footprint).CompareTo(FootprintRules.MovementPriority(na.Footprint)); if (c != 0) return c;
            return a.Value.CompareTo(b.Value);
        }
    }
}

public sealed class MovementIntentSystem : ISimSystem
{
    private static readonly Fix32 Two = Fix32.FromInt(2);
    public void Step(SimulationWorld world)
    {
        world.PendingVelocity.Clear();
        IReadOnlyList<EntityId> alive = world.Entities.Alive;
        for (int i = 0; i < alive.Count; i++)
        {
            EntityId id = alive[i];
            if (!world.Entities.Navigation.TryGet(id, out NavigationAgent nav) || !world.Entities.Movement.Has(id) || !world.Entities.Transform.TryGet(id, out SimTransform transform)) continue;
            ref Movement move = ref world.Entities.Movement.Get(id);
            move.DesiredMovement = FixVec2.Zero;
            if (!nav.HasTarget || move.State == MovementState.Holding || !world.Corridors.TryGetValue(id.Value, out RouteCorridor path) || move.PathIndex >= path.Cells.Count)
            { world.PendingVelocity[id.Value] = FixVec2.Zero; continue; }

            FixVec2 waypoint = MapGrid.NavCellCenterToBuild(path.Cells[move.PathIndex]);
            FixVec2 delta = waypoint - transform.Position;
            Fix32 distance = delta.Length();
            if (distance.Raw == 0) { world.PendingVelocity[id.Value] = FixVec2.Zero; continue; }
            // Intermediate waypoints are steering points, not stop points. v0.3 braked at every
            // 0.5-cell A* node, producing visibly jerky movement and queue delays.
            bool finalWaypoint = move.PathIndex >= path.Cells.Count - 1 && world.GetQueue(id).Count == 0;
            Fix32 brakingSpeed = finalWaypoint ? Fix32.Sqrt(Two * move.Deceleration * FixVec2.Distance(transform.Position, nav.Target)) : move.MaxSpeed;
            Fix32 desiredSpeed = Fix32.Min(move.MaxSpeed, brakingSpeed);
            move.DesiredMovement = delta.NormalizeSafe() * desiredSpeed;
            world.PendingVelocity[id.Value] = move.DesiredMovement * SimClock.TickSeconds;
        }
    }
}

public sealed class LocalSeparationSystem : ISimSystem
{
    public const int NeighborCap = 24;
    public const int LookaheadTicks = 6;
    private readonly List<EntityId> _neighbors = new List<EntityId>(64);
    private readonly NeighborDistanceComparer _neighborComparer = new NeighborDistanceComparer();
    private readonly MovementPriorityComparer _movementPriorityComparer = new MovementPriorityComparer();
    // Q16.16 checked-in deterministic rotation constants.
    private static readonly int[] Cos = { 65536, 63303, 63303, 56756, 56756, 46341, 46341, 32768, 32768, 0, 0, 32768, 0, -65536 };
    private static readonly int[] Sin = { 0, 16962, -16962, 32768, -32768, 46341, -46341, 56756, -56756, 65536, -65536, 0, 0, 0 };

    public void Step(SimulationWorld world)
    {
        world.CompressionUsed.Clear();
        IReadOnlyList<EntityId> alive = world.Entities.Alive;
        world.ScratchEntities.Clear();
        for (int i = 0; i < alive.Count; i++)
        {
            EntityId id = alive[i];
            if(!world.PendingVelocity.TryGetValue(id.Value,out FixVec2 desired)||desired.Equals(FixVec2.Zero)||!world.Entities.Navigation.TryGet(id,out NavigationAgent nav)||nav.Layer==MovementLayer.TrueAir)continue;
            world.ScratchEntities.Add(id);
        }
        _movementPriorityComparer.World=world;world.ScratchEntities.Sort(_movementPriorityComparer);
        for(int i=0;i<world.ScratchEntities.Count;i++)
        {
            EntityId id=world.ScratchEntities[i];FixVec2 desired=world.PendingVelocity[id.Value];
            SimTransform self=world.Entities.Transform.Get(id);NavigationAgent selfNav=world.Entities.Navigation.Get(id);Movement selfMove=world.Entities.Movement.Get(id);
            world.Spatial.Query(self.Position, 4, _neighbors);
            _neighborComparer.World=world;_neighborComparer.Center=self.Position;_neighbors.Sort(_neighborComparer);

            FixVec2 best = FixVec2.Zero; long bestScore = long.MaxValue;
            for (int candidateIndex = 0; candidateIndex < Cos.Length; candidateIndex++)
            {
                FixVec2 candidate;
                if(candidateIndex==11) candidate=desired*Fix32.Half; // slow
                else if(candidateIndex==12) candidate=FixVec2.Zero; // stop
                // The final candidate is a deterministic turn-around escape for
                // an expired compression contact that cannot clear sideways.
                else candidate=Rotate(desired,Cos[candidateIndex],Sin[candidateIndex]);
                long score = ScoreCandidate(world,id,self,selfNav,selfMove,candidate,desired,candidateIndex);
                if(score<bestScore){bestScore=score;best=candidate;}
            }
            world.PendingVelocity[id.Value]=best;
        }
        EnforceFinalSafety(world);
        UpdateFriendlyCompression(world);
    }

    private static void EnforceFinalSafety(SimulationWorld world)
    {
        IReadOnlyList<EntityId> alive=world.Entities.Alive;
        // Cancelling a step can invalidate an earlier prediction that expected
        // that mover to clear the way. Cancellations are monotonic, so at most
        // one per mover plus a final stable pass is required.
        for(int pass=0;pass<=alive.Count;pass++)
        {
            bool changed=false;
            for(int i=0;i<alive.Count;i++)
            {
                EntityId firstId=alive[i];
                if(!world.Entities.Transform.TryGet(firstId,out SimTransform first)||!world.Entities.Navigation.TryGet(firstId,out NavigationAgent firstNav)||firstNav.Layer==MovementLayer.TrueAir||!world.Entities.Movement.TryGet(firstId,out Movement firstMove))continue;
                for(int j=i+1;j<alive.Count;j++)
                {
                    EntityId secondId=alive[j];
                    if(!world.Entities.Transform.TryGet(secondId,out SimTransform second)||!world.Entities.Navigation.TryGet(secondId,out NavigationAgent secondNav)||secondNav.Layer==MovementLayer.TrueAir||!world.Entities.Movement.TryGet(secondId,out Movement secondMove))continue;
                    FixVec2 firstDesired=world.PendingVelocity.TryGetValue(firstId.Value,out FixVec2 fd)?fd:FixVec2.Zero;
                    FixVec2 secondDesired=world.PendingVelocity.TryGetValue(secondId.Value,out FixVec2 sd)?sd:FixVec2.Zero;
                    if(firstDesired.Equals(FixVec2.Zero)&&secondDesired.Equals(FixVec2.Zero))continue;

                    Fix32 nominal=FootprintRules.CollisionRadiusBuild(firstNav.Footprint)+FootprintRules.CollisionRadiusBuild(secondNav.Footprint);
                    bool friendly=world.Entities.Ownership.TryGet(firstId,out Ownership firstOwner)&&world.Entities.Ownership.TryGet(secondId,out Ownership secondOwner)&&firstOwner.PlayerSlot==secondOwner.PlayerSlot;
                    bool compressionAvailable=friendly&&firstMove.CompressionTicks<30&&secondMove.CompressionTicks<30;
                    Fix32 allowed=compressionAvailable?nominal*Fix32.FromRatio(85,100):nominal;
                    Fix32 currentDistance=FixVec2.Distance(first.Position,second.Position);
                    FixVec2 firstStep=MovementKinematics.ExecutableStep(firstMove,firstDesired);
                    FixVec2 secondStep=MovementKinematics.ExecutableStep(secondMove,secondDesired);
                    Fix32 projectedDistance=FixVec2.Distance(first.Position+firstStep,second.Position+secondStep);
                    bool unsafeStep=currentDistance>=allowed?projectedDistance<allowed:projectedDistance<currentDistance;
                    if(!unsafeStep)continue;

                    int priority=CompareMovementPriority(firstId,firstNav,secondId,secondNav);
                    EntityId yieldingId=priority>0?secondId:firstId;
                    EntityId otherId=priority>0?firstId:secondId;
                    FixVec2 yieldingDesired=world.PendingVelocity.TryGetValue(yieldingId.Value,out FixVec2 yd)?yd:FixVec2.Zero;
                    FixVec2 otherDesired=world.PendingVelocity.TryGetValue(otherId.Value,out FixVec2 od)?od:FixVec2.Zero;
                    Fix32 yieldingEscapeDistance=yieldingId==firstId
                        ?FixVec2.Distance(first.Position+firstStep,second.Position)
                        :FixVec2.Distance(first.Position,second.Position+secondStep);
                    if(!yieldingDesired.Equals(FixVec2.Zero)&&yieldingEscapeDistance>currentDistance&&!otherDesired.Equals(FixVec2.Zero))
                    {
                        world.PendingVelocity[otherId.Value]=FixVec2.Zero;
                        changed=true;
                    }
                    else if(!yieldingDesired.Equals(FixVec2.Zero))
                    {
                        world.PendingVelocity[yieldingId.Value]=FixVec2.Zero;
                        changed=true;
                    }
                    else if(!otherDesired.Equals(FixVec2.Zero))
                    {
                        world.PendingVelocity[otherId.Value]=FixVec2.Zero;
                        changed=true;
                    }
                }
            }
            if(!changed)return;
        }
    }

    private long ScoreCandidate(SimulationWorld world,EntityId selfId,SimTransform self,NavigationAgent selfNav,Movement selfMove,FixVec2 candidate,FixVec2 desired,int candidateIndex)
    {
        // Prefer route progress and small heading changes; stuck units progressively care less about heading change.
        FixVec2 executableCandidate=MovementKinematics.ExecutableStep(selfMove,candidate);
        FixVec2 immediate=self.Position+executableCandidate;
        NavCell immediateCell=MapGrid.BuildToNav(immediate);
        if(selfNav.Layer!=MovementLayer.TrueAir&&!world.Pathfinder.IsPassable(immediateCell,selfNav.Footprint))return long.MaxValue;
        if(world.Corridors.TryGetValue(selfId.Value,out RouteCorridor corridor)&&!corridor.Contains(immediateCell,selfMove.PathIndex))return long.MaxValue;
        long dotRaw = ((long)candidate.X.Raw*desired.X.Raw + (long)candidate.Y.Raw*desired.Y.Raw) >> Fix32.FractionalBits;
        long score = -dotRaw * 8L;
        int headingWeight=selfMove.StuckTicks>=20?2:12;
        score += (long)candidateIndex*headingWeight*Fix32.OneRaw;
        if(candidate.Equals(FixVec2.Zero))score += selfMove.StuckTicks>=20?Fix32.OneRaw:Fix32.OneRaw*20L;

        FixVec2 selfFuture=self.Position+executableCandidate*Fix32.FromInt(LookaheadTicks);
        int neighborCount=Math.Min(_neighbors.Count,NeighborCap);
        for(int n=0;n<neighborCount;n++)
        {
            EntityId otherId=_neighbors[n];if(otherId==selfId||!world.Entities.Transform.TryGet(otherId,out SimTransform other)||!world.Entities.Navigation.TryGet(otherId,out NavigationAgent otherNav))continue;
            if(otherNav.Layer==MovementLayer.TrueAir)continue;
            Movement otherMove=world.Entities.Movement.TryGet(otherId,out Movement om)?om:default;
            FixVec2 otherDesired=world.PendingVelocity.TryGetValue(otherId.Value,out FixVec2 ov)?ov:FixVec2.Zero;
            FixVec2 otherStep=MovementKinematics.ExecutableStep(otherMove,otherDesired);
            Fix32 min=FootprintRules.CollisionRadiusBuild(selfNav.Footprint)+FootprintRules.CollisionRadiusBuild(otherNav.Footprint);
            bool friendly=world.Entities.Ownership.TryGet(selfId,out Ownership a)&&world.Entities.Ownership.TryGet(otherId,out Ownership b)&&a.PlayerSlot==b.PlayerSlot;
            bool compressionAvailable=friendly&&selfMove.CompressionTicks<30&&otherMove.CompressionTicks<30;
            Fix32 allowed=compressionAvailable?min*Fix32.FromRatio(85,100):min;
            int priority=friendly?CompareMovementPriority(selfId,selfNav,otherId,otherNav):0;
            bool ownsRightOfWay=friendly&&priority>0;
            if(ownsRightOfWay)continue;

            // Immediate one-tick collision safety. v0.3 only evaluated the 12-tick horizon,
            // which allowed two units to overlap before the future penalty became useful.
            Fix32 currentDistance=FixVec2.Distance(self.Position,other.Position);
            Fix32 nextDistance=FixVec2.Distance(immediate,other.Position+otherStep);
            Fix32 yieldEscapeDistance=FixVec2.Distance(immediate,other.Position);
            bool yieldingEscape=friendly&&priority<0&&yieldEscapeDistance>currentDistance;
            if(currentDistance>=allowed&&nextDistance<allowed)
            {
                if(!yieldingEscape)return long.MaxValue;
                Fix32 overlap=allowed-nextDistance;score+=(long)overlap.Raw*overlap.Raw*4L;
            }
            if(currentDistance<allowed)
            {
                // Once a pair is compressed, never allow a step that makes the
                // overlap worse. The lower-priority mover must increase separation;
                // stopping remains the deterministic fallback when terrain blocks it.
                if(nextDistance<currentDistance&&!yieldingEscape)return long.MaxValue;
                if(priority<=0&&yieldEscapeDistance<=currentDistance&&!candidate.Equals(FixVec2.Zero))return long.MaxValue;
                Fix32 overlap=allowed-(yieldingEscape?yieldEscapeDistance:nextDistance);
                if(overlap.Raw>0)score+=(long)overlap.Raw*overlap.Raw*512L;
            }

            FixVec2 otherFuture=other.Position+otherStep*Fix32.FromInt(LookaheadTicks);
            Fix32 futureDistance=FixVec2.Distance(selfFuture,otherFuture);
            if(futureDistance<allowed)
            {
                Fix32 overlap=allowed-futureDistance;
                long yieldWeight=!friendly?64L:selfMove.StuckTicks>=20?96L:64L;
                score += (long)overlap.Raw*overlap.Raw*yieldWeight;
            }
        }
        return score;
    }

    private void UpdateFriendlyCompression(SimulationWorld world)
    {
        IReadOnlyList<EntityId> alive=world.Entities.Alive;
        for(int i=0;i<alive.Count;i++)
        {
            EntityId selfId=alive[i];
            if(!world.Entities.Transform.TryGet(selfId,out SimTransform self)||!world.Entities.Navigation.TryGet(selfId,out NavigationAgent selfNav)||selfNav.Layer==MovementLayer.TrueAir)continue;
            if(!world.Entities.Ownership.TryGet(selfId,out Ownership selfOwner))continue;
            Movement selfMove=world.Entities.Movement.TryGet(selfId,out Movement sm)?sm:default;
            FixVec2 selfDesired=world.PendingVelocity.TryGetValue(selfId.Value,out FixVec2 candidate)?candidate:FixVec2.Zero;
            FixVec2 selfStep=MovementKinematics.ExecutableStep(selfMove,selfDesired);
            world.Spatial.Query(self.Position,4,_neighbors);
            _neighborComparer.World=world;_neighborComparer.Center=self.Position;_neighbors.Sort(_neighborComparer);
            int neighborCount=Math.Min(_neighbors.Count,NeighborCap);
            for(int n=0;n<neighborCount;n++)
            {
                EntityId otherId=_neighbors[n];
                if(otherId==selfId||!world.Entities.Transform.TryGet(otherId,out SimTransform other)||!world.Entities.Navigation.TryGet(otherId,out NavigationAgent otherNav)||otherNav.Layer==MovementLayer.TrueAir)continue;
                if(!world.Entities.Ownership.TryGet(otherId,out Ownership otherOwner)||selfOwner.PlayerSlot!=otherOwner.PlayerSlot)continue;
                Movement otherMove=world.Entities.Movement.TryGet(otherId,out Movement om)?om:default;
                FixVec2 otherDesired=world.PendingVelocity.TryGetValue(otherId.Value,out FixVec2 ov)?ov:FixVec2.Zero;
                FixVec2 otherStep=MovementKinematics.ExecutableStep(otherMove,otherDesired);
                Fix32 nominal=FootprintRules.CollisionRadiusBuild(selfNav.Footprint)+FootprintRules.CollisionRadiusBuild(otherNav.Footprint);
                Fix32 currentDistance=FixVec2.Distance(self.Position,other.Position);
                Fix32 projectedDistance=FixVec2.Distance(self.Position+selfStep,other.Position+otherStep);
                if(currentDistance<nominal||projectedDistance<nominal)
                {
                    world.CompressionUsed[selfId.Value]=true;
                    break;
                }
            }
        }
    }

    private static int CompareMovementPriority(EntityId selfId,NavigationAgent selfNav,EntityId otherId,NavigationAgent otherNav)
    {
        int c=FootprintRules.MovementPriority(selfNav.Footprint).CompareTo(FootprintRules.MovementPriority(otherNav.Footprint));
        return c!=0?c:otherId.Value.CompareTo(selfId.Value);
    }

    private sealed class NeighborDistanceComparer : IComparer<EntityId>
    {
        public SimulationWorld? World;
        public FixVec2 Center;
        public int Compare(EntityId a,EntityId b)
        {
            SimulationWorld world=World ?? throw new InvalidOperationException("Neighbor comparer is not bound.");
            Fix32 da=world.Entities.Transform.TryGet(a,out SimTransform ta)?FixVec2.DistanceSquared(Center,ta.Position):Fix32.MaxValue;
            Fix32 db=world.Entities.Transform.TryGet(b,out SimTransform tb)?FixVec2.DistanceSquared(Center,tb.Position):Fix32.MaxValue;
            int c=da.Raw.CompareTo(db.Raw);return c!=0?c:a.Value.CompareTo(b.Value);
        }
    }

    private sealed class MovementPriorityComparer : IComparer<EntityId>
    {
        public SimulationWorld? World;
        public int Compare(EntityId a,EntityId b)
        {
            SimulationWorld world=World??throw new InvalidOperationException("Movement-priority comparer is not bound to a world.");
            NavigationAgent an=world.Entities.Navigation.Get(a),bn=world.Entities.Navigation.Get(b);
            int c=FootprintRules.MovementPriority(bn.Footprint).CompareTo(FootprintRules.MovementPriority(an.Footprint));
            return c!=0?c:a.Value.CompareTo(b.Value);
        }
    }

    private static FixVec2 Rotate(FixVec2 v,int cosRaw,int sinRaw)
    {
        long x=((long)v.X.Raw*cosRaw-(long)v.Y.Raw*sinRaw)>>Fix32.FractionalBits;
        long y=((long)v.X.Raw*sinRaw+(long)v.Y.Raw*cosRaw)>>Fix32.FractionalBits;
        return new FixVec2(Fix32.FromRaw(checked((int)x)),Fix32.FromRaw(checked((int)y)));
    }

}

public static class MovementKinematics
{
    public static Fix32 NextSpeed(Movement move,FixVec2 desiredStep)
    {
        if(desiredStep.Equals(FixVec2.Zero))return Fix32.Max(Fix32.Zero,move.CurrentSpeed-move.Deceleration*SimClock.TickSeconds);
        Fix32 targetSpeed=desiredStep.Length()/SimClock.TickSeconds;Fix32 speedDelta=targetSpeed-move.CurrentSpeed;
        if(speedDelta.Raw>0)return Fix32.Min(targetSpeed,move.CurrentSpeed+move.Acceleration*SimClock.TickSeconds);
        if(speedDelta.Raw<0)return Fix32.Max(targetSpeed,move.CurrentSpeed-move.Deceleration*SimClock.TickSeconds);
        return move.CurrentSpeed;
    }

    public static FixVec2 ExecutableStep(Movement move,FixVec2 desiredStep)
    {
        if(desiredStep.Equals(FixVec2.Zero))return FixVec2.Zero;
        FixVec2 delta=desiredStep.NormalizeSafe()*NextSpeed(move,desiredStep)*SimClock.TickSeconds;
        return delta.LengthSquared()>desiredStep.LengthSquared()?desiredStep:delta;
    }
}

public sealed class TransformMovementSystem : ISimSystem
{
    private static readonly Fix32 ArriveDistance = Fix32.FromRatio(3, 10);
    private static readonly Fix32 MeaningfulProgressDistance = Fix32.FromRatio(1, 4);

    public void Step(SimulationWorld world)
    {
        IReadOnlyList<EntityId> alive = world.Entities.Alive;
        for (int i = 0; i < alive.Count; i++)
        {
            EntityId id = alive[i]; if (!world.Entities.Transform.Has(id) || !world.Entities.Movement.Has(id) || !world.Entities.Navigation.Has(id)) continue;
            ref SimTransform transform = ref world.Entities.Transform.Get(id); ref Movement move = ref world.Entities.Movement.Get(id); ref NavigationAgent nav = ref world.Entities.Navigation.Get(id);
            int pathIndexBefore=move.PathIndex;FixVec2 progressTarget=nav.Target;
            if(world.Corridors.TryGetValue(id.Value,out RouteCorridor progressPath)&&move.PathIndex<progressPath.Cells.Count)progressTarget=MapGrid.NavCellCenterToBuild(progressPath.Cells[move.PathIndex]);
            FixVec2 desiredStep = world.PendingVelocity.TryGetValue(id.Value, out FixVec2 pending) ? pending : FixVec2.Zero;
            bool mayAdvance = !desiredStep.Equals(FixVec2.Zero);
            move.CurrentSpeed=MovementKinematics.NextSpeed(move,desiredStep);

            FixVec2 direction = mayAdvance ? desiredStep.NormalizeSafe() : move.CurrentVelocity.NormalizeSafe();
            if (!direction.Equals(FixVec2.Zero))
            {
                Angle16 targetOrientation = Angle16.FromDirection(direction);
                transform.Orientation = Angle16.TurnToward(transform.Orientation, targetOrientation, move.TurnRatePerTick);
            }

            if (mayAdvance && move.CurrentSpeed.Raw > 0)
            {
                move.CurrentVelocity = direction * move.CurrentSpeed;
                FixVec2 delta = move.CurrentVelocity * SimClock.TickSeconds;
                if (delta.LengthSquared() > desiredStep.LengthSquared()) delta = desiredStep;
                if (world.DiagnosticLastDelta.TryGetValue(id.Value, out FixVec2 previousDelta))
                {
                    long dot = (long)previousDelta.X.Raw * delta.X.Raw + (long)previousDelta.Y.Raw * delta.Y.Raw;
                    if (dot < 0)
                    {
                        int now = world.Tick.Value;
                        if (world.DiagnosticLastReversalTick.TryGetValue(id.Value, out int prior) && now - prior <= 10) world.OscillationDiagnostics++;
                        world.DiagnosticLastReversalTick[id.Value] = now;
                    }
                }
                world.DiagnosticLastDelta[id.Value] = delta;
                transform.Position += delta;
            }
            else
            {
                move.CurrentVelocity = FixVec2.Zero;
                if (!nav.HasTarget || move.State == MovementState.Holding) move.CurrentSpeed = Fix32.Zero;
            }

            bool compressed = world.CompressionUsed.TryGetValue(id.Value, out bool used) && used;
            move.CompressionTicks = compressed ? Math.Min(30, move.CompressionTicks + 1) : 0;

            if (world.Corridors.TryGetValue(id.Value, out RouteCorridor path) && move.PathIndex < path.Cells.Count)
            {
                FixVec2 waypoint = MapGrid.NavCellCenterToBuild(path.Cells[move.PathIndex]);
                if (FixVec2.Distance(transform.Position, waypoint) <= ArriveDistance) move.PathIndex++;
            }
            Fix32 accumulatedProgress=FixVec2.Distance(move.LastPosition,progressTarget)-FixVec2.Distance(transform.Position,progressTarget);
            bool meaningfulProgress=move.PathIndex>pathIndexBefore||accumulatedProgress>=MeaningfulProgressDistance;
            if(nav.HasTarget&&!meaningfulProgress)move.StuckTicks++;
            else
            {
                move.StuckTicks=0;
                move.LastPosition=transform.Position;
            }

            if (nav.HasTarget && FixVec2.Distance(transform.Position, nav.Target) <= Fix32.FromRatio(2, 5)) CompleteOrder(world, id, ref nav, ref move);
            else if (move.StuckTicks == 20)
            {
                move.State = MovementState.StuckRecovery; world.StuckRecoveryDiagnostics++;
            }
            else if (move.StuckTicks == 40)
            {
                nav.PathDirty = true; move.State = MovementState.StuckRecovery;
            }
            else if (move.StuckTicks >= 60 && move.StuckTicks % (SimClock.TicksPerSecond * 3) == 0)
            {
                if(!FormationPlanner.ReflowCohort(world,id))
                {
                    nav.PathDirty = true; nav.RequestAge = Math.Max(nav.RequestAge, 20); move.State = MovementState.StuckRecovery;
                }
            }
            else if (move.StuckTicks == 100)
            {
                world.DeadlockDiagnostics++; nav.PathDirty = true; nav.RequestAge = Math.Max(nav.RequestAge, 40); move.State = MovementState.StuckRecovery;
            }
        }
    }

    private static void CompleteOrder(SimulationWorld world, EntityId id, ref NavigationAgent nav, ref Movement move)
    {
        if (world.Entities.Worker.TryGet(id, out Worker worker) && (worker.TaskState == WorkerTaskState.MovingToResource || worker.TaskState == WorkerTaskState.ReturningToReceiver))
        {
            CommandExecutionSystem.StopMovement(world, id, ref nav, ref move);
        }
        else if (!CommandExecutionSystem.TryStartNextOrder(world, id))
        {
            CommandExecutionSystem.StopMovement(world, id, ref nav, ref move);
        }
    }
}

public sealed class SpatialIndexSystem : ISimSystem { public void Step(SimulationWorld world) => world.Spatial.Rebuild(world.Entities); }

public sealed class VisionSystem : ISimSystem
{
    public void Step(SimulationWorld world)
    {
        world.Fog.ClearCurrent();
        IReadOnlyList<EntityId> alive = world.Entities.Alive;
        for (int i = 0; i < alive.Count; i++)
        {
            EntityId id = alive[i];
            if (!world.Entities.Vision.TryGet(id, out Vision vision) || !world.Entities.Transform.TryGet(id, out SimTransform transform) || !world.Entities.Ownership.TryGet(id, out Ownership ownership)) continue;
            int sx = transform.Position.X.FloorToInt(), sy = transform.Position.Y.FloorToInt(); int r = vision.RadiusBuildCells;
            for (int y = sy - r; y <= sy + r; y++)
                for (int x = sx - r; x <= sx + r; x++)
                {
                    if ((uint)x >= MapGrid.BuildWidth || (uint)y >= MapGrid.BuildHeight) continue;
                    int dx = x - sx, dy = y - sy; if (dx * dx + dy * dy > r * r) continue;
                    if (HasLineOfSight(world.Map, sx, sy, x, y)) world.Fog.AddVisible(ownership.PlayerSlot, x, y);
                }
        }
    }

    private static bool HasLineOfSight(MapGrid map, int x0, int y0, int x1, int y1)
    {
        int e0=BuildElevation(map,x0,y0),e1=BuildElevation(map,x1,y1);
        int totalSteps=Math.Max(Math.Abs(x1-x0),Math.Abs(y1-y0));if(totalSteps==0)return true;
        int stepIndex=0;
        int dx = Math.Abs(x1 - x0), sx = x0 < x1 ? 1 : -1; int dy = -Math.Abs(y1 - y0), sy = y0 < y1 ? 1 : -1; int err = dx + dy;
        while (true)
        {
            if (x0 == x1 && y0 == y1) return true;
            int e2 = err << 1; if (e2 >= dy) { err += dy; x0 += sx; } if (e2 <= dx) { err += dx; y0 += sy; }
            stepIndex++;
            if (x0 == x1 && y0 == y1) return true;
            int nx = x0 * MapGrid.NavPerBuild, ny = y0 * MapGrid.NavPerBuild;
            for (int oy = 0; oy < MapGrid.NavPerBuild; oy++) for (int ox = 0; ox < MapGrid.NavPerBuild; ox++)
                if ((map.GetFlags(nx + ox, ny + oy) & MapCellFlags.GroundOccluder) != 0) return false;
            int terrainElevation=BuildElevation(map,x0,y0);
            int sightLineNumerator=e0*(totalSteps-stepIndex)+e1*stepIndex;
            if(terrainElevation*totalSteps>sightLineNumerator+totalSteps)return false;
        }
    }

    private static int BuildElevation(MapGrid map,int buildX,int buildY)
    {
        int nx=buildX*MapGrid.NavPerBuild,ny=buildY*MapGrid.NavPerBuild,max=sbyte.MinValue;
        for(int oy=0;oy<MapGrid.NavPerBuild;oy++)for(int ox=0;ox<MapGrid.NavPerBuild;ox++)max=Math.Max(max,map.GetElevation(nx+ox,ny+oy));
        return max;
    }
}
}
