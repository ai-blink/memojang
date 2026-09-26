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

        if (!PrepareTabForOpen(filePath))
        {
            return true;
        }

        try
        {
            AppDiagnostics.Log($"OpenDocument sync start path={filePath}");
            DecodedText decoded = TextFileCodec.Read(filePath);
            _currentFilePath = filePath;
            _fileStamp = FileStamp.Read(filePath);
            SetEditorText(decoded.Text, hasUnsavedChanges: false, decoded.Encoding);
            AddRecentFile(filePath);
            AppDiagnostics.Log($"OpenDocument sync success path={filePath} length={decoded.Text.Length} encoding={decoded.Encoding.DisplayName}");
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

        if (!PrepareTabForOpen(filePath))
        {
            return true;
        }

        try
        {
            AppDiagnostics.Log($"OpenDocument async start path={filePath}");
            DecodedText decoded = await TextFileCodec.ReadAsync(filePath);
            _currentFilePath = filePath;
            _fileStamp = FileStamp.Read(filePath);
            SetEditorText(decoded.Text, hasUnsavedChanges: false, decoded.Encoding);
            AddRecentFile(filePath);
            Activate();
            AppDiagnostics.Log($"OpenDocument async success path={filePath} length={decoded.Text.Length} encoding={decoded.Encoding.DisplayName}");
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

    internal async Task OpenDocumentsAsync(IReadOnlyList<string> filePaths)
    {
        foreach (string filePath in filePaths)
        {
            await OpenDocumentAsync(filePath);
        }
    }

    // Paths handed over by a second launch; an empty list means the user just started Memojang again.
    internal async Task OpenForwardedDocumentsAsync(IReadOnlyList<string> filePaths)
    {
        BringToFront();
        if (filePaths.Count == 0)
        {
            CreateNewDocumentTab(DocumentMode.PlainText);
            return;
        }

        await OpenDocumentsAsync(filePaths);
    }

    private void BringToFront()
    {
        if (WindowState == WindowState.Minimized)
        {
            WindowState = WindowState.Normal;
        }

        Activate();
        // Toggling Topmost raises the window even when Windows only flashed the taskbar for Activate().
        Topmost = true;
        Topmost = false;
        EditorTextBox.Focus();
    }

    private void OpenMenuItem_Click(object sender, RoutedEventArgs e)
    {
        var dialog = new OpenFileDialog
        {
            Filter = UiText.Get("DialogTextDocumentFilter"),
            Title = UiText.Get("DialogOpenTitle"),
            Multiselect = true
        };

        if (dialog.ShowDialog(this) == true)
        {
            foreach (string fileName in dialog.FileNames)
            {
                OpenDocument(fileName);
            }
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

        TextFileEncoding saveEncoding = _fileEncoding;
        if (!TextFileCodec.CanEncodeLosslessly(EditorTextBox.Text, saveEncoding))
        {
            // Never drop characters silently: an ANSI file that gained non-representable text is saved as UTF-8 only with consent.
            var answer = MessageBox.Show(
                this,
                UiText.Format("PromptEncodingLossy", saveEncoding.DisplayName),
                "Memojang",
                MessageBoxButton.OKCancel,
                MessageBoxImage.Warning);
            if (answer != MessageBoxResult.OK)
            {
                return false;
            }

            saveEncoding = TextFileEncoding.Utf8;
        }

        try
        {
            TextFileCodec.Write(targetPath, EditorTextBox.Text, saveEncoding);
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
        _fileEncoding = saveEncoding;
        _fileStamp = FileStamp.Read(targetPath);
        _lineEndingName = LineEnding.Detect(EditorTextBox.Text);
        UpdateDocumentModeFromPath();
        PersistActiveDocumentState();
        AddRecentFile(targetPath);
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
