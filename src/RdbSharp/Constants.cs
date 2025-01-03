namespace RdbSharp;

internal sealed class Constants
{
    public enum RDB_OPCODE : byte
    {
        SLOT_INFO = 244,
        FUNCTION2 = 245,
        FUNCTION = 246,
        MODULE_AUX = 247,
        IDLE = 248,
        FREQ = 249,
        AUX = 250,
        RESIZEDB = 251,
        EXPIRETIME_MS = 252,
        EXPIRETIME = 253,
        SELECTDB = 254,
        EOF = 255
    }
    
    public enum RDB_TYPE : byte
    {
        STRING = 0,
        LIST = 1,
        SET = 2,
        ZSET = 3,
        HASH = 4,
        ZSET_2 = 5,
        MODULE_PRE_GA = 6,
        MODULE_2 = 7,
        HASH_ZIPMAP = 9,
        LIST_ZIPLIST = 10,
        SET_INTSET = 11,
        ZSET_ZIPLIST = 12,
        HASH_ZIPLIST = 13,
        LIST_QUICKLIST = 14,
        STREAM_LISTPACKS = 15,
        HASH_LISTPACK = 16,
        ZSET_LISTPACK = 17,
        LIST_QUICKLIST_2 = 18,
        STREAM_LISTPACKS_2 = 19,
        SET_LISTPACK = 20,
        STREAM_LISTPACKS_3 = 21,
        HASH_METADATA_PRE_GA = 22,
        HASH_LISTPACK_EX_PRE_GA = 23,
        HASH_METADATA = 24,
        HASH_LISTPACK_EX = 25,
        MAX = 26
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