using System.Text;

namespace RdbSharp;

public class Parser
{
    private readonly string _filePath;
    
    public Parser(string filePath)
    {
        ArgumentNullException.ThrowIfNull(filePath);
        
        _filePath = filePath;
    }

    public void ParseLine(string line)
    {
        
    }

    public void Parse()
    {
        using var fs = new FileStream(_filePath, FileMode.Open, FileAccess.Read);
        using var br = new BinaryReader(fs);
        
        var magic = Encoding.ASCII.GetString(br.ReadBytes(5));
        if (magic != Constants.RDB_MAGIC)
        {
            throw new Exception("Invalid RDB file (missing 'REDIS' magic).");
        }
        
        var versionStr = Encoding.ASCII.GetString(br.ReadBytes(4));
        if (!int.TryParse(versionStr, out var version))
        {
            throw new Exception($"Invalid RDB version: {versionStr}");
        }
        Console.WriteLine($"RDB Version: {version}");

        while (true)
        {
            if (fs.Position >= fs.Length)
                break;
            
            var opcode = br.ReadByte();
            
            switch (opcode)
            {
                case (byte)Constants.OpCode.EOF:
                    Console.WriteLine("Found EOF opcode. Stopping parse.");
                    goto EOF;
                case (byte) Constants.OpCode.SELECTDB:
                {
                    var dbIndex = ReadLength(br, out var isEncoded);
                    Console.WriteLine($"Selecting DB {dbIndex}.");
                    break;
                }
                default:
                {
                    var objectType = (Constants.RdbObjectType)opcode;
                    ParseKeyValuePair(br, objectType);
                    break;
                }
            }
        }
        
        EOF: ;
    }

    private static void ParseKeyValuePair(BinaryReader br, Constants.RdbObjectType objectType)
    {
        string key = ReadString(br);
        Console.WriteLine($"Key: {key}");

        // 2. Parse the value depending on the object type.
        switch (objectType)
        {
            case Constants.RdbObjectType.String:
            {
                string value = ReadString(br);
                Console.WriteLine($"  Value (String): {value}");
                break;
            }
            case Constants.RdbObjectType.List:
            {
                // A simplistic approach: Redis lists can be stored as quicklists, ziplists, etc.
                // We’ll read an integer that might be the length, then read each item, etc.
                var listLength = ReadLength(br, out bool isEncoded);
                Console.WriteLine($"  List length: {listLength}");
                for (var i = 0; i < listLength; i++)
                {
                    var listItem = ReadString(br);
                    Console.WriteLine($"    List item {i}: {listItem}");
                }
                break;
            }
            case Constants.RdbObjectType.Set:
            case Constants.RdbObjectType.ZSet:
            case Constants.RdbObjectType.Hash:
            {
                // For demonstration, let's just skip them or handle them in a similar pattern
                // This is where you would implement your data structure parsing logic.
                Console.WriteLine($"  Object type {objectType} not fully implemented in this example.");
                break;
            }
            default:
            {
                Console.WriteLine($"  Unknown or unhandled object type: {objectType}");
                break;
            }
        }
    }
    
    private static string ReadString(BinaryReader br)
    {
        var length = ReadLength(br, out var isEncoded);
        
        if (isEncoded)
        {
            // Actual RDB can have integer encodings, LZF-compressions, etc.
            // We’re skipping all that for simplicity:
            return $"[Encoded data length={length}]";
        }
        else
        {
            var data = br.ReadBytes((int)length);
            return Encoding.UTF8.GetString(data);
        }
    }

    private static long ReadLength(BinaryReader br, out bool isEncoded)
    {
        var lenByte = br.ReadByte();
        isEncoded = false;

        // 2-bit encoding at the top of the byte
        var type = (byte)((lenByte & 0xC0) >> 6);
        var value = (byte)(lenByte & 0x3F);

        switch (type)
        {
            // 00xxxxxx: value is lenByte & 0x3F
            case Constants.RDB_6BITLEN:
                return value;
            // 01xxxxxx: 16-bit integer
            case Constants.RDB_14BITLEN:
            {
                var nextByte = br.ReadByte();
                return (value << 8) | nextByte;
            }
            // 10xxxxxx: 32-bit integer
            case Constants.RDB_32BITLEN:
            {
                // Actually read entire 4 bytes; we've read 1 already, so do more carefully:
                var b2 = br.ReadByte();
                var b3 = br.ReadByte();
                var b4 = br.ReadByte();

                // Reconstruct the integer: value << 24, etc. 
                // But note 'value' is the 6-bit from lenByte.
                // This is a simplified approach; real RDB can differ in layout.
                var result = (value << 24) | (b2 << 16) | (b3 << 8) | b4;
                return result;
            }
            default:
                // 11xxxxxx => special encoding
                // e.g., LZF compression or integer encoding
                isEncoded = true;
                return value; // In a real scenario, you'd parse further based on the type of encoding.
        }
    }
}