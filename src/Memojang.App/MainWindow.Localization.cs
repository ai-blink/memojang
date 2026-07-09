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
        ToolbarSmallFontButton.ToolTip = UiText.Get("ToolbarFontSmall");
        ToolbarMediumFontButton.ToolTip = UiText.Get("ToolbarFontMedium");
        ToolbarLargeFontButton.ToolTip = UiText.Get("ToolbarFontLarge");
        ToolbarDarkModeButton.ToolTip = UiText.Get("ToolbarDarkMode");
        ZoomLabelText.Text = UiText.Get("StatusZoom");
        PrivacyStatusText.Text = UiText.Get("PrivacyStatusVisible");

        UpdateThemeMenuState(AppThemeService.RequestedTheme);
        UpdateRecentFilesMenu();
    }
}
