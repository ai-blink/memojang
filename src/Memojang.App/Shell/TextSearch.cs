namespace Memojang.App.Shell;

public static class TextSearch
{
    public static int FindNext(string text, string query, int startIndex, StringComparison comparison)
    {
        if (string.IsNullOrEmpty(text) || string.IsNullOrEmpty(query))
        {
            return -1;
        }

        int safeStart = Math.Clamp(startIndex, 0, text.Length);
        int match = text.IndexOf(query, safeStart, comparison);

        if (match >= 0)
        {
            return match;
        }

        return safeStart == 0 ? -1 : text.IndexOf(query, 0, comparison);
    }
}
