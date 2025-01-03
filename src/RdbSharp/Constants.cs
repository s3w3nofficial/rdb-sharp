namespace RdbSharp;

public class Constants
{
    public enum OpCode : byte
    {
        SELECTDB = 0xFE,
        EOF = 0xFF
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