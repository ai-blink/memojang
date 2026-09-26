using System.Globalization;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;

namespace Memojang.App.Shell;

public sealed record TextFileEncoding(string DisplayName, Encoding Encoding, bool EmitsPreamble)
{
    public static TextFileEncoding Utf8 { get; } = new("UTF-8", new UTF8Encoding(false), false);
    public static TextFileEncoding Utf8Bom { get; } = new("UTF-8 BOM", new UTF8Encoding(true), true);
    public static TextFileEncoding Utf16Le { get; } = new("UTF-16 LE", new UnicodeEncoding(false, true), true);
    public static TextFileEncoding Utf16Be { get; } = new("UTF-16 BE", new UnicodeEncoding(true, true), true);

    public bool IsUnicode => Encoding is UTF8Encoding or UnicodeEncoding;

    public static TextFileEncoding Ansi(int codePage)
    {
        TextFileCodec.EnsureCodePagesRegistered();
        return new($"ANSI ({codePage})", Encoding.GetEncoding(codePage), false);
    }
}

public sealed record DecodedText(string Text, TextFileEncoding Encoding);

// Reads without trusting the platform default (UTF-8 on .NET) so legacy ANSI files such as CP949 are not
// silently decoded as U+FFFD and then overwritten. Detection order: BOM -> strict UTF-8 -> system ANSI code page.
public static class TextFileCodec
{
    private static readonly UTF8Encoding StrictUtf8 = new(false, throwOnInvalidBytes: true);
    private static bool _codePagesRegistered;

    public static DecodedText Decode(byte[] bytes) => Decode(bytes, GetSystemAnsiCodePage());

    public static DecodedText Decode(byte[] bytes, int fallbackAnsiCodePage)
    {
        if (StartsWith(bytes, 0xEF, 0xBB, 0xBF))
        {
            return new(Encoding.UTF8.GetString(bytes, 3, bytes.Length - 3), TextFileEncoding.Utf8Bom);
        }

        if (StartsWith(bytes, 0xFF, 0xFE))
        {
            return new(Encoding.Unicode.GetString(bytes, 2, bytes.Length - 2), TextFileEncoding.Utf16Le);
        }

        if (StartsWith(bytes, 0xFE, 0xFF))
        {
            return new(Encoding.BigEndianUnicode.GetString(bytes, 2, bytes.Length - 2), TextFileEncoding.Utf16Be);
        }

        try
        {
            return new(StrictUtf8.GetString(bytes), TextFileEncoding.Utf8);
        }
        catch (DecoderFallbackException)
        {
            TextFileEncoding ansi = TextFileEncoding.Ansi(fallbackAnsiCodePage);
            return new(ansi.Encoding.GetString(bytes), ansi);
        }
    }

    public static byte[] Encode(string text, TextFileEncoding encoding)
    {
        byte[] preamble = encoding.EmitsPreamble ? encoding.Encoding.GetPreamble() : [];
        byte[] body = encoding.Encoding.GetBytes(text);
        return [.. preamble, .. body];
    }

    public static bool CanEncodeLosslessly(string text, TextFileEncoding encoding)
    {
        if (encoding.IsUnicode)
        {
            return true;
        }

        Encoding strict = Encoding.GetEncoding(
            encoding.Encoding.CodePage,
            EncoderFallback.ExceptionFallback,
            DecoderFallback.ExceptionFallback);
        try
        {
            strict.GetBytes(text);
            return true;
        }
        catch (EncoderFallbackException)
        {
            return false;
        }
    }

    public static DecodedText Read(string path) => Decode(File.ReadAllBytes(path));

    public static async Task<DecodedText> ReadAsync(string path) => Decode(await File.ReadAllBytesAsync(path));

    public static void Write(string path, string text, TextFileEncoding encoding) =>
        File.WriteAllBytes(path, Encode(text, encoding));

    internal static void EnsureCodePagesRegistered()
    {
        if (!_codePagesRegistered)
        {
            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
            _codePagesRegistered = true;
        }
    }

    private static int GetSystemAnsiCodePage()
    {
        // GetACP returns 65001 when "Beta: Use Unicode UTF-8" is on; fall back to the culture's legacy page then.
        int codePage = GetACP();
        return codePage == 65001 ? CultureInfo.CurrentCulture.TextInfo.ANSICodePage : codePage;
    }

    private static bool StartsWith(byte[] bytes, params byte[] prefix) =>
        bytes.Length >= prefix.Length && bytes.AsSpan(0, prefix.Length).SequenceEqual(prefix);

    [DllImport("kernel32.dll")]
    private static extern int GetACP();
}
