namespace LegoSpaceRTS.SimCore
{
public static class DeterministicHash
{
    public const ulong FnvOffset64 = 14695981039346656037UL;
    public const ulong FnvPrime64 = 1099511628211UL;
    public static ulong Fnv1A64(byte[] bytes)
    {
        ulong hash = FnvOffset64;
        for (int i = 0; i < bytes.Length; i++) hash = unchecked((hash ^ bytes[i]) * FnvPrime64);
        return hash;
    }
}
}
