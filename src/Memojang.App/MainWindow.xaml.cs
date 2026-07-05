using System.IO;
using System.Windows;
using System.Windows.Interop;
using System.Windows.Input;
using Microsoft.Win32;
using Memojang.App.Shell;
using Memojang.App.WindowsInterop;
using CancelEventArgs = System.ComponentModel.CancelEventArgs;

namespace Memojang.App;

public partial class MainWindow : Window
{
    private bool _hasUnsavedChanges;
    private bool _isSettingEditorText;
    private bool _captureExclusionEnabled;
    private string? _currentFilePath;
    private string _encodingName = "UTF-8";
    private string _lineEndingName = LineEnding.Crlf;

    public MainWindow()
    {
        InitializeComponent();
        SourceInitialized += MainWindow_SourceInitialized;
        UpdateStatus(EditorStatus.Initial);
        UpdateTitle();
        EditorTextBox.Focus();
    }

    private void EditorTextBox_TextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e)
    {
        if (!_isSettingEditorText)
        {
            _hasUnsavedChanges = true;
            _lineEndingName = LineEnding.Detect(EditorTextBox.Text);
            UpdateTitle();
        }

        UpdateStatusFromEditor();
        UpdatePreview();
    }

    private void EditorTextBox_SelectionChanged(object sender, RoutedEventArgs e)
    {
        UpdateStatusFromEditor();
    }

    private void UpdateStatusFromEditor()
    {
        int characterIndex = EditorTextBox.CaretIndex;
        int lineIndex = EditorTextBox.GetLineIndexFromCharacterIndex(characterIndex);
        int lineStartIndex = EditorTextBox.GetCharacterIndexFromLineIndex(lineIndex);
        int column = characterIndex - lineStartIndex + 1;

        UpdateStatus(new EditorStatus(lineIndex + 1, column, _hasUnsavedChanges, _encodingName, _lineEndingName));
    }

    private void UpdateStatus(EditorStatus status)
    {
        CaretStatusText.Text = status.CaretText;
        ChangeStatusText.Text = status.ChangeText;
        EncodingStatusText.Text = status.EncodingName;
        LineEndingStatusText.Text = status.LineEndingName;
    }

    private void NewMenuItem_Click(object sender, RoutedEventArgs e)
    {
        if (!ConfirmDiscardUnsavedChanges())
        {
            return;
        }

        _currentFilePath = null;
        SetEditorText(string.Empty, hasUnsavedChanges: false);
    }

    private void OpenMenuItem_Click(object sender, RoutedEventArgs e)
    {
        if (!ConfirmDiscardUnsavedChanges())
        {
            return;
        }

        var dialog = new OpenFileDialog
        {
            Filter = "Text documents|*.txt;*.md;*.markdown|All files|*.*",
            Title = "Open"
        };

        if (dialog.ShowDialog(this) != true)
        {
            return;
        }

        string text = File.ReadAllText(dialog.FileName);
        _currentFilePath = dialog.FileName;
        SetEditorText(text, hasUnsavedChanges: false);
    }

    private void SaveMenuItem_Click(object sender, RoutedEventArgs e)
    {
        SaveDocument(saveAs: false);
    }

    private void SaveAsMenuItem_Click(object sender, RoutedEventArgs e)
    {
        SaveDocument(saveAs: true);
    }

    private void TimeDateMenuItem_Click(object sender, RoutedEventArgs e)
    {
        InsertTimeDate();
    }

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

    private void PreviewMenuItem_Click(object sender, RoutedEventArgs e)
    {
        bool isPreviewVisible = PreviewMenuItem.IsChecked;
        PreviewColumn.Width = isPreviewVisible ? new GridLength(1, GridUnitType.Star) : new GridLength(0);
        PreviewPane.Visibility = isPreviewVisible ? Visibility.Visible : Visibility.Collapsed;
        UpdatePreview();
    }

    private void InsertTableMenuItem_Click(object sender, RoutedEventArgs e)
    {
        int caretIndex = EditorTextBox.CaretIndex;
        EditorTextBox.Text = MarkdownTableHelper.InsertStarterTable(EditorTextBox.Text, caretIndex, GetLineEndingValue());
        EditorTextBox.CaretIndex = Math.Min(EditorTextBox.Text.Length, caretIndex);
        EditorTextBox.Focus();
    }

    private void AddTableRowMenuItem_Click(object sender, RoutedEventArgs e)
    {
        if (!MarkdownTableHelper.TryAddRow(EditorTextBox.Text, EditorTextBox.CaretIndex, GetLineEndingValue(), out string updatedText))
        {
            MessageBox.Show(this, "Place the cursor inside a Markdown table first.", "Memojang", MessageBoxButton.OK, MessageBoxImage.Information);
            return;
        }

        int caretIndex = EditorTextBox.CaretIndex;
        EditorTextBox.Text = updatedText;
        EditorTextBox.CaretIndex = Math.Min(EditorTextBox.Text.Length, caretIndex);
        EditorTextBox.Focus();
    }

    private void CaptureExclusionMenuItem_Click(object sender, RoutedEventArgs e)
    {
        _captureExclusionEnabled = CaptureExclusionMenuItem.IsChecked;
        ApplyCaptureExclusion();
    }

    private void ExitMenuItem_Click(object sender, RoutedEventArgs e)
    {
        Close();
    }

    private void MainWindow_Closing(object? sender, CancelEventArgs e)
    {
        e.Cancel = !ConfirmDiscardUnsavedChanges();
    }

    private void MainWindow_PreviewKeyDown(object sender, KeyEventArgs e)
    {
        bool isControl = Keyboard.Modifiers == ModifierKeys.Control;
        bool isControlShift = Keyboard.Modifiers == (ModifierKeys.Control | ModifierKeys.Shift);

        if (isControl && e.Key == Key.N)
        {
            NewMenuItem_Click(sender, e);
            e.Handled = true;
        }
        else if (isControl && e.Key == Key.O)
        {
            OpenMenuItem_Click(sender, e);
            e.Handled = true;
        }
        else if (isControl && e.Key == Key.S)
        {
            SaveMenuItem_Click(sender, e);
            e.Handled = true;
        }
        else if (isControlShift && e.Key == Key.S)
        {
            SaveAsMenuItem_Click(sender, e);
            e.Handled = true;
        }
        else if (e.Key == Key.F5)
        {
            InsertTimeDate();
            e.Handled = true;
        }
        else if (isControl && e.Key == Key.F)
        {
            ShowFindPanel();
            e.Handled = true;
        }
        else if (e.Key == Key.F3)
        {
            FindNext();
            e.Handled = true;
        }
    }

    private void MainWindow_SourceInitialized(object? sender, EventArgs e)
    {
        ApplyCaptureExclusion();
    }

    private bool SaveDocument(bool saveAs)
    {
        string? targetPath = _currentFilePath;

        if (saveAs || string.IsNullOrWhiteSpace(targetPath))
        {
            var dialog = new SaveFileDialog
            {
                Filter = "Text documents|*.txt;*.md;*.markdown|All files|*.*",
                FileName = string.IsNullOrWhiteSpace(targetPath) ? "Untitled.txt" : Path.GetFileName(targetPath),
                Title = "Save"
            };

            if (dialog.ShowDialog(this) != true)
            {
                return false;
            }

            targetPath = dialog.FileName;
        }

        File.WriteAllText(targetPath, EditorTextBox.Text, new System.Text.UTF8Encoding(encoderShouldEmitUTF8Identifier: false));
        _currentFilePath = targetPath;
        _hasUnsavedChanges = false;
        _encodingName = "UTF-8";
        _lineEndingName = LineEnding.Detect(EditorTextBox.Text);
        UpdateTitle();
        UpdateStatusFromEditor();
        return true;
    }

    private void SetEditorText(string text, bool hasUnsavedChanges)
    {
        _isSettingEditorText = true;
        EditorTextBox.Text = text;
        EditorTextBox.CaretIndex = 0;
        _isSettingEditorText = false;

        _hasUnsavedChanges = hasUnsavedChanges;
        _encodingName = "UTF-8";
        _lineEndingName = LineEnding.Detect(text);
        UpdateTitle();
        UpdateStatusFromEditor();
        UpdatePreview();
        EditorTextBox.Focus();
    }

    private void UpdateTitle()
    {
        string documentName = string.IsNullOrWhiteSpace(_currentFilePath)
            ? "Untitled"
            : Path.GetFileName(_currentFilePath);
        string dirtyMarker = _hasUnsavedChanges ? "*" : string.Empty;

        Title = $"{dirtyMarker}{documentName} - Memojang";
    }

    private bool ConfirmDiscardUnsavedChanges()
    {
        if (!_hasUnsavedChanges)
        {
            return true;
        }

        var result = MessageBox.Show(
            this,
            "Save changes before continuing?",
            "Memojang",
            MessageBoxButton.YesNoCancel,
            MessageBoxImage.Warning);

        return result switch
        {
            MessageBoxResult.Yes => SaveDocument(saveAs: false),
            MessageBoxResult.No => true,
            _ => false
        };
    }

    private string GetLineEndingValue()
    {
        return _lineEndingName switch
        {
            LineEnding.Lf => "\n",
            LineEnding.Cr => "\r",
            _ => "\r\n"
        };
    }

    private void InsertTimeDate()
    {
        string timestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm");
        int selectionStart = EditorTextBox.SelectionStart;
        EditorTextBox.SelectedText = timestamp;
        EditorTextBox.CaretIndex = selectionStart + timestamp.Length;
        EditorTextBox.Focus();
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
            FindStatusText.Text = string.IsNullOrEmpty(query) ? string.Empty : "No matches";
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
            FindStatusText.Text = string.IsNullOrEmpty(FindTextBox.Text) ? string.Empty : "No matches";
            return;
        }

        EditorTextBox.Text = result.Text;
        EditorTextBox.CaretIndex = 0;
        FindStatusText.Text = $"Replaced {result.Count}";
        EditorTextBox.Focus();
    }

    private void ApplyCaptureExclusion()
    {
        var helper = new WindowInteropHelper(this);
        CaptureExclusionResult result = CaptureExclusion.Apply(helper.Handle, _captureExclusionEnabled);

        PrivacyStatusText.Text = result.StatusText;
        PrivacyStatusText.ToolTip = result.DetailText;
        CaptureExclusionMenuItem.IsChecked = result.IsEnabled && result.IsProtected;
    }

    private void UpdatePreview()
    {
        if (!PreviewMenuItem.IsChecked)
        {
            return;
        }

        PreviewViewer.Document = MarkdownPreviewDocumentFactory.Create(EditorTextBox.Text);
    }
}
