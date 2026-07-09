using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using Memojang.App.Localization;
using Memojang.App.Theme;

namespace Memojang.App;

public partial class MainWindow
{
    private const double SmallEditorFontSize = 12;
    private const double MediumEditorFontSize = 14;
    private const double LargeEditorFontSize = 18;
    private const double MinEditorFontSize = 8;
    private const double MaxEditorFontSize = 72;
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
        double delta = sender == ToolbarSmallFontButton ? -1 : 1;
        SetEditorFontSize(_editorBaseFontSize + delta);
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

    private void FontSizeCustomMenuItem_Click(object sender, RoutedEventArgs e)
    {
        ShowFontSizeDialog();
    }

    private void ToolbarFontSizeTextBox_KeyDown(object sender, KeyEventArgs e)
    {
        if (e.Key == Key.Enter)
        {
            ApplyToolbarFontSizeInput();
            e.Handled = true;
        }
        else if (e.Key == Key.Escape)
        {
            UpdateFontSizeTextBox();
            EditorTextBox.Focus();
            e.Handled = true;
        }
    }

    private void ToolbarFontSizeTextBox_LostFocus(object sender, RoutedEventArgs e)
    {
        ApplyToolbarFontSizeInput();
    }

    private void SetEditorFontFamily(string fontFamilyName)
    {
        _editorFontFamilyName = fontFamilyName;
        EditorTextBox.FontFamily = new FontFamily(fontFamilyName);
        UpdateFontFamilyMenuState();
    }

    private void SetEditorFontSize(double fontSize)
    {
        _editorBaseFontSize = Math.Clamp(fontSize, MinEditorFontSize, MaxEditorFontSize);
        UpdateFontSizeButtons();
        UpdateFontSizeTextBox();
        ApplyEditorDisplayScale();
    }

    private void UpdateFontSizeButtons()
    {
        FontSizeSmallMenuItem.IsChecked = AreClose(_editorBaseFontSize, SmallEditorFontSize);
        FontSizeMediumMenuItem.IsChecked = AreClose(_editorBaseFontSize, MediumEditorFontSize);
        FontSizeLargeMenuItem.IsChecked = AreClose(_editorBaseFontSize, LargeEditorFontSize);
    }

    private void UpdateFontSizeTextBox()
    {
        ToolbarFontSizeTextBox.Text = _editorBaseFontSize.ToString("0.#", CultureInfo.CurrentCulture);
    }

    private void ApplyToolbarFontSizeInput()
    {
        if (TryApplyFontSizeText(ToolbarFontSizeTextBox.Text))
        {
            EditorTextBox.Focus();
            return;
        }

        UpdateFontSizeTextBox();
    }

    private bool TryApplyFontSizeText(string text)
    {
        if (!double.TryParse(text, NumberStyles.Float, CultureInfo.CurrentCulture, out double fontSize))
        {
            return false;
        }

        if (fontSize < MinEditorFontSize || fontSize > MaxEditorFontSize)
        {
            return false;
        }

        SetEditorFontSize(fontSize);
        return true;
    }

    private void ShowFontSizeDialog()
    {
        var fontSizeBox = new TextBox
        {
            Width = 160,
            Text = _editorBaseFontSize.ToString("0.#", CultureInfo.CurrentCulture),
            Margin = new Thickness(0, 6, 0, 12),
            HorizontalContentAlignment = HorizontalAlignment.Right
        };

        var dialog = new Window
        {
            Title = UiText.Get("DialogFontSizeTitle"),
            Owner = this,
            WindowStartupLocation = WindowStartupLocation.CenterOwner,
            SizeToContent = SizeToContent.WidthAndHeight,
            ResizeMode = ResizeMode.NoResize
        };

        var panel = new StackPanel();
        panel.Children.Add(new TextBlock
        {
            Text = UiText.Format("FontSizeNumberLabel", MinEditorFontSize, MaxEditorFontSize),
            Margin = new Thickness(0, 0, 0, 2)
        });
        panel.Children.Add(fontSizeBox);

        var buttons = new StackPanel
        {
            Orientation = Orientation.Horizontal,
            HorizontalAlignment = HorizontalAlignment.Right
        };
        var okButton = new Button
        {
            Content = UiText.Get("OkButton"),
            IsDefault = true,
            MinWidth = 72,
            Margin = new Thickness(0, 0, 8, 0)
        };
        var cancelButton = new Button
        {
            Content = UiText.Get("CloseButton"),
            IsCancel = true,
            MinWidth = 72
        };

        okButton.Click += (_, _) =>
        {
            if (!TryApplyFontSizeText(fontSizeBox.Text))
            {
                MessageBox.Show(this, UiText.Format("MessageInvalidFontSize", MinEditorFontSize, MaxEditorFontSize), "Memojang", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            dialog.DialogResult = true;
        };

        buttons.Children.Add(okButton);
        buttons.Children.Add(cancelButton);
        panel.Children.Add(buttons);
        dialog.Content = new Border
        {
            Padding = new Thickness(16),
            Child = panel
        };
        dialog.Loaded += (_, _) =>
        {
            fontSizeBox.SelectAll();
            fontSizeBox.Focus();
        };

        dialog.ShowDialog();
        EditorTextBox.Focus();
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
