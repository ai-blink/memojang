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

    public static (string Text, int Count) ReplaceAll(string text, string query, string replacement, StringComparison comparison)
    {
        if (string.IsNullOrEmpty(text) || string.IsNullOrEmpty(query))
        {
            return (text, 0);
        }

        var updated = new List<string>();
        int count = 0;
        int currentIndex = 0;

        while (currentIndex < text.Length)
        {
            int matchIndex = text.IndexOf(query, currentIndex, comparison);
            if (matchIndex < 0)
            {
                break;
            }

            updated.Add(text[currentIndex..matchIndex]);
            updated.Add(replacement);
            currentIndex = matchIndex + query.Length;
            count++;
        }

        if (count == 0)
        {
            return (text, 0);
        }

        updated.Add(text[currentIndex..]);
        return (string.Concat(updated), count);
    }
}
