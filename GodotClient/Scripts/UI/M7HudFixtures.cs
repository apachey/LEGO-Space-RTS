namespace LegoSpaceRTS.UI;

public enum M7HudScenario
{
    RockRaiderUnit,
    MixedArmy,
    Production,
    Brownout,
    AstronautTransform,
    AlienResonance,
    MartianNetwork,
    CriticalTooltip
}

public static class M7HudFixtures
{
    public static HudFrame Create(M7HudScenario scenario) => scenario switch
    {
        M7HudScenario.RockRaiderUnit => RockRaiderUnit(),
        M7HudScenario.MixedArmy => MixedArmy(),
        M7HudScenario.Production => Production(),
        M7HudScenario.Brownout => Brownout(),
        M7HudScenario.AstronautTransform => AstronautTransform(),
        M7HudScenario.AlienResonance => AlienResonance(),
        M7HudScenario.MartianNetwork => MartianNetwork(),
        M7HudScenario.CriticalTooltip => CriticalTooltip(),
        _ => RockRaiderUnit()
    };

    public static string Slug(M7HudScenario scenario) => scenario switch
    {
        M7HudScenario.RockRaiderUnit => "rock-unit", M7HudScenario.MixedArmy => "mixed-army",
        M7HudScenario.Production => "production", M7HudScenario.Brownout => "brownout",
        M7HudScenario.AstronautTransform => "astronaut-transform", M7HudScenario.AlienResonance => "alien-resonance",
        M7HudScenario.MartianNetwork => "martian-network", M7HudScenario.CriticalTooltip => "critical-tooltip", _ => "rock-unit"
    };

    public static M7HudScenario Parse(string value) => value.Trim().ToLowerInvariant() switch
    {
        "mixed" or "mixed-army" => M7HudScenario.MixedArmy,
        "production" => M7HudScenario.Production,
        "brownout" => M7HudScenario.Brownout,
        "astronaut" or "astronaut-transform" => M7HudScenario.AstronautTransform,
        "alien" or "alien-resonance" => M7HudScenario.AlienResonance,
        "martian" or "martian-network" => M7HudScenario.MartianNetwork,
        "critical" or "critical-tooltip" => M7HudScenario.CriticalTooltip,
        _ => M7HudScenario.RockRaiderUnit
    };

    private static HudFrame Base(HudFaction faction, string mechanic)
    {
        HudFrame frame = new()
        {
            Faction = faction,
            MatchState = "18:42  •  PLAYER 1",
            Ore = "1,480",
            Energy = "86 / 250  |  18↑  15↓  |  +3/s",
            Crystals = "7",
            Operations = "72 / 100",
            FactionMechanic = mechanic,
            Events =
            {
                new HudEventFrame { Priority = HudAlertPriority.Normal, Time = "18:31", Text = "Ore Processing Plant complete" },
                new HudEventFrame { Priority = HudAlertPriority.Informational, Time = "18:18", Text = "Hover Scout ready" }
            },
            EnergyPopover = "HQ DOMAIN\nReserve  86 / 250\nGeneration  +18 E/s\nDemand  -15 E/s\nNet  +3 E/s\nState  STABLE"
        };
        frame.Minimap = BuildMinimap(faction);
        return frame;
    }

    private static HudFrame RockRaiderUnit()
    {
        HudFrame frame = Base(HudFaction.RockRaiders, "WORKSITE CONNECTED");
        frame.Selection = new HudSelectionFrame
        {
            Title = "CHROME CRUSHER", Subtitle = "HEAVY FRONTLINE  •  GROUND", PortraitCaption = "◆\nCHROME CRUSHER",
            Count = 1, HealthPercent = 68, HealthText = "HP 816 / 1,200  •  A4  •  DAMAGED",
            StatusText = "Target HEAVY  •  Drill ready\nWorksite #12  •  SERVICED\nPower ONLINE  •  NORMAL priority", ShowPowerPriority = true, PowerPriority = 1
        };
        frame.Commands = StandardCommands("DRILL", "E");
        return frame;
    }

    private static HudFrame MixedArmy()
    {
        HudFrame frame = Base(HudFaction.RockRaiders, "2 WORKSITE POOLS");
        frame.Ore = "1,180  •  2 pools";
        frame.Operations = "91 / 100";
        frame.ResourcePriority = HudAlertPriority.High;
        frame.Selection = new HudSelectionFrame
        {
            Title = "45 UNITS SELECTED", Subtitle = "FIELD FORCE  •  5 UNIT TYPES",
            PortraitCaption = "45\nFIELD FORCE", Count = 45,
            StatusText = "9 damaged  •  7 receiving support\nSelect a composition card to focus that unit type.", ShowPowerPriority = true, MixedPowerPriority = true,
            Groups =
            {
                new HudSelectionGroupFrame { Id = "crew", Name = "Crew", Count = 12, AverageHealthPercent = 91, StateSummary = "2 damaged  •  5 repairing" },
                new HudSelectionGroupFrame { Id = "crusher", Name = "Chrome Crusher", Count = 9, AverageHealthPercent = 72, StateSummary = "4 damaged" },
                new HudSelectionGroupFrame { Id = "rider", Name = "Rapid Rider", Count = 8, AverageHealthPercent = 88, StateSummary = "2 transporting" },
                new HudSelectionGroupFrame { Id = "dozer", Name = "Loader Dozer", Count = 10, AverageHealthPercent = 96, StateSummary = "healthy" },
                new HudSelectionGroupFrame { Id = "scout", Name = "Hover Scout", Count = 6, AverageHealthPercent = 63, StateSummary = "3 damaged  •  2 surveying" }
            }
        };
        frame.Commands = StandardCommands();
        return frame;
    }

    private static HudFrame Production()
    {
        HudFrame frame = Base(HudFaction.RockRaiders, "WORKSITE CONNECTED");
        frame.Selection = new HudSelectionFrame
        {
            Title = "VEHICLE SERVICE BAY ×3", Subtitle = "AGGREGATE PRODUCTION  •  shortest projected queue",
            PortraitCaption = "▦\nSERVICE BAYS ×3", Count = 3, HealthPercent = 100,
            HealthText = "3 facilities  •  all operational", StatusText = "Shift-click queues five across eligible facilities.\nRally: south expansion."
        };
        frame.Commands = new List<HudCommandFrame>
        {
            ProductionCommand("crew", "CREW", "50 ORE • 1 OC"), ProductionCommand("scout", "HOVER SCOUT", "75 ORE • 10 E • 1 OC"),
            ProductionCommand("rider", "RAPID RIDER", "90 ORE • 10 E • 2 OC"), ProductionCommand("dozer", "LOADER DOZER", "125 ORE • 15 E • 3 OC")
        };
        frame.Queue = new List<HudQueueFrame>
        {
            new HudQueueFrame { Name = "Hover Scout", ProgressPercent = 62 },
            new HudQueueFrame { Name = "Rapid Rider", ProgressPercent = 0 },
            new HudQueueFrame { Name = "Crew", ProgressPercent = 0, Count = 2 }
        };
        return frame;
    }

    private static HudFrame Brownout()
    {
        HudFrame frame = Base(HudFaction.RockRaiders, "WORKSITE #12 DEFICIT");
        frame.Energy = "12 / 250  |  18↑  27↓  |  −9/s";
        frame.ResourcePriority = HudAlertPriority.High;
        frame.Alert = new HudAlertFrame { Priority = HudAlertPriority.High, Text = "BROWNOUT — HQ Domain demand exceeds generation by 9 E/s", Actionable = true };
        frame.Events.Insert(0, new HudEventFrame { Priority = HudAlertPriority.High, Time = "NOW", Text = "Brownout began in HQ Domain" });
        frame.Selection = new HudSelectionFrame
        {
            Title = "ORE PROCESSING PLANT", Subtitle = "ECONOMY STRUCTURE  •  HQ DOMAIN", PortraitCaption = "▦\nORE PROCESSING",
            Count = 1, HealthPercent = 84, HealthText = "HP 1,260 / 1,500  •  A2  •  HEALTHY",
            StatusText = "BROWNOUT — Low-priority structure disabled.\nDomain demand exceeds generation by 9 E/s.", ShowPowerPriority = true, PowerPriority = 2
        };
        frame.Commands = StandardCommands();
        frame.EnergyPopoverVisible = true;
        frame.EnergyPopover = "HQ DOMAIN\nReserve  12 / 250\nGeneration  +18 E/s\nDemand  -27 E/s\nNet  −9 E/s\nState  BROWNOUT\nLow-priority structures disabled";
        frame.Minimap.Pings.Add(new HudMinimapPingFrame { StableId = 3, BuildX = 73, BuildY = 91, Priority = HudAlertPriority.High, Phase = 0.28f });
        return frame;
    }

    private static HudFrame AstronautTransform()
    {
        HudFrame frame = Base(HudFaction.Astronauts, "FORWARD SERVICE AVAILABLE");
        frame.Selection = new HudSelectionFrame
        {
            Title = "MX-41 SWITCH FIGHTER", Subtitle = "ANTI-AIR / FRONTLINE  •  TRANSFORMING", PortraitCaption = "⇄\nMX-41",
            Count = 1, HealthPercent = 93, HealthText = "HP 930 / 1,000  •  A3  •  HEALTHY",
            StatusText = "GROUND → FLIGHT  •  61%\nCancellation committed  •  reversal lock follows completion\nForward Service available"
        };
        frame.Commands = StandardCommands("REFIT", "E");
        frame.Objective = "FORWARD DEPLOYMENT\nHold the central service corridor  •  02:18";
        return frame;
    }

    private static HudFrame AlienResonance()
    {
        HudFrame frame = Base(HudFaction.Aliens, "CHARGE 72 / 100  •  +1.6/s  •  4 COMMITTED");
        frame.Crystals = "9 spendable";
        frame.Selection = new HudSelectionFrame
        {
            Title = "RESONANCE CORE", Subtitle = "FACTION SYSTEM  •  SURGE ANCHOR", PortraitCaption = "◇\nRESONANCE CORE",
            Count = 1, HealthPercent = 76, HealthText = "HP 1,140 / 1,500  •  A2  •  HEALTHY",
            StatusText = "Committed Crystals 4 / 6\nCharge contribution +1.6/s  •  demand 8 E/s\nSurge available  •  50 Charge"
        };
        frame.Commands = StandardCommands("SURGE", "E");
        frame.Events.Insert(0, new HudEventFrame { Priority = HudAlertPriority.Normal, Time = "18:40", Text = "Charge reached 70" });
        return frame;
    }

    private static HudFrame MartianNetwork()
    {
        HudFrame frame = Base(HudFaction.Martians, "AERO NETWORK 5 STATIONS  •  1 SEGMENT");
        frame.Selection = new HudSelectionFrame
        {
            Title = "SETTLEMENT STATION", Subtitle = "NETWORK / PRODUCTION  •  COMPONENT A", PortraitCaption = "⌁\nSETTLEMENT STATION",
            Count = 1, HealthPercent = 100, HealthText = "HP 1,800 / 1,800  •  A3  •  HEALTHY",
            StatusText = "Aero Tube 3 / 4 connections\nNetwork 5 stations  •  6 active routes\nThroughput 3 units  •  reachable stations highlighted"
        };
        frame.Commands = StandardCommands("TUBE TRANSFER", "E");
        frame.Objective = "NETWORK STATUS\nAll owned Stations connected";
        frame.Minimap.Lines.AddRange(new[]
        {
            new HudMinimapLineFrame { StableId = 701, FromBuildX = 60, FromBuildY = 96, ToBuildX = 75, ToBuildY = 88, Kind = HudMinimapLineKind.OwnedNetwork },
            new HudMinimapLineFrame { StableId = 702, FromBuildX = 75, FromBuildY = 88, ToBuildX = 88, ToBuildY = 101, Kind = HudMinimapLineKind.OwnedNetwork },
            new HudMinimapLineFrame { StableId = 703, FromBuildX = 75, FromBuildY = 88, ToBuildX = 93, ToBuildY = 72, Kind = HudMinimapLineKind.OwnedNetwork, Operational = false },
            new HudMinimapLineFrame { StableId = 704, FromBuildX = 130.5f, FromBuildY = 82.5f, ToBuildX = 136.5f, ToBuildY = 82.5f, Kind = HudMinimapLineKind.KnownEnemyNetwork }
        });
        return frame;
    }

    private static HudFrame CriticalTooltip()
    {
        HudFrame frame = Base(HudFaction.RockRaiders, "WORKSITE SEGMENTED");
        frame.Alert = new HudAlertFrame { Priority = HudAlertPriority.Critical, Text = "COMMAND STRUCTURE IN SEVERE DANGER", Actionable = true };
        frame.Events.Insert(0, new HudEventFrame { Priority = HudAlertPriority.Critical, Time = "NOW", Text = "Rock Raiders HQ critical" });
        frame.Selection = new HudSelectionFrame
        {
            Title = "ROCK RAIDERS HQ", Subtitle = "COMMAND STRUCTURE  •  UNDER ATTACK", PortraitCaption = "▦\nROCK RAIDERS HQ",
            Count = 1, HealthPercent = 17, HealthText = "HP 425 / 2,500  •  A4  •  CRITICAL",
            StatusText = "Strategic structure under siege\nWorksite disconnected  •  local supplied work may continue\nRepairers approaching ×3"
        };
        frame.Commands = StandardCommands();
        frame.TooltipTitle = "REPAIR [R]";
        frame.TooltipQuick = "Restore a damaged owned target.\nCost: Ore per HP restored.";
        frame.TooltipExpanded = "Counters attrition and preserves expensive infrastructure. Repairers must reach service range; brownout can disable supporting structures. Valid layers: ground structures and grounded units.";
        frame.ExpandedTooltip = true;
        frame.Minimap.Pings.Add(new HudMinimapPingFrame { StableId = 2, BuildX = 69, BuildY = 84, Priority = HudAlertPriority.Critical, Phase = 0.62f });
        return frame;
    }

    private static HudMinimapFrame BuildMinimap(HudFaction faction)
    {
        byte[] terrain = new byte[HudMinimapFrame.CellCount];
        byte[] knowledge = new byte[HudMinimapFrame.CellCount];
        for (int y = 0; y < HudMinimapFrame.Height; y++)
        for (int x = 0; x < HudMinimapFrame.Width; x++)
        {
            int index = y * HudMinimapFrame.Width + x;
            HudMinimapTerrain terrainKind = HudMinimapTerrain.Ground;
            if ((x > 17 && x < 46 && y > 18 && y < 39) || (x > 118 && x < 151 && y > 113 && y < 145))
                terrainKind = HudMinimapTerrain.Rough;
            if ((x - 126) * (x - 126) + (y - 38) * (y - 38) < 15 * 15 || (x > 12 && x < 27 && y > 70 && y < 109))
                terrainKind = HudMinimapTerrain.Blocked;
            if (x > 116 && x < 146 && y > 52 && y < 69)
                terrainKind = HudMinimapTerrain.Excavatable;
            terrain[index] = (byte)terrainKind;

            if (x >= 9 && x <= 150 && y >= 9 && y <= 150) knowledge[index] = 1;
            if (x >= 45 && x <= 121 && y >= 44 && y <= 120) knowledge[index] = 2;
        }

        byte owned = 0;
        HudMinimapFrame frame = new()
        {
            Revision = 11240 + (int)faction,
            TerrainRevision = 7,
            Terrain = terrain,
            Knowledge = knowledge,
            Markers =
            {
                new HudMinimapMarkerFrame { StableId = 1, Owner = owned, BuildX = 61, BuildY = 96, Kind = HudMinimapMarkerKind.Structure, Relation = HudMinimapRelation.Owned },
                new HudMinimapMarkerFrame { StableId = 2, Owner = owned, BuildX = 69, BuildY = 84, Kind = HudMinimapMarkerKind.Structure, Relation = HudMinimapRelation.Owned, Selected = true },
                new HudMinimapMarkerFrame { StableId = 3, Owner = owned, BuildX = 73, BuildY = 91, Kind = HudMinimapMarkerKind.GroundMobile, Relation = HudMinimapRelation.Owned, Selected = true },
                new HudMinimapMarkerFrame { StableId = 4, Owner = owned, BuildX = 78, BuildY = 94, Kind = HudMinimapMarkerKind.GroundMobile, Relation = HudMinimapRelation.Owned },
                new HudMinimapMarkerFrame { StableId = 5, Owner = owned, BuildX = 82, BuildY = 88, Kind = HudMinimapMarkerKind.TrueAir, Relation = HudMinimapRelation.Owned },
                new HudMinimapMarkerFrame { StableId = 101, Owner = 1, BuildX = 106, BuildY = 82, Kind = HudMinimapMarkerKind.GroundMobile, Relation = HudMinimapRelation.Enemy },
                new HudMinimapMarkerFrame { StableId = 102, Owner = 1, BuildX = 111, BuildY = 77, Kind = HudMinimapMarkerKind.TrueAir, Relation = HudMinimapRelation.Enemy },
                new HudMinimapMarkerFrame { StableId = 103, Owner = 1, BuildX = 115, BuildY = 91, Kind = HudMinimapMarkerKind.Structure, Relation = HudMinimapRelation.Enemy },
                new HudMinimapMarkerFrame { StableId = 201, Owner = byte.MaxValue, BuildX = 92, BuildY = 109, Kind = HudMinimapMarkerKind.Resource, Relation = HudMinimapRelation.Neutral },
                new HudMinimapMarkerFrame { StableId = 301, Owner = 1, BuildX = 134, BuildY = 82, Kind = HudMinimapMarkerKind.Structure, Relation = HudMinimapRelation.Enemy, Remembered = true },
                new HudMinimapMarkerFrame { StableId = 302, Owner = byte.MaxValue, BuildX = 31, BuildY = 117, Kind = HudMinimapMarkerKind.Resource, Relation = HudMinimapRelation.Neutral, Remembered = true }
            }
        };
        return frame;
    }

    private static List<HudCommandFrame> StandardCommands(string special = "SPECIAL", string specialHotkey = "E") => new()
    {
        Command("move", "MOVE", "M"), Command("attack", "ATTACK", "A"), Command("stop", "STOP", "S"),
        Command("hold", "HOLD", "H"), Command("patrol", "PATROL", "P"), Command("spread", "SPREAD", "V"),
        Command("repair", "REPAIR", "R"), Command("load", "LOAD", "L"), Command("unload", "UNLOAD", "U"),
        Command("special", special, specialHotkey), Command("state", "STATE CHANGE", "Q"), Command("detail", "DETAIL", "ALT")
    };

    private static HudCommandFrame Command(string id, string name, string hotkey) => new()
    {
        Id = id, Name = name, Hotkey = hotkey, Enabled = true, Tooltip = CommandTooltip(id)
    };

    private static string CommandTooltip(string id) => id switch
    {
        "move" => "Move selected units to a ground position.",
        "attack" => "Target an enemy or issue Attack-Move on ground.",
        "stop" => "Cancel ordinary queued orders and stop.",
        "hold" => "Hold the current position.",
        "patrol" => "Patrol between ordered waypoints.",
        "spread" => "Increase formation spacing.",
        "repair" => "Repair a legal damaged target.",
        "load" => "Load eligible selected units.",
        "unload" => "Unload carried units at a legal position.",
        "state" => "Change the unit's tactical configuration.",
        "detail" => "Open expanded selection information.",
        _ => "Use the selected faction action."
    };
    private static HudCommandFrame ProductionCommand(string id, string name, string cost) => new() { Id = id, Name = name, Cost = cost, Enabled = true, QueueFive = true, Tooltip = "Click +1  •  Shift-click +5" };
}
