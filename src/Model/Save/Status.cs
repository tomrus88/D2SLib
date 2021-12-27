﻿using D2SLib.IO;
using System.Text.Json.Serialization;

namespace D2SLib.Model.Save;

public class Status
{
    public Status(byte flags)
    {
        Flags = new InternalBitArray(stackalloc byte[] { flags });
    }

    [JsonIgnore]
    public IList<bool> Flags { get; } 
    public bool IsHardcore { get => Flags[2]; set => Flags[2] = value; }
    public bool IsDead { get => Flags[3]; set => Flags[3] = value; }
    public bool IsExpansion { get => Flags[5]; set => Flags[5] = value; }
    public bool IsLadder { get => Flags[6]; set => Flags[6] = value; }

    public void Write(BitWriter writer)
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
}
