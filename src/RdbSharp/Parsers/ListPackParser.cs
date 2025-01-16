// Adapted from https://github.com/jwhitbeck/java-rdb-parser/blob/master/src/main/java/net/whitbeck/rdbparser/ListpackList.java

using System.Text;

namespace RdbSharp.Parsers;

public static class ListPackParser
{
    private const int LP_ENCODING_7BIT_UINT         = 0;     // 0xxx xxxx
    private const int LP_ENCODING_7BIT_UINT_MASK    = 0x80;  // 1000 0000

    private const int LP_ENCODING_6BIT_STR          = 0x80;  // 10xx xxxx
    private const int LP_ENCODING_6BIT_STR_MASK     = 0xC0;  // 1100 0000

    private const int LP_ENCODING_13BIT_INT         = 0xC0;  // 110x xxxx
    private const int LP_ENCODING_13BIT_INT_MASK    = 0xE0;  // 1110 0000

    private const int LP_ENCODING_12BIT_STR         = 0xE0;  // 1110 xxxx
    private const int LP_ENCODING_12BIT_STR_MASK    = 0xF0;  // 1111 0000

    // Sub-encodings
    private const int LP_ENCODING_16BIT_INT         = 0xF1;  // 1111 0001
    private const int LP_ENCODING_16BIT_INT_MASK    = 0xFF;
    private const int LP_ENCODING_24BIT_INT         = 0xF2;  // 1111 0010
    private const int LP_ENCODING_24BIT_INT_MASK    = 0xFF;
    private const int LP_ENCODING_32BIT_INT         = 0xF3;  // 1111 0011
    private const int LP_ENCODING_32BIT_INT_MASK    = 0xFF;
    private const int LP_ENCODING_64BIT_INT         = 0xF4;  // 1111 0100
    private const int LP_ENCODING_64BIT_INT_MASK    = 0xFF;
    private const int LP_ENCODING_32BIT_STR         = 0xF0;  // 1111 0000
    private const int LP_ENCODING_32BIT_STR_MASK    = 0xFF;

    /// <summary>
    /// Parses a listpack byte[] and returns the decoded entries as a List of strings.
    /// </summary>
    public static List<string> ParseListPack(byte[] envelope)
    {
        var parser = new InnerListPackParser(envelope);
        return parser.Parse();
    }

    private class InnerListPackParser
    {
        private readonly byte[] envelope;
        private int pos;
        private readonly List<string> list;

        public InnerListPackParser(byte[] envelope)
        {
            this.envelope = envelope;
            this.pos = 0;
            this.list = new List<string>();
        }

        public List<string> Parse()
        {
            // According to the listpack format:
            // 1) 4 bytes: total number of bytes (not used directly here).
            pos += 4;

            // 2) 2 bytes: number of elements (little-endian).
            int numElements = (envelope[pos++] & 0xFF)
                              | ((envelope[pos++] & 0xFF) << 8);

            // 3) Decode each element
            for (int i = 0; i < numElements; i++)
            {
                DecodeElement();
            }

            // 4) Verify that the next byte is the terminator 0xFF
            if (pos >= envelope.Length || (envelope[pos] & 0xFF) != 0xFF)
            {
                throw new InvalidOperationException("ListPack did not end with 0xFF byte.");
            }

            return list;
        }

        private void DecodeElement()
        {
            // The first byte indicates the encoding or string-length info
            // We do & 0xFF to get an unsigned interpretation in int form.
            var b = envelope[pos++] & 0xFF;

            // Handle possible string encodings first
            var strLen = 0;

            // 6-bit string: 10xxxxxx
            if ((b & LP_ENCODING_6BIT_STR_MASK) == LP_ENCODING_6BIT_STR)
            {
                // Lower 6 bits is the string length
                strLen = b & ~LP_ENCODING_6BIT_STR_MASK; // i.e. b & 0x3F
            }
            // 12-bit string: 1110xxxx
            else if ((b & LP_ENCODING_12BIT_STR_MASK) == LP_ENCODING_12BIT_STR)
            {
                // Combine the leftover lower bits of b with the next byte
                var lowerByte = envelope[pos++] & 0xFF;
                var highBits = (b & ~LP_ENCODING_12BIT_STR_MASK) & 0x0F; // leftover bits
                strLen = (lowerByte) | (highBits << 8);
            }
            // 32-bit string: 1111 0000 => 0xF0
            else if ((b & LP_ENCODING_32BIT_STR_MASK) == LP_ENCODING_32BIT_STR)
            {
                // Next 4 bytes = strLen
                if (pos + 4 > envelope.Length)
                    throw new InvalidOperationException("Invalid envelope; not enough bytes for 32-bit length.");

                strLen = (envelope[pos++] & 0xFF)
                       | ((envelope[pos++] & 0xFF) << 8)
                       | ((envelope[pos++] & 0xFF) << 16)
                       | ((envelope[pos++] & 0xFF) << 24);
            }

            if (strLen > 0)
            {
                if (pos + strLen > envelope.Length)
                    throw new InvalidOperationException("Invalid string length exceeds buffer.");

                var strValue = Encoding.ASCII.GetString(envelope, pos, strLen);
                list.Add(strValue);

                pos += strLen;

                var backlenSize = GetLenBytes(strLen);
                pos += backlenSize;
                return;
            }

            long val;
            long negStart, negMax;

            // 7-bit unsigned int: 0xxxxxxx
            if ((b & LP_ENCODING_7BIT_UINT_MASK) == LP_ENCODING_7BIT_UINT)
            {
                val = b & ~LP_ENCODING_7BIT_UINT_MASK;
                pos++;
                list.Add(val.ToString());
                return;
            }
            // 13-bit int: 110xxxxx
            else if ((b & LP_ENCODING_13BIT_INT_MASK) == LP_ENCODING_13BIT_INT)
            {
                if (pos >= envelope.Length)
                    throw new InvalidOperationException("Not enough bytes for 13-bit int.");

                val = ((b & ~LP_ENCODING_13BIT_INT_MASK) << 8)
                    | (envelope[pos++] & 0xFF);

                negStart = 1 << 12;     // 4096
                negMax = (1 << 13) - 1; // 8191
            }
            // 16-bit int: 1111 0001 => 0xF1
            else if ((b & LP_ENCODING_16BIT_INT_MASK) == LP_ENCODING_16BIT_INT)
            {
                if (pos + 2 > envelope.Length)
                    throw new InvalidOperationException("Not enough bytes for 16-bit int.");

                val = (envelope[pos++] & 0xFF)
                    | ((envelope[pos++] & 0xFF) << 8);

                negStart = 1 << 15;    // 32768
                negMax = (1 << 16) - 1; // 65535
            }
            // 24-bit int: 1111 0010 => 0xF2
            else if ((b & LP_ENCODING_24BIT_INT_MASK) == LP_ENCODING_24BIT_INT)
            {
                if (pos + 3 > envelope.Length)
                    throw new InvalidOperationException("Not enough bytes for 24-bit int.");

                val = (envelope[pos++] & 0xFF)
                    | ((envelope[pos++] & 0xFF) << 8)
                    | ((envelope[pos++] & 0xFF) << 16);

                negStart = 1L << 23;      // 8,388,608
                negMax = (1L << 24) - 1;  // 16,777,215
            }
            // 32-bit int: 1111 0011 => 0xF3
            else if ((b & LP_ENCODING_32BIT_INT_MASK) == LP_ENCODING_32BIT_INT)
            {
                if (pos + 4 > envelope.Length)
                    throw new InvalidOperationException("Not enough bytes for 32-bit int.");

                val = (envelope[pos++] & 0xFF)
                    | ((envelope[pos++] & 0xFF) << 8)
                    | ((envelope[pos++] & 0xFF) << 16)
                    | ((envelope[pos++] & 0xFF) << 24);

                negStart = 1L << 31;      // 2,147,483,648
                negMax = (1L << 32) - 1;  // 4,294,967,295
            }
            // 64-bit int: 1111 0100 => 0xF4
            else if ((b & LP_ENCODING_64BIT_INT_MASK) == LP_ENCODING_64BIT_INT)
            {
                if (pos + 8 > envelope.Length)
                    throw new InvalidOperationException("Not enough bytes for 64-bit int.");

                val = ((long)envelope[pos++] & 0xFF)
                    | (((long)envelope[pos++] & 0xFF) << 8)
                    | (((long)envelope[pos++] & 0xFF) << 16)
                    | (((long)envelope[pos++] & 0xFF) << 24)
                    | (((long)envelope[pos++] & 0xFF) << 32)
                    | (((long)envelope[pos++] & 0xFF) << 40)
                    | (((long)envelope[pos++] & 0xFF) << 48)
                    | (((long)envelope[pos++] & 0xFF) << 56);

                list.Add(val.ToString());
                pos++;
                
                return;
            }
            else
            {
                throw new InvalidOperationException("Invalid ListPack envelope encoding");
            }

            // Two's-complement adjustment if value is in negative range
            if (val >= negStart)
            {
                var diff = negMax - val;
                val = diff;
                val = -val - 1;
            }

            pos++;

            // Finally, store the int as string
            list.Add(val.ToString());
        }

        private int GetLenBytes(int len)
        {
            return len switch
            {
                < 128 => 1,
                < 16384 => 2,
                < 2097152 => 3,
                < 268435456 => 4,
                _ => 5
            };
        }
    }
}
