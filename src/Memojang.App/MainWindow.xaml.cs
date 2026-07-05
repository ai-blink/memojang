using System.IO;
using System.Windows;
using System.Windows.Documents;
using System.Windows.Interop;
using System.Windows.Media;
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

    private void ApplyCaptureExclusion()
    {
        var helper = new WindowInteropHelper(this);
        CaptureExclusionResult result = CaptureExclusion.Apply(helper.Handle, _captureExclusionEnabled);

        PrivacyStatusText.Text = result.StatusText;
        CaptureExclusionMenuItem.IsChecked = result.IsEnabled && result.IsProtected;
    }

    private void UpdatePreview()
    {
        if (!PreviewMenuItem.IsChecked)
        {
            return;
        }

        var document = new FlowDocument
        {
            FontFamily = new FontFamily("Segoe UI"),
            FontSize = 14,
            PagePadding = new Thickness(0)
        };

        foreach (MarkdownPreviewBlock block in MarkdownPreviewParser.Parse(EditorTextBox.Text))
        {
            document.Blocks.Add(CreatePreviewBlock(block));
        }

        PreviewViewer.Document = document;
    }

    private static Block CreatePreviewBlock(MarkdownPreviewBlock block)
    {
        return block.Kind switch
        {
            MarkdownPreviewBlockKind.Heading => new Paragraph(new Run(block.Text))
            {
                FontSize = block.HeadingLevel == 1 ? 24 : 20,
                FontWeight = FontWeights.SemiBold,
                Margin = new Thickness(0, 0, 0, 10)
            },
            MarkdownPreviewBlockKind.Table => CreateTablePreview(block.TableRows ?? []),
            _ => new Paragraph(new Run(block.Text))
            {
                Margin = new Thickness(0, 0, 0, 8)
            }
        };
    }

    private static Block CreateTablePreview(IReadOnlyList<IReadOnlyList<string>> rows)
    {
        var table = new Table
        {
            CellSpacing = 0,
            Margin = new Thickness(0, 4, 0, 12)
        };

        int columnCount = rows.Count == 0 ? 0 : rows.Max(row => row.Count);
        for (int i = 0; i < columnCount; i++)
        {
            table.Columns.Add(new TableColumn());
        }

        var group = new TableRowGroup();
        table.RowGroups.Add(group);

        for (int rowIndex = 0; rowIndex < rows.Count; rowIndex++)
        {
            var tableRow = new TableRow();
            group.Rows.Add(tableRow);

            for (int columnIndex = 0; columnIndex < columnCount; columnIndex++)
            {
                string text = columnIndex < rows[rowIndex].Count ? rows[rowIndex][columnIndex] : string.Empty;
                tableRow.Cells.Add(new TableCell(new Paragraph(new Run(text)))
                {
                    BorderBrush = Brushes.Gainsboro,
                    BorderThickness = new Thickness(0, 0, 1, 1),
                    FontWeight = rowIndex == 0 ? FontWeights.SemiBold : FontWeights.Normal,
                    Padding = new Thickness(6, 4, 6, 4)
                });
            }
        }

        return table;
    }
}
