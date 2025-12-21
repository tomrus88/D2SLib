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
    private static readonly int[] _numBits = [9, 9, 9, 3, 9];

    private WaypointsDifficulty(IBitReader reader)
    {
        Header = reader.ReadUInt16();

        for (int i = 0; i < _acts.Length; i++)
        {
            _acts[i] = Waypoints.Read(reader, _numBits[i]);
        }

        reader.ReadInt32(9);
        reader.Align();
        reader.AdvanceBits(16 * 8);
    }

    //[0x02, 0x01]
    public ushort? Header { get; set; }
    public Waypoints[] Acts => _acts;

    public void Write(IBitWriter writer)
    {
        writer.WriteUInt16(Header ?? 0x102);

        for (int i = 0; i < _acts.Length; i++)
        {
            _acts[i].Write(writer, _numBits[i]);
        }

        writer.WriteInt32(0, 9);
        writer.Align();
        Span<byte> padding = stackalloc byte[16];
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
public enum WaypointFlags : ushort
{
    None = 0x0,
    Town = 0x1,
    Waypoint1 = 0x2,
    Waypoint2 = 0x4,
    Waypoint3 = 0x8,
    Waypoint4 = 0x10,
    Waypoint5 = 0x20,
    Waypoint6 = 0x40,
    Waypoint7 = 0x80,
    Waypoint8 = 0x100,
    All = 0x1FF
}

[Flags]
public enum ActIWaypoints : ushort
{
    None = 0x0,
    RogueEncampment = 0x1,
    ColdPlains = 0x2,
    StonyField = 0x4,
    DarkWoods = 0x8,
    BlackMarsh = 0x10,
    OuterCloister = 0x20,
    JailLvl1 = 0x40,
    InnerCloister = 0x80,
    CatacombsLvl2 = 0x100,
    All = 0x1FF
}

[Flags]
public enum ActIIWaypoints : ushort
{
    None = 0x0,
    LutGholein = 0x1,
    SewersLvl2 = 0x2,
    DryHills = 0x4,
    HallsOfTheDeadLvl2 = 0x8,
    FarOasis = 0x10,
    LostCity = 0x20,
    PalaceCellarLvl1 = 0x40,
    ArcaneSanctuary = 0x80,
    CanyonOfTheMagi = 0x100,
    All = 0x1FF
}

[Flags]
public enum ActIIIWaypoints : ushort
{
    None = 0x0,
    KurastDocks = 0x1,
    SpiderForest = 0x2,
    GreatMarsh = 0x4,
    FlayerJungle = 0x8,
    LowerKurast = 0x10,
    KurastBazaar = 0x20,
    UpperKurast = 0x40,
    Travincal = 0x80,
    DuranceOfHateLvl2 = 0x100,
    All = 0x1FF
}

[Flags]
public enum ActIVWaypoints : ushort
{
    None = 0x0,
    ThePandemoniumFortress = 0x1,
    CityOfTheDamned = 0x2,
    RiverOfFlame = 0x4,
    All = 0x7
}

[Flags]
public enum ActVWaypoints : ushort
{
    None = 0x0,
    Harrogath = 0x1,
    FrigidHighlands = 0x2,
    ArreatPlateau = 0x4,
    CrystallinePassage = 0x8,
    HallsOfPain = 0x10,
    GlacialTrail = 0x20,
    FrozenTundra = 0x40,
    TheAncientsWay = 0x80,
    WorldstoneKeepLvl2 = 0x100,
    All = 0x1FF
}

public struct Waypoints
{
    private WaypointFlags _flags;

    private Waypoints(WaypointFlags flags) => _flags = flags;
    private Waypoints(ushort flags) => _flags = (WaypointFlags)flags;

    public ushort Value => (ushort)_flags;

    // Implicit conversions FROM act enums
    public static implicit operator Waypoints(ActIWaypoints wp) => new((WaypointFlags)wp);
    public static implicit operator Waypoints(ActIIWaypoints wp) => new((WaypointFlags)wp);
    public static implicit operator Waypoints(ActIIIWaypoints wp) => new((WaypointFlags)wp);
    public static implicit operator Waypoints(ActIVWaypoints wp) => new((WaypointFlags)wp);
    public static implicit operator Waypoints(ActVWaypoints wp) => new((WaypointFlags)wp);
    public static implicit operator Waypoints(WaypointFlags wp) => new(wp);

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

    public void Write(IBitWriter writer, int numBits)
    {
        writer.WriteUInt16((ushort)_flags, numBits);
    }

    public static Waypoints Read(IBitReader reader, int numBits)
    {
        ushort bits = reader.ReadUInt16(numBits);
        return new(bits);
    }
}
