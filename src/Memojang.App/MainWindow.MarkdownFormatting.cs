using System.Windows;

namespace Memojang.App;

public partial class MainWindow
{
    private void ToolbarHeadingButton_Click(object sender, RoutedEventArgs e)
    {
        InsertLinePrefix("# ");
    }

    private void ToolbarListButton_Click(object sender, RoutedEventArgs e)
    {
        InsertLinePrefix("- ");
    }

    private void ToolbarBoldButton_Click(object sender, RoutedEventArgs e)
    {
        WrapSelection("**", "**", "bold");
    }

    private void ToolbarItalicButton_Click(object sender, RoutedEventArgs e)
    {
        WrapSelection("*", "*", "italic");
    }

    private void ToolbarStrikeButton_Click(object sender, RoutedEventArgs e)
    {
        WrapSelection("~~", "~~", "strike");
    }

    private void ToolbarLinkButton_Click(object sender, RoutedEventArgs e)
    {
        int start = EditorTextBox.SelectionStart;
        string label = string.IsNullOrEmpty(EditorTextBox.SelectedText) ? "link text" : EditorTextBox.SelectedText;
        string replacement = $"[{label}](https://)";

        EditorTextBox.SelectedText = replacement;
        int urlStart = start + label.Length + 3;
        EditorTextBox.Focus();
        EditorTextBox.Select(urlStart, "https://".Length);
    }

    private void InsertLinePrefix(string prefix)
    {
        string text = EditorTextBox.Text;
        int caretIndex = EditorTextBox.CaretIndex;
        int lineStart = GetLineStartIndex(text, caretIndex);

        if (text[lineStart..].StartsWith(prefix, StringComparison.Ordinal))
        {
            EditorTextBox.CaretIndex = caretIndex;
            EditorTextBox.Focus();
            return;
        }

        EditorTextBox.Text = text.Insert(lineStart, prefix);
        EditorTextBox.CaretIndex = caretIndex + prefix.Length;
        EditorTextBox.Focus();
    }

    private void WrapSelection(string prefix, string suffix, string placeholder)
    {
        int start = EditorTextBox.SelectionStart;
        int selectedLength = EditorTextBox.SelectionLength;
        string selectedText = selectedLength == 0 ? placeholder : EditorTextBox.SelectedText;

        EditorTextBox.SelectedText = $"{prefix}{selectedText}{suffix}";
        EditorTextBox.Focus();

        if (selectedLength == 0)
        {
            EditorTextBox.Select(start + prefix.Length, selectedText.Length);
        }
        else
        {
            EditorTextBox.CaretIndex = start + prefix.Length + selectedText.Length + suffix.Length;
        }
    }

    private static int GetLineStartIndex(string text, int caretIndex)
    {
        int searchStart = Math.Min(caretIndex, text.Length) - 1;
        for (int i = searchStart; i >= 0; i--)
        {
            if (text[i] is '\r' or '\n')
            {
                return i + 1;
            }
        }

        return 0;
    }
}
