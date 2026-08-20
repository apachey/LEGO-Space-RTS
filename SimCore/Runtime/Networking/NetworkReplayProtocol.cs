using System;
using System.Collections.Generic;
using System.IO;

namespace LegoSpaceRTS.SimCore
{
public readonly struct NetworkReplayRequest
{
    public readonly ulong SessionToken;
    public NetworkReplayRequest(ulong sessionToken) => SessionToken = sessionToken;
}

public readonly struct NetworkReplayChunk
{
    public readonly uint TransferId;
    public readonly ushort ChunkIndex;
    public readonly ushort ChunkCount;
    public readonly int TotalLength;
    public readonly ulong ReplayHash;
    public readonly byte[] Payload;

    public NetworkReplayChunk(uint transferId, ushort chunkIndex, ushort chunkCount, int totalLength, ulong replayHash, byte[] payload)
    {
        TransferId = transferId; ChunkIndex = chunkIndex; ChunkCount = chunkCount; TotalLength = totalLength; ReplayHash = replayHash; Payload = payload;
    }
}

/// <summary>Reliable-bulk T063 replay request and bounded chunk format.</summary>
public static class NetworkReplayProtocol
{
    public const ushort FormatVersion = 1;
    public const int MaximumChunkPayloadBytes = 48 * 1024;
    public const int MaximumReplayBytes = 512 * 1024 * 1024;
    private const uint RequestMagic = 0x5150524C; // LRPQ
    private const uint ChunkMagic = 0x4350524C; // LRPC

    public static byte[] EncodeRequest(NetworkReplayRequest request)
    {
        if (request.SessionToken == 0) throw new ArgumentOutOfRangeException(nameof(request));
        using MemoryStream stream = new(); using BinaryWriter writer = new(stream);
        WriteHeader(writer, RequestMagic); writer.Write(request.SessionToken); writer.Flush(); return stream.ToArray();
    }

    public static bool TryDecodeRequest(byte[] packet, out NetworkReplayRequest request)
    {
        request = default;
        if (packet == null) return false;
        try
        {
            using MemoryStream stream = new(packet, false); using BinaryReader reader = new(stream);
            if (!ReadHeader(reader, RequestMagic)) return false;
            ulong token = reader.ReadUInt64(); if (token == 0 || stream.Position != stream.Length) return false;
            request = new NetworkReplayRequest(token); return true;
        }
        catch (EndOfStreamException) { return false; }
        catch (IOException) { return false; }
    }

    public static NetworkReplayChunk[] CreateChunks(uint transferId, byte[] replayBytes)
    {
        if (transferId == 0) throw new ArgumentOutOfRangeException(nameof(transferId));
        if (replayBytes == null) throw new ArgumentNullException(nameof(replayBytes));
        if (replayBytes.Length == 0 || replayBytes.Length > MaximumReplayBytes) throw new ArgumentOutOfRangeException(nameof(replayBytes));
        int count = (replayBytes.Length + MaximumChunkPayloadBytes - 1) / MaximumChunkPayloadBytes;
        if (count > ushort.MaxValue) throw new InvalidOperationException("Replay requires too many reliable-bulk chunks.");
        ulong hash = DeterministicHash.Fnv1A64(replayBytes);
        NetworkReplayChunk[] chunks = new NetworkReplayChunk[count];
        for (int i = 0; i < count; i++)
        {
            int offset = i * MaximumChunkPayloadBytes; int length = Math.Min(MaximumChunkPayloadBytes, replayBytes.Length - offset);
            byte[] payload = new byte[length]; Array.Copy(replayBytes, offset, payload, 0, length);
            chunks[i] = new NetworkReplayChunk(transferId, checked((ushort)i), checked((ushort)count), replayBytes.Length, hash, payload);
        }
        return chunks;
    }

    public static byte[] EncodeChunk(NetworkReplayChunk chunk)
    {
        ValidateChunk(chunk);
        using MemoryStream stream = new(); using BinaryWriter writer = new(stream);
        WriteHeader(writer, ChunkMagic); writer.Write(chunk.TransferId); writer.Write(chunk.ChunkIndex); writer.Write(chunk.ChunkCount);
        writer.Write(chunk.TotalLength); writer.Write(chunk.ReplayHash); writer.Write(chunk.Payload.Length); writer.Write(chunk.Payload); writer.Flush();
        return stream.ToArray();
    }

    public static bool TryDecodeChunk(byte[] packet, out NetworkReplayChunk chunk)
    {
        chunk = default;
        if (packet == null || packet.Length > NetworkSnapshotProtocol.MaximumPacketBytes) return false;
        try
        {
            using MemoryStream stream = new(packet, false); using BinaryReader reader = new(stream);
            if (!ReadHeader(reader, ChunkMagic)) return false;
            uint transfer = reader.ReadUInt32(); ushort index = reader.ReadUInt16(), count = reader.ReadUInt16(); int total = reader.ReadInt32();
            ulong hash = reader.ReadUInt64(); int length = reader.ReadInt32();
            if (length < 0 || length > MaximumChunkPayloadBytes) return false;
            byte[] payload = reader.ReadBytes(length); if (payload.Length != length || stream.Position != stream.Length) return false;
            chunk = new NetworkReplayChunk(transfer, index, count, total, hash, payload); ValidateChunk(chunk); return true;
        }
        catch (EndOfStreamException) { return false; }
        catch (IOException) { return false; }
        catch (ArgumentException) { return false; }
    }

    private static void ValidateChunk(NetworkReplayChunk chunk)
    {
        if (chunk.TransferId == 0 || chunk.ChunkCount == 0 || chunk.ChunkIndex >= chunk.ChunkCount || chunk.TotalLength <= 0 ||
            chunk.TotalLength > MaximumReplayBytes || chunk.Payload == null || chunk.Payload.Length > MaximumChunkPayloadBytes)
            throw new ArgumentException("Invalid replay chunk.", nameof(chunk));
        int expectedLength = chunk.ChunkIndex + 1 == chunk.ChunkCount
            ? chunk.TotalLength - chunk.ChunkIndex * MaximumChunkPayloadBytes
            : MaximumChunkPayloadBytes;
        if (expectedLength <= 0 || chunk.Payload.Length != expectedLength) throw new ArgumentException("Replay chunk length does not match its index.", nameof(chunk));
    }

    private static void WriteHeader(BinaryWriter writer, uint magic)
    {
        writer.Write(magic); writer.Write(FormatVersion); writer.Write(SnapshotSerializer.SimulationProtocolVersion);
    }

    private static bool ReadHeader(BinaryReader reader, uint magic) => reader.ReadUInt32() == magic && reader.ReadUInt16() == FormatVersion &&
        reader.ReadUInt16() == SnapshotSerializer.SimulationProtocolVersion;
}

public sealed class NetworkReplayAssembler
{
    private uint _transferId;
    private ushort _chunkCount;
    private int _totalLength;
    private ulong _hash;
    private readonly SortedDictionary<ushort, byte[]> _chunks = new();

    public bool TryAdd(NetworkReplayChunk chunk, out byte[] replayBytes)
    {
        replayBytes = Array.Empty<byte>();
        if (_transferId == 0)
        {
            _transferId = chunk.TransferId; _chunkCount = chunk.ChunkCount; _totalLength = chunk.TotalLength; _hash = chunk.ReplayHash;
        }
        else if (chunk.TransferId != _transferId || chunk.ChunkCount != _chunkCount || chunk.TotalLength != _totalLength || chunk.ReplayHash != _hash)
            return false;
        if (_chunks.TryGetValue(chunk.ChunkIndex, out byte[] existing))
        {
            if (!BytesEqual(existing, chunk.Payload)) return false;
        }
        else _chunks.Add(chunk.ChunkIndex, chunk.Payload);
        if (_chunks.Count != _chunkCount) return false;

        replayBytes = new byte[_totalLength]; int offset = 0;
        for (ushort i = 0; i < _chunkCount; i++)
        {
            if (!_chunks.TryGetValue(i, out byte[] payload)) { replayBytes = Array.Empty<byte>(); return false; }
            Array.Copy(payload, 0, replayBytes, offset, payload.Length); offset += payload.Length;
        }
        if (offset != _totalLength || DeterministicHash.Fnv1A64(replayBytes) != _hash)
        {
            replayBytes = Array.Empty<byte>(); return false;
        }
        return true;
    }

    private static bool BytesEqual(byte[] a, byte[] b)
    {
        if (a.Length != b.Length) return false;
        for (int i = 0; i < a.Length; i++) if (a[i] != b[i]) return false;
        return true;
    }
}
}
