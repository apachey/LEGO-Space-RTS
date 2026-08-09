using System;
using System.IO;

namespace LegoSpaceRTS.SimCore
{
public sealed class FogState
{
    public const int Width = MapGrid.BuildWidth;
    public const int Height = MapGrid.BuildHeight;
    private readonly bool[][] _explored;
    private readonly ushort[][] _visible;

    public int PlayerCount => _explored.Length;
    public FogState(int players)
    {
        if (players <= 0 || players > 8) throw new ArgumentOutOfRangeException(nameof(players));
        _explored = new bool[players][]; _visible = new ushort[players][];
        for (int p = 0; p < players; p++) { _explored[p] = new bool[Width * Height]; _visible[p] = new ushort[Width * Height]; }
    }
    public VisibilityState Get(byte player, int x, int y)
    {
        int i = y * Width + x; if (_visible[player][i] > 0) return VisibilityState.Visible; return _explored[player][i] ? VisibilityState.Explored : VisibilityState.Unseen;
    }
    public bool IsVisible(byte player, int x, int y) => _visible[player][y * Width + x] > 0;
    public bool IsExplored(byte player, int x, int y) => _explored[player][y * Width + x];
    public void ClearCurrent() { for (int p = 0; p < _visible.Length; p++) Array.Clear(_visible[p], 0, _visible[p].Length); }
    public void AddVisible(byte player, int x, int y) { if ((uint)x >= Width || (uint)y >= Height) return; int i = y * Width + x; if (_visible[player][i] < ushort.MaxValue) _visible[player][i]++; _explored[player][i] = true; }
    public void Serialize(BinaryWriter w)
    {
        w.Write(PlayerCount); for (int p = 0; p < PlayerCount; p++) for (int i = 0; i < Width * Height; i++) w.Write(_explored[p][i]);
        for (int p = 0; p < PlayerCount; p++) for (int i = 0; i < Width * Height; i++) w.Write(_visible[p][i]);
    }
    public static FogState Deserialize(BinaryReader r)
    {
        FogState f = new(r.ReadInt32()); for (int p = 0; p < f.PlayerCount; p++) for (int i = 0; i < Width * Height; i++) f._explored[p][i] = r.ReadBoolean();
        for (int p = 0; p < f.PlayerCount; p++) for (int i = 0; i < Width * Height; i++) f._visible[p][i] = r.ReadUInt16(); return f;
    }
}
}
