using System.Text.RegularExpressions;

namespace Memojang.App.Shell;

public enum MarkdownInlineKind
{
    Text,
    Bold,
    Italic,
    Strike,
    Code,
    Link
}

public sealed record MarkdownInlineSpan(MarkdownInlineKind Kind, string Text, string? Url = null);

// Covers exactly what the toolbar inserts (bold, italic, strike, link) plus inline code. Nesting is not
// supported; an unmatched marker stays literal text, which keeps the preview honest about the source.
public static class MarkdownInlineParser
{
    private static readonly Regex InlineRegex = new(
        @"`(?<code>[^`]+)`" +
        @"|\*\*(?<bold>.+?)\*\*" +
        @"|~~(?<strike>.+?)~~" +
        @"|(?<![\*\w])\*(?<italic>[^\*\s](?:[^\*]*[^\*\s])?)\*(?![\*\w])" +
        @"|\[(?<label>[^\]]+)\]\((?<url>[^)\s]+)\)",
        RegexOptions.Compiled);

    public static IReadOnlyList<MarkdownInlineSpan> Parse(string text)
    {
        var spans = new List<MarkdownInlineSpan>();
        int currentIndex = 0;

        foreach (Match match in InlineRegex.Matches(text))
        {
            if (match.Index > currentIndex)
            {
                spans.Add(new(MarkdownInlineKind.Text, text[currentIndex..match.Index]));
            }

            spans.Add(ToSpan(match));
            currentIndex = match.Index + match.Length;
        }

        if (currentIndex < text.Length)
        {
            spans.Add(new(MarkdownInlineKind.Text, text[currentIndex..]));
        }

        return spans;
    }

    private static MarkdownInlineSpan ToSpan(Match match)
    {
        if (match.Groups["code"].Success)
        {
            return new(MarkdownInlineKind.Code, match.Groups["code"].Value);
        }

        if (match.Groups["bold"].Success)
        {
            return new(MarkdownInlineKind.Bold, match.Groups["bold"].Value);
        }

        if (match.Groups["strike"].Success)
        {
            return new(MarkdownInlineKind.Strike, match.Groups["strike"].Value);
        }

        if (match.Groups["italic"].Success)
        {
            return new(MarkdownInlineKind.Italic, match.Groups["italic"].Value);
        }

        return new(MarkdownInlineKind.Link, match.Groups["label"].Value, match.Groups["url"].Value);
    }
}
