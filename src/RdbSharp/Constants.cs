namespace RdbSharp;

/// <summary>
/// Internal constants
/// </summary>
internal sealed class Constants
{
    internal enum RDB_OPCODE : byte
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