namespace LegoSpaceRTS.SimCore
{
public enum FootprintClass : byte { Tiny = 0, Small = 1, Medium = 2, Large = 3, Huge = 4 }
public enum MovementState : byte { Idle = 0, Moving = 1, Holding = 2, WaitingForPath = 3, StuckRecovery = 4 }
public enum MovementLayer : byte { Ground = 0, GroundHover = 1, TrueAir = 2 }
public enum ReversePolicy : byte { None = 0, Reduced = 1, Full = 2 }
public enum VisibilityState : byte { Unseen = 0, Explored = 1, Visible = 2 }
public enum SelectableKind : byte { CombatSupport = 0, Worker = 1, Building = 2 }

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

public struct NavigationAgent
{
    public FootprintClass Footprint;
    public MovementLayer Layer;
    public FixVec2 Target;
    public bool HasTarget;
    public bool PathDirty;
    public int PathTopologyVersion;
    public int RequestAge;
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
}
