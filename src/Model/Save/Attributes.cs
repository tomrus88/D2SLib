using D2SLib.IO;

namespace D2SLib.Model.Save;

public record Stat(int Layer, long Value);

//variable size. depends on # of attributes
public class Attributes
{
    public ushort? Header { get; set; }
    public Dictionary<ushort, Stat> Stats { get; } = new Dictionary<ushort, Stat>();

    public static Attributes Read(IBitReader reader)
    {
        var itemStatCost = Core.MetaData.ItemStatCostData;
        var attributes = new Attributes
        {
            Header = reader.ReadUInt16()
        };
        ushort id = reader.ReadUInt16(9);
        while (id != 0x1ff)
        {
            var property = itemStatCost.GetById(id);
            if (property == null)
            {
                throw new Exception($"No ItemStatCost record found for id: {id} at bit {reader.Position - 9}");
            }
            int layer = 0;

            int csvParamBits = property["CSvParam"].ToInt32();

            if (csvParamBits > 0)
            {
                layer = reader.ReadInt32Signed(csvParamBits);
            }

            int csvBits = property["CSvBits"].ToInt32();
            int csvSigned = property["CSvSigned"].ToInt32();

            long value;

            if (csvBits < 32 && csvSigned > 0)
                value = reader.ReadInt32Signed(csvBits);
            else
                value = reader.ReadInt32(csvBits);

            //int valShift = property["ValShift"].ToInt32();
            //value >>= valShift;

            attributes.Stats.Add(id, new Stat(layer, value));

            id = reader.ReadUInt16(9);
        }
        reader.Align();
        return attributes;
    }

    public void Write(IBitWriter writer)
    {
        var itemStatCost = Core.MetaData.ItemStatCostData;
        writer.WriteUInt16(Header ?? 0x6667);
        foreach (var entry in Stats)
        {
            var property = itemStatCost.GetById(entry.Key);
            if (property == null)
            {
                throw new Exception($"No ItemStatCost record found for id: {entry.Key}");
            }
            writer.WriteUInt16(property["ID"].ToUInt16(), 9);

            int csvParamBits = property["CSvParam"].ToInt32();

            if (csvParamBits > 0)
            {
                writer.WriteInt32(entry.Value.Layer, csvParamBits);
            }

            int csvBits = property["CSvBits"].ToInt32();

            long value = entry.Value.Value;

            //int valShift = property["ValShift"].ToInt32();
            //value <<= valShift;

            writer.WriteUInt32((uint)value, csvBits);
        }
        writer.WriteUInt16(0x1ff, 9);
        writer.Align();
    }

    [Obsolete("Try the non-allocating overload!")]
    public static byte[] Write(Attributes attributes)
    {
        using var writer = new BitWriter();
        attributes.Write(writer);
        return writer.ToArray();
    }
}
