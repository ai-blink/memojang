using System.IO;
using System.Windows;
using Memojang.App.Localization;
using Memojang.App.Shell;

namespace Memojang.App;

public partial class MainWindow
{
    private FileStamp? _fileStamp;
    private bool _isCheckingExternalChange;

    private void MainWindow_Activated(object? sender, EventArgs e)
    {
        CheckActiveDocumentForExternalChange();
    }

    private void CheckActiveDocumentForExternalChange()
    {
        // A message box deactivates and re-activates the window; the guard stops a second prompt.
        if (_isCheckingExternalChange || string.IsNullOrWhiteSpace(_currentFilePath))
        {
            return;
        }

        FileStamp? current = FileStamp.Read(_currentFilePath);
        ExternalChangeAction action = FileStamp.Decide(_fileStamp, current, _hasUnsavedChanges);
        if (action == ExternalChangeAction.None)
        {
            return;
        }

        _isCheckingExternalChange = true;
        try
        {
            // Remember what we have seen so the same disk change is handled once.
            _fileStamp = current;
            PersistActiveDocumentState();

            switch (action)
            {
                case ExternalChangeAction.ReloadSilently:
                    ReloadActiveDocumentFromDisk();
                    break;
                case ExternalChangeAction.AskReload:
                    MessageBoxResult answer = MessageBox.Show(
                        this,
                        UiText.Format("PromptFileChangedReload", Path.GetFileName(_currentFilePath)),
                        "Memojang",
                        MessageBoxButton.YesNo,
                        MessageBoxImage.Question);
                    if (answer == MessageBoxResult.Yes)
                    {
                        ReloadActiveDocumentFromDisk();
                    }

                    break;
                case ExternalChangeAction.ReportMissing:
                    // Keep the text and mark it unsaved so closing still offers to save it somewhere.
                    _hasUnsavedChanges = true;
                    PersistActiveDocumentState();
                    UpdateTitle();
                    UpdateStatusFromEditor();
                    MessageBox.Show(
                        this,
                        UiText.Format("MessageFileMissing", Path.GetFileName(_currentFilePath)),
                        "Memojang",
                        MessageBoxButton.OK,
                        MessageBoxImage.Warning);
                    break;
            }
        }
        finally
        {
            _isCheckingExternalChange = false;
        }
    }

    private void ReloadActiveDocumentFromDisk()
    {
        string path = _currentFilePath!;
        try
        {
            int caretIndex = EditorTextBox.CaretIndex;
            DecodedText decoded = TextFileCodec.Read(path);
            _fileStamp = FileStamp.Read(path);
            SetEditorText(decoded.Text, hasUnsavedChanges: false, decoded.Encoding);
            EditorTextBox.CaretIndex = Math.Min(caretIndex, EditorTextBox.Text.Length);
            AppDiagnostics.Log($"External change reloaded path={path} encoding={decoded.Encoding.DisplayName}");
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException)
        {
            AppDiagnostics.Log($"External change reload failure path={path} {ex}");
        }
    }
}
