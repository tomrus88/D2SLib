using D2SLib.IO;
using System.Runtime.CompilerServices;

namespace D2SLib.Model.Huffman;

//hardcoded....
internal class HuffmanTree
{
    // Compact node structure for cache-friendly traversal
    // Each node is 2 ints: [0] = left/symbol, [1] = right
    // If value >= 0, it's an index to child node
    // If value < 0, it's ~symbol (leaf node)
    private int[] _nodes = null!;
    private int _nodeCount;

    // Pre-computed encoding table: stores bits and length for each character
    private readonly (uint bits, int length)[] _encodingTable = new (uint, int)[128];

    // (symbol, bits, length) - bits stored MSB first
    private static readonly (char symbol, uint bits, int length)[] TABLE =
    [
        ('0', 0b11111011, 8),
        (' ', 0b10, 2),
        ('1', 0b1111100, 7),
        ('2', 0b001100, 6),
        ('3', 0b1101101, 7),
        ('4', 0b11111010, 8),
        ('5', 0b00010110, 8),
        ('6', 0b1101111, 7),
        ('7', 0b01111, 5),
        ('8', 0b000100, 6),
        ('9', 0b01110, 5),
        ('a', 0b11110, 5),
        ('b', 0b0101, 4),
        ('c', 0b01000, 5),
        ('d', 0b110001, 6),
        ('e', 0b110000, 6),
        ('f', 0b010011, 6),
        ('g', 0b11010, 5),
        ('h', 0b00011, 5),
        ('i', 0b1111110, 7),
        ('j', 0b000101110, 9),
        ('k', 0b010010, 6),
        ('l', 0b11101, 5),
        ('m', 0b01101, 5),
        ('n', 0b001101, 6),
        ('o', 0b1111111, 7),
        ('p', 0b11001, 5),
        ('q', 0b11011001, 8),
        ('r', 0b11100, 5),
        ('s', 0b0010, 4),
        ('t', 0b01100, 5),
        ('u', 0b00001, 5),
        ('v', 0b1101110, 7),
        ('w', 0b00000, 5),
        ('x', 0b00111, 5),
        ('y', 0b0001010, 7),
        ('z', 0b11011000, 8)
    ];

    //todo find a way to build this like d2?
    public void Build()
    {
        // Allocate enough space: each entry can create up to 9 nodes (max code length)
        _nodes = new int[TABLE.Length * 9 * 2];
        _nodeCount = 1; // Root is node 0

        // Initialize root
        _nodes[0] = -1; // Left: no child yet
        _nodes[1] = -1; // Right: no child yet

        foreach (var (symbol, bits, length) in TABLE)
        {
            int currentNode = 0;

            for (int i = length - 1; i >= 0; i--)
            {
                // Extract bit from MSB to LSB
                int bit = (int)((bits >> i) & 1);
                int childSlot = currentNode * 2 + bit;

                if (i == 0)
                {
                    // Leaf node
                    _nodes[childSlot] = ~symbol;
                }
                else
                {
                    // Internal node
                    if (_nodes[childSlot] <= 0 && childSlot != 0 || _nodes[childSlot] < 0)
                    {
                        if (_nodes[childSlot] < 0 || _nodes[childSlot] == 0)
                        {
                            _nodes[childSlot] = _nodeCount;
                            _nodes[_nodeCount * 2] = -1;
                            _nodes[_nodeCount * 2 + 1] = -1;
                            _nodeCount++;
                        }
                    }
                    currentNode = _nodes[childSlot];
                }
            }

            // Store pre-computed encoding
            _encodingTable[symbol] = (bits, length);
        }
    }

    /// <summary>
    /// Fast encoding - returns bits packed into uint and the bit count
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public (uint bits, int length) GetEncodedBits(char c) => _encodingTable[c];

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public byte DecodeChar(IBitReader reader)
    {
        int node = 0;
        while (true)
        {
            int childSlot = node * 2 + (reader.ReadBit() ? 1 : 0);
            int value = _nodes[childSlot];

            if (value < 0)
                return (byte)(~value);

            node = value;
        }
    }
}
