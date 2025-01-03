namespace RdbSharp;

/// <summary>
/// Utils for working with redis length encoding
/// </summary>
public static class RedisLengthEncodingUtils
{
    /// <summary>
    /// Decodes the redis length encoded length and returns payload start
    /// </summary>
    /// <param name="buff"></param>
    /// <returns></returns>
    /// <exception cref="ArgumentException"></exception>
    public static long DecodeLength(ref ReadOnlySpan<byte> buff)
    {
        if (buff.Length == 0)
            throw new ArgumentException("Encoded length cannot be empty.", nameof(buff));

        var firstByte = buff[0];
        return (firstByte >> 6) switch
        {
            // 6-bit encoding
            0 => firstByte & 0x3F,
            // 14-bit encoding
            1 => ((firstByte & 0x3F) << 8) | buff[1],
            // 32-bit encoding
            2 => (long)((buff[1] << 24) | (buff[2] << 16) | (buff[3] << 8) | buff[4]),
            _ => throw new ArgumentException("Invalid encoding type.", nameof(buff))
        };
    }

    /// <summary>
    /// Encoded payload length to redis encoded payload length
    /// </summary>
    /// <param name="length"></param>
    /// <returns></returns>
    /// <exception cref="ArgumentOutOfRangeException"></exception>
    public static byte[] EncodeLength(long length)
    {
        switch (length)
        {
            // 6-bit encoding (length ≤ 63)
            case < 1 << 6:
                return [(byte)(length & 0x3F)]; // 00xxxxxx
            // 14-bit encoding (64 ≤ length ≤ 16,383)
            case < 1 << 14:
                {
                    var firstByte = (byte)(((length >> 8) & 0x3F) | (1 << 6)); // 01xxxxxx
                    var secondByte = (byte)(length & 0xFF);
                    return [firstByte, secondByte];
                }
            // 32-bit encoding (length ≤ 4,294,967,295)
            case <= 0xFFFFFFFF:
                {
                    var firstByte = (byte)(2 << 6); // 10xxxxxx
                    var lengthBytes = BitConverter.GetBytes((uint)length); // Ensure unsigned
                    if (BitConverter.IsLittleEndian)
                    {
                        Array.Reverse(lengthBytes); // Convert to big-endian
                    }
                    return new[] { firstByte }.Concat(lengthBytes).ToArray();
                }
            default:
                throw new ArgumentOutOfRangeException("Length exceeds maximum allowed for Redis encoding (4,294,967,295).");
        }
    }
}