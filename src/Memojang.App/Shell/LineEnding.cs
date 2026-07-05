namespace Memojang.App.Shell;

public static class LineEnding
{
    public const string Crlf = "CRLF";
    public const string Lf = "LF";
    public const string Cr = "CR";

    public static string Detect(string text)
    {
        int crlfCount = Count(text, "\r\n");
        int lfCount = CountLfOnly(text);
        int crCount = CountCrOnly(text);

        if (lfCount > crlfCount && lfCount >= crCount)
        {
            return Lf;
        }

        if (crCount > crlfCount && crCount > lfCount)
        {
            return Cr;
        }

        return Crlf;
    }

    private static int Count(string text, string value)
    {
        int count = 0;
        int index = 0;

        while ((index = text.IndexOf(value, index, StringComparison.Ordinal)) >= 0)
        {
            count++;
            index += value.Length;
        }

        return count;
    }

    private static int CountLfOnly(string text)
    {
        int count = 0;

        for (int i = 0; i < text.Length; i++)
        {
            if (text[i] == '\n' && (i == 0 || text[i - 1] != '\r'))
            {
                count++;
            }
        }

        return count;
    }

    private static int CountCrOnly(string text)
    {
        int count = 0;

        for (int i = 0; i < text.Length; i++)
        {
            if (text[i] == '\r' && (i == text.Length - 1 || text[i + 1] != '\n'))
            {
                count++;
            }
        }

        return count;
    }
}
