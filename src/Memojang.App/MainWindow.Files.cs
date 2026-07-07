using System.IO;
using System.Windows;
using Microsoft.Win32;
using Memojang.App.Localization;
using Memojang.App.Shell;

namespace Memojang.App;

public partial class MainWindow
{
    internal bool OpenDocument(string? filePath)
    {
        if (string.IsNullOrWhiteSpace(filePath))
        {
            return false;
        }

        try
        {
            AppDiagnostics.Log($"OpenDocument sync start path={filePath}");
            string text = File.ReadAllText(filePath);
            _currentFilePath = filePath;
            SetEditorText(text, hasUnsavedChanges: false);
            AppDiagnostics.Log($"OpenDocument sync success path={filePath} length={text.Length}");
            return true;
        }
        catch (IOException ex)
        {
            AppDiagnostics.Log($"OpenDocument sync IO failure path={filePath} {ex}");
            MessageBox.Show(this, UiText.Format("MessageOpenFileFailed", ex.Message), "Memojang", MessageBoxButton.OK, MessageBoxImage.Error);
            return false;
        }
        catch (UnauthorizedAccessException ex)
        {
            AppDiagnostics.Log($"OpenDocument sync access failure path={filePath} {ex}");
            MessageBox.Show(this, UiText.Format("MessageOpenFileFailed", ex.Message), "Memojang", MessageBoxButton.OK, MessageBoxImage.Error);
            return false;
        }
    }

    internal async Task<bool> OpenDocumentAsync(string? filePath)
    {
        if (string.IsNullOrWhiteSpace(filePath))
        {
            return false;
        }

        try
        {
            AppDiagnostics.Log($"OpenDocument async start path={filePath}");
            string text = await File.ReadAllTextAsync(filePath);
            _currentFilePath = filePath;
            SetEditorText(text, hasUnsavedChanges: false);
            Activate();
            AppDiagnostics.Log($"OpenDocument async success path={filePath} length={text.Length}");
            return true;
        }
        catch (IOException ex)
        {
            AppDiagnostics.Log($"OpenDocument async IO failure path={filePath} {ex}");
            MessageBox.Show(this, UiText.Format("MessageOpenFileFailed", ex.Message), "Memojang", MessageBoxButton.OK, MessageBoxImage.Error);
            return false;
        }
        catch (UnauthorizedAccessException ex)
        {
            AppDiagnostics.Log($"OpenDocument async access failure path={filePath} {ex}");
            MessageBox.Show(this, UiText.Format("MessageOpenFileFailed", ex.Message), "Memojang", MessageBoxButton.OK, MessageBoxImage.Error);
            return false;
        }
        catch (Exception ex)
        {
            AppDiagnostics.Log($"OpenDocument async unexpected failure path={filePath} {ex}");
            MessageBox.Show(this, UiText.Format("MessageOpenFileFailed", ex.Message), "Memojang", MessageBoxButton.OK, MessageBoxImage.Error);
            return false;
        }
    }

    private void OpenMenuItem_Click(object sender, RoutedEventArgs e)
    {
        if (!ConfirmDiscardUnsavedChanges())
        {
            return;
        }

        var dialog = new OpenFileDialog
        {
            Filter = UiText.Get("DialogTextDocumentFilter"),
            Title = UiText.Get("DialogOpenTitle")
        };

        if (dialog.ShowDialog(this) == true)
        {
            OpenDocument(dialog.FileName);
        }
    }

    private void SaveMenuItem_Click(object sender, RoutedEventArgs e)
    {
        SaveDocument(saveAs: false);
    }

    private void SaveAsMenuItem_Click(object sender, RoutedEventArgs e)
    {
        SaveDocument(saveAs: true);
    }

    private bool SaveDocument(bool saveAs)
    {
        string? targetPath = _currentFilePath;

        if (saveAs || string.IsNullOrWhiteSpace(targetPath))
        {
            var dialog = new SaveFileDialog
            {
                AddExtension = true,
                DefaultExt = DocumentModeDetector.GetDefaultExtension(_documentMode),
                Filter = UiText.Get("DialogTextDocumentFilter"),
                FilterIndex = DocumentModeDetector.GetSaveDialogFilterIndex(_documentMode),
                FileName = string.IsNullOrWhiteSpace(targetPath) ? UiText.Get("UntitledFileName") : Path.GetFileName(targetPath),
                Title = UiText.Get("DialogSaveTitle")
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
        UpdateDocumentModeFromPath();
        UpdateTitle();
        UpdateStatusFromEditor();
        return true;
    }
}
