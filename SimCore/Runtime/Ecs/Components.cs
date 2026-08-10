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
public enum BuilderJobState : byte { Idle = 0, MovingToSite = 1, Constructing = 2 }
public enum BuildingState : byte { ConstructionSite = 0, Completed = 1 }
public enum EnergyFunctionalClass : byte { CommandAndBasicEconomy = 1, ResourceProcessing = 2, ProductionAndResearch = 3, ServiceAndFactionSystems = 4, StaticDefenseAndNonessential = 5 }
public enum EnergyPriority : byte { High = 0, Normal = 1, Low = 2 }
public enum BrownoutEventKind : byte { None = 0, Entered = 1, Changed = 2, Recovered = 3 }
public enum CombatTargetClass : byte { Personnel = 0, LightMachine = 1, MediumMachine = 2, HeavyMachine = 3, MassiveMachine = 4, Structure = 5, FortifiedStructure = 6 }
public enum CombatTargetLayer : byte { Ground = 0, TrueAir = 1 }
[System.Flags] public enum TargetLayerMask : byte { None = 0, Ground = 1, TrueAir = 2, All = Ground | TrueAir }
[System.Flags] public enum TargetClassMask : byte { None = 0, Personnel = 1, LightMachine = 2, MediumMachine = 4, HeavyMachine = 8, MassiveMachine = 16, Structure = 32, FortifiedStructure = 64, All = 127 }
[System.Flags] public enum CombatTargetFlags : ushort
{
    None = 0, CombatThreat = 1, Worker = 2, Transport = 4, Support = 8,
    DefensiveStructure = 16, Production = 32, EconomicInfrastructure = 64, Command = 128
}
public enum TargetPriorityProfile : byte { AntiLight = 0, AntiHeavy = 1, AntiAir = 2, Siege = 3, Harassment = 4, Generalist = 5, Scout = 6, Support = 7, Control = 8 }
public enum TargetSelectionKind : byte { None = 0, Automatic = 1, DirectOrder = 2 }

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

public struct Builder
{
    public EntityId ConstructionTarget;
    public BuilderJobState JobState;
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

public struct Building
{
    public ContentId Type;
    public short AnchorX;
    public short AnchorY;
    public byte Orientation;
    public byte FootprintWidth;
    public byte FootprintHeight;
    public BuildingState State;
}

public struct EnergyDomain
{
    public Fix32 Reserve;
    public Fix32 ReserveCapacity;
    public int GenerationPerSecond;
    public int ContinuousDemandPerSecond;
    public int PoweredDemandPerSecond;
    public int FlowRemainderRaw;
    public uint BrownoutRevision;
    public BrownoutEventKind LastBrownoutEvent;
    public bool IsBrownout;

    public bool IsDeficit => ContinuousDemandPerSecond > GenerationPerSecond;
}

public struct EnergyDomainMember
{
    public EntityId DomainRoot;
}

public struct PowerState
{
    public EnergyPriority Priority;
    public bool IsPowered;
}

public struct Targetable
{
    public CombatTargetClass Class;
    public CombatTargetLayer Layer;
    public CombatTargetFlags Flags;
}

public struct Targeting
{
    public EntityId CurrentTarget;
    public Fix32 AcquisitionRadius;
    public TargetLayerMask LegalLayers;
    public TargetClassMask LegalClasses;
    public TargetPriorityProfile PriorityProfile;
    public TargetSelectionKind SelectionKind;
}

public struct ConstructionSite
{
    public EntityId AssignedBuilder;
    public EntityId FundingBank;
    public int ReservedOre;
    public int ConsumedOre;
    public int RequiredEnergy;
    public int ReservedEnergy;
    public int ConsumedEnergy;
    public EntityId EnergyDomainRoot;
    public ushort RequiredTicks;
    public ushort ProgressTicks;
}

public struct ProductionQueueItem
{
    public ContentId UnitType;
    public EntityId FundingBank;
    public ushort ReservedOre;
    public ushort RequiredEnergy;
    public byte RequiredCrystals;
    public byte ReservedOperationsCapacity;
    public ushort TotalTicks;
    public ushort RemainingTicks;
}

public struct Production
{
    public const int Capacity = 8;
    public byte Count;
    public bool SpawnBlocked;
    public bool HasRallyPoint;
    public FixVec2 RallyPoint;
    public EntityId RallyTargetEntity;
    public ProductionQueueItem Item0;
    public ProductionQueueItem Item1;
    public ProductionQueueItem Item2;
    public ProductionQueueItem Item3;
    public ProductionQueueItem Item4;
    public ProductionQueueItem Item5;
    public ProductionQueueItem Item6;
    public ProductionQueueItem Item7;

    public ProductionQueueItem Get(int index) => index switch
    {
        0 => Item0, 1 => Item1, 2 => Item2, 3 => Item3,
        4 => Item4, 5 => Item5, 6 => Item6, 7 => Item7,
        _ => throw new System.ArgumentOutOfRangeException(nameof(index))
    };

    public void Set(int index, ProductionQueueItem item)
    {
        switch (index)
        {
            case 0: Item0 = item; break; case 1: Item1 = item; break; case 2: Item2 = item; break; case 3: Item3 = item; break;
            case 4: Item4 = item; break; case 5: Item5 = item; break; case 6: Item6 = item; break; case 7: Item7 = item; break;
            default: throw new System.ArgumentOutOfRangeException(nameof(index));
        }
    }

    public bool TryEnqueue(ProductionQueueItem item)
    {
        if (Count >= Capacity) return false;
        Set(Count, item); Count++; return true;
    }

    public void RemoveFirst()
    {
        if (Count == 0) return;
        for (int i = 1; i < Count; i++) Set(i - 1, Get(i));
        Count--; Set(Count, default); SpawnBlocked = false;
    }

    public int ProjectedTicks
    {
        get
        {
            int result = 0;
            for (int i = 0; i < Count; i++) result = checked(result + Get(i).RemainingTicks);
            return result;
        }
    }
}
}
