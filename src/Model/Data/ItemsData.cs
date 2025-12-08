using D2SLib.Model.Huffman;

namespace D2SLib.Model.Data;

//collections or ArmorData MiscData WeaponsData with helper methods
public sealed class ItemsData
{
    public ItemsData(ArmorData armorData, WeaponsData weaponsData, MiscData miscData)
    {
        ArmorData = armorData;
        WeaponsData = weaponsData;
        MiscData = miscData;
    }

    public ArmorData ArmorData { get; }
    public WeaponsData WeaponsData { get; }
    public MiscData MiscData { get; }

    private HuffmanTree? _itemCodeTree = null;
    internal HuffmanTree ItemCodeTree
    {
        get => _itemCodeTree ??= InitializeHuffmanTree();
        set => _itemCodeTree = value;
    }

    public DataRow? this[string code] => GetByCode(code);

    public DataRow? GetByCode(string code)
    {
        return ArmorData[code]
            ?? WeaponsData[code] 
            ?? MiscData[code];
    }

    public bool IsArmor(string code) => ArmorData[code] is not null;

    public bool IsWeapon(string code) => WeaponsData[code] is not null;

    public bool IsMisc(string code) => MiscData[code] is not null;

    public bool IsQuest(string code) => GetByCode(code)?["type"].Value == "ques";
    public bool IsBook(string code) => GetByCode(code)?["type"].Value == "book";
    public bool IsScroll(string code) => GetByCode(code)?["type"].Value == "scro";

    private HuffmanTree InitializeHuffmanTree()
    {
        /*
        List<string> items = new List<string>();
        foreach(var row in ArmorData.Rows)
        {
            items.Add(row["code"]);
        }
        foreach (var row in WeaponsData.Rows)
        {
            items.Add(row["code"]);
        }
        foreach (var row in MiscData.Rows)
        {
            items.Add(row["code"]);
        }
        */
        var itemCodeTree = new HuffmanTree();
        itemCodeTree.Build();
        return itemCodeTree;
    }
}

public sealed class ArmorData : DataFile
{
    private Dictionary<string, int> CodeToRowIndex = [];
    private Dictionary<string, int>.AlternateLookup<ReadOnlySpan<char>> lookup;

    public DataRow? this[string code] => GetRowByCode(code);

    private DataRow? GetRowByCode(string code)
    {
        if (lookup.TryGetValue(code.AsSpan().Trim(), out var rowIndex))
        {
            return GetRowByIndex(rowIndex);
        }

        return null;
    }

    public static ArmorData Read(Stream data)
    {
        var armor = new ArmorData();
        armor.ReadData(data);

        var rows = armor.GetRows();

        foreach (var row in rows)
        {
            armor.CodeToRowIndex[row["code"].Value] = row["code"].RowIndex;
        }

        armor.lookup = armor.CodeToRowIndex.GetAlternateLookup<ReadOnlySpan<char>>();

        return armor;
    }

    public static ArmorData Read(string file)
    {
        using Stream stream = File.OpenRead(file);
        return Read(stream);
    }
}

public sealed class WeaponsData : DataFile
{
    private Dictionary<string, int> CodeToRowIndex = [];
    private Dictionary<string, int>.AlternateLookup<ReadOnlySpan<char>> lookup;

    public DataRow? this[string code] => GetRowByCode(code);

    private DataRow? GetRowByCode(string code)
    {
        if (lookup.TryGetValue(code.AsSpan().Trim(), out var rowIndex))
        {
            return GetRowByIndex(rowIndex);
        }

        return null;
    }

    public static WeaponsData Read(Stream data)
    {
        var weapons = new WeaponsData();
        weapons.ReadData(data);

        var rows = weapons.GetRows();

        foreach (var row in rows)
        {
            weapons.CodeToRowIndex[row["code"].Value] = row["code"].RowIndex;
        }

        weapons.lookup = weapons.CodeToRowIndex.GetAlternateLookup<ReadOnlySpan<char>>();

        return weapons;
    }

    public static WeaponsData Read(string file)
    {
        using Stream stream = File.OpenRead(file);
        return Read(stream);
    }
}

public sealed class MiscData : DataFile
{
    private Dictionary<string, int> CodeToRowIndex = [];
    private Dictionary<string, int>.AlternateLookup<ReadOnlySpan<char>> lookup;

    public DataRow? this[string code] => GetRowByCode(code);

    private DataRow? GetRowByCode(string code)
    {
        if (lookup.TryGetValue(code.AsSpan().Trim(), out var rowIndex))
        {
            return GetRowByIndex(rowIndex);
        }

        return null;
    }

    public static MiscData Read(Stream data)
    {
        var misc = new MiscData();
        misc.ReadData(data);

        var rows = misc.GetRows();

        foreach (var row in rows)
        {
            misc.CodeToRowIndex[row["code"].Value] = row["code"].RowIndex;
        }

        misc.lookup = misc.CodeToRowIndex.GetAlternateLookup<ReadOnlySpan<char>>();

        return misc;
    }

    public static MiscData Read(string file)
    {
        using Stream stream = File.OpenRead(file);
        return Read(stream);
    }
}
