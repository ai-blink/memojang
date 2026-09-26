namespace Memojang.Tests;

using System.Text;
using Memojang.App.Shell;

public class TextFileCodecTests
{
    private const int KoreanAnsiCodePage = 949;

    [Fact]
    public void Decode_PlainUtf8_RoundTripsWithoutBom()
    {
        byte[] bytes = Encoding.UTF8.GetBytes("안녕 hello");

        DecodedText decoded = TextFileCodec.Decode(bytes, KoreanAnsiCodePage);

        Assert.Equal("안녕 hello", decoded.Text);
        Assert.Equal(TextFileEncoding.Utf8, decoded.Encoding);
        Assert.Equal(bytes, TextFileCodec.Encode(decoded.Text, decoded.Encoding));
    }

    [Fact]
    public void Decode_Utf8Bom_PreservesBomOnSave()
    {
        byte[] bytes = [0xEF, 0xBB, 0xBF, .. Encoding.UTF8.GetBytes("memo")];

        DecodedText decoded = TextFileCodec.Decode(bytes, KoreanAnsiCodePage);

        Assert.Equal("memo", decoded.Text);
        Assert.Equal("UTF-8 BOM", decoded.Encoding.DisplayName);
        Assert.Equal(bytes, TextFileCodec.Encode(decoded.Text, decoded.Encoding));
    }

    [Fact]
    public void Decode_Utf16Le_RoundTrips()
    {
        byte[] bytes = [0xFF, 0xFE, .. Encoding.Unicode.GetBytes("메모\r\n")];

        DecodedText decoded = TextFileCodec.Decode(bytes, KoreanAnsiCodePage);

        Assert.Equal("메모\r\n", decoded.Text);
        Assert.Equal(TextFileEncoding.Utf16Le, decoded.Encoding);
        Assert.Equal(bytes, TextFileCodec.Encode(decoded.Text, decoded.Encoding));
    }

    [Fact]
    public void Decode_Cp949_IsDetectedAndRoundTripsByteForByte()
    {
        Encoding cp949 = TextFileEncoding.Ansi(KoreanAnsiCodePage).Encoding;
        byte[] bytes = cp949.GetBytes("한글 메모장");

        DecodedText decoded = TextFileCodec.Decode(bytes, KoreanAnsiCodePage);

        Assert.Equal("한글 메모장", decoded.Text);
        Assert.Equal("ANSI (949)", decoded.Encoding.DisplayName);
        Assert.Equal(bytes, TextFileCodec.Encode(decoded.Text, decoded.Encoding));
    }

    [Fact]
    public void CanEncodeLosslessly_FlagsCharactersMissingFromAnsiCodePage()
    {
        TextFileEncoding ansi = TextFileEncoding.Ansi(KoreanAnsiCodePage);

        Assert.True(TextFileCodec.CanEncodeLosslessly("한글", ansi));
        Assert.False(TextFileCodec.CanEncodeLosslessly("한글 😀", ansi));
        Assert.True(TextFileCodec.CanEncodeLosslessly("한글 😀", TextFileEncoding.Utf8));
    }

    [Fact]
    public void Decode_EmptyFile_IsUtf8()
    {
        DecodedText decoded = TextFileCodec.Decode([], KoreanAnsiCodePage);

        Assert.Equal(string.Empty, decoded.Text);
        Assert.Equal(TextFileEncoding.Utf8, decoded.Encoding);
    }
}
