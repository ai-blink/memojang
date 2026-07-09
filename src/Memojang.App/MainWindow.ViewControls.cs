using System.Windows;
using System.Windows.Media;
using Memojang.App.Theme;

namespace Memojang.App;

public partial class MainWindow
{
    private const double SmallEditorFontSize = 12;
    private const double MediumEditorFontSize = 14;
    private const double LargeEditorFontSize = 18;
    private const double DefaultZoomPercent = 100;
    private const string DefaultEditorFontFamily = "Consolas";

    private double _editorBaseFontSize = MediumEditorFontSize;
    private double _zoomPercent = DefaultZoomPercent;
    private string _editorFontFamilyName = DefaultEditorFontFamily;

    private double ZoomScale => _zoomPercent / 100d;

    private void InitializeViewControls()
    {
        ZoomSlider.ValueChanged += ZoomSlider_ValueChanged;
        SetEditorFontFamily(DefaultEditorFontFamily);
        SetEditorFontSize(MediumEditorFontSize);
        SetZoom(DefaultZoomPercent);
        UpdateThemeToolbarState();
    }

    private void ToolbarFontSizeButton_Click(object sender, RoutedEventArgs e)
    {
        double requestedFontSize = sender == ToolbarSmallFontButton
            ? SmallEditorFontSize
            : sender == ToolbarLargeFontButton
                ? LargeEditorFontSize
                : MediumEditorFontSize;

        SetEditorFontSize(requestedFontSize);
        EditorTextBox.Focus();
    }

    private void EditorFontFamilyMenuItem_Click(object sender, RoutedEventArgs e)
    {
        string requestedFontFamily = sender == FontCascadiaMenuItem
            ? "Cascadia Mono"
            : sender == FontSegoeMenuItem
                ? "Segoe UI"
                : DefaultEditorFontFamily;

        SetEditorFontFamily(requestedFontFamily);
        EditorTextBox.Focus();
    }

    private void EditorFontSizeMenuItem_Click(object sender, RoutedEventArgs e)
    {
        double requestedFontSize = sender == FontSizeSmallMenuItem
            ? SmallEditorFontSize
            : sender == FontSizeLargeMenuItem
                ? LargeEditorFontSize
                : MediumEditorFontSize;

        SetEditorFontSize(requestedFontSize);
        EditorTextBox.Focus();
    }

    private void SetEditorFontFamily(string fontFamilyName)
    {
        _editorFontFamilyName = fontFamilyName;
        EditorTextBox.FontFamily = new FontFamily(fontFamilyName);
        UpdateFontFamilyMenuState();
    }

    private void SetEditorFontSize(double fontSize)
    {
        _editorBaseFontSize = fontSize;
        UpdateFontSizeButtons();
        ApplyEditorDisplayScale();
    }

    private void UpdateFontSizeButtons()
    {
        ToolbarSmallFontButton.IsChecked = AreClose(_editorBaseFontSize, SmallEditorFontSize);
        ToolbarMediumFontButton.IsChecked = AreClose(_editorBaseFontSize, MediumEditorFontSize);
        ToolbarLargeFontButton.IsChecked = AreClose(_editorBaseFontSize, LargeEditorFontSize);
        FontSizeSmallMenuItem.IsChecked = AreClose(_editorBaseFontSize, SmallEditorFontSize);
        FontSizeMediumMenuItem.IsChecked = AreClose(_editorBaseFontSize, MediumEditorFontSize);
        FontSizeLargeMenuItem.IsChecked = AreClose(_editorBaseFontSize, LargeEditorFontSize);
    }

    private void UpdateFontFamilyMenuState()
    {
        FontConsolasMenuItem.IsChecked = _editorFontFamilyName == DefaultEditorFontFamily;
        FontCascadiaMenuItem.IsChecked = _editorFontFamilyName == "Cascadia Mono";
        FontSegoeMenuItem.IsChecked = _editorFontFamilyName == "Segoe UI";
    }

    private void ZoomSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
    {
        SetZoom(e.NewValue);
    }

    private void SetZoom(double zoomPercent)
    {
        _zoomPercent = Math.Clamp(zoomPercent, ZoomSlider.Minimum, ZoomSlider.Maximum);
        ZoomStatusText.Text = $"{_zoomPercent:0}%";

        if (!AreClose(ZoomSlider.Value, _zoomPercent))
        {
            ZoomSlider.Value = _zoomPercent;
        }

        ApplyEditorDisplayScale();
        ApplyPreviewDisplayScale();
    }

    private void ZoomInMenuItem_Click(object sender, RoutedEventArgs e)
    {
        AdjustZoom(10);
    }

    private void ZoomOutMenuItem_Click(object sender, RoutedEventArgs e)
    {
        AdjustZoom(-10);
    }

    private void ResetZoomMenuItem_Click(object sender, RoutedEventArgs e)
    {
        ResetZoom();
    }

    private void AdjustZoom(double deltaPercent)
    {
        SetZoom(_zoomPercent + deltaPercent);
    }

    private void ResetZoom()
    {
        SetZoom(DefaultZoomPercent);
    }

    private void ApplyEditorDisplayScale()
    {
        EditorTextBox.FontSize = _editorBaseFontSize * ZoomScale;
    }

    private void ApplyPreviewDisplayScale()
    {
        PreviewViewer.LayoutTransform = new ScaleTransform(ZoomScale, ZoomScale);
    }

    private void ToolbarDarkModeButton_Click(object sender, RoutedEventArgs e)
    {
        AppTheme requestedTheme = ToolbarDarkModeButton.IsChecked == true
            ? AppTheme.Dark
            : AppTheme.Light;

        ApplyTheme(requestedTheme);
        EditorTextBox.Focus();
    }

    private void ApplyTheme(AppTheme requestedTheme)
    {
        AppThemeService.Apply(Application.Current, requestedTheme);
        UpdateThemeMenuState(requestedTheme);
        UpdateThemeToolbarState();
        SchedulePreviewUpdate();
    }

    private void UpdateThemeToolbarState()
    {
        ToolbarDarkModeButton.IsChecked = AppThemeService.IsDarkThemeActive;
    }

    private static bool AreClose(double left, double right)
    {
        return Math.Abs(left - right) < 0.01;
    }
}
