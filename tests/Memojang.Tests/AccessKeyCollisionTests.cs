namespace Memojang.Tests;

using System.Globalization;
using System.Resources;

// Sibling menu items must not share an access key (Alt+letter), or WPF cycles between them instead of invoking.
// Groups mirror MainWindow.xaml, the editor context menu, and the key → header mapping in MainWindow.Localization.cs.
public class AccessKeyCollisionTests
{
    private static readonly ResourceManager Strings =
        new("Memojang.App.Resources.Strings", typeof(Memojang.App.App).Assembly);

    private static readonly Dictionary<string, string[]> MenuGroups = new()
    {
        ["MenuBar"] = ["MenuFile", "MenuEdit", "MenuView", "MenuMarkdown", "MenuPrivacy", "MenuHelp"],
        ["File"] = ["MenuRecent", "MenuNewTab", "MenuNewText", "MenuNewMarkdown", "MenuOpen", "MenuSave", "MenuSaveAs", "MenuSaveAll", "MenuExit"],
        ["Edit"] = ["MenuUndo", "MenuCut", "MenuCopy", "MenuPaste", "MenuFind", "MenuFindNext", "MenuFindPrevious", "MenuSelectAll", "MenuGoTo", "MenuTimeDate", "MenuSettings"],
        ["Settings"] = ["MenuFont", "MenuFontSize", "MenuSessionRestore"],
        ["FontSize"] = ["FontSizeSmall", "FontSizeMedium", "FontSizeLarge", "FontSizeCustom"],
        ["View"] = ["MenuWordWrap", "MenuStatusBar", "MenuZoom", "MenuMarkdownPreview", "MenuAppTheme"],
        ["Zoom"] = ["MenuZoomIn", "MenuZoomOut", "MenuResetZoom"],
        ["Theme"] = ["ThemeSystem", "ThemeLight", "ThemeDark"],
        ["Markdown"] = ["MenuInsertTable", "MenuAddTableRow", "MenuInsertImage"],
        ["EditorContext"] = ["MenuCut", "MenuCopy", "MenuPaste", "MenuSelectAll", "ContextTable"]
    };

    public static TheoryData<string> Cultures => new() { "", "ar", "bn", "es", "fr", "hi", "id", "ja", "ko", "pt", "ru", "zh" };

    [Theory]
    [MemberData(nameof(Cultures))]
    public void SiblingMenuItems_HaveDistinctAccessKeys(string cultureName)
    {
        CultureInfo culture = CultureInfo.GetCultureInfo(cultureName);
        var collisions = new List<string>();

        foreach ((string group, string[] keys) in MenuGroups)
        {
            IEnumerable<IGrouping<char, string>> duplicates = keys
                .Select(key => (Key: key, AccessKey: AccessKeyOf(Strings.GetString(key, culture) ?? string.Empty)))
                .Where(item => item.AccessKey is not null)
                .GroupBy(item => char.ToUpperInvariant(item.AccessKey!.Value), item => item.Key)
                .Where(grouping => grouping.Count() > 1);

            collisions.AddRange(duplicates.Select(d => $"{group}: '{d.Key}' used by {string.Join(", ", d)}"));
        }

        Assert.True(collisions.Count == 0, $"[{cultureName}] " + string.Join(" | ", collisions));
    }

    private static char? AccessKeyOf(string header)
    {
        for (int i = 0; i < header.Length - 1; i++)
        {
            if (header[i] == '_')
            {
                if (header[i + 1] == '_')
                {
                    i++;
                    continue;
                }

                return header[i + 1];
            }
        }

        return null;
    }
}
