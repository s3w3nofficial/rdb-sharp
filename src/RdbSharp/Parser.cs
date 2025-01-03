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
                case (byte)Constants.RDB_OPCODE.EOF:
                    Console.WriteLine("Found EOF opcode. Stopping parse.");
                    goto EOF;
                case (byte) Constants.RDB_OPCODE.SELECTDB:
                {
                    var dbIndex = ReadLength(br);
                    Console.WriteLine($"Selecting DB {dbIndex}.");
                    break;
                }
                case (byte)Constants.RDB_OPCODE.AUX:
                    Console.WriteLine(ReadString(br));
                    Console.WriteLine(ReadString(br));
                    break;
                case (byte) Constants.RDB_OPCODE.RESIZEDB:
                {
                    Console.WriteLine("Found RESIZE DB opcode.");
                    var dbIndex = ReadLength(br);
                    var expireSize = ReadLength(br);
                    break;
                }
                case (byte)Constants.RDB_OPCODE.EXPIRETIME_MS:
                    Console.WriteLine("Found EXPIRETIME MS opcode.");
                    break;
                case (byte)Constants.RDB_OPCODE.EXPIRETIME:
                    Console.WriteLine("Found EXPIRET MS opcode.");
                    break;
                default:
                {
                    var objectType = (Constants.RDB_TYPE)opcode;
                    var key = ReadString(br);
                    Console.WriteLine($"Key: {key}");
                    ReadObject(br, objectType);
                    break;
                }
            }
        }
        
        EOF: ;
    }

    private static void ReadObject(BinaryReader br, Constants.RDB_TYPE objectType)
    {
        //Console.WriteLine(objectType);
        
        switch (objectType)
        {
            case Constants.RDB_TYPE.STRING:
            {
                var value = ReadString(br);
                Console.WriteLine($"  Value (String): {value}");
                break;
            }
            case Constants.RDB_TYPE.LIST:
            {
                var listLength = ReadLength(br);
                Console.WriteLine($"  List length: {listLength}");
                for (var i = 0; i < listLength; i++)
                {
                    var item = ReadString(br);
                    Console.WriteLine($"    List item {i}: {item}");
                }
                break;
            }
            case Constants.RDB_TYPE.LIST_QUICKLIST_2:
            {
                var items = ParseQuickList2(br);
                Console.WriteLine($"  QuickList length: {items.Count}");

                foreach (var (i, item) in items.Index())
                {
                    Console.WriteLine($"    List item {i}: {item}");
                }
                
                break;
            }
            case Constants.RDB_TYPE.SET:
            case Constants.RDB_TYPE.ZSET:
            case Constants.RDB_TYPE.HASH:
            case Constants.RDB_TYPE.ZSET_2:
            case Constants.RDB_TYPE.MODULE_PRE_GA:
            case Constants.RDB_TYPE.MODULE_2:
            /* Object types for encoded objects. */
            case Constants.RDB_TYPE.HASH_ZIPMAP:
            case Constants.RDB_TYPE.LIST_ZIPLIST:
            case Constants.RDB_TYPE.SET_INTSET:
            case Constants.RDB_TYPE.ZSET_ZIPLIST:
            case Constants.RDB_TYPE.HASH_ZIPLIST:
            case Constants.RDB_TYPE.LIST_QUICKLIST:
            case Constants.RDB_TYPE.STREAM_LISTPACKS:
            case Constants.RDB_TYPE.HASH_LISTPACK:
            case Constants.RDB_TYPE.ZSET_LISTPACK:
            //case Constants.RDB_TYPE.LIST_QUICKLIST_2:
            case Constants.RDB_TYPE.STREAM_LISTPACKS_2:
            case Constants.RDB_TYPE.SET_LISTPACK:
            case Constants.RDB_TYPE.STREAM_LISTPACKS_3:
            case Constants.RDB_TYPE.HASH_METADATA_PRE_GA:
            case Constants.RDB_TYPE.HASH_LISTPACK_EX_PRE_GA:
            case Constants.RDB_TYPE.HASH_METADATA:
            case Constants.RDB_TYPE.HASH_LISTPACK_EX:
            case Constants.RDB_TYPE.MAX:
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
                bytes.Add(br.ReadByte());
                bytes.Add(br.ReadByte());
                bytes.Add(br.ReadByte());
                length = (bytes[0] << 24) | (bytes[1] << 16) | (bytes[2] << 8) | bytes[3];
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

    private static List<string> ParseQuickList2(BinaryReader br)
    {
        var length = ReadLength(br);
        var items = new List<string>();

        for (var i = 0; i < length; i++)
        {
            var entries = ParseListPack(br);
            items.AddRange(entries);
        }

        return items;
    }

    private static List<string> ParseListPack(BinaryReader br)
    {
        var items = new List<string>();
        
        var length = ReadLength(br);
        
        var payload = ReadString(br);

        var bytes = new ReadOnlySpan<byte>(Encoding.ASCII.GetBytes(payload));

        var pos = 0;

        pos += 4;
        
        var numElements = (bytes[pos++] & 0xFF) << 0 | (bytes[pos++] & 0xFF) << 8;

        for (int i = 0; i < numElements; i++)
        {
            items.Add("a");
        }

        return items;
    }

}