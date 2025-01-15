using D2SLib.IO;
using System.Text.Json.Serialization;

namespace D2SLib.Model.Save;

public sealed class Status : IDisposable
{
    private InternalBitArray _flags;
    public Status(byte flags)
    {
        _flags = new InternalBitArray(stackalloc byte[] { flags });
    }

    [JsonIgnore]
    public IList<bool> Flags => _flags;
    public bool IsNewbie { get => Flags[0]; set => Flags[0] = value; }
    public bool IsError { get => Flags[1]; set => Flags[1] = value; }
    public bool IsHardcore { get => Flags[2]; set => Flags[2] = value; }
    public bool IsDead { get => Flags[3]; set => Flags[3] = value; }
    public bool IsSaveProcess { get => Flags[4]; set => Flags[4] = value; }
    public bool IsExpansion { get => Flags[5]; set => Flags[5] = value; }
    public bool IsLadder { get => Flags[6]; set => Flags[6] = value; }
    public bool IsNeedsRenaming { get => Flags[7]; set => Flags[7] = value; }

    public void Write(IBitWriter writer)
    {
        var bits = (InternalBitArray)Flags;
        writer.WriteBits(bits);
    }

    public static Status Read(byte bytes)
    {
        var status = new Status(bytes);
        return status;
    }

    [Obsolete("Try the non-allocating overload!")]
    public static byte[] Write(Status status)
    {
        using var writer = new BitWriter();
        status.Write(writer);
        return writer.ToArray();
    }

    public void Dispose() => Interlocked.Exchange(ref _flags!, null)?.Dispose();
}
