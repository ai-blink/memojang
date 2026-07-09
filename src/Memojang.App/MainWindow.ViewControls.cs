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

    private double _editorBaseFontSize = MediumEditorFontSize;
    private double _zoomPercent = DefaultZoomPercent;

    private double ZoomScale => _zoomPercent / 100d;

    private void InitializeViewControls()
    {
        ZoomSlider.ValueChanged += ZoomSlider_ValueChanged;
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
