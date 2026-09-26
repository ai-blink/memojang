using System.Windows;
using System.Windows.Interop;
using Memojang.App.WindowsInterop;

namespace Memojang.App;

public partial class MainWindow
{
    private void CaptureExclusionMenuItem_Click(object sender, RoutedEventArgs e)
    {
        _captureExclusionRequested = CaptureExclusionMenuItem.IsChecked;
        ToolbarPrivacyButton.IsChecked = CaptureExclusionMenuItem.IsChecked;
        SavePersistentSettings();
        ApplyCaptureExclusion();
    }

    private void ToolbarPrivacyButton_Click(object sender, RoutedEventArgs e)
    {
        CaptureExclusionMenuItem.IsChecked = ToolbarPrivacyButton.IsChecked == true;
        CaptureExclusionMenuItem_Click(sender, e);
    }

    private void ApplyCaptureExclusion()
    {
        var helper = new WindowInteropHelper(this);
        CaptureExclusionResult result = CaptureExclusion.Apply(helper.Handle, _captureExclusionRequested);

        PrivacyStatusText.Text = result.StatusText;
        PrivacyStatusText.ToolTip = result.DetailText;
    }
}
