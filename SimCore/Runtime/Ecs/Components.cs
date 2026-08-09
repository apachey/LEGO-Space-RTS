namespace LegoSpaceRTS.SimCore
{
public enum FootprintClass : byte { Tiny = 0, Small = 1, Medium = 2, Large = 3, Huge = 4 }
public enum MovementState : byte { Idle = 0, Moving = 1, Holding = 2, WaitingForPath = 3, StuckRecovery = 4 }
public enum MovementLayer : byte { Ground = 0, GroundHover = 1, TrueAir = 2 }
public enum ReversePolicy : byte { None = 0, Reduced = 1, Full = 2 }
public enum VisibilityState : byte { Unseen = 0, Explored = 1, Visible = 2 }
public enum SelectableKind : byte { CombatSupport = 0, Worker = 1, Building = 2, ResourceNode = 3 }
public enum ResourceType : byte { Ore = 0, Crystal = 1 }
public enum ResourceDepositSize : byte { Small = 0, Standard = 1, Rich = 2, DeepContestedSeam = 3 }
public enum HarvestInteraction : byte { Mine = 0, Harvest = 1 }
public enum ResourceDepletionProfile : byte { Finite = 0 }
public enum ResourceVisualState : byte { Full = 0, Reduced = 1, Low = 2, Critical = 3, Exhausted = 4 }
public enum WorkerTaskState : byte { Idle = 0, MovingToResource = 1, Mining = 2, ReturningToReceiver = 3, AwaitingDelivery = 4 }

public struct Ownership
{
    public byte PlayerSlot;
}

public struct SimTransform
{
    public FixVec2 Position;
    public Angle16 Orientation;
}

public struct Movement
{
    public Fix32 MaxSpeed;
    public Fix32 Acceleration;
    public Fix32 Deceleration;
    public ushort TurnRatePerTick;
    public ReversePolicy ReversePolicy;
    public Fix32 CurrentSpeed;
    public FixVec2 CurrentVelocity;
    public FixVec2 DesiredMovement;
    public int PathIndex;
    public MovementState State;
    public int StuckTicks;
    public int CompressionTicks;
    public FixVec2 LastPosition;
}

public struct FormationIntent
{
    public uint CohortId;
    public FixVec2 Anchor;
    public FixVec2 Heading;
    public int SlotIndex;
    public int MemberCount;
    public int Columns;
    public FootprintClass SpacingFootprint;
    public int LastReflowTick;
    public bool IsActive => MemberCount > 1;
}

public struct NavigationAgent
{
    public FootprintClass Footprint;
    public MovementLayer Layer;
    public FixVec2 Target;
    public bool HasTarget;
    public bool PathDirty;
    public int PathTopologyVersion;
    public int RequestAge;
    public FormationIntent Formation;
}

public struct Selectable
{
    public bool IsSelectable;
    public ContentId ContentType;
    public SelectableKind Kind;
}

public struct Vision
{
    public byte RadiusBuildCells;
    public bool IsAirVision;
    public int LastFogX;
    public int LastFogY;
}

public struct ResourceNode
{
    public ResourceType Type;
    public ResourceDepositSize DepositSize;
    public HarvestInteraction HarvestInteraction;
    public ResourceDepletionProfile DepletionProfile;
    public int Capacity;
    public int Remaining;
    public ushort ReducedThresholdBasisPoints;
    public ushort LowThresholdBasisPoints;
    public ushort CriticalThresholdBasisPoints;

    public bool IsDepleted => Remaining == 0;

    public ResourceVisualState VisualState
    {
        get
        {
            if (Remaining <= 0) return ResourceVisualState.Exhausted;
            long basisPoints = (long)Remaining * 10_000 / Capacity;
            if (basisPoints <= CriticalThresholdBasisPoints) return ResourceVisualState.Critical;
            if (basisPoints <= LowThresholdBasisPoints) return ResourceVisualState.Low;
            if (basisPoints <= ReducedThresholdBasisPoints) return ResourceVisualState.Reduced;
            return ResourceVisualState.Full;
        }
    }
}

public struct Worker
{
    public EntityId ResourceTarget;
    public EntityId ReceiverTarget;
    public WorkerTaskState TaskState;
    public ushort ExtractionTicks;
    public ushort TicksPerOre;
}

public struct ResourceCarrier
{
    public ResourceType Type;
    public byte Amount;
    public byte Capacity;
    public bool IsFull => Amount >= Capacity;
}

public struct ResourceReceiver
{
    public ResourceType AcceptedType;
    public int PendingHauledAmount;
    public bool IsHqEmergencyReceiver;
}

public struct ResourceBank
{
    public ResourceType Type;
    public int ProcessedAmount;
}
}
