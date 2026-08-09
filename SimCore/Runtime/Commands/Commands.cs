using System;
using System.Collections.Generic;
using System.IO;

namespace LegoSpaceRTS.SimCore
{
public enum SimCommandType : ushort { Move = 1, Stop = 2, HoldPosition = 3, DebugOpenExcavatable = 1000 }
[Flags] public enum CommandModifiers : byte { None = 0, Queue = 1 }

public readonly struct CommandEnvelope
{
    public readonly SimTick ExecutionTick;
    public readonly byte PlayerSlot;
    public readonly uint Sequence;
    public readonly SimCommandType Type;
    public readonly EntityId[] Entities;
    public readonly EntityId TargetEntity;
    public readonly FixVec2 TargetPosition;
    public readonly CommandModifiers Modifiers;
    public readonly ushort DebugFeatureId;

    public CommandEnvelope(SimTick executionTick, byte playerSlot, uint sequence, SimCommandType type, EntityId[] entities,
        FixVec2 targetPosition, CommandModifiers modifiers = CommandModifiers.None, EntityId targetEntity = default, ushort debugFeatureId = 0)
    {
        ValidateType(type);
        if (entities == null) throw new ArgumentNullException(nameof(entities));
        if (entities.Length > 128) throw new ArgumentOutOfRangeException(nameof(entities), "A command may address at most 128 entities in M2.");
        if ((modifiers & ~CommandModifiers.Queue) != 0) throw new ArgumentOutOfRangeException(nameof(modifiers), "Unknown M2 command modifier bits.");
        if (type != SimCommandType.Move && modifiers != CommandModifiers.None) throw new ArgumentException("Only Move may be queued in M2.", nameof(modifiers));
        ExecutionTick = executionTick; PlayerSlot = playerSlot; Sequence = sequence; Type = type; Entities = entities;
        TargetEntity = targetEntity; TargetPosition = targetPosition; Modifiers = modifiers; DebugFeatureId = debugFeatureId;
    }

    private static void ValidateType(SimCommandType type)
    {
        if (type != SimCommandType.Move && type != SimCommandType.Stop && type != SimCommandType.HoldPosition && type != SimCommandType.DebugOpenExcavatable)
            throw new ArgumentOutOfRangeException(nameof(type), type, "Unknown M2 command type.");
    }

    public void Write(BinaryWriter w)
    {
        w.Write(ExecutionTick.Value); w.Write(PlayerSlot); w.Write(Sequence); w.Write((ushort)Type); w.Write((byte)Modifiers);
        w.Write(TargetEntity.Value); w.Write(TargetPosition.X.Raw); w.Write(TargetPosition.Y.Raw); w.Write(DebugFeatureId);
        w.Write(Entities.Length); for (int i = 0; i < Entities.Length; i++) w.Write(Entities[i].Value);
    }

    public static CommandEnvelope Read(BinaryReader r)
    {
        SimTick tick = new(r.ReadInt32()); byte player = r.ReadByte(); uint seq = r.ReadUInt32(); SimCommandType type = (SimCommandType)r.ReadUInt16();
        ValidateType(type);
        CommandModifiers mod = (CommandModifiers)r.ReadByte(); EntityId target = new(r.ReadUInt32());
        FixVec2 pos = new(Fix32.FromRaw(r.ReadInt32()), Fix32.FromRaw(r.ReadInt32())); ushort feature = r.ReadUInt16();
        int count = r.ReadInt32(); if (count < 0 || count > 128) throw new InvalidDataException("Invalid command entity count.");
        EntityId[] ids = new EntityId[count]; for (int i = 0; i < count; i++) ids[i] = new EntityId(r.ReadUInt32());
        return new CommandEnvelope(tick, player, seq, type, ids, pos, mod, target, feature);
    }
}

public enum UnitOrderType : byte { Move = 1, Hold = 2 }
public readonly struct UnitOrder
{
    public readonly UnitOrderType Type;
    public readonly FixVec2 Position;
    public readonly FormationIntent Formation;
    public UnitOrder(UnitOrderType type, FixVec2 position, FormationIntent formation = default) { Type = type; Position = position; Formation = formation; }
}

public sealed class UnitCommandQueue
{
    public const int Capacity = 16;
    private readonly UnitOrder[] _orders = new UnitOrder[Capacity];
    public int Count { get; private set; }
    public UnitOrder this[int index] => _orders[index];
    public void Clear() => Count = 0;
    public bool Enqueue(UnitOrder order)
    {
        if (Count >= Capacity) return false; // deterministic overflow: reject newest queued order.
        _orders[Count++] = order; return true;
    }
    public bool TryPeek(out UnitOrder order) { if (Count == 0) { order = default; return false; } order = _orders[0]; return true; }
    public void Dequeue()
    {
        if (Count == 0) return;
        for (int i = 1; i < Count; i++) _orders[i - 1] = _orders[i];
        Count--;
    }
    public void Serialize(BinaryWriter w)
    {
        w.Write(Count);
        for (int i = 0; i < Count; i++)
        {
            UnitOrder order=_orders[i];w.Write((byte)order.Type);w.Write(order.Position.X.Raw);w.Write(order.Position.Y.Raw);
            FormationIntentCodec.Write(w,order.Formation);
        }
    }
    public void Deserialize(BinaryReader r)
    {
        Clear();int count=r.ReadInt32();if(count<0||count>Capacity)throw new InvalidDataException("Invalid order queue.");
        for(int i=0;i<count;i++)Enqueue(new UnitOrder((UnitOrderType)r.ReadByte(),new FixVec2(Fix32.FromRaw(r.ReadInt32()),Fix32.FromRaw(r.ReadInt32())),FormationIntentCodec.Read(r)));
    }
}

internal static class FormationIntentCodec
{
    public static void Write(BinaryWriter w,FormationIntent intent)
    {
        w.Write(intent.CohortId);w.Write(intent.Anchor.X.Raw);w.Write(intent.Anchor.Y.Raw);w.Write(intent.Heading.X.Raw);w.Write(intent.Heading.Y.Raw);
        w.Write(intent.SlotIndex);w.Write(intent.MemberCount);w.Write(intent.Columns);w.Write((byte)intent.SpacingFootprint);w.Write(intent.LastReflowTick);
    }
    public static FormationIntent Read(BinaryReader r)
    {
        FormationIntent intent=new FormationIntent
        {
            CohortId=r.ReadUInt32(),Anchor=new FixVec2(Fix32.FromRaw(r.ReadInt32()),Fix32.FromRaw(r.ReadInt32())),Heading=new FixVec2(Fix32.FromRaw(r.ReadInt32()),Fix32.FromRaw(r.ReadInt32())),
            SlotIndex=r.ReadInt32(),MemberCount=r.ReadInt32(),Columns=r.ReadInt32(),SpacingFootprint=(FootprintClass)r.ReadByte(),LastReflowTick=r.ReadInt32()
        };
        if(intent.MemberCount==0)
        {
            if(intent.SlotIndex!=0||intent.Columns!=0)throw new InvalidDataException("Invalid empty formation intent.");
            return intent;
        }
        if(intent.MemberCount<2||intent.MemberCount>128||intent.SlotIndex<0||intent.SlotIndex>=intent.MemberCount||intent.Columns<1||intent.Columns>intent.MemberCount||
            intent.SpacingFootprint<FootprintClass.Tiny||intent.SpacingFootprint>FootprintClass.Huge||intent.LastReflowTick< -1)
            throw new InvalidDataException("Invalid formation intent.");
        return intent;
    }
}

public sealed class CommandBuffer
{
    private readonly List<CommandEnvelope> _commands = new();
    public int Count => _commands.Count;
    public IReadOnlyList<CommandEnvelope> All => _commands;
    public void Enqueue(CommandEnvelope command) { _commands.Add(command); _commands.Sort(Compare); }
    public void DrainForTick(SimTick tick, List<CommandEnvelope> output)
    {
        output.Clear(); int remove = 0;
        for (int i = 0; i < _commands.Count; i++)
        {
            int scheduled = _commands[i].ExecutionTick.Value;
            if (scheduled > tick.Value) break;
            if (scheduled < tick.Value) throw new InvalidOperationException("A command became overdue; authoritative command ticks may not be silently skipped.");
            output.Add(_commands[i]); remove++;
        }
        if (remove > 0) _commands.RemoveRange(0, remove);
    }
    public void Serialize(BinaryWriter w) { w.Write(_commands.Count); for (int i = 0; i < _commands.Count; i++) _commands[i].Write(w); }
    public void Deserialize(BinaryReader r) { _commands.Clear(); int n = r.ReadInt32(); if (n < 0 || n > 100000) throw new InvalidDataException("Invalid command buffer."); for (int i = 0; i < n; i++) _commands.Add(CommandEnvelope.Read(r)); _commands.Sort(Compare); }
    private static int Compare(CommandEnvelope a, CommandEnvelope b)
    {
        int c = a.ExecutionTick.Value.CompareTo(b.ExecutionTick.Value); if (c != 0) return c;
        c = a.PlayerSlot.CompareTo(b.PlayerSlot); return c != 0 ? c : a.Sequence.CompareTo(b.Sequence);
    }
}
}
