namespace RdbSharp;

internal sealed class Constants
{
    public enum OpCode : byte
    {
        EOF = 0xFF,
        SELECTDB = 0xFE,
        EXPIRETIME = 0xFD,
        EXPIRETIMEMS = 0xFC,
        RESIZEDB = 0xDB,
        AUX = 0xFA
    }
    
    public enum RdbObjectType : byte
    {
        String = 0,
        List = 1,
        Set = 2,
        ZSet = 3,
        Hash = 4,
    }
    
    public const byte RDB_6BITLEN = 0;
    public const byte RDB_14BITLEN = 1;
    public const byte RDB_32BITLEN = 2;
    public const string RDB_MAGIC = "REDIS";
} 