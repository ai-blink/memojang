using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Automation;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using Memojang.App.Localization;
using Memojang.App.Theme;
using Memojang.App.WindowsInterop;

namespace Memojang.App;

// Modal settings dialog. Every control applies to the owner immediately (no OK/Cancel), and the owner persists
// the result once the dialog closes, so this window never touches the settings file itself.
public partial class SettingsWindow : Window
{
    private static readonly double[] FontSizePresets = [8, 9, 10, 11, 12, 13, 14, 16, 18, 20, 22, 24, 28, 32, 36, 48, 72];
    private const int DwmUseImmersiveDarkMode = 20;

    private readonly MainWindow _owner;
    private bool _syncing = true;

    public SettingsWindow(MainWindow owner)
    {
        _owner = owner;
        Owner = owner;
        InitializeComponent();
        FlowDirection = UiText.FlowDirection;
        ApplyLocalizedText();
        LoadFromOwner();
        _syncing = false;
        GeneralPageButton.IsChecked = true;
    }

    protected override void OnSourceInitialized(EventArgs e)
    {
        base.OnSourceInitialized(e);
        ApplyWindowChromeState();
    }

    private void ApplyLocalizedText()
    {
        Title = UiText.Get("SettingsTitle");
        SidebarSubtitleText.Text = UiText.Get("SettingsTitle");
        GeneralPageButton.Content = UiText.Get("SettingsNavGeneral");
        EditorPageButton.Content = UiText.Get("SettingsNavEditor");
        AppearancePageButton.Content = UiText.Get("SettingsNavAppearance");
        AboutPageButton.Content = UiText.Get("SettingsNavAbout");

        GeneralTitleText.Text = UiText.Get("SettingsNavGeneral");
        GeneralSubtitleText.Text = UiText.Get("SettingsGeneralSubtitle");
        Label(SessionRestoreLabel, SessionRestoreToggle, "SettingsSessionRestore");
        SessionRestoreHint.Text = UiText.Get("SettingsSessionRestoreHint");
        Label(CaptureExclusionLabel, CaptureExclusionToggle, "SettingsCaptureExclusion");
        CaptureExclusionHint.Text = UiText.Get("SettingsCaptureExclusionHint");

        EditorTitleText.Text = UiText.Get("SettingsNavEditor");
        EditorSubtitleText.Text = UiText.Get("SettingsEditorSubtitle");
        Label(FontFamilyLabel, FontFamilyBox, "SettingsFontFamily");
        Label(FontSizeLabel, FontSizeBox, "SettingsFontSize");
        FontSizeHint.Text = UiText.Format("SettingsFontSizeHint", FontSizePresets[0], FontSizePresets[^1]);
        Label(WordWrapLabel, WordWrapToggle, "SettingsWordWrap");

        AppearanceTitleText.Text = UiText.Get("SettingsNavAppearance");
        AppearanceSubtitleText.Text = UiText.Get("SettingsAppearanceSubtitle");
        ThemeLabel.Text = UiText.Get("SettingsTheme");
        ThemeSystemChoice.Content = UiText.Get("SettingsThemeSystem");
        ThemeLightChoice.Content = UiText.Get("SettingsThemeLight");
        ThemeDarkChoice.Content = UiText.Get("SettingsThemeDark");
        Label(StatusBarLabel, StatusBarToggle, "SettingsStatusBar");

        AboutSubtitleText.Text = UiText.Get("SettingsAboutSubtitle");
        VersionText.Text = UiText.Format("SettingsVersion", GetDisplayVersion());
        OpenDataFolderButton.Content = UiText.Get("SettingsOpenDataFolder");
        CloseSettingsButton.Content = UiText.Get("CloseButton");
    }

    // Switches and pickers have no visible caption of their own; give screen readers the row label.
    private static void Label(TextBlock label, FrameworkElement control, string resourceKey)
    {
        label.Text = UiText.Get(resourceKey);
        AutomationProperties.SetName(control, label.Text);
    }

    private void LoadFromOwner()
    {
        SessionRestoreToggle.IsChecked = _owner.IsSessionRestoreEnabled;
        CaptureExclusionToggle.IsChecked = _owner.IsCaptureExclusionRequested;
        WordWrapToggle.IsChecked = _owner.IsWordWrapEnabled;
        StatusBarToggle.IsChecked = _owner.IsStatusBarVisible;

        List<string> fontNames = Fonts.SystemFontFamilies
            .Select(family => family.Source)
            .Append(_owner.EditorFontFamilyName)
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .OrderBy(name => name, StringComparer.CurrentCultureIgnoreCase)
            .ToList();
        FontFamilyBox.ItemsSource = fontNames;
        FontFamilyBox.SelectedItem = fontNames.First(name => name.Equals(_owner.EditorFontFamilyName, StringComparison.OrdinalIgnoreCase));

        // The toolbar can step to sizes outside the presets; keep the current one selectable.
        List<double> sizes = FontSizePresets.Append(_owner.EditorBaseFontSize).Distinct().Order().ToList();
        FontSizeBox.ItemsSource = sizes.Select(FormatSize).ToList();
        FontSizeBox.SelectedItem = FormatSize(_owner.EditorBaseFontSize);
        UpdateFontPreview();

        (AppThemeService.RequestedTheme switch
        {
            AppTheme.Light => ThemeLightChoice,
            AppTheme.Dark => ThemeDarkChoice,
            _ => ThemeSystemChoice
        }).IsChecked = true;
    }

    private static string FormatSize(double size) => size.ToString("0.#", CultureInfo.CurrentCulture);

    private void PageButton_Checked(object sender, RoutedEventArgs e)
    {
        GeneralPanel.Visibility = sender == GeneralPageButton ? Visibility.Visible : Visibility.Collapsed;
        EditorPanel.Visibility = sender == EditorPageButton ? Visibility.Visible : Visibility.Collapsed;
        AppearancePanel.Visibility = sender == AppearancePageButton ? Visibility.Visible : Visibility.Collapsed;
        AboutPanel.Visibility = sender == AboutPageButton ? Visibility.Visible : Visibility.Collapsed;
    }

    private void SessionRestoreToggle_Click(object sender, RoutedEventArgs e)
    {
        _owner.ChangeSessionRestore(SessionRestoreToggle.IsChecked == true);
    }

    private void CaptureExclusionToggle_Click(object sender, RoutedEventArgs e)
    {
        _owner.ChangeCaptureExclusion(CaptureExclusionToggle.IsChecked == true);
        ApplyWindowChromeState();
    }

    private void FontFamilyBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (_syncing || FontFamilyBox.SelectedItem is not string fontName)
        {
            return;
        }

        _owner.ChangeEditorFontFamily(fontName);
        UpdateFontPreview();
    }

    private void FontSizeBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (_syncing || FontSizeBox.SelectedItem is not string text
            || !double.TryParse(text, NumberStyles.Float, CultureInfo.CurrentCulture, out double size))
        {
            return;
        }

        _owner.ChangeEditorFontSize(size);
        UpdateFontPreview();
    }

    private void UpdateFontPreview()
    {
        FontPreviewText.FontFamily = new FontFamily(_owner.EditorFontFamilyName);
        FontPreviewText.FontSize = _owner.EditorBaseFontSize;
    }

    private void WordWrapToggle_Click(object sender, RoutedEventArgs e)
    {
        _owner.ChangeWordWrap(WordWrapToggle.IsChecked == true);
    }

    private void StatusBarToggle_Click(object sender, RoutedEventArgs e)
    {
        _owner.ChangeStatusBarVisible(StatusBarToggle.IsChecked == true);
    }

    private void ThemeChoice_Checked(object sender, RoutedEventArgs e)
    {
        if (_syncing)
        {
            return;
        }

        AppTheme theme = sender == ThemeDarkChoice ? AppTheme.Dark : sender == ThemeLightChoice ? AppTheme.Light : AppTheme.System;
        _owner.ChangeTheme(theme);
        ApplyWindowChromeState();
    }

    private void OpenDataFolderButton_Click(object sender, RoutedEventArgs e)
    {
        string folder = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "Memojang");
        Directory.CreateDirectory(folder);
        Process.Start(new ProcessStartInfo { FileName = folder, UseShellExecute = true });
    }

    private void CloseSettingsButton_Click(object sender, RoutedEventArgs e) => Close();

    private void SettingsWindow_PreviewKeyDown(object sender, KeyEventArgs e)
    {
        if (e.Key == Key.Escape)
        {
            Close();
            e.Handled = true;
        }
    }

    // A dialog over an excluded editor must not leak the same content, and its OS title bar should follow the theme.
    private void ApplyWindowChromeState()
    {
        nint handle = new WindowInteropHelper(this).Handle;
        if (handle == nint.Zero)
        {
            return;
        }

        CaptureExclusion.Apply(handle, _owner.IsCaptureExclusionRequested);
        int useDark = AppThemeService.IsDarkThemeActive ? 1 : 0;
        _ = DwmSetWindowAttribute(handle, DwmUseImmersiveDarkMode, ref useDark, sizeof(int));
    }

    private static string GetDisplayVersion()
    {
        Assembly assembly = Assembly.GetExecutingAssembly();
        string? informational = assembly.GetCustomAttribute<AssemblyInformationalVersionAttribute>()?.InformationalVersion;
        return !string.IsNullOrWhiteSpace(informational)
            ? informational.Split('+', 2)[0]
            : assembly.GetName().Version?.ToString(3) ?? "dev";
    }

    [DllImport("dwmapi.dll")]
    private static extern int DwmSetWindowAttribute(nint hwnd, int attribute, ref int value, int valueSize);
}
