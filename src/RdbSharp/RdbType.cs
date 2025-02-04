namespace RdbSharp;

/// <summary>
/// RdbType
/// </summary>
public enum RdbType : byte
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