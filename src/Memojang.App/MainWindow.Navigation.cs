using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Memojang.App.Localization;

namespace Memojang.App;

public partial class MainWindow
{
    private void GoToMenuItem_Click(object sender, RoutedEventArgs e)
    {
        ShowGoToLineDialog();
    }

    private void ShowGoToLineDialog()
    {
        int currentLine = EditorTextBox.GetLineIndexFromCharacterIndex(EditorTextBox.CaretIndex) + 1;
        var lineBox = new TextBox
        {
            Width = 220,
            Text = currentLine.ToString(CultureInfo.CurrentCulture),
            Margin = new Thickness(0, 6, 0, 12)
        };

        var dialog = new Window
        {
            Title = UiText.Get("DialogGoToTitle"),
            Owner = this,
            WindowStartupLocation = WindowStartupLocation.CenterOwner,
            SizeToContent = SizeToContent.WidthAndHeight,
            ResizeMode = ResizeMode.NoResize
        };

        var panel = new StackPanel();
        panel.Children.Add(new TextBlock
        {
            Text = UiText.Get("GoToLineLabel"),
            Margin = new Thickness(0, 0, 0, 2)
        });
        panel.Children.Add(lineBox);

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
            if (!TryReadLineNumber(lineBox.Text, out int lineNumber))
            {
                return;
            }

            GoToLineNumber(lineNumber);
            dialog.DialogResult = true;
        };

        lineBox.KeyDown += (_, keyArgs) =>
        {
            if (keyArgs.Key == Key.Enter && TryReadLineNumber(lineBox.Text, out int lineNumber))
            {
                GoToLineNumber(lineNumber);
                dialog.DialogResult = true;
                keyArgs.Handled = true;
            }
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
            lineBox.SelectAll();
            lineBox.Focus();
        };

        dialog.ShowDialog();
    }

    private bool TryReadLineNumber(string text, out int lineNumber)
    {
        int maxLine = Math.Max(1, EditorTextBox.LineCount);
        if (int.TryParse(text, NumberStyles.Integer, CultureInfo.CurrentCulture, out lineNumber)
            && lineNumber >= 1
            && lineNumber <= maxLine)
        {
            return true;
        }

        MessageBox.Show(this, UiText.Format("MessageInvalidLineNumber", maxLine), "Memojang", MessageBoxButton.OK, MessageBoxImage.Information);
        lineNumber = 0;
        return false;
    }

    private void GoToLineNumber(int lineNumber)
    {
        int lineIndex = Math.Clamp(lineNumber - 1, 0, Math.Max(0, EditorTextBox.LineCount - 1));
        int characterIndex = EditorTextBox.GetCharacterIndexFromLineIndex(lineIndex);
        EditorTextBox.Focus();
        EditorTextBox.CaretIndex = Math.Clamp(characterIndex, 0, EditorTextBox.Text.Length);
        EditorTextBox.ScrollToLine(lineIndex);
    }
}
