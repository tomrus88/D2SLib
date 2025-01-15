using D2SLib.IO;
using System.Text;

namespace D2SLib.Model.Save;

// 4+1+1+2+4
public class PreviewItem
{
    public uint Code { get; set; }
    public byte Transform { get; set; }
    public ItemQuality Quality { get; set; }
    public ushort FileIndex { get; set; }
    public ItemFlags Flags { get; set; }
    public string CodeString => Encoding.ASCII.GetString(BitConverter.GetBytes(Code)).TrimEnd(['\0', ' ']);

    public static PreviewItem Read(IBitReader reader)
    {
        var previewItem = new PreviewItem
        {
            Code = reader.ReadUInt32(),
            Transform = reader.ReadByte(),
            Quality = (ItemQuality)reader.ReadByte(),
            FileIndex = reader.ReadUInt16(),
            Flags = (ItemFlags)reader.ReadUInt32(),
        };

        return previewItem;
    }

    public void Write(IBitWriter writer)
    {
        writer.WriteUInt32(Code);
        writer.WriteByte(Transform);
        writer.WriteByte((byte)Quality);
        writer.WriteUInt16(FileIndex);
        writer.WriteUInt32((uint)Flags);
    }
}

// 8+8+4+4+4+4*12+64
public class PreviewData
{
    public ulong ExpansionSaveTime { get; set; }
    public ulong ClassicSaveTime { get; set; }
    public uint GuildEmblemColor { get; set; } // One byte but padded to 4
    public uint ExpansionExperience { get; set; }
    public uint ClassicExperience { get; set; }
    public PreviewItem LeftHand { get; set; }
    public PreviewItem RightHand { get; set; }
    public PreviewItem Torso { get; set; }
    public PreviewItem Head { get; set; }
    public string Name { get; set; }
    public uint Unk1 { get; set; }

    public static PreviewData Read(IBitReader reader)
    {
        var previewData = new PreviewData
        {
            ExpansionSaveTime = reader.ReadUInt64(),
            ClassicSaveTime = reader.ReadUInt64(),
            GuildEmblemColor = reader.ReadUInt32(),
            ExpansionExperience = reader.ReadUInt32(),
            ClassicExperience = reader.ReadUInt32(),
            LeftHand = PreviewItem.Read(reader),
            RightHand = PreviewItem.Read(reader),
            Torso = PreviewItem.Read(reader),
            Head = PreviewItem.Read(reader),
            Name = reader.ReadString(64),
            Unk1 = reader.ReadUInt32(),
        };

        return previewData;
    }

    public void Write(IBitWriter writer)
    {
        writer.WriteUInt64(ExpansionSaveTime);
        writer.WriteUInt64(ClassicSaveTime);
        writer.WriteUInt32(GuildEmblemColor);
        writer.WriteUInt32(ExpansionExperience);
        writer.WriteUInt32(ClassicExperience);
        LeftHand.Write(writer);
        RightHand.Write(writer);
        Torso.Write(writer);
        Head.Write(writer);
        writer.WriteString(Name, 64);
        writer.WriteUInt32(Unk1);
    }
}
