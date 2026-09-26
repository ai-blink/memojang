namespace Memojang.App.Shell;

// Logical (file) lines treat CRLF, LF, and CR as one break each. WPF TextBox line APIs count visual lines once
// word wrap is on, so status bar and go-to-line use these instead.
public static class TextLines
{
    public static (int Line, int Column) GetPosition(string text, int index)
    {
        index = Math.Clamp(index, 0, text.Length);
        int line = 1;
        int lineStart = 0;

        for (int i = 0; i < index; i++)
        {
            if (text[i] == '\n' || (text[i] == '\r' && (i + 1 >= text.Length || text[i + 1] != '\n')))
            {
                line++;
                lineStart = i + 1;
            }
        }

        return (line, index - lineStart + 1);
    }

    public static int Count(string text) => GetPosition(text, text.Length).Line;

    public static int GetLineStartIndex(string text, int lineNumber)
    {
        int line = 1;
        for (int i = 0; i < text.Length && line < lineNumber; i++)
        {
            if (text[i] == '\n' || (text[i] == '\r' && (i + 1 >= text.Length || text[i + 1] != '\n')))
            {
                line++;
                if (line == lineNumber)
                {
                    return i + 1;
                }
            }
        }

        return lineNumber <= 1 ? 0 : text.Length;
    }
}
