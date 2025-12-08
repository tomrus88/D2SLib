using D2SLib.IO;
using System.Diagnostics;

namespace D2SLib.Model.Save;

public sealed class QuestsSection : IDisposable
{
    private readonly QuestsDifficulty[] _difficulties = new QuestsDifficulty[3];

    //0x014f [quests header identifier = 0x57, 0x6f, 0x6f, 0x21 "Woo!"]
    public uint? Header { get; set; }
    //0x0153 [version = 0x6, 0x0, 0x0, 0x0]
    public uint? Version { get; set; }
    //0x0153 [quests header length = 0x2a, 0x1]
    public ushort? Length { get; set; }

    public QuestsDifficulty Normal => _difficulties[0];
    public QuestsDifficulty Nightmare => _difficulties[1];
    public QuestsDifficulty Hell => _difficulties[2];

    public void Write(IBitWriter writer)
    {
        writer.WriteUInt32(Header ?? 0x216F6F57);
        writer.WriteUInt32(Version ?? 0x6);
        writer.WriteUInt16(Length ?? 0x12A);

        for (int i = 0; i < _difficulties.Length; i++)
        {
            _difficulties[i].Write(writer);
        }
    }

    public static QuestsSection Read(IBitReader reader)
    {
        var questSection = new QuestsSection
        {
            Header = reader.ReadUInt32(),
            Version = reader.ReadUInt32(),
            Length = reader.ReadUInt16()
        };

        for (int i = 0; i < questSection._difficulties.Length; i++)
        {
            questSection._difficulties[i] = QuestsDifficulty.Read(reader);
        }

        return questSection;
    }

    [Obsolete("Try the direct-read overload!")]
    public static QuestsSection Read(byte[] bytes)
    {
        using var reader = new BitReader(bytes);
        return Read(reader);
    }

    [Obsolete("Try the non-allocating overload!")]
    public static byte[] Write(QuestsSection questSection)
    {
        using var writer = new BitWriter();
        questSection.Write(writer);
        return writer.ToArray();
    }

    public void Dispose()
    {
        for (int i = 0; i < _difficulties.Length; i++)
        {
            Interlocked.Exchange(ref _difficulties[i]!, null)?.Dispose();
        }
    }
}

public sealed class QuestsDifficulty : IDisposable
{
    private QuestsDifficulty(IBitReader reader)
    {
        ActI = ActIQuests.Read(reader);
        ActII = ActIIQuests.Read(reader);
        ActIII = ActIIIQuests.Read(reader);
        ActIV = ActIVQuests.Read(reader);
        ActV = ActVQuests.Read(reader);
    }

    public ActIQuests ActI { get; set; }
    public ActIIQuests ActII { get; set; }
    public ActIIIQuests ActIII { get; set; }
    public ActIVQuests ActIV { get; set; }
    public ActVQuests ActV { get; set; }

    public void Write(IBitWriter writer)
    {
        ActI.Write(writer);
        ActII.Write(writer);
        ActIII.Write(writer);
        ActIV.Write(writer);
        ActV.Write(writer);
    }

    public static QuestsDifficulty Read(IBitReader reader)
    {
        var qd = new QuestsDifficulty(reader);
        return qd;
    }

    [Obsolete("Try the direct-read overload!")]
    public static QuestsDifficulty Read(ReadOnlySpan<byte> bytes)
    {
        using var reader = new BitReader(bytes);
        return Read(reader);
    }

    [Obsolete("Try the non-allocating overload!")]
    public static byte[] Write(QuestsDifficulty questsDifficulty)
    {
        using var writer = new BitWriter();
        questsDifficulty.Write(writer);
        Debug.Assert(writer.Position == 96 * 8);
        return writer.ToArray();
    }

    public void Dispose()
    {
        ActI.Dispose();
        ActII.Dispose();
        ActIII.Dispose();
        ActIV.Dispose();
        ActV.Dispose();
    }
}

[Flags]
public enum QuestFlags : ushort
{
    None = 0,
    RewardGranted = 0x1,
    RewardPending = 0x2,
    Started = 0x4,
    LeftTown = 0x8,
    EnterArea = 0x10,
    Custom1 = 0x20,
    Custom2 = 0x40,
    Custom3 = 0x80,
    Custom4 = 0x100,
    Custom5 = 0x200,
    Custom6 = 0x400,
    Custom7 = 0x800,
    QuestLog = 0x1000,
    PrimaryGoalAchieved = 0x2000,
    CompletedNow = 0x4000,
    CompletedBefore = 0x8000,
}

public sealed class Quest : IDisposable
{
    private ushort _flags;

    public QuestFlags Flags { get => (QuestFlags)_flags; set => _flags = (ushort)value; }

    private Quest(ushort flags) => _flags = flags;

    public void Write(IBitWriter writer)
    {
        writer.WriteUInt16(_flags);
    }

    public static Quest Read(IBitReader reader)
    {
        ushort bits = reader.ReadUInt16(16);
        return new Quest(bits);
    }

    [Obsolete("Try the direct-read overload!")]
    public static Quest Read(ReadOnlySpan<byte> bytes)
    {
        ushort bits = BitConverter.ToUInt16(bytes);
        return new Quest(bits);
    }

    [Obsolete("Try the non-allocating overload!")]
    public static byte[] Write(Quest quest)
    {
        using var writer = new BitWriter();
        quest.Write(writer);
        return writer.ToArray();
    }

    public void Dispose() { }
}

public sealed class ActIQuests : IDisposable
{
    private readonly Quest[] _quests = new Quest[7];

    public Quest Introduction => _quests[0];
    public Quest DenOfEvil => _quests[1];
    public Quest SistersBurialGrounds => _quests[2];
    public Quest ToolsOfTheTrade => _quests[3];
    public Quest TheSearchForCain => _quests[4];
    public Quest TheForgottenTower => _quests[5];
    public Quest SistersToTheSlaughter => _quests[6];

    public void Write(IBitWriter writer)
    {
        for (int i = 0; i < _quests.Length; i++)
        {
            _quests[i].Write(writer);
        }
    }

    public static ActIQuests Read(IBitReader reader)
    {
        var quests = new ActIQuests();
        for (int i = 0; i < quests._quests.Length; i++)
        {
            quests._quests[i] = Quest.Read(reader);
        }
        return quests;
    }

    public void Dispose()
    {
        for (int i = 0; i < _quests.Length; i++)
        {
            Interlocked.Exchange(ref _quests[i]!, null)?.Dispose();
        }
    }
}

public sealed class ActIIQuests : IDisposable
{
    private readonly Quest[] _quests = new Quest[8];

    public Quest Arrival => _quests[0];
    public Quest Introduction => _quests[1];
    public Quest RadamentsLair => _quests[2];
    public Quest TheHoradricStaff => _quests[3];
    public Quest TaintedSun => _quests[4];
    public Quest ArcaneSanctuary => _quests[5];
    public Quest TheSummoner => _quests[6];
    public Quest TheSevenTombs => _quests[7];

    public void Write(IBitWriter writer)
    {
        for (int i = 0; i < _quests.Length; i++)
        {
            _quests[i].Write(writer);
        }
    }

    public static ActIIQuests Read(IBitReader reader)
    {
        var quests = new ActIIQuests();
        for (int i = 0; i < quests._quests.Length; i++)
        {
            quests._quests[i] = Quest.Read(reader);
        }
        return quests;
    }

    public void Dispose()
    {
        for (int i = 0; i < _quests.Length; i++)
        {
            Interlocked.Exchange(ref _quests[i]!, null)?.Dispose();
        }
    }
}

public sealed class ActIIIQuests : IDisposable
{
    private readonly Quest[] _quests = new Quest[8];

    public Quest Arrival => _quests[0];
    public Quest Introduction => _quests[1];
    public Quest LamEsensTome => _quests[2];
    public Quest KhalimsWill => _quests[3];
    public Quest BladeOfTheOldReligion => _quests[4];
    public Quest TheGoldenBird => _quests[5];
    public Quest TheBlackenedTemple => _quests[6];
    public Quest TheGuardian => _quests[7];

    public void Write(IBitWriter writer)
    {
        for (int i = 0; i < _quests.Length; i++)
        {
            _quests[i].Write(writer);
        }
    }

    public static ActIIIQuests Read(IBitReader reader)
    {
        var quests = new ActIIIQuests();
        for (int i = 0; i < quests._quests.Length; i++)
        {
            quests._quests[i] = Quest.Read(reader);
        }
        return quests;
    }

    public void Dispose()
    {
        for (int i = 0; i < _quests.Length; i++)
        {
            Interlocked.Exchange(ref _quests[i]!, null)?.Dispose();
        }
    }
}

public sealed class ActIVQuests : IDisposable
{
    private readonly Quest[] _quests = new Quest[8];

    public Quest Arrival => _quests[0];
    public Quest Introduction => _quests[1];
    public Quest TheFallenAngel => _quests[2];
    public Quest TerrorsEnd => _quests[3];
    public Quest Hellforge => _quests[4];

    //3 shorts at the end of ActIV completion. presumably for extra quests never used.
    public Quest Extra1 => _quests[5];
    public Quest Extra2 => _quests[6];
    public Quest Extra3 => _quests[7];

    public void Write(IBitWriter writer)
    {
        for (int i = 0; i < _quests.Length; i++)
        {
            _quests[i].Write(writer);
        }
    }

    public static ActIVQuests Read(IBitReader reader)
    {
        var quests = new ActIVQuests();
        for (int i = 0; i < quests._quests.Length; i++)
        {
            quests._quests[i] = Quest.Read(reader);
        }
        return quests;
    }

    public void Dispose()
    {
        for (int i = 0; i < _quests.Length; i++)
        {
            Interlocked.Exchange(ref _quests[i]!, null)?.Dispose();
        }
    }
}

public sealed class ActVQuests : IDisposable
{
    private readonly Quest[] _quests = new Quest[17];

    public Quest Arrival => _quests[0];
    public Quest Introduction => _quests[1];
    //2 shorts after ActV introduction. presumably for extra quests never used.
    public Quest Extra1 => _quests[2];
    public Quest Extra2 => _quests[3];
    public Quest SiegeOnHarrogath => _quests[4];
    public Quest RescueOnMountArreat => _quests[5];
    public Quest PrisonOfIce => _quests[6];
    public Quest BetrayalOfHarrogath => _quests[7];
    public Quest RiteOfPassage => _quests[8];
    public Quest EveOfDestruction => _quests[9];
    public Quest Completion => _quests[10];
    //6 shorts after ActV completion. presumably for extra quests never used.
    public Quest Extra3 => _quests[11];
    public Quest Extra4 => _quests[12];
    public Quest Extra5 => _quests[13];
    public Quest Extra6 => _quests[14];
    public Quest Extra7 => _quests[15];
    public Quest Extra8 => _quests[16];

    public void Write(IBitWriter writer)
    {
        for (int i = 0; i < _quests.Length; i++)
        {
            _quests[i].Write(writer);
        }
    }

    public static ActVQuests Read(IBitReader reader)
    {
        var quests = new ActVQuests();
        for (int i = 0; i < quests._quests.Length; i++)
        {
            quests._quests[i] = Quest.Read(reader);
        }
        return quests;
    }

    public void Dispose()
    {
        for (int i = 0; i < _quests.Length; i++)
        {
            Interlocked.Exchange(ref _quests[i]!, null)?.Dispose();
        }
    }
}
