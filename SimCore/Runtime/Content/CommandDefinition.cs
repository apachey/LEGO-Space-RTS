using System;
using System.Collections.Generic;

namespace LegoSpaceRTS.SimCore
{
public enum ContentActionPrerequisiteKind : byte
{
    Building = 1,
    Research = 2
}

/// <summary>One alternative inside an AND-of-OR action prerequisite expression.</summary>
public readonly struct ContentActionPrerequisite
{
    public readonly ContentActionPrerequisiteKind Kind;
    public readonly string TargetStableKey;
    public readonly ContentId TargetId;

    public ContentActionPrerequisite(ContentActionPrerequisiteKind kind, string targetStableKey)
    {
        if (kind < ContentActionPrerequisiteKind.Building || kind > ContentActionPrerequisiteKind.Research)
            throw new ArgumentOutOfRangeException(nameof(kind));
        if (string.IsNullOrWhiteSpace(targetStableKey)) throw new ArgumentNullException(nameof(targetStableKey));
        Kind = kind;
        TargetStableKey = targetStableKey;
        TargetId = StableId.FromKey(targetStableKey);
    }
}

public readonly struct ContentActionPrerequisiteGroup
{
    public readonly ContentActionPrerequisite[] Alternatives;

    public ContentActionPrerequisiteGroup(ContentActionPrerequisite[] alternatives)
    {
        if (alternatives == null || alternatives.Length == 0)
            throw new ArgumentException("A prerequisite group requires at least one alternative.", nameof(alternatives));
        Alternatives = alternatives;
    }
}

public enum CommandTargetType : byte
{
    None = 0,
    Ground = 1,
    EnemyEntity = 2,
    FriendlyEntity = 3,
    ResourceEntity = 4,
    ConstructionSite = 5,
    Producer = 6,
    ResearchProvider = 7,
    Configuration = 8,
    SurgeAnchor = 9,
    TubeStation = 10,
    ExcavatableFeature = 11,
    DisplacementTarget = 12,
    MapPoint = 13
}

public enum CommandQueuePolicy : byte
{
    Never = 0,
    Optional = 1,
    Conditional = 2
}

/// <summary>Validated command-family metadata. The enum value is the stable network code.</summary>
public readonly struct CommandDefinition
{
    public readonly string StableKey;
    public readonly ContentId Id;
    public readonly SimCommandType CommandType;
    public readonly string[] EligibleEntityTags;
    public readonly CommandTargetType TargetType;
    public readonly CommandQueuePolicy QueuePolicy;
    public readonly string RequiredResearchStableKey;
    public readonly ContentId RequiredResearch;
    public readonly string ValidationHandlerId;
    public readonly string ExecutionHandlerId;
    public readonly string UiSlotProfile;
    public readonly string TargetingPreviewProfile;

    public CommandDefinition(string stableKey, SimCommandType commandType, string[] eligibleEntityTags,
        CommandTargetType targetType, CommandQueuePolicy queuePolicy, string requiredResearchStableKey,
        string validationHandlerId, string executionHandlerId, string uiSlotProfile, string targetingPreviewProfile)
    {
        if (string.IsNullOrWhiteSpace(stableKey)) throw new ArgumentNullException(nameof(stableKey));
        if (eligibleEntityTags == null || eligibleEntityTags.Length == 0) throw new ArgumentException("A command requires at least one eligibility tag.", nameof(eligibleEntityTags));
        if (targetType < CommandTargetType.None || targetType > CommandTargetType.MapPoint) throw new ArgumentOutOfRangeException(nameof(targetType));
        if (queuePolicy < CommandQueuePolicy.Never || queuePolicy > CommandQueuePolicy.Conditional) throw new ArgumentOutOfRangeException(nameof(queuePolicy));
        if (string.IsNullOrWhiteSpace(validationHandlerId) || string.IsNullOrWhiteSpace(executionHandlerId) ||
            string.IsNullOrWhiteSpace(uiSlotProfile) || string.IsNullOrWhiteSpace(targetingPreviewProfile))
            throw new ArgumentException("Command handler and UI/preview profiles are required.");
        StableKey = stableKey;
        Id = StableId.FromKey(stableKey);
        CommandType = commandType;
        EligibleEntityTags = eligibleEntityTags;
        TargetType = targetType;
        QueuePolicy = queuePolicy;
        RequiredResearchStableKey = requiredResearchStableKey ?? string.Empty;
        RequiredResearch = string.IsNullOrEmpty(RequiredResearchStableKey) ? default : StableId.FromKey(RequiredResearchStableKey);
        ValidationHandlerId = validationHandlerId;
        ExecutionHandlerId = executionHandlerId;
        UiSlotProfile = uiSlotProfile;
        TargetingPreviewProfile = targetingPreviewProfile;
    }
}

public static class CanonicalCommandDefinitions
{
    private static readonly string[] Mobile = { "component.navigation" };
    private static readonly string[] Combat = { "component.targeting" };
    private static readonly string[] Worker = { "component.worker" };
    private static readonly string[] Builder = { "component.builder" };
    private static readonly string[] Repairer = { "component.repairer" };
    private static readonly string[] Producer = { "component.production" };
    private static readonly string[] ResearchProvider = { "tag.research_provider" };
    private static readonly string[] Powered = { "component.energy_domain_member" };
    private static readonly string[] Passenger = { "component.passenger" };
    private static readonly string[] Transport = { "component.transport" };
    private static readonly string[] StateChangeAssets =
    {
        "component.transformation",
        "unit.astronauts.solar_explorer",
        "unit.astronauts.mt201_ultra_drill_walker",
        "unit.aliens.etx_alien_strike",
        "unit.aliens.etx_alien_infiltrator",
        "building.ali.etx_defense_node"
    };
    private static readonly string[] Refit = { "capability.ast.mission_refit" };
    private static readonly string[] ResonanceCore = { "component.resonance_core" };
    private static readonly string[] SurgeAnchor = { "capability.ali.surge_anchor" };
    private static readonly string[] AlienProduction = { "building.ali.etx_fabricator", "building.ali.reconfiguration_dock" };
    private static readonly string[] DefenseNode = { "building.ali.etx_defense_node" };
    private static readonly string[] TubePassenger = { "capability.mar.tube_transfer" };
    private static readonly string[] TubeStation = { "component.tube_station" };
    private static readonly string[] Excavator = { "capability.rr.excavation" };
    private static readonly string[] Protector = { "unit.martians.red_planet_protector" };
    private static readonly string[] Searcher = { "unit.martians.excavation_searcher" };
    private static readonly string[] AnyOwned = { "tag.owned_selectable" };
    private static readonly string[] Player = { "tag.player" };

    public static CommandDefinition[] Create()
    {
        CommandDefinition[] result =
        {
        Def("command.move", SimCommandType.Move, Mobile, CommandTargetType.Ground, CommandQueuePolicy.Optional, "", "validate.move", "execute.move", "slot.movement", "preview.move"),
        Def("command.attack", SimCommandType.Attack, Combat, CommandTargetType.EnemyEntity, CommandQueuePolicy.Optional, "", "validate.attack", "execute.attack", "slot.combat", "preview.attack"),
        Def("command.attack_move", SimCommandType.AttackMove, Mobile, CommandTargetType.Ground, CommandQueuePolicy.Optional, "", "validate.attack_move", "execute.attack_move", "slot.combat", "preview.attack_move"),
        Def("command.stop", SimCommandType.Stop, Mobile, CommandTargetType.None, CommandQueuePolicy.Never, "", "validate.stop", "execute.stop", "slot.movement", "preview.none"),
        Def("command.hold", SimCommandType.HoldPosition, Mobile, CommandTargetType.None, CommandQueuePolicy.Never, "", "validate.hold", "execute.hold", "slot.movement", "preview.none"),
        Def("command.patrol", SimCommandType.Patrol, Mobile, CommandTargetType.Ground, CommandQueuePolicy.Optional, "", "validate.patrol", "execute.patrol", "slot.movement", "preview.patrol"),
        Def("command.spread", SimCommandType.SetSpread, Mobile, CommandTargetType.None, CommandQueuePolicy.Never, "", "validate.spread", "execute.spread", "slot.movement", "preview.formation"),
        Def("command.harvest", SimCommandType.Harvest, Worker, CommandTargetType.ResourceEntity, CommandQueuePolicy.Optional, "", "validate.harvest", "execute.harvest", "slot.economy", "preview.harvest"),
        Def("command.build", SimCommandType.Build, Builder, CommandTargetType.Ground, CommandQueuePolicy.Optional, "", "validate.build", "execute.build", "slot.economy", "preview.build"),
        Def("command.cancel_construction", SimCommandType.CancelConstruction, AnyOwned, CommandTargetType.ConstructionSite, CommandQueuePolicy.Never, "", "validate.cancel_construction", "execute.cancel_construction", "slot.cancel", "preview.none"),
        Def("command.assist_construction", SimCommandType.AssistConstruction, Builder, CommandTargetType.ConstructionSite, CommandQueuePolicy.Optional, "", "validate.assist_construction", "execute.assist_construction", "slot.economy", "preview.assist"),
        Def("command.queue_production", SimCommandType.QueueProduction, Producer, CommandTargetType.Producer, CommandQueuePolicy.Never, "", "validate.queue_production", "execute.queue_production", "slot.production", "preview.none"),
        Def("command.cancel_production", SimCommandType.CancelProduction, Producer, CommandTargetType.Producer, CommandQueuePolicy.Never, "", "validate.cancel_production", "execute.cancel_production", "slot.cancel", "preview.none"),
        Def("command.reorder_production", SimCommandType.ReorderProduction, Producer, CommandTargetType.Producer, CommandQueuePolicy.Never, "", "validate.reorder_production", "execute.reorder_production", "slot.production", "preview.none"),
        Def("command.set_rally_point", SimCommandType.SetRallyPoint, Producer, CommandTargetType.Ground, CommandQueuePolicy.Never, "", "validate.set_rally", "execute.set_rally", "slot.production", "preview.rally"),
        Def("command.set_energy_priority", SimCommandType.SetEnergyPriority, Powered, CommandTargetType.None, CommandQueuePolicy.Never, "", "validate.energy_priority", "execute.energy_priority", "slot.status", "preview.energy_domain"),
        Def("command.repair", SimCommandType.Repair, Repairer, CommandTargetType.FriendlyEntity, CommandQueuePolicy.Optional, "", "validate.repair", "execute.repair", "slot.service", "preview.repair"),
        Def("command.load", SimCommandType.Load, Passenger, CommandTargetType.FriendlyEntity, CommandQueuePolicy.Conditional, "", "validate.load", "execute.load", "slot.transport", "preview.load"),
        Def("command.unload", SimCommandType.Unload, Transport, CommandTargetType.Ground, CommandQueuePolicy.Conditional, "", "validate.unload", "execute.unload", "slot.transport", "preview.unload"),
        Def("command.state_change", SimCommandType.StateChange, StateChangeAssets, CommandTargetType.None, CommandQueuePolicy.Conditional, "", "validate.state_change", "execute.state_change", "slot.state_change", "preview.state_change"),
        Def("command.mission_refit", SimCommandType.MissionRefit, Refit, CommandTargetType.Configuration, CommandQueuePolicy.Never, "research.ast.mission_refit_protocols", "validate.mission_refit", "execute.mission_refit", "slot.service", "preview.refit"),
        Def("command.cancel_mission_refit", SimCommandType.CancelMissionRefit, Refit, CommandTargetType.FriendlyEntity, CommandQueuePolicy.Never, "", "validate.cancel_mission_refit", "execute.cancel_mission_refit", "slot.cancel", "preview.refit"),
        Def("command.set_resonance_commitment", SimCommandType.SetResonanceCommitment, ResonanceCore, CommandTargetType.Configuration, CommandQueuePolicy.Never, "", "validate.resonance_commitment", "execute.resonance_commitment", "slot.service", "preview.resonance_commitment"),
        Def("command.start_surge", SimCommandType.StartSurge, SurgeAnchor, CommandTargetType.SurgeAnchor, CommandQueuePolicy.Never, "research.ali.resonance_initiation", "validate.surge", "execute.surge", "slot.special", "preview.surge"),
        Def("command.rapid_fabrication", SimCommandType.RapidFabrication, AlienProduction, CommandTargetType.Producer, CommandQueuePolicy.Never, "research.ali.rapid_fabrication_conduits", "validate.rapid_fabrication", "execute.rapid_fabrication", "slot.special", "preview.none"),
        Def("command.defense_resonance_shunt", SimCommandType.DefenseResonanceShunt, DefenseNode, CommandTargetType.None, CommandQueuePolicy.Never, "research.ali.defense_resonance_shunt", "validate.defense_shunt", "execute.defense_shunt", "slot.special", "preview.defense_shunt"),
        Def("command.tube_transfer", SimCommandType.TubeTransfer, TubePassenger, CommandTargetType.TubeStation, CommandQueuePolicy.Conditional, "", "validate.tube_transfer", "execute.tube_transfer", "slot.special", "preview.tube_transfer"),
        Def("command.tube_build", SimCommandType.TubeBuild, TubeStation, CommandTargetType.TubeStation, CommandQueuePolicy.Never, "", "validate.tube_build", "execute.tube_build", "slot.service", "preview.tube_build"),
        Def("command.excavate", SimCommandType.Excavate, Excavator, CommandTargetType.ExcavatableFeature, CommandQueuePolicy.Optional, "", "validate.excavate", "execute.excavate", "slot.special", "preview.excavate"),
        Def("command.protector_stance", SimCommandType.ProtectorStance, Protector, CommandTargetType.None, CommandQueuePolicy.Conditional, "", "validate.protector_stance", "execute.protector_stance", "slot.state_change", "preview.state_change"),
        Def("command.searcher_brace", SimCommandType.SearcherBrace, Searcher, CommandTargetType.None, CommandQueuePolicy.Conditional, "", "validate.searcher_brace", "execute.searcher_brace", "slot.state_change", "preview.state_change"),
        Def("command.excavation_clamp", SimCommandType.ExcavationClamp, Searcher, CommandTargetType.DisplacementTarget, CommandQueuePolicy.Never, "research.mar.advanced_excavation_systems", "validate.excavation_clamp", "execute.excavation_clamp", "slot.special", "preview.excavation_clamp"),
        Def("command.start_research", SimCommandType.StartResearch, ResearchProvider, CommandTargetType.ResearchProvider, CommandQueuePolicy.Never, "", "validate.start_research", "execute.start_research", "slot.research", "preview.none"),
        Def("command.cancel_research", SimCommandType.CancelResearch, ResearchProvider, CommandTargetType.ResearchProvider, CommandQueuePolicy.Never, "", "validate.cancel_research", "execute.cancel_research", "slot.cancel", "preview.none"),
        Def("command.ping", SimCommandType.Ping, Player, CommandTargetType.MapPoint, CommandQueuePolicy.Never, "", "validate.ping", "execute.ping", "slot.communication", "preview.ping")
        };
        Array.Sort(result, (a, b) => string.CompareOrdinal(a.StableKey, b.StableKey));
        return result;
    }

    private static CommandDefinition Def(string stableKey, SimCommandType commandType, string[] tags,
        CommandTargetType targetType, CommandQueuePolicy queuePolicy, string requiredResearch,
        string validationHandler, string executionHandler, string slot, string preview)
        => new(stableKey, commandType, tags, targetType, queuePolicy, requiredResearch,
            validationHandler, executionHandler, slot, preview);
}

public static class CommandDefinitionValidator
{
    public static void Validate(PrototypeContentCatalog catalog)
    {
        // Legacy/test catalogs may intentionally omit the optional command table.
        // Once present it must be complete so a partial public surface cannot ship.
        if (catalog.Commands.Length == 0) return;
        HashSet<uint> occupiedIds = CollectOccupiedIds(catalog);
        HashSet<uint> ids = new();
        HashSet<ushort> networkCodes = new();
        string previousKey = string.Empty;
        for (int i = 0; i < catalog.Commands.Length; i++)
        {
            CommandDefinition command = catalog.Commands[i];
            ushort code = (ushort)command.CommandType;
            if (!Enum.IsDefined(typeof(SimCommandType), command.CommandType) || code == 0 ||
                code > (ushort)SimCommandType.CancelMissionRefit)
                throw new ArgumentException($"Command {command.StableKey} uses an invalid public network code.");
            if (!command.StableKey.StartsWith("command.", StringComparison.Ordinal))
                throw new ArgumentException($"Command {command.StableKey} has an invalid stable key.");
            if (i > 0 && string.CompareOrdinal(previousKey, command.StableKey) >= 0)
                throw new ArgumentException("Command definitions must be unique and sorted by stable key.");
            previousKey = command.StableKey;
            if (!ids.Add(command.Id.Value)) throw new ArgumentException($"Duplicate command stable ID {command.StableKey}.");
            if (!occupiedIds.Add(command.Id.Value)) throw new ArgumentException($"Stable ID collision at command {command.StableKey}.");
            if (!networkCodes.Add(code)) throw new ArgumentException($"Duplicate command network code {code}.");
            if (command.RequiredResearch.Value != 0 &&
                (!catalog.TryGetResearch(command.RequiredResearch, out ResearchDefinition research) ||
                 !string.Equals(research.StableKey, command.RequiredResearchStableKey, StringComparison.Ordinal)))
                throw new ArgumentException($"Command {command.StableKey} references missing research {command.RequiredResearchStableKey}.");
            HashSet<string> eligibility = new(StringComparer.Ordinal);
            for (int tagIndex = 0; tagIndex < command.EligibleEntityTags.Length; tagIndex++)
            {
                string tag = command.EligibleEntityTags[tagIndex];
                if (string.IsNullOrWhiteSpace(tag))
                    throw new ArgumentException($"Command {command.StableKey} contains an empty eligibility tag.");
                if (!eligibility.Add(tag)) throw new ArgumentException($"Command {command.StableKey} repeats eligibility {tag}.");
                bool symbolic = tag.StartsWith("component.", StringComparison.Ordinal) ||
                    tag.StartsWith("capability.", StringComparison.Ordinal) || tag.StartsWith("tag.", StringComparison.Ordinal);
                if (!symbolic && !catalog.TryGetEntity(tag, out _))
                    throw new ArgumentException($"Command {command.StableKey} references missing eligible entity {tag}.");
            }
        }

        foreach (SimCommandType type in Enum.GetValues(typeof(SimCommandType)))
        {
            ushort code = (ushort)type;
            if (code >= (ushort)SimCommandType.DebugOpenExcavatable) continue;
            if (!networkCodes.Contains(code)) throw new ArgumentException($"Public command {type} is absent from the command catalog.");
        }
    }

    private static HashSet<uint> CollectOccupiedIds(PrototypeContentCatalog catalog)
    {
        HashSet<uint> result = new();
        for (int i = 0; i < catalog.MovementProfiles.Length; i++) result.Add(catalog.MovementProfiles[i].Id.Value);
        for (int i = 0; i < catalog.Entities.Length; i++) result.Add(catalog.Entities[i].Id.Value);
        for (int i = 0; i < catalog.ResourceNodes.Length; i++) result.Add(catalog.ResourceNodes[i].Id.Value);
        for (int i = 0; i < catalog.Weapons.Length; i++) result.Add(catalog.Weapons[i].Id.Value);
        for (int i = 0; i < catalog.Transformations.Length; i++)
        {
            result.Add(catalog.Transformations[i].Id.Value);
            result.Add(catalog.Transformations[i].ModeA.StateId.Value);
            result.Add(catalog.Transformations[i].ModeB.StateId.Value);
        }
        for (int i = 0; i < catalog.Research.Length; i++) result.Add(catalog.Research[i].Id.Value);
        return result;
    }
}
}
