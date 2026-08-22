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
            .Append(Selection.Signature()).Append('|').Append((int)Alert.Priority).Append('|')
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
