using System.Windows;
using System.Windows.Automation;
using Memojang.App.Localization;
using Memojang.App.Theme;

namespace Memojang.App;

public partial class MainWindow
{
    private void ApplyLocalizedText()
    {
        FileMenuItem.Header = UiText.Get("MenuFile");
        RecentMenuItem.Header = UiText.Get("MenuRecent");
        NewTabMenuItem.Header = UiText.Get("MenuNewTab");
        NewTextMenuItem.Header = UiText.Get("MenuNewText");
        NewMarkdownMenuItem.Header = UiText.Get("MenuNewMarkdown");
        OpenMenuItem.Header = UiText.Get("MenuOpen");
        SaveMenuItem.Header = UiText.Get("MenuSave");
        SaveAsMenuItem.Header = UiText.Get("MenuSaveAs");
        SaveAllMenuItem.Header = UiText.Get("MenuSaveAll");
        ExitMenuItem.Header = UiText.Get("MenuExit");
        EditMenuItem.Header = UiText.Get("MenuEdit");
        UndoMenuItem.Header = UiText.Get("MenuUndo");
        CutMenuItem.Header = UiText.Get("MenuCut");
        CopyMenuItem.Header = UiText.Get("MenuCopy");
        PasteMenuItem.Header = UiText.Get("MenuPaste");
        FindMenuItem.Header = UiText.Get("MenuFind");
        FindNextMenuItem.Header = UiText.Get("MenuFindNext");
        FindPreviousMenuItem.Header = UiText.Get("MenuFindPrevious");
        SelectAllMenuItem.Header = UiText.Get("MenuSelectAll");
        GoToMenuItem.Header = UiText.Get("MenuGoTo");
        TimeDateMenuItem.Header = UiText.Get("MenuTimeDate");
        SettingsMenuItem.Header = UiText.Get("MenuSettings");
        FontMenuItem.Header = UiText.Get("MenuFont");
        FontSizeMenuItem.Header = UiText.Get("MenuFontSize");
        FontSizeSmallMenuItem.Header = UiText.Get("FontSizeSmall");
        FontSizeMediumMenuItem.Header = UiText.Get("FontSizeMedium");
        FontSizeLargeMenuItem.Header = UiText.Get("FontSizeLarge");
        FontSizeCustomMenuItem.Header = UiText.Get("FontSizeCustom");
        SessionRestoreMenuItem.Header = UiText.Get("MenuSessionRestore");
        ViewMenuItem.Header = UiText.Get("MenuView");
        WordWrapMenuItem.Header = UiText.Get("MenuWordWrap");
        StatusBarMenuItem.Header = UiText.Get("MenuStatusBar");
        ZoomMenuItem.Header = UiText.Get("MenuZoom");
        ZoomInMenuItem.Header = UiText.Get("MenuZoomIn");
        ZoomOutMenuItem.Header = UiText.Get("MenuZoomOut");
        ResetZoomMenuItem.Header = UiText.Get("MenuResetZoom");
        PreviewMenuItem.Header = UiText.Get("MenuMarkdownPreview");
        ThemeMenuItem.Header = UiText.Get("MenuAppTheme");
        ThemeSystemMenuItem.Header = UiText.Get("ThemeSystem");
        ThemeLightMenuItem.Header = UiText.Get("ThemeLight");
        ThemeDarkMenuItem.Header = UiText.Get("ThemeDark");
        MarkdownMenuItem.Header = UiText.Get("MenuMarkdown");
        InsertTableMenuItem.Header = UiText.Get("MenuInsertTable");
        AddTableRowMenuItem.Header = UiText.Get("MenuAddTableRow");
        InsertImageMenuItem.Header = UiText.Get("MenuInsertImage");
        PrivacyMenuItem.Header = UiText.Get("MenuPrivacy");
        CaptureExclusionMenuItem.Header = UiText.Get("MenuCaptureExclusion");
        HelpMenuItem.Header = UiText.Get("MenuHelp");
        AboutMenuItem.Header = UiText.Get("MenuAbout");

        FindLabelText.Text = UiText.Get("FindLabel");
        ReplaceLabelText.Text = UiText.Get("ReplaceLabel");
        FindPreviousButton.Content = UiText.Get("FindPreviousButton");
        FindNextButton.Content = UiText.Get("FindNextButton");
        ReplaceButton.Content = UiText.Get("ReplaceButton");
        ReplaceAllButton.Content = UiText.Get("ReplaceAllButton");
        CloseFindButton.Content = UiText.Get("CloseButton");
        ToolbarImageButton.Content = UiText.Get("ToolbarImage");
        ToolbarPreviewButton.Content = UiText.Get("ToolbarPreview");
        ToolbarDarkModeButton.Content = UiText.Get("ToolbarDarkMode");
        ToolbarPrivacyButton.Content = UiText.Get("ToolbarPrivacy");
        // Glyph buttons ("H1", "B", ...) need a spoken name; WPF would otherwise read the glyph itself.
        Describe(ToolbarHeadingButton, "ToolbarHeading");
        Describe(ToolbarListButton, "ToolbarList");
        Describe(ToolbarBoldButton, "ToolbarBold");
        Describe(ToolbarItalicButton, "ToolbarItalic");
        Describe(ToolbarStrikeButton, "ToolbarStrike");
        Describe(ToolbarLinkButton, "ToolbarLink");
        Describe(ToolbarTableButton, "ToolbarTable");
        Describe(ToolbarImageButton, "ToolbarImage");
        Describe(ToolbarPreviewButton, "ToolbarPreview");
        Describe(ToolbarSmallFontButton, "ToolbarFontSmall");
        Describe(ToolbarLargeFontButton, "ToolbarFontLarge");
        Describe(ToolbarFontSizeTextBox, "ToolbarFontSizeNumber");
        Describe(ToolbarDarkModeButton, "ToolbarDarkMode");
        Describe(ToolbarPrivacyButton, "ToolbarPrivacy");
        Describe(NewDocumentButton, "WindowNewDocument");
        Describe(MinimizeWindowButton, "WindowMinimize");
        Describe(MaximizeRestoreButton, "WindowMaximizeRestore");
        Describe(CloseWindowButton, "CloseButton");
        ZoomLabelText.Text = UiText.Get("StatusZoom");
        PrivacyStatusText.Text = UiText.Get("PrivacyStatusVisible");

        UpdateThemeMenuState(AppThemeService.RequestedTheme);
        UpdateRecentFilesMenu();
    }

    private static void Describe(FrameworkElement element, string resourceKey)
    {
        string text = UiText.Get(resourceKey);
        element.ToolTip = text;
        AutomationProperties.SetName(element, text);
    }
}
