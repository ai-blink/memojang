using System.IO;
using System.Windows;
using Memojang.App.Shell;

namespace Memojang.App;

public partial class MainWindow
{
    private readonly SessionStore _sessionStore = new();
    private bool _sessionRestoreEnabled = true;

    private bool TryRestoreSession()
    {
        SessionSnapshot? snapshot = _sessionRestoreEnabled ? _sessionStore.Load() : null;
        if (snapshot is null)
        {
            return false;
        }

        foreach (SessionTab saved in snapshot.Tabs)
        {
            if (CreateRestoredTab(saved) is DocumentTabState tab)
            {
                _documentTabs.Add(tab);
            }
        }

        if (_documentTabs.Count == 0)
        {
            return false;
        }

        _untitledDocumentCount = _documentTabs.Count + 1;
        ActivateDocumentTab(_documentTabs[Math.Clamp(snapshot.ActiveIndex, 0, _documentTabs.Count - 1)]);
        AppDiagnostics.Log($"Session restored tabs={_documentTabs.Count}");
        return true;
    }

    private static DocumentTabState? CreateRestoredTab(SessionTab saved)
    {
        string text;
        TextFileEncoding encoding = TextFileEncoding.FromKey(saved.EncodingKey);
        bool isDirty = saved.UnsavedText is not null;

        if (saved.UnsavedText is not null)
        {
            text = saved.UnsavedText;
        }
        else if (saved.FilePath is not null && File.Exists(saved.FilePath))
        {
            try
            {
                DecodedText decoded = TextFileCodec.Read(saved.FilePath);
                (text, encoding) = (decoded.Text, decoded.Encoding);
            }
            catch (Exception ex) when (ex is IOException or UnauthorizedAccessException)
            {
                AppDiagnostics.Log($"Session tab skipped path={saved.FilePath} {ex}");
                return null;
            }
        }
        else
        {
            return null;
        }

        return new DocumentTabState
        {
            UntitledName = saved.UntitledName,
            FilePath = saved.FilePath,
            Text = text,
            HasUnsavedChanges = isDirty,
            FileEncoding = encoding,
            LineEndingName = isDirty ? saved.LineEnding : LineEnding.Detect(text),
            Mode = Enum.TryParse(saved.Mode, out DocumentMode mode) ? mode : DocumentMode.PlainText,
            CaretIndex = Math.Clamp(saved.CaretIndex, 0, text.Length),
            FileStamp = FileStamp.Read(saved.FilePath)
        };
    }

    private bool TrySaveSession()
    {
        PersistActiveDocumentState();
        List<SessionTab> tabs = _documentTabs
            .Where(tab => tab.FilePath is not null || tab.Text.Length > 0)
            .Select(tab => new SessionTab(
                tab.FilePath,
                tab.UntitledName,
                tab.Mode.ToString(),
                tab.HasUnsavedChanges ? tab.Text : null,
                tab.FileEncoding.Key,
                tab.LineEndingName,
                tab.CaretIndex))
            .ToList();

        int activeIndex = _activeDocumentTab is null ? 0 : Math.Max(0, tabs.FindIndex(tab =>
            tab.UntitledName == _activeDocumentTab.UntitledName && tab.FilePath == _activeDocumentTab.FilePath));

        return _sessionStore.Save(new SessionSnapshot { Tabs = tabs, ActiveIndex = activeIndex });
    }

    // Opening never overwrites another document: switch to the file's tab if it is already open, reuse a blank
    // untitled tab, otherwise open a new tab. Returns false when the existing tab was activated instead.
    private bool PrepareTabForOpen(string filePath)
    {
        DocumentTabState? existing = _documentTabs.FirstOrDefault(tab =>
            tab.FilePath is not null && Path.GetFullPath(tab.FilePath).Equals(Path.GetFullPath(filePath), StringComparison.OrdinalIgnoreCase));
        if (existing is not null)
        {
            ActivateDocumentTab(existing);
            return false;
        }

        if (_activeDocumentTab is { FilePath: not null } || EditorTextBox.Text.Length > 0)
        {
            CreateNewDocumentTab(DocumentMode.PlainText);
        }

        return true;
    }
}
