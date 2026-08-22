using System.Text;

namespace LegoSpaceRTS.UI;

public enum HudFaction
{
    RockRaiders,
    Astronauts,
    Aliens,
    Martians
}

public enum HudAlertPriority
{
    None,
    Informational,
    Normal,
    High,
    Critical
}

public enum HudMinimapTerrain : byte
{
    Ground,
    Rough,
    Blocked,
    Excavatable
}

public enum HudMinimapMarkerKind : byte
{
    GroundMobile,
    TrueAir,
    Structure,
    Resource
}

public enum HudMinimapRelation : byte
{
    Owned,
    Allied,
    Enemy,
    Neutral
}

public enum HudMinimapLineKind : byte
{
    OwnedNetwork,
    KnownEnemyNetwork
}

public sealed class HudMinimapFrame
{
    public const int Width = 160;
    public const int Height = 160;
    public const int CellCount = Width * Height;
    public const int MarkerCapacity = 512;
    public const int LineCapacity = 256;
    public const int PingCapacity = 32;

    public int Revision { get; set; }
    public int TerrainRevision { get; set; }
    public byte[] Terrain { get; set; } = Array.Empty<byte>();
    public byte[] Knowledge { get; set; } = Array.Empty<byte>();
    public List<HudMinimapMarkerFrame> Markers { get; set; } = new();
    public List<HudMinimapLineFrame> Lines { get; set; } = new();
    public List<HudMinimapPingFrame> Pings { get; set; } = new();

    public byte GetKnowledge(int x, int y)
    {
        if ((uint)x >= Width || (uint)y >= Height || Knowledge.Length != CellCount) return 0;
        return Knowledge[y * Width + x];
    }

    public bool ValidateClientKnowledge(byte viewerPlayer, out string error)
    {
        if (Terrain.Length != CellCount || Knowledge.Length != CellCount)
        {
            error = "Minimap terrain/fog raster size is invalid.";
            return false;
        }
        for (int i = 0; i < Markers.Count; i++)
        {
            HudMinimapMarkerFrame marker = Markers[i];
            if (!float.IsFinite(marker.BuildX) || !float.IsFinite(marker.BuildY) ||
                marker.BuildX < 0 || marker.BuildY < 0 || marker.BuildX >= Width || marker.BuildY >= Height)
            {
                error = $"Minimap marker {marker.StableId} is outside map bounds.";
                return false;
            }
            int x = Math.Clamp((int)marker.BuildX, 0, Width - 1);
            int y = Math.Clamp((int)marker.BuildY, 0, Height - 1);
            byte knowledge = GetKnowledge(x, y);
            if (marker.Relation == HudMinimapRelation.Enemy && !marker.Remembered && knowledge != 2)
            {
                error = $"Current enemy marker {marker.StableId} is outside visible knowledge.";
                return false;
            }
            if (marker.Kind == HudMinimapMarkerKind.Resource && !marker.Remembered && knowledge != 2)
            {
                error = $"Current resource marker {marker.StableId} is outside visible knowledge.";
                return false;
            }
            if (marker.Remembered && (knowledge != 1 ||
                (marker.Kind != HudMinimapMarkerKind.Structure && marker.Kind != HudMinimapMarkerKind.Resource)))
            {
                error = $"Remembered marker {marker.StableId} is not a legal explored static marker.";
                return false;
            }
            if (marker.Relation == HudMinimapRelation.Owned && marker.Owner != viewerPlayer)
            {
                error = $"Owned marker {marker.StableId} has the wrong owner.";
                return false;
            }
        }
        for (int i = 0; i < Lines.Count; i++)
        {
            HudMinimapLineFrame line = Lines[i];
            if (!InBounds(line.FromBuildX, line.FromBuildY) || !InBounds(line.ToBuildX, line.ToBuildY))
            {
                error = $"Minimap network line {line.StableId} is outside map bounds.";
                return false;
            }
            if (line.Kind == HudMinimapLineKind.KnownEnemyNetwork &&
                (GetKnowledge((int)line.FromBuildX, (int)line.FromBuildY) == 0 ||
                 GetKnowledge((int)line.ToBuildX, (int)line.ToBuildY) == 0))
            {
                error = $"Known enemy network line {line.StableId} crosses unseen knowledge.";
                return false;
            }
        }
        for (int i = 0; i < Pings.Count; i++)
        {
            HudMinimapPingFrame ping = Pings[i];
            if (!InBounds(ping.BuildX, ping.BuildY))
            {
                error = $"Minimap ping {ping.StableId} is outside map bounds.";
                return false;
            }
        }
        if (Markers.Count > MarkerCapacity || Lines.Count > LineCapacity || Pings.Count > PingCapacity)
        {
            error = "Minimap presentation exceeded its bounded capacities.";
            return false;
        }
        error = string.Empty;
        return true;
    }

    private static bool InBounds(float x, float y) => float.IsFinite(x) && float.IsFinite(y) &&
        x >= 0 && y >= 0 && x < Width && y < Height;

    public string Signature() => $"{Revision}|{TerrainRevision}|{Terrain.Length}|{Knowledge.Length}|{Markers.Count}|{Lines.Count}|{Pings.Count}";
}

public sealed class HudMinimapMarkerFrame
{
    public uint StableId { get; set; }
    public byte Owner { get; set; }
    public float BuildX { get; set; }
    public float BuildY { get; set; }
    public HudMinimapMarkerKind Kind { get; set; }
    public HudMinimapRelation Relation { get; set; }
    public bool Remembered { get; set; }
    public bool Selected { get; set; }
}

public sealed class HudMinimapLineFrame
{
    public uint StableId { get; set; }
    public float FromBuildX { get; set; }
    public float FromBuildY { get; set; }
    public float ToBuildX { get; set; }
    public float ToBuildY { get; set; }
    public HudMinimapLineKind Kind { get; set; }
    public bool Operational { get; set; } = true;
}

public sealed class HudMinimapPingFrame
{
    public uint StableId { get; set; }
    public float BuildX { get; set; }
    public float BuildY { get; set; }
    public HudAlertPriority Priority { get; set; }
    public float Phase { get; set; }
}

public sealed class HudFrame
{
    public HudFaction Faction { get; set; } = HudFaction.RockRaiders;
    public string MatchState { get; set; } = "08:42  •  PLAYER 1";
    public string Ore { get; set; } = "0";
    public string Energy { get; set; } = "NO DOMAIN";
    public string Crystals { get; set; } = "0";
    public string Operations { get; set; } = "0 / 0";
    public string FactionMechanic { get; set; } = "WORKSITE CONNECTED";
    public HudAlertPriority ResourcePriority { get; set; }
    public HudMinimapFrame Minimap { get; set; } = new();
    public HudSelectionFrame Selection { get; set; } = new();
    public List<HudCommandFrame> Commands { get; set; } = new();
    public List<HudQueueFrame> Queue { get; set; } = new();
    public List<HudEventFrame> Events { get; set; } = new();
    public HudAlertFrame Alert { get; set; } = new();
    public string Objective { get; set; } = string.Empty;
    public string TooltipTitle { get; set; } = string.Empty;
    public string TooltipQuick { get; set; } = string.Empty;
    public string TooltipExpanded { get; set; } = string.Empty;
    public bool ExpandedTooltip { get; set; }
    public bool EnergyPopoverVisible { get; set; }
    public string EnergyPopover { get; set; } = string.Empty;

    public string ContentSignature()
    {
        StringBuilder signature = new(1024);
        signature.Append((int)Faction).Append('|').Append(MatchState).Append('|').Append(Ore).Append('|')
            .Append(Energy).Append('|').Append(Crystals).Append('|').Append(Operations).Append('|')
            .Append(FactionMechanic).Append('|').Append((int)ResourcePriority).Append('|')
            .Append(Minimap.Signature()).Append('|').Append(Selection.Signature()).Append('|').Append((int)Alert.Priority).Append('|')
            .Append(Alert.Text).Append('|').Append(Objective).Append('|').Append(TooltipTitle).Append('|')
            .Append(TooltipQuick).Append('|').Append(TooltipExpanded).Append('|').Append(ExpandedTooltip)
            .Append('|').Append(EnergyPopoverVisible).Append('|').Append(EnergyPopover);
        for (int i = 0; i < Commands.Count; i++) signature.Append("|C:").Append(Commands[i].Signature());
        for (int i = 0; i < Queue.Count; i++) signature.Append("|Q:").Append(Queue[i].Signature());
        for (int i = 0; i < Events.Count; i++) signature.Append("|E:").Append(Events[i].Signature());
        return signature.ToString();
    }
}

public sealed class HudSelectionFrame
{
    public string Title { get; set; } = "NO SELECTION";
    public string Subtitle { get; set; } = "Select an entity or issue a command.";
    public string PortraitCaption { get; set; } = "NO SELECTION";
    public int Count { get; set; }
    public int HealthPercent { get; set; } = -1;
    public string HealthText { get; set; } = string.Empty;
    public string StatusText { get; set; } = string.Empty;
    public bool ShowPowerPriority { get; set; }
    public int PowerPriority { get; set; } = 1;
    public bool MixedPowerPriority { get; set; }
    public List<HudSelectionGroupFrame> Groups { get; set; } = new();

    public string Signature()
    {
        StringBuilder signature = new(512);
        signature.Append(Title).Append('|').Append(Subtitle).Append('|').Append(PortraitCaption).Append('|')
            .Append(Count).Append('|').Append(HealthPercent).Append('|').Append(HealthText).Append('|')
            .Append(StatusText).Append('|').Append(ShowPowerPriority).Append('|').Append(PowerPriority)
            .Append('|').Append(MixedPowerPriority);
        for (int i = 0; i < Groups.Count; i++) signature.Append("|G:").Append(Groups[i].Signature());
        return signature.ToString();
    }
}

public sealed class HudSelectionGroupFrame
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public int Count { get; set; }
    public int AverageHealthPercent { get; set; } = 100;
    public string StateSummary { get; set; } = string.Empty;
    public string Signature() => $"{Id}|{Name}|{Count}|{AverageHealthPercent}|{StateSummary}";
}

public sealed class HudCommandFrame
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Hotkey { get; set; } = string.Empty;
    public string Cost { get; set; } = string.Empty;
    public string Tooltip { get; set; } = string.Empty;
    public bool Visible { get; set; } = true;
    public bool Enabled { get; set; } = true;
    public bool Active { get; set; }
    public bool QueueFive { get; set; }
    public string Signature() => $"{Id}|{Name}|{Hotkey}|{Cost}|{Tooltip}|{Visible}|{Enabled}|{Active}|{QueueFive}";
}

public sealed class HudQueueFrame
{
    public string Name { get; set; } = string.Empty;
    public int ProgressPercent { get; set; }
    public int Count { get; set; } = 1;
    public bool Paused { get; set; }
    public string Signature() => $"{Name}|{ProgressPercent}|{Count}|{Paused}";
}

public sealed class HudEventFrame
{
    public HudAlertPriority Priority { get; set; }
    public string Time { get; set; } = string.Empty;
    public string Text { get; set; } = string.Empty;
    public string Signature() => $"{(int)Priority}|{Time}|{Text}";
}

public sealed class HudAlertFrame
{
    public HudAlertPriority Priority { get; set; }
    public string Text { get; set; } = string.Empty;
    public bool Actionable { get; set; }
}
