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

    private bool SaveDocument(bool saveAs, DocumentMode? forcedMode = null)
    {
        string? targetPath = _currentFilePath;
        DocumentMode saveMode = forcedMode ?? _documentMode;

        if (saveAs || string.IsNullOrWhiteSpace(targetPath))
        {
            var dialog = new SaveFileDialog
            {
                AddExtension = true,
                DefaultExt = DocumentModeDetector.GetDefaultExtension(saveMode),
                Filter = UiText.Get("DialogTextDocumentFilter"),
                FilterIndex = DocumentModeDetector.GetSaveDialogFilterIndex(saveMode),
                FileName = CreateDefaultSaveFileName(targetPath, saveMode),
                Title = UiText.Get("DialogSaveTitle")
            };

            if (dialog.ShowDialog(this) != true)
            {
                return false;
            }

            targetPath = dialog.FileName;
        }

        try
        {
            File.WriteAllText(targetPath, EditorTextBox.Text, new System.Text.UTF8Encoding(encoderShouldEmitUTF8Identifier: false));
        }
        catch (IOException ex)
        {
            AppDiagnostics.Log($"SaveDocument IO failure path={targetPath} {ex}");
            MessageBox.Show(this, UiText.Format("MessageSaveFileFailed", ex.Message), "Memojang", MessageBoxButton.OK, MessageBoxImage.Error);
            return false;
        }
        catch (UnauthorizedAccessException ex)
        {
            AppDiagnostics.Log($"SaveDocument access failure path={targetPath} {ex}");
            MessageBox.Show(this, UiText.Format("MessageSaveFileFailed", ex.Message), "Memojang", MessageBoxButton.OK, MessageBoxImage.Error);
            return false;
        }

        _currentFilePath = targetPath;
        _hasUnsavedChanges = false;
        _encodingName = "UTF-8";
        _lineEndingName = LineEnding.Detect(EditorTextBox.Text);
        UpdateDocumentModeFromPath();
        UpdateTitle();
        UpdateStatusFromEditor();
        return true;
    }

    private static string CreateDefaultSaveFileName(string? targetPath, DocumentMode saveMode)
    {
        if (!string.IsNullOrWhiteSpace(targetPath))
        {
            string fileName = Path.GetFileName(targetPath);
            return saveMode == DocumentMode.Markdown
                ? Path.ChangeExtension(fileName, ".md")
                : fileName;
        }

        return saveMode == DocumentMode.Markdown
            ? Path.ChangeExtension(UiText.Get("UntitledFileName"), ".md")
            : UiText.Get("UntitledFileName");
    }
}
