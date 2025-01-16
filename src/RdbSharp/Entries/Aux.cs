using System.Text;

namespace RdbSharp.Entries;

/// <summary>
/// Aux field entry type
/// </summary>
public class Aux : IEntry
{
    private readonly byte[] _key;
    private readonly byte[] _value;
    
    public Aux(byte[] key, byte[] value)
    {
        _key = key;
        _value = value;
    }

    public override string ToString()
    {
        return string.Format("AUX (k: {0}, v: {1})",
            GetPrintableString(_key),
            GetPrintableString(_value));
    }
    
    private static string GetPrintableString(byte[] bytes)
    {
        var sb = new StringBuilder();
        foreach (var b in bytes)
        {
            // 'b' in C# is already unsigned [0..255], so no sign-extension needed.
            if (b > 31 && b < 127) // printable ASCII range (32..126)
            {
                sb.Append((char)b);
            }
            else
            {
                // Escape non-printable characters as \xNN
                sb.AppendFormat("\\x{0:X2}", b);
            }
        }
        return sb.ToString();
    }

    public EntryType Type => EntryType.AUX;
}