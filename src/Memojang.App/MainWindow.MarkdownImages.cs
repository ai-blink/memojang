using System.IO;
using System.Windows;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
using Memojang.App.Localization;
using Memojang.App.Shell;
using Microsoft.Win32;

namespace Memojang.App;

public partial class MainWindow
{
    private readonly DispatcherTimer _previewUpdateTimer = new()
    {
        Interval = TimeSpan.FromMilliseconds(200)
    };

    private DocumentMode _documentMode = DocumentMode.PlainText;

    private void InitializeDocumentModeAndPreview()
    {
        _previewUpdateTimer.Tick += (_, _) =>
        {
            _previewUpdateTimer.Stop();
            UpdatePreview();
        };

        ApplyDocumentModeState();
    }

    private void UpdateDocumentModeFromPath()
    {
        _documentMode = DocumentModeDetector.FromPath(_currentFilePath);
        ApplyDocumentModeState();
    }

    private void ApplyDocumentModeState()
    {
        bool isMarkdown = _documentMode == DocumentMode.Markdown;

        DocumentModeStatusText.Text = DocumentModeDetector.GetStatusText(_documentMode);
        MarkdownToolbarPanel.Visibility = Visibility.Visible;
        MarkdownMenuItem.IsEnabled = true;
        PreviewMenuItem.IsEnabled = isMarkdown;
        ToolbarTableButton.IsEnabled = true;
        ToolbarImageButton.IsEnabled = isMarkdown;
        ToolbarPreviewButton.IsEnabled = isMarkdown;

        if (!isMarkdown && PreviewMenuItem.IsChecked)
        {
            PreviewMenuItem.IsChecked = false;
            PreviewColumn.Width = new GridLength(0);
            PreviewPane.Visibility = Visibility.Collapsed;
            _previewUpdateTimer.Stop();
        }
    }

    private void SchedulePreviewUpdate()
    {
        if (!PreviewMenuItem.IsChecked || _documentMode != DocumentMode.Markdown)
        {
            _previewUpdateTimer.Stop();
            return;
        }

        _previewUpdateTimer.Stop();
        _previewUpdateTimer.Start();
    }

    private void InsertImageMenuItem_Click(object sender, RoutedEventArgs e)
    {
        var dialog = new OpenFileDialog
        {
            Filter = UiText.Get("DialogImageFilter"),
            Multiselect = true,
            Title = UiText.Get("MenuInsertImage")
        };

        if (dialog.ShowDialog(this) == true)
        {
            AttachImageFiles(dialog.FileNames);
        }
    }

    private void ToolbarImageButton_Click(object sender, RoutedEventArgs e)
    {
        InsertImageMenuItem_Click(sender, e);
    }

    private void EditorTextBox_PreviewDragOver(object sender, DragEventArgs e)
    {
        string[] imagePaths = GetImagePaths(e.Data);
        if (imagePaths.Length == 0)
        {
            return;
        }

        e.Effects = _documentMode == DocumentMode.Markdown || string.IsNullOrWhiteSpace(_currentFilePath)
            ? DragDropEffects.Copy
            : DragDropEffects.None;
        e.Handled = true;
    }

    private void EditorTextBox_Drop(object sender, DragEventArgs e)
    {
        string[] imagePaths = GetImagePaths(e.Data);
        if (imagePaths.Length == 0)
        {
            return;
        }

        AttachImageFiles(imagePaths);
        e.Handled = true;
    }

    private bool TryPasteImageAttachment()
    {
        if (Clipboard.ContainsFileDropList())
        {
            string[] imagePaths = Clipboard.GetFileDropList()
                .Cast<string>()
                .Where(MarkdownImageAttachmentService.IsSupportedImagePath)
                .ToArray();

            if (imagePaths.Length > 0)
            {
                AttachImageFiles(imagePaths);
                return true;
            }
        }

        if (!Clipboard.ContainsImage())
        {
            return false;
        }

        if (!EnsureMarkdownDocumentReadyForImages())
        {
            return true;
        }

        BitmapSource? bitmap = Clipboard.GetImage();
        if (bitmap is null)
        {
            return true;
        }

        string targetPath = MarkdownImageAttachmentService.GetAvailableImagePath(_currentFilePath!, ".png", DateTime.Now);
        Directory.CreateDirectory(Path.GetDirectoryName(targetPath)!);

        using (var stream = File.Create(targetPath))
        {
            var encoder = new PngBitmapEncoder();
            encoder.Frames.Add(BitmapFrame.Create(bitmap));
            encoder.Save(stream);
        }

        InsertMarkdownImageLinks([
            MarkdownImageAttachmentService.CreateMarkdownImageLink(_currentFilePath!, targetPath, "clipboard image")
        ]);
        return true;
    }

    private bool AttachImageFiles(IEnumerable<string> imagePaths)
    {
        if (!EnsureMarkdownDocumentReadyForImages())
        {
            return false;
        }

        IReadOnlyList<MarkdownImageAttachment> attachments =
            MarkdownImageAttachmentService.CopyImagesToSidecar(_currentFilePath!, imagePaths, DateTime.Now);

        if (attachments.Count == 0)
        {
            return false;
        }

        InsertMarkdownImageLinks(attachments.Select(attachment => attachment.MarkdownText));
        return true;
    }

    private bool EnsureMarkdownDocumentReadyForImages()
    {
        if (string.IsNullOrWhiteSpace(_currentFilePath))
        {
            MessageBox.Show(this, UiText.Get("MessageSaveMarkdownBeforeImages"), "Memojang", MessageBoxButton.OK, MessageBoxImage.Information);
            if (!SaveDocument(saveAs: true))
            {
                return false;
            }
        }

        if (_documentMode == DocumentMode.Markdown)
        {
            return true;
        }

        MessageBox.Show(this, UiText.Get("MessageImagesMarkdownOnly"), "Memojang", MessageBoxButton.OK, MessageBoxImage.Information);
        return false;
    }

    private void InsertMarkdownImageLinks(IEnumerable<string> markdownLinks)
    {
        string lineEnding = GetLineEndingValue();
        string markdown = string.Join(lineEnding, markdownLinks);
        int selectionStart = EditorTextBox.SelectionStart;
        string prefix = selectionStart > 0 && !IsLineBoundaryBefore(selectionStart) ? lineEnding : string.Empty;
        string suffix = selectionStart < EditorTextBox.Text.Length && !IsLineBoundaryAt(selectionStart) ? lineEnding : string.Empty;

        EditorTextBox.SelectedText = $"{prefix}{markdown}{lineEnding}{suffix}";
        EditorTextBox.CaretIndex = Math.Min(EditorTextBox.Text.Length, selectionStart + prefix.Length + markdown.Length + lineEnding.Length);
        EditorTextBox.Focus();
        SchedulePreviewUpdate();
    }

    private bool IsLineBoundaryBefore(int index)
    {
        return index == 0 || EditorTextBox.Text[index - 1] is '\r' or '\n';
    }

    private bool IsLineBoundaryAt(int index)
    {
        return index >= EditorTextBox.Text.Length || EditorTextBox.Text[index] is '\r' or '\n';
    }

    private static string[] GetImagePaths(IDataObject data)
    {
        if (!data.GetDataPresent(DataFormats.FileDrop) || data.GetData(DataFormats.FileDrop) is not string[] paths)
        {
            return [];
        }

        return paths.Where(MarkdownImageAttachmentService.IsSupportedImagePath).ToArray();
    }
}
