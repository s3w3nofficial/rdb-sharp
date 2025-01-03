namespace RdbSharp;

internal sealed class Constants
{
    public enum OpCode : byte
    {
        EOF = 0xFF,
        SELECTDB = 0xFE,
        EXPIRETIME = 0xFD,
        EXPIRETIMEMS = 0xFC,
        RESIZEDB = 0xFB,
        AUX = 0xFA
    }
    
    public enum RdbObjectType : byte
    {
        String = 0,
        List = 1,
        Set = 2,
        SortedSet = 3,
        Hash = 4,
        ZipMap = 9,
        ZipList = 10,
        IntSet = 11,
        SortedSetZipList = 12,
        HashMapZipList = 13,
        ListQuickList = 14,
    }
    
    public const byte RDB_6BITLEN = 0;
    public const byte RDB_14BITLEN = 1;
    public const byte RDB_32BITLEN = 0x80;
    public const byte RDB_64BITLEN = 0x81;
    public const byte RDB_ENCVAL = 3;

    public const byte RDB_ENC_INT8 = 0;
    public const byte RDB_ENC_INT16 = 1;
    public const byte RDB_ENC_INT32 = 2;
    public const byte RDB_ENC_LZF = 3;
    
    public const string RDB_MAGIC = "REDIS";
} 