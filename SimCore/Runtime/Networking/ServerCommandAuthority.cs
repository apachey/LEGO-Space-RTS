using System;
using System.Collections.Generic;
using System.IO;

namespace LegoSpaceRTS.SimCore
{
public enum NetworkCommandRejection : ushort
{
    None = 0,
    MalformedPacket = 1,
    PacketTooLarge = 2,
    ProtocolMismatch = 3,
    SessionTokenMismatch = 4,
    PlayerSlotMismatch = 5,
    SequenceMismatch = 6,
    RateLimitExceeded = 7,
    DebugCommandForbidden = 8,
    EmptyEntitySet = 9,
    EntityOrderInvalid = 10,
    EntityMissing = 11,
    EntityNotOwned = 12,
    CommandIneligible = 13,
    TargetMissing = 14,
    TargetNotOwned = 15,
    TargetNotVisible = 16,
    TargetIllegal = 17,
    InsufficientResources = 18,
    InsufficientEnergyOrCharge = 19,
    TechnologyLocked = 20,
    StateBlocked = 21,
    InvalidPlacement = 22,
    ServiceMembershipRequired = 23
}

public readonly struct NetworkCommandSessionWelcome
{
    public readonly ulong SessionToken;
    public readonly byte PlayerSlot;
    public NetworkCommandSessionWelcome(ulong sessionToken, byte playerSlot)
    {
        SessionToken = sessionToken;
        PlayerSlot = playerSlot;
    }
}

public readonly struct NetworkCommandAcknowledgment
{
    public readonly uint ClientSequence;
    public readonly NetworkCommandRejection Rejection;
    public readonly SimTick ExecutionTick;
    public bool Accepted => Rejection == NetworkCommandRejection.None;

    public NetworkCommandAcknowledgment(uint clientSequence, NetworkCommandRejection rejection, SimTick executionTick)
    {
        ClientSequence = clientSequence;
        Rejection = rejection;
        ExecutionTick = executionTick;
    }
}

/// <summary>Project-owned T059 packet format. ENet remains only the carrier.</summary>
public static class NetworkCommandProtocol
{
    public const ushort FormatVersion = 1;
    public const int MaximumRequestBytes = 4096;
    private const uint RequestMagic = 0x444D434C; // LCMD
    private const uint WelcomeMagic = 0x5345534C; // LSES
    private const uint AcknowledgmentMagic = 0x4B43414C; // LACK

    public static byte[] EncodeRequest(ulong sessionToken, CommandEnvelope command)
    {
        if (sessionToken == 0) throw new ArgumentOutOfRangeException(nameof(sessionToken));
        using MemoryStream stream = new();
        using BinaryWriter writer = new(stream);
        writer.Write(RequestMagic);
        writer.Write(FormatVersion);
        writer.Write(SnapshotSerializer.SimulationProtocolVersion);
        writer.Write(sessionToken);
        command.Write(writer);
        writer.Flush();
        if (stream.Length > MaximumRequestBytes) throw new InvalidOperationException("Encoded command exceeded the network request bound.");
        return stream.ToArray();
    }

    public static bool TryDecodeRequest(byte[] packet, out ulong sessionToken, out CommandEnvelope command, out NetworkCommandRejection rejection)
    {
        sessionToken = 0;
        command = default;
        rejection = NetworkCommandRejection.MalformedPacket;
        if (packet == null) return false;
        if (packet.Length > MaximumRequestBytes)
        {
            rejection = NetworkCommandRejection.PacketTooLarge;
            return false;
        }
        try
        {
            using MemoryStream stream = new(packet, false);
            using BinaryReader reader = new(stream);
            if (reader.ReadUInt32() != RequestMagic) return false;
            ushort format = reader.ReadUInt16();
            ushort simulationProtocol = reader.ReadUInt16();
            if (format != FormatVersion || simulationProtocol != SnapshotSerializer.SimulationProtocolVersion)
            {
                rejection = NetworkCommandRejection.ProtocolMismatch;
                return false;
            }
            sessionToken = reader.ReadUInt64();
            command = CommandEnvelope.Read(reader);
            if (stream.Position != stream.Length) return false;
            rejection = NetworkCommandRejection.None;
            return true;
        }
        catch (EndOfStreamException) { return false; }
        catch (IOException) { return false; }
        catch (ArgumentException) { return false; }
        catch (OverflowException) { return false; }
    }

    public static byte[] EncodeWelcome(NetworkCommandSessionWelcome welcome)
    {
        if (welcome.SessionToken == 0) throw new ArgumentOutOfRangeException(nameof(welcome));
        using MemoryStream stream = new();
        using BinaryWriter writer = new(stream);
        writer.Write(WelcomeMagic);
        writer.Write(FormatVersion);
        writer.Write(SnapshotSerializer.SimulationProtocolVersion);
        writer.Write(welcome.SessionToken);
        writer.Write(welcome.PlayerSlot);
        writer.Flush();
        return stream.ToArray();
    }

    public static bool TryDecodeWelcome(byte[] packet, out NetworkCommandSessionWelcome welcome)
    {
        welcome = default;
        if (packet == null) return false;
        try
        {
            using MemoryStream stream = new(packet, false);
            using BinaryReader reader = new(stream);
            if (reader.ReadUInt32() != WelcomeMagic || reader.ReadUInt16() != FormatVersion ||
                reader.ReadUInt16() != SnapshotSerializer.SimulationProtocolVersion) return false;
            ulong token = reader.ReadUInt64();
            byte playerSlot = reader.ReadByte();
            if (token == 0 || stream.Position != stream.Length) return false;
            welcome = new NetworkCommandSessionWelcome(token, playerSlot);
            return true;
        }
        catch (EndOfStreamException) { return false; }
        catch (IOException) { return false; }
    }

    public static byte[] EncodeAcknowledgment(NetworkCommandAcknowledgment acknowledgment)
    {
        using MemoryStream stream = new();
        using BinaryWriter writer = new(stream);
        writer.Write(AcknowledgmentMagic);
        writer.Write(FormatVersion);
        writer.Write(SnapshotSerializer.SimulationProtocolVersion);
        writer.Write(acknowledgment.ClientSequence);
        writer.Write((ushort)acknowledgment.Rejection);
        writer.Write(acknowledgment.ExecutionTick.Value);
        writer.Flush();
        return stream.ToArray();
    }

    public static bool TryDecodeAcknowledgment(byte[] packet, out NetworkCommandAcknowledgment acknowledgment)
    {
        acknowledgment = default;
        if (packet == null) return false;
        try
        {
            using MemoryStream stream = new(packet, false);
            using BinaryReader reader = new(stream);
            if (reader.ReadUInt32() != AcknowledgmentMagic || reader.ReadUInt16() != FormatVersion ||
                reader.ReadUInt16() != SnapshotSerializer.SimulationProtocolVersion) return false;
            uint sequence = reader.ReadUInt32();
            NetworkCommandRejection rejection = (NetworkCommandRejection)reader.ReadUInt16();
            int executionTick = reader.ReadInt32();
            if (!Enum.IsDefined(typeof(NetworkCommandRejection), rejection) || (rejection == NetworkCommandRejection.None) != (executionTick >= 0) || stream.Position != stream.Length) return false;
            acknowledgment = new NetworkCommandAcknowledgment(sequence, rejection, new SimTick(executionTick));
            return true;
        }
        catch (EndOfStreamException) { return false; }
        catch (IOException) { return false; }
        catch (ArgumentOutOfRangeException) { return false; }
    }
}

public sealed class ServerCommandSession
{
    public const int MaximumCommandsPerTick = 64;
    private int _rateLimitTick = -1;
    private int _commandsThisTick;

    public ServerCommandSession(int peerId, byte playerSlot, ulong sessionToken, uint lastProcessedSequence = 0)
    {
        if (peerId <= 0) throw new ArgumentOutOfRangeException(nameof(peerId));
        if (sessionToken == 0) throw new ArgumentOutOfRangeException(nameof(sessionToken));
        PeerId = peerId;
        PlayerSlot = playerSlot;
        SessionToken = sessionToken;
        LastProcessedSequence = lastProcessedSequence;
    }

    public int PeerId { get; }
    public byte PlayerSlot { get; }
    public ulong SessionToken { get; }
    public uint LastProcessedSequence { get; private set; }

    public ServerCommandSession Rebind(int peerId) => new(peerId, PlayerSlot, SessionToken, LastProcessedSequence);

    internal bool TryConsumeSequence(uint sequence)
    {
        if (LastProcessedSequence == uint.MaxValue || sequence != LastProcessedSequence + 1) return false;
        LastProcessedSequence = sequence;
        return true;
    }

    internal bool TryConsumeRateLimit(SimTick serverTick)
    {
        if (_rateLimitTick != serverTick.Value)
        {
            _rateLimitTick = serverTick.Value;
            _commandsThisTick = 0;
        }
        if (_commandsThisTick >= MaximumCommandsPerTick) return false;
        _commandsThisTick++;
        return true;
    }
}

public sealed class ServerCommandAuthority
{
    public NetworkCommandAcknowledgment Process(SimulationWorld world, ServerCommandSession session, byte[] packet)
        => Process(world, session, packet, out _);

    public NetworkCommandAcknowledgment Process(SimulationWorld world, ServerCommandSession session, byte[] packet, out CommandEnvelope acceptedCommand)
    {
        acceptedCommand = default;
        if (world == null) throw new ArgumentNullException(nameof(world));
        if (session == null) throw new ArgumentNullException(nameof(session));
        if (!NetworkCommandProtocol.TryDecodeRequest(packet, out ulong token, out CommandEnvelope request, out NetworkCommandRejection rejection))
            return Reject(0, rejection);
        if (token != session.SessionToken) return Reject(request.Sequence, NetworkCommandRejection.SessionTokenMismatch);
        if (session.PlayerSlot >= world.PlayerCount || request.PlayerSlot != session.PlayerSlot)
            return Reject(request.Sequence, NetworkCommandRejection.PlayerSlotMismatch);
        if (!session.TryConsumeSequence(request.Sequence)) return Reject(request.Sequence, NetworkCommandRejection.SequenceMismatch);
        if (!session.TryConsumeRateLimit(world.Tick)) return Reject(request.Sequence, NetworkCommandRejection.RateLimitExceeded);

        rejection = ServerCommandValidator.Validate(world, session.PlayerSlot, request);
        if (rejection != NetworkCommandRejection.None) return Reject(request.Sequence, rejection);

        SimTick executionTick = world.Tick.Next();
        CommandEnvelope accepted = new(executionTick, session.PlayerSlot, request.Sequence, request.Type, request.Entities,
            request.TargetPosition, request.Modifiers, request.TargetEntity, request.DebugFeatureId, request.ContentType,
            request.Orientation, request.EnergyPriority, request.MissionConfiguration, request.DesiredResonanceCommitment);
        world.Commands.Enqueue(accepted);
        acceptedCommand = accepted;
        return new NetworkCommandAcknowledgment(request.Sequence, NetworkCommandRejection.None, executionTick);
    }

    private static NetworkCommandAcknowledgment Reject(uint sequence, NetworkCommandRejection rejection)
        => new(sequence, rejection, new SimTick(-1));
}

public static class ServerCommandValidator
{
    public static NetworkCommandRejection Validate(SimulationWorld world, byte playerSlot, CommandEnvelope command)
    {
        if ((ushort)command.Type >= (ushort)SimCommandType.DebugOpenExcavatable)
            return NetworkCommandRejection.DebugCommandForbidden;
        if (!HasCanonicalPayloadShape(command)) return NetworkCommandRejection.CommandIneligible;
        if (RequiresEntities(command.Type) && command.Entities.Length == 0)
            return NetworkCommandRejection.EmptyEntitySet;

        uint previous = 0;
        for (int i = 0; i < command.Entities.Length; i++)
        {
            EntityId id = command.Entities[i];
            if (id.Value == 0 || id.Value <= previous) return NetworkCommandRejection.EntityOrderInvalid;
            previous = id.Value;
            if (!world.Entities.Exists(id) || !world.Entities.Ownership.TryGet(id, out Ownership owner) || owner.PlayerSlot != playerSlot)
                return NetworkCommandRejection.EntityNotOwned;
        }

        switch (command.Type)
        {
            case SimCommandType.Move:
            case SimCommandType.AttackMove:
            case SimCommandType.Patrol:
                if (!IsPointInMap(command.TargetPosition)) return NetworkCommandRejection.TargetIllegal;
                return ValidateMovers(world, command.Entities);
            case SimCommandType.Stop:
            case SimCommandType.HoldPosition:
            case SimCommandType.SetSpread:
                return ValidateMovers(world, command.Entities);
            case SimCommandType.Harvest:
                return ValidateHarvest(world, playerSlot, command);
            case SimCommandType.Build:
                return ValidateBuild(world, playerSlot, command);
            case SimCommandType.CancelConstruction:
                return ValidateOwnedTarget(world, playerSlot, command.TargetEntity, id => world.Entities.ConstructionSite.Has(id));
            case SimCommandType.AssistConstruction:
                return ValidateAssist(world, playerSlot, command);
            case SimCommandType.QueueProduction:
                return ValidateProduction(world, playerSlot, command);
            case SimCommandType.SetRallyPoint:
                return ValidateRally(world, playerSlot, command);
            case SimCommandType.SetEnergyPriority:
                return ValidateEnergyPriority(world, command);
            case SimCommandType.Attack:
                return ValidateAttack(world, playerSlot, command);
            case SimCommandType.Repair:
                return ValidateRepair(world, playerSlot, command);
            case SimCommandType.Load:
                return ValidateLoad(world, playerSlot, command);
            case SimCommandType.Unload:
                return ValidateUnload(world, command);
            case SimCommandType.StateChange:
                return ValidateStateChange(world, command);
            case SimCommandType.MissionRefit:
                return ValidateMissionRefit(world, playerSlot, command);
            case SimCommandType.SetResonanceCommitment:
                return ValidateResonance(world, playerSlot, command);
            case SimCommandType.StartSurge:
                return ValidateSurge(world, playerSlot, command.TargetEntity);
            case SimCommandType.CancelProduction:
                return ValidateOwnedTarget(world, playerSlot, command.TargetEntity, id => world.Entities.Production.Has(id) && command.Orientation < world.Entities.Production.Get(id).Count);
            case SimCommandType.ReorderProduction:
                return ValidateOwnedTarget(world, playerSlot, command.TargetEntity, id => world.Entities.Production.Has(id) && command.Orientation > 0 && command.DesiredResonanceCommitment > 0 && command.Orientation < world.Entities.Production.Get(id).Count && command.DesiredResonanceCommitment < world.Entities.Production.Get(id).Count);
            case SimCommandType.StartResearch:
                return ValidateOwnedTarget(world, playerSlot, command.TargetEntity, id => world.Entities.Building.TryGet(id, out Building b) && world.Content.TryGetResearch(command.ContentType, out ResearchDefinition research) && research.SourceBuildingType == b.Type && !world.ResearchJobs.ContainsKey(id.Value));
            case SimCommandType.CancelResearch:
                return ValidateOwnedTarget(world, playerSlot, command.TargetEntity, id => world.ResearchJobs.ContainsKey(id.Value));
            case SimCommandType.CancelMissionRefit:
                return ValidateOwnedTarget(world, playerSlot, command.TargetEntity, id => world.Entities.MissionRefitJob.Has(id));
            case SimCommandType.Excavate:
                if (!world.Entities.Excavatable.TryGet(command.TargetEntity, out Excavatable feature) || feature.State != ExcavatableFeatureState.Blocked) return NetworkCommandRejection.TargetIllegal;
                for (int i = 0; i < command.Entities.Length; i++) if (!world.Entities.Selectable.TryGet(command.Entities[i], out Selectable s) || !ExcavationSystem.TryDuration(s.ContentType, feature.TerrainClass == ExcavatableTerrainClass.ReinforcedBedrockBarrier, out _)) return NetworkCommandRejection.CommandIneligible;
                return NetworkCommandRejection.None;
            case SimCommandType.TubeTransfer:
                for (int i = 0; i < command.Entities.Length; i++) if (!TubeTransferSystem.IsEligible(world, command.Entities[i])) return NetworkCommandRejection.CommandIneligible;
                return ValidateOwnedTarget(world, playerSlot, command.TargetEntity, id => world.Entities.TubeStation.Has(id));
            case SimCommandType.TubeBuild:
                return command.Entities.Length == 1 && world.Entities.TubeStation.Has(command.Entities[0]) ? ValidateOwnedTarget(world, playerSlot, command.TargetEntity, id => world.Entities.TubeStation.Has(id)) : NetworkCommandRejection.CommandIneligible;
            case SimCommandType.DefenseResonanceShunt:
                return ValidateOwnedTarget(world, playerSlot, command.TargetEntity, id => world.DefenseNodeStates.ContainsKey(id.Value));
            case SimCommandType.RapidFabrication:
                return ValidateOwnedTarget(world, playerSlot, command.TargetEntity, id => world.Entities.Production.TryGet(id, out Production p) && p.Count > 0);
            case SimCommandType.ExcavationClamp:
                return command.Entities.Length == 1 ? ValidateVisibleTarget(world, playerSlot, command.TargetEntity) : NetworkCommandRejection.CommandIneligible;
            case SimCommandType.ProtectorStance:
            case SimCommandType.SearcherBrace:
                return command.Entities.Length > 0 ? NetworkCommandRejection.None : NetworkCommandRejection.EmptyEntitySet;
            case SimCommandType.Ping:
                return IsPointInMap(command.TargetPosition) ? NetworkCommandRejection.None : NetworkCommandRejection.TargetIllegal;
            default:
                return NetworkCommandRejection.CommandIneligible;
        }
    }

    private static bool RequiresEntities(SimCommandType type)
        => type == SimCommandType.Move || type == SimCommandType.AttackMove || type == SimCommandType.Patrol || type == SimCommandType.SetSpread || type == SimCommandType.Stop || type == SimCommandType.HoldPosition ||
           type == SimCommandType.Harvest || type == SimCommandType.Build || type == SimCommandType.AssistConstruction ||
           type == SimCommandType.SetRallyPoint || type == SimCommandType.SetEnergyPriority || type == SimCommandType.Attack ||
           type == SimCommandType.Repair || type == SimCommandType.Load || type == SimCommandType.Unload || type == SimCommandType.StateChange ||
           type == SimCommandType.Excavate || type == SimCommandType.TubeTransfer || type == SimCommandType.TubeBuild ||
           type == SimCommandType.ProtectorStance || type == SimCommandType.SearcherBrace || type == SimCommandType.ExcavationClamp;

    private static bool HasCanonicalPayloadShape(CommandEnvelope command)
    {
        bool usesEntities = RequiresEntities(command.Type);
        bool usesTargetEntity = command.Type == SimCommandType.Harvest || command.Type == SimCommandType.CancelConstruction ||
            command.Type == SimCommandType.AssistConstruction || command.Type == SimCommandType.QueueProduction ||
            command.Type == SimCommandType.SetRallyPoint || command.Type == SimCommandType.Attack || command.Type == SimCommandType.Repair ||
            command.Type == SimCommandType.Load || command.Type == SimCommandType.MissionRefit ||
            command.Type == SimCommandType.SetResonanceCommitment || command.Type == SimCommandType.StartSurge || command.Type == SimCommandType.CancelProduction ||
            command.Type == SimCommandType.ReorderProduction || command.Type == SimCommandType.StartResearch || command.Type == SimCommandType.CancelResearch ||
            command.Type == SimCommandType.CancelMissionRefit || command.Type == SimCommandType.Excavate || command.Type == SimCommandType.TubeTransfer ||
            command.Type == SimCommandType.TubeBuild || command.Type == SimCommandType.DefenseResonanceShunt || command.Type == SimCommandType.RapidFabrication ||
            command.Type == SimCommandType.ExcavationClamp;
        bool usesTargetPosition = command.Type == SimCommandType.Move || command.Type == SimCommandType.AttackMove || command.Type == SimCommandType.Patrol || command.Type == SimCommandType.Ping || command.Type == SimCommandType.Build ||
            command.Type == SimCommandType.SetRallyPoint || command.Type == SimCommandType.Unload;
        bool usesContentType = command.Type == SimCommandType.Build || command.Type == SimCommandType.QueueProduction || command.Type == SimCommandType.StartResearch;
        return (usesEntities || command.Entities.Length == 0) &&
               (usesTargetEntity || command.TargetEntity == EntityId.None) &&
               (usesTargetPosition || command.TargetPosition.Equals(FixVec2.Zero)) &&
               command.DebugFeatureId == 0 &&
               (usesContentType || command.ContentType.Value == 0) &&
               (command.Type == SimCommandType.Build || command.Type == SimCommandType.StateChange || command.Type == SimCommandType.CancelProduction || command.Type == SimCommandType.ReorderProduction || command.Orientation == 0) &&
               (command.Type == SimCommandType.SetEnergyPriority || command.EnergyPriority == EnergyPriority.Normal) &&
               (command.Type == SimCommandType.MissionRefit || command.MissionConfiguration == MissionConfiguration.None) &&
               (command.Type == SimCommandType.SetResonanceCommitment || command.Type == SimCommandType.ReorderProduction || command.DesiredResonanceCommitment == 0);
    }

    private static NetworkCommandRejection ValidateMovers(SimulationWorld world, EntityId[] entities)
    {
        for (int i = 0; i < entities.Length; i++)
        {
            EntityId id = entities[i];
            if (!world.Entities.Navigation.Has(id) || !world.Entities.Movement.Has(id) || TransportSystem.IsLoadedPassenger(world, id) ||
                world.Entities.MissionRefitJob.Has(id) || world.Entities.TubeTransfer.Has(id)) return NetworkCommandRejection.CommandIneligible;
        }
        return NetworkCommandRejection.None;
    }

    private static NetworkCommandRejection ValidateHarvest(SimulationWorld world, byte playerSlot, CommandEnvelope command)
    {
        NetworkCommandRejection target = ValidateVisibleTarget(world, playerSlot, command.TargetEntity);
        if (target != NetworkCommandRejection.None) return target;
        if (!world.Entities.ResourceNode.TryGet(command.TargetEntity, out ResourceNode node) || node.IsDepleted)
            return NetworkCommandRejection.TargetIllegal;
        for (int i = 0; i < command.Entities.Length; i++)
        {
            EntityId id = command.Entities[i];
            if (!world.Entities.Worker.Has(id) || !world.Entities.Navigation.Has(id) || !world.Entities.Movement.Has(id) ||
                TransportSystem.IsPassengerBusy(world, id) || !world.Entities.ResourceCarrier.TryGet(id, out ResourceCarrier carrier) ||
                carrier.IsFull || carrier.Type != node.Type) return NetworkCommandRejection.CommandIneligible;
        }
        return NetworkCommandRejection.None;
    }

    private static NetworkCommandRejection ValidateBuild(SimulationWorld world, byte playerSlot, CommandEnvelope command)
    {
        if ((command.TargetPosition.X.Raw & (Fix32.OneRaw - 1)) != 0 || (command.TargetPosition.Y.Raw & (Fix32.OneRaw - 1)) != 0)
            return NetworkCommandRejection.InvalidPlacement;
        int x = command.TargetPosition.X.FloorToInt(), y = command.TargetPosition.Y.FloorToInt();
        if (x < short.MinValue || x > short.MaxValue || y < short.MinValue || y > short.MaxValue)
            return NetworkCommandRejection.InvalidPlacement;
        if (!world.Content.TryGetBuilding(command.ContentType, out BuildingDefinition definition) || command.Orientation > 3 ||
            (!definition.Rotatable && command.Orientation != 0 && !(command.ContentType == DefenseNodeSystem.DefenseNodeType && command.Orientation <= 1))) return NetworkCommandRejection.InvalidPlacement;
        if (!ConstructionPlacement.IsBuildCommandAvailable(command.ContentType)) return NetworkCommandRejection.TechnologyLocked;
        byte width = definition.RotatedWidth(command.Orientation), height = definition.RotatedHeight(command.Orientation);
        if (x < 0 || y < 0 || x + width > MapGrid.BuildWidth || y + height > MapGrid.BuildHeight)
            return NetworkCommandRejection.InvalidPlacement;
        for (byte localY = 0; localY < height; localY++)
        for (byte localX = 0; localX < width; localX++)
            if (definition.Occupies(localX, localY, command.Orientation) && !world.Fog.IsVisible(playerSlot, x + localX, y + localY))
                return NetworkCommandRejection.TargetNotVisible;
        PlacementValidation placement = ConstructionPlacement.Validate(world, playerSlot, command.Entities, command.ContentType, (short)x, (short)y, command.Orientation);
        if (placement.IsValid) return NetworkCommandRejection.None;
        return placement.Failure switch
        {
            PlacementFailure.NoEligibleBuilder => NetworkCommandRejection.CommandIneligible,
            PlacementFailure.MissingPrerequisite or PlacementFailure.CommandUnavailable => NetworkCommandRejection.TechnologyLocked,
            PlacementFailure.InsufficientOre or PlacementFailure.InsufficientCrystals => NetworkCommandRejection.InsufficientResources,
            PlacementFailure.NoEnergyDomain or PlacementFailure.InsufficientEnergy => NetworkCommandRejection.InsufficientEnergyOrCharge,
            _ => NetworkCommandRejection.InvalidPlacement
        };
    }

    private static NetworkCommandRejection ValidateAssist(SimulationWorld world, byte playerSlot, CommandEnvelope command)
    {
        NetworkCommandRejection target = ValidateOwnedTarget(world, playerSlot, command.TargetEntity, id => world.Entities.ConstructionSite.Has(id));
        if (target != NetworkCommandRejection.None) return target;
        for (int i = 0; i < command.Entities.Length; i++)
            if (!world.Entities.Builder.Has(command.Entities[i]) || !world.Entities.Navigation.Has(command.Entities[i]) ||
                !world.Entities.Movement.Has(command.Entities[i]) || TransportSystem.IsPassengerBusy(world, command.Entities[i]))
                return NetworkCommandRejection.CommandIneligible;
        return NetworkCommandRejection.None;
    }

    private static NetworkCommandRejection ValidateProduction(SimulationWorld world, byte playerSlot, CommandEnvelope command)
    {
        NetworkCommandRejection target = ValidateOwnedTarget(world, playerSlot, command.TargetEntity, id => world.Entities.Production.Has(id));
        if (target != NetworkCommandRejection.None) return target;
        if (!world.Entities.Building.TryGet(command.TargetEntity, out Building building) || building.State != BuildingState.Completed ||
            !ProductionSystem.IsRuntimeEnabledUnit(command.ContentType) ||
            !world.Content.TryGetProduction(command.ContentType, out UnitProductionDefinition definition) || !definition.CanProduceAt(building.Type) ||
            !ActionPrerequisites.AreMet(world, playerSlot, definition.PrerequisiteGroups))
            return NetworkCommandRejection.CommandIneligible;
        Production production = world.Entities.Production.Get(command.TargetEntity);
        if (production.Count >= Production.Capacity || !OperationsCapacitySystem.CanReserve(world, playerSlot, definition.OperationsCapacity))
            return NetworkCommandRejection.StateBlocked;
        if (!EnergyDomainSystem.TryResolveForEntity(world, command.TargetEntity, playerSlot, out EntityId energyRoot) ||
            !EnergyDomainSystem.CanSpend(world, energyRoot, definition.EnergyCost)) return NetworkCommandRejection.InsufficientEnergyOrCharge;
        EntityId componentRoot = WorksiteGraphSystem.TryGetComponentForEntity(world, command.TargetEntity, out EntityId component) ? component : EntityId.None;
        if (!ProductionSystem.TryFindFundingBank(world, playerSlot, componentRoot, ResourceType.Ore, definition.OreCost, command.TargetEntity, out _) ||
            !ProductionSystem.TryFindFundingBank(world, playerSlot, componentRoot, ResourceType.Crystal, definition.CrystalCost, command.TargetEntity, out _))
            return NetworkCommandRejection.InsufficientResources;
        return NetworkCommandRejection.None;
    }

    private static NetworkCommandRejection ValidateRally(SimulationWorld world, byte playerSlot, CommandEnvelope command)
    {
        if (!IsPointInMap(command.TargetPosition)) return NetworkCommandRejection.TargetIllegal;
        for (int i = 0; i < command.Entities.Length; i++)
            if (!world.Entities.Production.Has(command.Entities[i])) return NetworkCommandRejection.CommandIneligible;
        if (command.TargetEntity == EntityId.None) return NetworkCommandRejection.None;
        NetworkCommandRejection visible = ValidateVisibleTarget(world, playerSlot, command.TargetEntity);
        return visible != NetworkCommandRejection.None ? visible :
            world.Entities.ResourceNode.Has(command.TargetEntity) ? NetworkCommandRejection.None : NetworkCommandRejection.TargetIllegal;
    }

    private static NetworkCommandRejection ValidateEnergyPriority(SimulationWorld world, CommandEnvelope command)
    {
        bool changes = false;
        for (int i = 0; i < command.Entities.Length; i++)
        {
            EntityId id = command.Entities[i];
            if (!world.Entities.PowerState.TryGet(id, out PowerState power) || !world.Entities.EnergyDomainMember.Has(id))
                return NetworkCommandRejection.CommandIneligible;
            changes |= power.Priority != command.EnergyPriority;
        }
        return changes ? NetworkCommandRejection.None : NetworkCommandRejection.StateBlocked;
    }

    private static NetworkCommandRejection ValidateAttack(SimulationWorld world, byte playerSlot, CommandEnvelope command)
    {
        NetworkCommandRejection target = ValidateVisibleTarget(world, playerSlot, command.TargetEntity);
        if (target != NetworkCommandRejection.None) return target;
        for (int i = 0; i < command.Entities.Length; i++)
        {
            EntityId source = command.Entities[i];
            if (!world.Entities.Targeting.Has(source) || TransportSystem.IsPassengerBusy(world, source))
                return NetworkCommandRejection.CommandIneligible;
            if (!TargetingSystem.IsLegalTarget(world, source, command.TargetEntity, requireVisible: true))
                return NetworkCommandRejection.TargetIllegal;
        }
        return NetworkCommandRejection.None;
    }

    private static NetworkCommandRejection ValidateRepair(SimulationWorld world, byte playerSlot, CommandEnvelope command)
    {
        NetworkCommandRejection target = ValidateOwnedTarget(world, playerSlot, command.TargetEntity, id => world.Entities.Health.Has(id));
        if (target != NetworkCommandRejection.None) return target;
        Health health = world.Entities.Health.Get(command.TargetEntity);
        if (health.Current <= Fix32.Zero || health.Current >= health.Maximum || !world.Entities.Transform.Has(command.TargetEntity) ||
            !world.Entities.Selectable.Has(command.TargetEntity)) return NetworkCommandRejection.TargetIllegal;
        for (int i = 0; i < command.Entities.Length; i++)
        {
            EntityId id = command.Entities[i];
            if (id == command.TargetEntity || !world.Entities.Builder.Has(id) || !world.Entities.Navigation.Has(id) ||
                !world.Entities.Movement.Has(id) || TransportSystem.IsPassengerBusy(world, id)) return NetworkCommandRejection.CommandIneligible;
        }
        return NetworkCommandRejection.None;
    }

    private static NetworkCommandRejection ValidateLoad(SimulationWorld world, byte playerSlot, CommandEnvelope command)
    {
        NetworkCommandRejection target = ValidateOwnedTarget(world, playerSlot, command.TargetEntity, id => world.Entities.Transport.Has(id));
        if (target != NetworkCommandRejection.None) return target;
        Transport transport = world.Entities.Transport.Get(command.TargetEntity);
        if (transport.JobState == TransportJobState.MovingToUnload || transport.JobState == TransportJobState.UnloadSettling ||
            transport.JobState == TransportJobState.Unloading || transport.JobState == TransportJobState.UnloadBlocked)
            return NetworkCommandRejection.StateBlocked;
        int requested = transport.OccupiedPoints;
        for (int i = 0; i < command.Entities.Length; i++)
        {
            EntityId id = command.Entities[i];
            if (id == command.TargetEntity || !world.Entities.Passenger.TryGet(id, out Passenger passenger) ||
                passenger.State != PassengerState.Grounded || !world.Entities.Navigation.Has(id) || !world.Entities.Movement.Has(id))
                return NetworkCommandRejection.CommandIneligible;
            requested = checked(requested + passenger.SizePoints);
        }
        return requested <= transport.CapacityPoints ? NetworkCommandRejection.None : NetworkCommandRejection.StateBlocked;
    }

    private static NetworkCommandRejection ValidateUnload(SimulationWorld world, CommandEnvelope command)
    {
        if (!IsPointInMap(command.TargetPosition)) return NetworkCommandRejection.TargetIllegal;
        for (int i = 0; i < command.Entities.Length; i++)
        {
            EntityId id = command.Entities[i];
            if (!world.Entities.Transport.TryGet(id, out Transport transport) || transport.PassengerCount == 0 ||
                !world.Entities.Navigation.Has(id) || !world.Entities.Movement.Has(id) || !world.Entities.Transform.Has(id))
                return NetworkCommandRejection.CommandIneligible;
        }
        return NetworkCommandRejection.None;
    }

    private static NetworkCommandRejection ValidateStateChange(SimulationWorld world, CommandEnvelope command)
    {
        for (int i = 0; i < command.Entities.Length; i++)
        {
            EntityId id = command.Entities[i];
            if (world.DefenseNodeStates.TryGetValue(id.Value, out DefenseNodeState node))
            {
                if (command.Orientation > 1 || node.IsReconfiguring || node.CurrentMode == (DefenseNodeMode)command.Orientation ||
                    DefenseNodeSystem.IsUnderPressure(world.Tick.Value, node.LastHostileCombatTick)) return NetworkCommandRejection.StateBlocked;
                continue;
            }
            if (!world.Entities.Transformation.TryGet(id, out Transformation state) || state.Phase == TransformationPhase.RollingBack ||
                (world.Entities.Health.TryGet(id, out Health health) && health.IsDepleted) ||
                (state.Phase == TransformationPhase.Idle && world.Tick.Value < state.ReversalLockedUntilTick))
                return NetworkCommandRejection.StateBlocked;
        }
        return NetworkCommandRejection.None;
    }

    private static NetworkCommandRejection ValidateMissionRefit(SimulationWorld world, byte playerSlot, CommandEnvelope command)
    {
        NetworkCommandRejection target = ValidateOwnedTarget(world, playerSlot, command.TargetEntity, id => world.Entities.Selectable.Has(id));
        if (target != NetworkCommandRejection.None) return target;
        if (world.Entities.Selectable.Get(command.TargetEntity).ContentType != StableId.FromKey("unit.ast.t3_trike") ||
            world.Entities.MissionRefitJob.Has(command.TargetEntity)) return NetworkCommandRejection.CommandIneligible;
        if (!ForwardServiceSystem.TryGetProviderForMember(world, command.TargetEntity, out EntityId provider))
            return NetworkCommandRejection.ServiceMembershipRequired;
        MissionConfiguration current = MissionConfiguration.T3Escort;
        byte ownedMask = 1;
        bool surveyUnlocked = false;
        ushort lockTicks = 0;
        if (world.Entities.MissionRefitState.TryGet(command.TargetEntity, out MissionRefitState state))
        { current = state.CurrentConfiguration; ownedMask = state.OwnedConfigurationMask; surveyUnlocked = state.SurveyUnlocked; lockTicks = state.ConfigurationLockTicks; }
        if (current == command.MissionConfiguration || lockTicks > 0) return NetworkCommandRejection.StateBlocked;
        if (command.MissionConfiguration == MissionConfiguration.T3Survey && !surveyUnlocked) return NetworkCommandRejection.TechnologyLocked;
        byte targetBit = command.MissionConfiguration == MissionConfiguration.T3Escort ? (byte)1 : (byte)2;
        bool ownsTarget = (ownedMask & targetBit) != 0;
        int ore = ownsTarget ? MissionRefitSystem.LaterSwapOre : MissionRefitSystem.FirstSurveyInstallOre;
        int energy = ownsTarget ? MissionRefitSystem.LaterSwapEnergy : MissionRefitSystem.FirstSurveyInstallEnergy;
        if (!HasResourceBankWithAmount(world, playerSlot, ResourceType.Ore, ore)) return NetworkCommandRejection.InsufficientResources;
        if (!EnergyDomainSystem.TryResolveForEntity(world, provider, playerSlot, out EntityId energyRoot) ||
            !EnergyDomainSystem.CanSpend(world, energyRoot, energy)) return NetworkCommandRejection.InsufficientEnergyOrCharge;
        return NetworkCommandRejection.None;
    }

    private static NetworkCommandRejection ValidateResonance(SimulationWorld world, byte playerSlot, CommandEnvelope command)
    {
        NetworkCommandRejection target = ValidateOwnedTarget(world, playerSlot, command.TargetEntity, id => ResonanceCoreSystem.IsValidCore(world, id));
        if (target != NetworkCommandRejection.None) return target;
        ResonanceCore core = world.Entities.ResonanceCore.Get(command.TargetEntity);
        if (command.DesiredResonanceCommitment > ResonanceCoreSystem.MaximumSlots(core)) return NetworkCommandRejection.TechnologyLocked;
        int projected = ResonanceCoreSystem.CountCommitted(core) + (core.TransitionKind == ResonanceTransitionKind.Commit ? 1 : 0);
        if (command.DesiredResonanceCommitment > projected && CountResource(world, playerSlot, ResourceType.Crystal) < command.DesiredResonanceCommitment - projected)
            return NetworkCommandRejection.InsufficientResources;
        return command.DesiredResonanceCommitment == core.DesiredCommittedCrystals ? NetworkCommandRejection.StateBlocked : NetworkCommandRejection.None;
    }

    private static NetworkCommandRejection ValidateSurge(SimulationWorld world, byte playerSlot, EntityId anchor)
    {
        NetworkCommandRejection target = ValidateOwnedTarget(world, playerSlot, anchor, id => ResonanceCoreSystem.IsValidCore(world, id));
        if (target != NetworkCommandRejection.None) return target;
        if (world.Entities.SurgeZone.Has(anchor) || !BrownoutSystem.IsOperational(world, anchor)) return NetworkCommandRejection.StateBlocked;
        AlienChargeState charge = world.GetAlienCharge(playerSlot);
        if (!charge.ResonanceInitiationUnlocked) return NetworkCommandRejection.TechnologyLocked;
        return charge.CurrentMillicharge >= AlienChargeSystem.SurgeCostMillicharge ? NetworkCommandRejection.None : NetworkCommandRejection.InsufficientEnergyOrCharge;
    }

    private static NetworkCommandRejection ValidateOwnedTarget(SimulationWorld world, byte playerSlot, EntityId target, Func<EntityId, bool> eligibility)
    {
        if (target == EntityId.None || !world.Entities.Exists(target) ||
            !world.Entities.Ownership.TryGet(target, out Ownership owner) || owner.PlayerSlot != playerSlot)
            return NetworkCommandRejection.TargetNotOwned;
        return eligibility(target) ? NetworkCommandRejection.None : NetworkCommandRejection.TargetIllegal;
    }

    private static NetworkCommandRejection ValidateVisibleTarget(SimulationWorld world, byte playerSlot, EntityId target)
    {
        if (target == EntityId.None) return NetworkCommandRejection.TargetMissing;
        if (!world.Entities.Exists(target) || !world.Entities.Transform.TryGet(target, out SimTransform transform))
            return NetworkCommandRejection.TargetNotVisible;
        int x = transform.Position.X.FloorToInt(), y = transform.Position.Y.FloorToInt();
        if ((uint)x >= FogState.Width || (uint)y >= FogState.Height || !world.Fog.IsVisible(playerSlot, x, y))
            return NetworkCommandRejection.TargetNotVisible;
        return NetworkCommandRejection.None;
    }

    private static bool IsPointInMap(FixVec2 point)
        => point.X >= Fix32.Zero && point.Y >= Fix32.Zero && point.X < Fix32.FromInt(MapGrid.BuildWidth) && point.Y < Fix32.FromInt(MapGrid.BuildHeight);

    private static int CountResource(SimulationWorld world, byte playerSlot, ResourceType type)
    {
        int total = 0;
        IReadOnlyList<EntityId> alive = world.Entities.Alive;
        for (int i = 0; i < alive.Count; i++)
        {
            EntityId id = alive[i];
            if (world.Entities.Ownership.TryGet(id, out Ownership owner) && owner.PlayerSlot == playerSlot &&
                world.Entities.ResourceBank.TryGet(id, out ResourceBank bank) && bank.Type == type)
                total = checked(total + bank.ProcessedAmount);
        }
        return total;
    }

    private static bool HasResourceBankWithAmount(SimulationWorld world, byte playerSlot, ResourceType type, int amount)
    {
        IReadOnlyList<EntityId> alive = world.Entities.Alive;
        for (int i = 0; i < alive.Count; i++)
        {
            EntityId id = alive[i];
            if (world.Entities.Ownership.TryGet(id, out Ownership owner) && owner.PlayerSlot == playerSlot &&
                world.Entities.ResourceBank.TryGet(id, out ResourceBank bank) && bank.Type == type && bank.ProcessedAmount >= amount)
                return true;
        }
        return false;
    }
}
}
