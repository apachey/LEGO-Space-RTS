using System;
using System.Linq;
using LegoSpaceRTS.SimCore;
using NUnit.Framework;

public class M6ServerCommandAuthorityTests
{
    private const ulong SessionToken = 0x1020304050607080UL;

    [Test]
    public void ProjectOwnedPacketsRoundTripAndRejectMalformedOrIncompatibleData()
    {
        CommandEnvelope command = new(new SimTick(999), 0, 7, SimCommandType.Move, new[] { new EntityId(3) },
            FixVec2.FromInts(44, 55), CommandModifiers.Queue);
        byte[] request = NetworkCommandProtocol.EncodeRequest(SessionToken, command);

        Assert.That(NetworkCommandProtocol.TryDecodeRequest(request, out ulong token, out CommandEnvelope decoded, out NetworkCommandRejection rejection), Is.True);
        Assert.Multiple(() =>
        {
            Assert.That(token, Is.EqualTo(SessionToken));
            Assert.That(rejection, Is.EqualTo(NetworkCommandRejection.None));
            Assert.That(decoded.ExecutionTick.Value, Is.EqualTo(999));
            Assert.That(decoded.PlayerSlot, Is.EqualTo(0));
            Assert.That(decoded.Sequence, Is.EqualTo(7));
            Assert.That(decoded.Type, Is.EqualTo(SimCommandType.Move));
            Assert.That(decoded.Entities.Select(id => id.Value), Is.EqualTo(new uint[] { 3 }));
            Assert.That(decoded.TargetPosition, Is.EqualTo(FixVec2.FromInts(44, 55)));
            Assert.That(decoded.Modifiers, Is.EqualTo(CommandModifiers.Queue));
        });

        NetworkCommandSessionWelcome welcome = new(SessionToken, 1);
        Assert.That(NetworkCommandProtocol.TryDecodeWelcome(NetworkCommandProtocol.EncodeWelcome(welcome), out NetworkCommandSessionWelcome decodedWelcome), Is.True);
        Assert.Multiple(() =>
        {
            Assert.That(decodedWelcome.SessionToken, Is.EqualTo(SessionToken));
            Assert.That(decodedWelcome.PlayerSlot, Is.EqualTo(1));
        });

        NetworkCommandAcknowledgment acknowledgment = new(7, NetworkCommandRejection.None, new SimTick(12));
        Assert.That(NetworkCommandProtocol.TryDecodeAcknowledgment(NetworkCommandProtocol.EncodeAcknowledgment(acknowledgment), out NetworkCommandAcknowledgment decodedAck), Is.True);
        Assert.Multiple(() =>
        {
            Assert.That(decodedAck.Accepted, Is.True);
            Assert.That(decodedAck.ClientSequence, Is.EqualTo(7));
            Assert.That(decodedAck.ExecutionTick.Value, Is.EqualTo(12));
        });

        byte[] trailing = request.Concat(new byte[] { 0xff }).ToArray();
        Assert.That(NetworkCommandProtocol.TryDecodeRequest(trailing, out _, out _, out NetworkCommandRejection trailingError), Is.False);
        Assert.That(trailingError, Is.EqualTo(NetworkCommandRejection.MalformedPacket));

        byte[] incompatible = (byte[])request.Clone();
        incompatible[4] = 0xff;
        Assert.That(NetworkCommandProtocol.TryDecodeRequest(incompatible, out _, out _, out NetworkCommandRejection versionError), Is.False);
        Assert.That(versionError, Is.EqualTo(NetworkCommandRejection.ProtocolMismatch));

        byte[] oversized = new byte[NetworkCommandProtocol.MaximumRequestBytes + 1];
        Assert.That(NetworkCommandProtocol.TryDecodeRequest(oversized, out _, out _, out NetworkCommandRejection sizeError), Is.False);
        Assert.That(sizeError, Is.EqualTo(NetworkCommandRejection.PacketTooLarge));
    }

    [Test]
    public void AuthorityRejectsForgeryThenSchedulesLegalIntentOnServerChosenTickExactlyOnce()
    {
        SimulationWorld world = ScenarioFactory.CreateFirstControllable(1);
        EntityId playerZero = ScenarioFactory.OwnedIds(world, 0).Single();
        EntityId playerOne = ScenarioFactory.OwnedIds(world, 1).Single();
        ServerCommandSession session = new(2, 0, SessionToken);
        ServerCommandAuthority authority = new();

        CommandEnvelope forged = Move(0, 1, playerOne, 80, 40);
        NetworkCommandAcknowledgment rejected = authority.Process(world, session, NetworkCommandProtocol.EncodeRequest(SessionToken, forged));
        Assert.Multiple(() =>
        {
            Assert.That(rejected.Rejection, Is.EqualTo(NetworkCommandRejection.EntityNotOwned));
            Assert.That(rejected.ExecutionTick.Value, Is.EqualTo(-1));
            Assert.That(world.Commands.Count, Is.Zero);
            Assert.That(session.LastProcessedSequence, Is.EqualTo(1));
        });

        CommandEnvelope legal = Move(0, 2, playerZero, 80, 40, requestedTick: 5000);
        byte[] legalPacket = NetworkCommandProtocol.EncodeRequest(SessionToken, legal);
        NetworkCommandAcknowledgment accepted = authority.Process(world, session, legalPacket);
        Assert.Multiple(() =>
        {
            Assert.That(accepted.Accepted, Is.True);
            Assert.That(accepted.ExecutionTick.Value, Is.EqualTo(1));
            Assert.That(world.Commands.Count, Is.EqualTo(1));
            Assert.That(world.Commands.All[0].ExecutionTick.Value, Is.EqualTo(1));
            Assert.That(world.Commands.All[0].PlayerSlot, Is.EqualTo(0));
        });

        NetworkCommandAcknowledgment duplicate = authority.Process(world, session, legalPacket);
        Assert.Multiple(() =>
        {
            Assert.That(duplicate.Rejection, Is.EqualTo(NetworkCommandRejection.SequenceMismatch));
            Assert.That(world.Commands.Count, Is.EqualTo(1));
        });

        new SimulationRunner(world).StepOneTick();
        NavigationAgent navigation = world.Entities.Navigation.Get(playerZero);
        Assert.Multiple(() =>
        {
            Assert.That(navigation.HasTarget, Is.True);
            Assert.That(FixVec2.Distance(navigation.Target, FixVec2.FromInts(80, 40)), Is.LessThan(Fix32.FromInt(2)));
            Assert.That(world.Commands.Count, Is.Zero);
        });
    }

    [Test]
    public void AuthenticationPlayerBindingAndDebugBoundaryDoNotTrustClientFields()
    {
        SimulationWorld world = ScenarioFactory.CreateFirstControllable(1);
        EntityId owned = ScenarioFactory.OwnedIds(world, 0).Single();
        ServerCommandSession session = new(2, 0, SessionToken);
        ServerCommandAuthority authority = new();

        CommandEnvelope playerMismatch = Move(1, 1, owned, 60, 40);
        Assert.That(authority.Process(world, session, NetworkCommandProtocol.EncodeRequest(SessionToken, playerMismatch)).Rejection,
            Is.EqualTo(NetworkCommandRejection.PlayerSlotMismatch));
        Assert.That(session.LastProcessedSequence, Is.Zero);

        CommandEnvelope validSequenceOne = Move(0, 1, owned, 60, 40);
        Assert.That(authority.Process(world, session, NetworkCommandProtocol.EncodeRequest(SessionToken ^ 1UL, validSequenceOne)).Rejection,
            Is.EqualTo(NetworkCommandRejection.SessionTokenMismatch));
        Assert.That(session.LastProcessedSequence, Is.Zero);

        CommandEnvelope debug = new(new SimTick(1), 0, 1, SimCommandType.DebugDrainEnergy, Array.Empty<EntityId>(), FixVec2.Zero);
        Assert.That(authority.Process(world, session, NetworkCommandProtocol.EncodeRequest(SessionToken, debug)).Rejection,
            Is.EqualTo(NetworkCommandRejection.DebugCommandForbidden));
        Assert.That(session.LastProcessedSequence, Is.EqualTo(1));

        Assert.That(authority.Process(world, session, new byte[] { 1, 2, 3 }).Rejection,
            Is.EqualTo(NetworkCommandRejection.MalformedPacket));
        Assert.That(session.LastProcessedSequence, Is.EqualTo(1));
    }

    [Test]
    public void VisibilityAndTargetRulesRejectHiddenEnemyOrders()
    {
        SimulationWorld world = ScenarioFactory.CreateFirstControllable(1);
        EntityId source = ScenarioFactory.OwnedIds(world, 0).Single();
        EntityId hiddenEnemy = ScenarioFactory.OwnedIds(world, 1).Single();
        SimTransform enemyTransform = world.Entities.Transform.Get(hiddenEnemy);
        Assert.That(world.Fog.IsVisible(0, enemyTransform.Position.X.FloorToInt(), enemyTransform.Position.Y.FloorToInt()), Is.False);

        CommandEnvelope attack = new(new SimTick(1), 0, 1, SimCommandType.Attack, new[] { source }, FixVec2.Zero, targetEntity: hiddenEnemy);
        ServerCommandSession session = new(2, 0, SessionToken);
        NetworkCommandAcknowledgment result = new ServerCommandAuthority().Process(world, session, NetworkCommandProtocol.EncodeRequest(SessionToken, attack));

        ServerCommandSession missingSession = new(3, 0, SessionToken + 1);
        CommandEnvelope missingAttack = new(new SimTick(1), 0, 1, SimCommandType.Attack, new[] { source }, FixVec2.Zero,
            targetEntity: new EntityId(world.Entities.NextEntityValue + 100));
        NetworkCommandAcknowledgment missing = new ServerCommandAuthority().Process(world, missingSession,
            NetworkCommandProtocol.EncodeRequest(SessionToken + 1, missingAttack));

        Assert.Multiple(() =>
        {
            Assert.That(result.Rejection, Is.EqualTo(NetworkCommandRejection.TargetNotVisible));
            Assert.That(missing.Rejection, Is.EqualTo(NetworkCommandRejection.TargetNotVisible));
            Assert.That(world.Commands.Count, Is.Zero);
        });
    }

    [Test]
    public void BuildValidationRequiresTheWholeFootprintToBeVisibleBeforeCheckingPlacement()
    {
        SimulationWorld world = ScenarioFactory.CreateCanonicalOpening();
        EntityId[] builders = ScenarioFactory.OwnedIds(world, 0).Where(world.Entities.Builder.Has).ToArray();
        ContentId hq = StableId.FromKey("building.rock_raiders.hq");
        Assert.That(world.Content.TryGetBuilding(hq, out BuildingDefinition definition), Is.True);

        (int X, int Y)? boundary = null;
        for (int y = 0; y <= MapGrid.BuildHeight - definition.FootprintHeight && !boundary.HasValue; y++)
        for (int x = 0; x <= MapGrid.BuildWidth - definition.FootprintWidth; x++)
        {
            if (!world.Fog.IsVisible(0, x, y)) continue;
            bool allVisible = true;
            for (byte localY = 0; localY < definition.FootprintHeight; localY++)
            for (byte localX = 0; localX < definition.FootprintWidth; localX++)
                if (definition.Occupies(localX, localY, 0) && !world.Fog.IsVisible(0, x + localX, y + localY)) allVisible = false;
            if (!allVisible) { boundary = (x, y); break; }
        }
        Assert.That(boundary.HasValue, Is.True, "The deterministic opening should expose a partially visible HQ footprint at the vision boundary.");

        CommandEnvelope build = new(new SimTick(1), 0, 1, SimCommandType.Build, builders,
            FixVec2.FromInts(boundary!.Value.X, boundary.Value.Y), contentType: hq);
        Assert.That(ServerCommandValidator.Validate(world, 0, build), Is.EqualTo(NetworkCommandRejection.TargetNotVisible));
    }

    [Test]
    public void ImportedT071InfrastructureCannotBypassThePreT073CommandCatalog()
    {
        SimulationWorld world = ScenarioFactory.CreateCanonicalOpening();
        EntityId[] builders = ScenarioFactory.OwnedIds(world, 0).Where(world.Entities.Builder.Has).ToArray();
        ContentId importedBuilding = StableId.FromKey("building.rock_raiders.engineering_workshop");
        Assert.That(world.Content.TryGetBuilding(importedBuilding, out _), Is.True);

        CommandEnvelope build = new(new SimTick(1), 0, 1, SimCommandType.Build, builders,
            FixVec2.Zero, contentType: importedBuilding);

        Assert.That(ServerCommandValidator.Validate(world, 0, build), Is.EqualTo(NetworkCommandRejection.TechnologyLocked));
    }

    [Test]
    public void UnusedPayloadFieldsCannotEnterTheAuthoritativeCommandLog()
    {
        SimulationWorld world = ScenarioFactory.CreateFirstControllable(1);
        EntityId owned = ScenarioFactory.OwnedIds(world, 0).Single();
        CommandEnvelope pollutedMove = new(new SimTick(1), 0, 1, SimCommandType.Move, new[] { owned }, FixVec2.FromInts(60, 40),
            debugFeatureId: 12, energyPriority: EnergyPriority.High);
        ServerCommandSession session = new(2, 0, SessionToken);

        NetworkCommandAcknowledgment result = new ServerCommandAuthority().Process(world, session,
            NetworkCommandProtocol.EncodeRequest(SessionToken, pollutedMove));

        Assert.Multiple(() =>
        {
            Assert.That(result.Rejection, Is.EqualTo(NetworkCommandRejection.CommandIneligible));
            Assert.That(world.Commands.Count, Is.Zero);
            Assert.That(session.LastProcessedSequence, Is.EqualTo(1));
        });
    }

    [Test]
    public void PerTickRateLimitIsBoundedAndResetsAfterAuthoritativeTick()
    {
        SimulationWorld world = ScenarioFactory.CreateFirstControllable(1);
        EntityId owned = ScenarioFactory.OwnedIds(world, 0).Single();
        ServerCommandSession session = new(2, 0, SessionToken);
        ServerCommandAuthority authority = new();

        for (uint sequence = 1; sequence <= ServerCommandSession.MaximumCommandsPerTick; sequence++)
        {
            NetworkCommandAcknowledgment result = authority.Process(world, session,
                NetworkCommandProtocol.EncodeRequest(SessionToken, Move(0, sequence, owned, 70, 40)));
            Assert.That(result.Accepted, Is.True, $"sequence {sequence}");
        }
        NetworkCommandAcknowledgment limited = authority.Process(world, session,
            NetworkCommandProtocol.EncodeRequest(SessionToken, Move(0, ServerCommandSession.MaximumCommandsPerTick + 1u, owned, 70, 40)));
        Assert.That(limited.Rejection, Is.EqualTo(NetworkCommandRejection.RateLimitExceeded));

        new SimulationRunner(world).StepOneTick();
        NetworkCommandAcknowledgment nextTick = authority.Process(world, session,
            NetworkCommandProtocol.EncodeRequest(SessionToken, Move(0, ServerCommandSession.MaximumCommandsPerTick + 2u, owned, 71, 40)));
        Assert.Multiple(() =>
        {
            Assert.That(nextTick.Accepted, Is.True);
            Assert.That(nextTick.ExecutionTick.Value, Is.EqualTo(2));
        });
    }

    private static CommandEnvelope Move(byte player, uint sequence, EntityId entity, int x, int y, int requestedTick = 1)
        => new(new SimTick(requestedTick), player, sequence, SimCommandType.Move, new[] { entity }, FixVec2.FromInts(x, y));
}
