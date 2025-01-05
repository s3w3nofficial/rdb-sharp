using System.Text;

namespace RdbSharp;

public sealed class RdbSharpParser : IDisposable
{
    private readonly BinaryReader _br;
    private readonly FileStream _fs;

    private bool hasNext = true;
    
    public RdbSharpParser(string filePath)
    {
        var fs = new FileStream(filePath, FileMode.Open, FileAccess.Read);
        var br = new BinaryReader(fs);
        
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
        
        _fs = fs;
        _br = br;
    }

    public IEntry? NextEntry()
    {
        if (!hasNext)
        {
            return null;
        }
        
        var opcode = _br.ReadByte();
        
        switch (opcode)
        {
            case (byte)Constants.RDB_OPCODE.EOF:
                hasNext = false;
                return new EOF();
            case (byte) Constants.RDB_OPCODE.SELECTDB:
            {
                var dbIndex = ReadLength(_br);
                return new SelectDb(dbIndex);
            }
            case (byte) Constants.RDB_OPCODE.AUX:
            {
                var k = Encoding.ASCII.GetBytes(ReadString(_br));
                var v = Encoding.ASCII.GetBytes(ReadString(_br));
                var aux = new Aux(k, v);
                return aux;
            }
            case (byte) Constants.RDB_OPCODE.RESIZEDB:
            {
                var dbIndex = ReadLength(_br);
                var expireSize = ReadLength(_br);
                return new ResizeDb(dbIndex, expireSize);
            }
            case (byte)Constants.RDB_OPCODE.EXPIRETIME_MS:
                Console.WriteLine("Found EXPIRETIME MS opcode.");
                long expireTimeMs = _br.ReadInt64(); 
                break;
            case (byte)Constants.RDB_OPCODE.EXPIRETIME:
                Console.WriteLine("Found EXPIRET MS opcode.");
                break;
            default:
            {
                var objectType = (Constants.RDB_TYPE)opcode;
                return ReadObject(_br, objectType);
            }
        }

        return null;
    }
    
    private static KeyValuePair ReadObject(BinaryReader br, Constants.RDB_TYPE objectType)
    {
        var key = ReadString(br);
        Console.WriteLine($"Key: {key}");
        //Console.WriteLine(objectType);
        
        switch (objectType)
        {
            case Constants.RDB_TYPE.STRING:
            {
                var value = ReadString(br);
                Console.WriteLine($"  Value (String): {value}");
                return new KeyValuePair(key, value, ValueType.VALUE);
            }
            case Constants.RDB_TYPE.LIST:
            {
                var length = ReadLength(br);
                Console.WriteLine($"  List length: {length}");
                for (var i = 0; i < length; i++)
                {
                    var item = ReadString(br);
                    Console.WriteLine($"    List item {i}: {item}");
                }
                return new KeyValuePair(key, new List<string>(), ValueType.LIST);
            }
            case Constants.RDB_TYPE.SET:
            {
                var length = ReadLength(br);
                Console.WriteLine($"  Set length: {length}");
                for (var i = 0; i < length; i++)
                {
                    var item = ReadString(br);
                    Console.WriteLine($"    Set item {i}: {item}");
                }
                return new KeyValuePair(key, new List<string>(), ValueType.SET);
            }
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
            case Constants.RDB_TYPE.LIST_QUICKLIST_2:
            {
                var items = ParseQuickList2(br);
                Console.WriteLine($"  List length: {items.Count}");

                foreach (var (i, item) in items.Index())
                {
                    Console.WriteLine($"    List item {i}: {item}");
                }
                
                return new KeyValuePair(key, items, ValueType.QUICKLIST2);
            }
            case Constants.RDB_TYPE.STREAM_LISTPACKS_2:
            case Constants.RDB_TYPE.SET_LISTPACK:
            {
                var items = ParseSetListPack(br);
                Console.WriteLine($"  Set length: {items.Count}");

                foreach (var (i, item) in items.Index())
                {
                    Console.WriteLine($"    Set item {i}: {item}");
                }
                
                return new KeyValuePair(key, items, ValueType.SET_AS_LISTPACK);
            }
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

        return new KeyValuePair(key, "", ValueType.VALUE);
    }
    
    private static string ReadString(BinaryReader br)
    {
        var (length, isEncoded) = ReadLengthWithEncoding(br);

        if (isEncoded)
        {
            if (length == Constants.RDB_ENC_INT8)
            {
                // TODO fix this
                return $"{(byte)br.ReadChar()}";
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
                var clen = ReadLength(br);
                // expected length
                var l = ReadLength(br);
                var value = CLZF2.Decompress(br.ReadBytes((int)clen));
                return Encoding.ASCII.GetString(value);
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

    private static HashSet<string> ParseSetListPack(BinaryReader br)
    {
        var entries = ParseListPack(br);

        return [..entries];
    }

    private static List<string> ParseQuickList2(BinaryReader br)
    {
        var items = new List<string>();
        
        // quick list length
        var length = ReadLength(br);
        
        var len = ReadLength(br);
        
        for (var i = 0; i < length; i++)
        {
            var entries = ParseListPack(br);
            items.AddRange(entries);
        }

        return items;
    }

    private static List<string> ParseListPack(BinaryReader br)
    {
        var length = ReadLength(br);

        var payload = br.ReadBytes((int)length);

        var bytes = new ReadOnlySpan<byte>(payload);
        
        return ListPackParser.ParseListpack(bytes.ToArray());
    }

    public void Dispose()
    {
        _br.Dispose();
        _fs.Dispose();
    }
}