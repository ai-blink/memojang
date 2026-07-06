using System.Windows;
using System.Windows.Input;
using Memojang.App.Localization;
using Memojang.App.Shell;

namespace Memojang.App;

public partial class MainWindow
{
    private void FindMenuItem_Click(object sender, RoutedEventArgs e)
    {
        ShowFindPanel();
    }

    private void FindNextMenuItem_Click(object sender, RoutedEventArgs e)
    {
        FindNext();
    }

    private void FindNextButton_Click(object sender, RoutedEventArgs e)
    {
        FindNext();
    }

    private void ReplaceButton_Click(object sender, RoutedEventArgs e)
    {
        ReplaceCurrent();
    }

    private void ReplaceAllButton_Click(object sender, RoutedEventArgs e)
    {
        ReplaceAll();
    }

    private void CloseFindButton_Click(object sender, RoutedEventArgs e)
    {
        FindPanel.Visibility = Visibility.Collapsed;
        EditorTextBox.Focus();
    }

    private void FindTextBox_KeyDown(object sender, KeyEventArgs e)
    {
        if (e.Key == Key.Enter)
        {
            FindNext();
            e.Handled = true;
        }
        else if (e.Key == Key.Escape)
        {
            FindPanel.Visibility = Visibility.Collapsed;
            EditorTextBox.Focus();
            e.Handled = true;
        }
    }

    private void ShowFindPanel()
    {
        FindPanel.Visibility = Visibility.Visible;

        if (!string.IsNullOrEmpty(EditorTextBox.SelectedText))
        {
            FindTextBox.Text = EditorTextBox.SelectedText;
        }

        FindTextBox.SelectAll();
        FindTextBox.Focus();
    }

    private void FindNext()
    {
        string query = FindTextBox.Text;
        int startIndex = EditorTextBox.SelectionStart + Math.Max(EditorTextBox.SelectionLength, 1);
        int matchIndex = TextSearch.FindNext(EditorTextBox.Text, query, startIndex, StringComparison.CurrentCultureIgnoreCase);

        if (matchIndex < 0)
        {
            FindStatusText.Text = string.IsNullOrEmpty(query) ? string.Empty : UiText.Get("FindNoMatches");
            return;
        }

        EditorTextBox.Focus();
        EditorTextBox.Select(matchIndex, query.Length);
        FindStatusText.Text = string.Empty;
    }

    private void ReplaceCurrent()
    {
        string query = FindTextBox.Text;
        if (string.IsNullOrEmpty(query))
        {
            return;
        }

        if (!EditorTextBox.SelectedText.Equals(query, StringComparison.CurrentCultureIgnoreCase))
        {
            FindNext();
            return;
        }

        int selectionStart = EditorTextBox.SelectionStart;
        EditorTextBox.SelectedText = ReplaceTextBox.Text;
        EditorTextBox.CaretIndex = selectionStart + ReplaceTextBox.Text.Length;
        FindNext();
    }

    private void ReplaceAll()
    {
        var result = TextSearch.ReplaceAll(
            EditorTextBox.Text,
            FindTextBox.Text,
            ReplaceTextBox.Text,
            StringComparison.CurrentCultureIgnoreCase);

        if (result.Count == 0)
        {
            FindStatusText.Text = string.IsNullOrEmpty(FindTextBox.Text) ? string.Empty : UiText.Get("FindNoMatches");
            return;
        }

        EditorTextBox.Text = result.Text;
        EditorTextBox.CaretIndex = 0;
        FindStatusText.Text = UiText.Format("FindReplacedCount", result.Count);
        EditorTextBox.Focus();
    }
}
