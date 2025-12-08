using D2SLib.IO;

namespace D2SLib.Model.Save;

public sealed class WaypointsSection
{
    private readonly WaypointsDifficulty[] _difficulties = new WaypointsDifficulty[3];

    //0x0279 [waypoint data = 0x57, 0x53 "WS"]
    public ushort? Header { get; set; }
    //0x027b [waypoint header version = 0x1, 0x0, 0x0, 0x0]
    public uint? Version { get; set; }
    //0x027f [waypoint header length = 0x50, 0x0]
    public ushort? Length { get; set; }
    public WaypointsDifficulty Normal => _difficulties[0];
    public WaypointsDifficulty Nightmare => _difficulties[1];
    public WaypointsDifficulty Hell => _difficulties[2];
    public WaypointsDifficulty[] Difficulties => _difficulties;

    public void Write(IBitWriter writer)
    {
        writer.WriteUInt16(Header ?? 0x5357);
        writer.WriteUInt32(Version ?? 0x1);
        writer.WriteUInt16(Length ?? 0x50);

        for (int i = 0; i < _difficulties.Length; i++)
        {
            _difficulties[i].Write(writer);
        }
    }

    public static WaypointsSection Read(IBitReader reader)
    {
        var waypointsSection = new WaypointsSection
        {
            Header = reader.ReadUInt16(),
            Version = reader.ReadUInt32(),
            Length = reader.ReadUInt16()
        };

        for (int i = 0; i < waypointsSection._difficulties.Length; i++)
        {
            waypointsSection._difficulties[i] = WaypointsDifficulty.Read(reader);
        }

        return waypointsSection;
    }

    [Obsolete("Try the direct-read overload!")]
    public static WaypointsSection Read(ReadOnlySpan<byte> bytes)
    {
        using var reader = new BitReader(bytes);
        return Read(reader);
    }

    [Obsolete("Try the non-allocating overload!")]
    public static byte[] Write(WaypointsSection waypointsSection)
    {
        using var writer = new BitWriter();
        waypointsSection.Write(writer);
        return writer.ToArray();
    }
}

public sealed class WaypointsDifficulty
{
    private readonly Waypoints[] _acts = new Waypoints[5];

    private WaypointsDifficulty(IBitReader reader)
    {
        Header = reader.ReadUInt16();

        for (int i = 0; i < _acts.Length; i++)
        {
            _acts[i] = Waypoints.Read(reader);
        }

        reader.Align();
        reader.AdvanceBits(17 * 8);
    }

    //[0x02, 0x01]
    public ushort? Header { get; set; }
    public Waypoints[] Acts => _acts;

    public void Write(IBitWriter writer)
    {
        writer.WriteUInt16(Header ?? 0x102);

        int startPos = writer.Position;
        for (int i = 0; i < _acts.Length; i++)
        {
            _acts[i].Write(writer);
        }
        int endPos = writer.Position;

        writer.Align();
        Span<byte> padding = stackalloc byte[17];
        padding.Clear();
        writer.WriteBytes(padding);
    }

    public static WaypointsDifficulty Read(IBitReader reader)
    {
        var waypointsDifficulty = new WaypointsDifficulty(reader);
        return waypointsDifficulty;
    }

    [Obsolete("Try the direct-read overload!")]
    public static WaypointsDifficulty Read(ReadOnlySpan<byte> bytes)
    {
        using var reader = new BitReader(bytes);
        return Read(reader);
    }

    [Obsolete("Try the non-allocating overload!")]
    public static byte[] Write(WaypointsDifficulty waypointsDifficulty)
    {
        using var writer = new BitWriter();
        waypointsDifficulty.Write(writer);
        return writer.ToArray();
    }
}

[Flags]
public enum WaypointFlags : byte
{
    Town = 0x0,
    Waypoint1 = 0x1,
    Waypoint2 = 0x2,
    Waypoint3 = 0x4,
    Waypoint4 = 0x8,
    Waypoint5 = 0x10,
    Waypoint6 = 0x20,
    Waypoint7 = 0x40,
    Waypoint8 = 0x80,
    All = 0xFF
}

[Flags]
public enum ActIWaypoints : byte
{
    RogueEncampment = 0x0, // Always available (town)
    ColdPlains = 0x1,
    StonyField = 0x2,
    DarkWoods = 0x4,
    BlackMarsh = 0x8,
    OuterCloister = 0x10,
    JailLvl1 = 0x20,
    InnerCloister = 0x40,
    CatacombsLvl2 = 0x80,
    All = 0xFF
}

[Flags]
public enum ActIIWaypoints : byte
{
    LutGholein, // Always available (town)
    SewersLvl2,
    DryHills,
    HallsOfTheDeadLvl2,
    FarOasis,
    LostCity,
    PalaceCellarLvl1,
    ArcaneSanctuary,
    CanyonOfTheMagi,
    All = 0xFF
}

[Flags]
public enum ActIIIWaypoints : byte
{
    KurastDocks = 0x0, // Always available (town)
    SpiderForest = 0x1,
    GreatMarsh = 0x2,
    FlayerJungle = 0x4,
    LowerKurast = 0x8,
    KurastBazaar = 0x10,
    UpperKurast = 0x20,
    Travincal = 0x40,
    DuranceOfHateLvl2 = 0x80,
    All = 0xFF
}

[Flags]
public enum ActIVWaypoints : byte
{
    ThePandemoniumFortress = 0x0, // Always available (town)
    CityOfTheDamned = 0x1,
    RiverOfFlame = 0x2,
    All = 0x3
}

[Flags]
public enum ActVWaypoints : byte
{
    Harrogath = 0x0, // Always available (town)
    FrigidHighlands = 0x1,
    ArreatPlateau = 0x2,
    CrystallinePassage = 0x4,
    HallsOfPain = 0x8,
    GlacialTrail = 0x10,
    FrozenTundra = 0x20,
    TheAncientsWay = 0x40,
    WorldstoneKeepLvl2 = 0x80,
    All = 0xFF
}

public struct Waypoints
{
    private WaypointFlags _flags;

    private Waypoints(WaypointFlags flags) => _flags = flags;

    public byte Value => (byte)_flags;

    // Implicit conversions FROM act enums
    public static implicit operator Waypoints(ActIWaypoints wp) => new((WaypointFlags)wp);
    public static implicit operator Waypoints(ActIIWaypoints wp) => new((WaypointFlags)wp);
    public static implicit operator Waypoints(ActIIIWaypoints wp) => new((WaypointFlags)wp);
    public static implicit operator Waypoints(ActIVWaypoints wp) => new((WaypointFlags)wp);
    public static implicit operator Waypoints(ActVWaypoints wp) => new((WaypointFlags)wp);
    public static implicit operator Waypoints(WaypointFlags wp) => new(wp);
    public static implicit operator Waypoints(byte wp) => new((WaypointFlags)wp);

    // Implicit conversions TO
    public static implicit operator WaypointFlags(Waypoints wp) => wp._flags;

    // Bitwise operators with Waypoints
    public static Waypoints operator &(Waypoints left, Waypoints right) =>
        new(left._flags & right._flags);

    public static Waypoints operator |(Waypoints left, Waypoints right) =>
        new(left._flags | right._flags);

    public static Waypoints operator ~(Waypoints wp) =>
        new(~wp._flags);

    // Bitwise operators with each act enum
    public static Waypoints operator &(Waypoints left, ActIWaypoints right) =>
        new(left._flags & (WaypointFlags)right);

    public static Waypoints operator |(Waypoints left, ActIWaypoints right) =>
        new(left._flags | (WaypointFlags)right);

    public static Waypoints operator &(Waypoints left, ActIIWaypoints right) =>
        new(left._flags & (WaypointFlags)right);

    public static Waypoints operator |(Waypoints left, ActIIWaypoints right) =>
        new(left._flags | (WaypointFlags)right);

    public static Waypoints operator &(Waypoints left, ActIIIWaypoints right) =>
        new(left._flags & (WaypointFlags)right);

    public static Waypoints operator |(Waypoints left, ActIIIWaypoints right) =>
        new(left._flags | (WaypointFlags)right);

    public static Waypoints operator &(Waypoints left, ActIVWaypoints right) =>
        new(left._flags & (WaypointFlags)right);

    public static Waypoints operator |(Waypoints left, ActIVWaypoints right) =>
        new(left._flags | (WaypointFlags)right);

    public static Waypoints operator &(Waypoints left, ActVWaypoints right) =>
        new(left._flags & (WaypointFlags)right);

    public static Waypoints operator |(Waypoints left, ActVWaypoints right) =>
        new(left._flags | (WaypointFlags)right);

    public void Write(IBitWriter writer)
    {
        writer.WriteByte((byte)_flags);
    }

    public static Waypoints Read(IBitReader reader)
    {
        byte bits = reader.ReadByte();
        return (Waypoints)bits;
    }
}
