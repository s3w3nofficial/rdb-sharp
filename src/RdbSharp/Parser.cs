using System.Text;

namespace RdbSharp;

public sealed class Parser
{
    public static void ParseLine(string line)
    {
        
    }

    public static void Parse(string filePath)
    {
        using var fs = new FileStream(filePath, FileMode.Open, FileAccess.Read);
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
                    var dbIndex = ReadLength(br);
                    Console.WriteLine($"Selecting DB {dbIndex}.");
                    break;
                }
                case (byte)Constants.OpCode.AUX:
                    Console.WriteLine(ReadString(br));
                    Console.WriteLine(ReadString(br));
                    break;
                case (byte) Constants.OpCode.RESIZEDB:
                {
                    Console.WriteLine("Found RESIZE DB opcode.");
                    var dbIndex = ReadLength(br);
                    var expireSize = ReadLength(br);
                    break;
                }
                case (byte)Constants.OpCode.EXPIRETIMEMS:
                    Console.WriteLine("Found EXPIRETIME MS opcode.");
                    break;
                case (byte)Constants.OpCode.EXPIRETIME:
                    Console.WriteLine("Found EXPIRET MS opcode.");
                    break;
                default:
                {
                    var objectType = (Constants.RdbObjectType)opcode;
                    Console.WriteLine(objectType);
                    ParseKeyValuePair(br, objectType);
                    break;
                }
            }
        }
        
        EOF: ;
    }

    private static void ParseKeyValuePair(BinaryReader br, Constants.RdbObjectType objectType)
    {
        var key = ReadString(br);
        Console.WriteLine($"Key: {key}");
        
        // 2. Parse the value depending on the object type.
        switch (objectType)
        {
            case Constants.RdbObjectType.String:
            {
                var value = ReadString(br);
                Console.WriteLine($"  Value (String): {value}");
                break;
            }
            case Constants.RdbObjectType.List:
            {
                // A simplistic approach: Redis lists can be stored as quicklists, ziplists, etc.
                // We’ll read an integer that might be the length, then read each item, etc.
                var listLength = ReadLength(br);
                Console.WriteLine($"  List length: {listLength}");
                for (var i = 0; i < listLength; i++)
                {
                    var listItem = ReadString(br);
                    Console.WriteLine($"    List item {i}: {listItem}");
                }
                break;
            }
            case Constants.RdbObjectType.Set:
            case Constants.RdbObjectType.SortedSet:
            case Constants.RdbObjectType.Hash:
            case Constants.RdbObjectType.ZipMap:
            case Constants.RdbObjectType.ZipList:
            case Constants.RdbObjectType.IntSet:
            case Constants.RdbObjectType.SortedSetZipList:
            case Constants.RdbObjectType.HashMapZipList:
            case Constants.RdbObjectType.ListQuickList:
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
        //var length = ReadLength(br, out var isEncoded);
        var (length, isEncoded) = ReadLengthWithEncoding(br);

        if (isEncoded)
        {
            if (length == Constants.RDB_ENC_INT8)
            {
                var value = br.ReadBytes(1);
                return Encoding.ASCII.GetString(value);
            }
            else if (length == Constants.RDB_ENC_INT16)
            {
                var value = br.ReadBytes(2);
                return Encoding.ASCII.GetString(value);
            }
            else if (length == Constants.RDB_ENC_INT32)
            {
                var value = br.ReadBytes(4);
                return Encoding.ASCII.GetString(value);
            }
            else if (length == Constants.RDB_ENC_LZF)
            {
                return "LZF";
            }
        }

        var bytes = br.ReadBytes(length);
        return Encoding.ASCII.GetString(bytes);
    }

    private static (int length, bool isEncoded) ReadLengthWithEncoding(BinaryReader br)
    {
        var length = 0;
        var isEncoded = false;
        
        var bytes = new List<byte>();
        bytes.Add(br.ReadByte());

        var encType = (bytes[0] & 0xC0) >> 6;

        switch (encType)
        {
            case Constants.RDB_ENCVAL:
            {
                isEncoded = true;
                length = bytes[0] & 0x3F;
                break;
            }
            case Constants.RDB_6BITLEN:
                length = bytes[0] & 0x3F;
                break;
            case Constants.RDB_14BITLEN:
            {
                bytes.Add(br.ReadByte());
                length = ((bytes[0] & 0x3F) << 8) | bytes[1];
                break;
            }
            case Constants.RDB_32BITLEN:
            {
                /*
                bytes.Add(br.ReadByte());
                bytes.Add(br.ReadByte());
                bytes.Add(br.ReadByte());
                length = (bytes[0] << 24) | (bytes[1] << 16) | (bytes[2] << 8) | bytes[3];
                */
                length = (int)br.ReadUInt32();
                break;
            }
            case Constants.RDB_64BITLEN:
            {
                length = (int)br.ReadUInt64();
                break;
            }
            default:
                throw new Exception($"read_length_with_encoding: Invalid string encoding {encType} (encoding byte %{bytes[0]})");
        }
        
        return (length, isEncoded);
    }

    private static long ReadLength(BinaryReader br)
    {
        var (length, isEncoded) = ReadLengthWithEncoding(br);

        return length;
    }
}