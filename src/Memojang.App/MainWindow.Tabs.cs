using System.IO;
using System.Windows;
using System.Windows.Automation;
using System.Windows.Controls;
using System.Windows.Media;
using Memojang.App.Localization;
using Memojang.App.Shell;

namespace Memojang.App;

public partial class MainWindow
{
    private readonly List<DocumentTabState> _documentTabs = [];
    private readonly List<string> _recentFilePaths = [];
    private DocumentTabState? _activeDocumentTab;
    private int _untitledDocumentCount = 1;

    private void InitializeTabs()
    {
        CreateNewDocumentTab(DocumentMode.PlainText);
        UpdateRecentFilesMenu();
    }

    private void CreateNewDocumentTab(DocumentMode mode)
    {
        var tab = new DocumentTabState
        {
            UntitledName = CreateUntitledName(mode),
            Mode = mode
        };

        _documentTabs.Add(tab);
        ActivateDocumentTab(tab);
    }

    private void ActivateDocumentTab(DocumentTabState tab)
    {
        if (ReferenceEquals(_activeDocumentTab, tab))
        {
            UpdateTabStrip();
            return;
        }

        PersistActiveDocumentState();
        _activeDocumentTab = tab;
        _currentFilePath = tab.FilePath;
        _hasUnsavedChanges = tab.HasUnsavedChanges;
        _fileEncoding = tab.FileEncoding;
        _lineEndingName = tab.LineEndingName;
        _documentMode = tab.Mode;

        _isSettingEditorText = true;
        EditorTextBox.Text = tab.Text;

        int selectionStart = Math.Clamp(tab.SelectionStart, 0, EditorTextBox.Text.Length);
        int selectionLength = Math.Clamp(tab.SelectionLength, 0, EditorTextBox.Text.Length - selectionStart);
        if (selectionLength > 0)
        {
            EditorTextBox.Select(selectionStart, selectionLength);
        }
        else
        {
            EditorTextBox.CaretIndex = Math.Clamp(tab.CaretIndex, 0, EditorTextBox.Text.Length);
        }

        _isSettingEditorText = false;

        ApplyDocumentModeState();
        UpdateTitle();
        UpdateStatusFromEditor();
        UpdatePreview();
        EditorTextBox.Focus();
    }

    private void SwitchDocumentTab(int offset)
    {
        if (_activeDocumentTab is null || _documentTabs.Count < 2)
        {
            return;
        }

        int index = _documentTabs.IndexOf(_activeDocumentTab);
        int next = ((index + offset) % _documentTabs.Count + _documentTabs.Count) % _documentTabs.Count;
        ActivateDocumentTab(_documentTabs[next]);
    }

    private void CloseDocumentTab(DocumentTabState tab)
    {
        int removedIndex = _documentTabs.IndexOf(tab);
        if (removedIndex < 0)
        {
            return;
        }

        if (!ReferenceEquals(_activeDocumentTab, tab))
        {
            ActivateDocumentTab(tab);
        }

        if (!ConfirmDiscardUnsavedChanges())
        {
            return;
        }

        _documentTabs.Remove(tab);
        if (_documentTabs.Count == 0)
        {
            _activeDocumentTab = null;
            CreateNewDocumentTab(DocumentMode.PlainText);
            return;
        }

        _activeDocumentTab = null;
        ActivateDocumentTab(_documentTabs[Math.Min(removedIndex, _documentTabs.Count - 1)]);
    }

    private bool ConfirmCloseAllTabs()
    {
        foreach (DocumentTabState tab in _documentTabs.ToArray())
        {
            ActivateDocumentTab(tab);
            if (!ConfirmDiscardUnsavedChanges())
            {
                return false;
            }
        }

        return true;
    }

    private void SaveAllMenuItem_Click(object sender, RoutedEventArgs e)
    {
        SaveAllDocuments();
    }

    private bool SaveAllDocuments()
    {
        PersistActiveDocumentState();
        DocumentTabState? originalTab = _activeDocumentTab;

        foreach (DocumentTabState tab in _documentTabs.ToArray())
        {
            ActivateDocumentTab(tab);
            if (!SaveDocument(saveAs: false))
            {
                return false;
            }
        }

        if (originalTab is not null && _documentTabs.Contains(originalTab))
        {
            ActivateDocumentTab(originalTab);
        }

        return true;
    }

    private void PersistActiveDocumentState()
    {
        if (_activeDocumentTab is null)
        {
            return;
        }

        _activeDocumentTab.Text = EditorTextBox.Text;
        _activeDocumentTab.FilePath = _currentFilePath;
        _activeDocumentTab.HasUnsavedChanges = _hasUnsavedChanges;
        _activeDocumentTab.FileEncoding = _fileEncoding;
        _activeDocumentTab.LineEndingName = _lineEndingName;
        _activeDocumentTab.Mode = _documentMode;
        PersistActiveDocumentSelection();
    }

    private void PersistActiveDocumentSelection()
    {
        if (_activeDocumentTab is null || _isSettingEditorText)
        {
            return;
        }

        _activeDocumentTab.CaretIndex = EditorTextBox.CaretIndex;
        _activeDocumentTab.SelectionStart = EditorTextBox.SelectionStart;
        _activeDocumentTab.SelectionLength = EditorTextBox.SelectionLength;
    }

    private void UpdateTabStrip()
    {
        if (TabStripPanel is null)
        {
            return;
        }

        TabStripPanel.Children.Clear();
        foreach (DocumentTabState tab in _documentTabs)
        {
            TabStripPanel.Children.Add(CreateTabElement(tab));
        }
    }

    private FrameworkElement CreateTabElement(DocumentTabState tab)
    {
        bool isActive = ReferenceEquals(tab, _activeDocumentTab);
        var root = new Border
        {
            Width = 220,
            Height = 34,
            Margin = new Thickness(0, 0, 4, 0),
            CornerRadius = new CornerRadius(8, 8, 0, 0),
            BorderThickness = new Thickness(1, 1, 1, 0)
        };
        root.SetResourceReference(Border.BackgroundProperty, isActive ? "AppTabBrush" : "AppTopBarBrush");
        root.SetResourceReference(Border.BorderBrushProperty, "AppBorderBrush");

        var layout = new Grid();
        layout.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
        layout.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });

        var titleText = new TextBlock
        {
            Text = GetDocumentTabTitle(tab),
            FontSize = 14,
            FontWeight = isActive ? FontWeights.SemiBold : FontWeights.Normal,
            TextTrimming = TextTrimming.CharacterEllipsis,
            VerticalAlignment = VerticalAlignment.Center
        };
        titleText.SetResourceReference(TextBlock.ForegroundProperty, "AppTextBrush");

        var titleButton = new Button
        {
            Content = titleText,
            Background = Brushes.Transparent,
            BorderThickness = new Thickness(0),
            HorizontalContentAlignment = HorizontalAlignment.Stretch,
            Padding = new Thickness(12, 0, 6, 0),
            ToolTip = GetDocumentName(tab)
        };
        titleButton.Click += (_, _) => ActivateDocumentTab(tab);
        Grid.SetColumn(titleButton, 0);
        layout.Children.Add(titleButton);

        var closeButton = new Button
        {
            Content = "×",
            Background = Brushes.Transparent,
            BorderThickness = new Thickness(0),
            MinWidth = 32,
            Padding = new Thickness(4, 0, 8, 0),
            ToolTip = UiText.Get("CloseButton")
        };
        AutomationProperties.SetName(closeButton, UiText.Get("CloseButton"));
        closeButton.Click += (_, e) =>
        {
            e.Handled = true;
            CloseDocumentTab(tab);
        };
        Grid.SetColumn(closeButton, 1);
        layout.Children.Add(closeButton);

        root.Child = layout;
        return root;
    }

    private void AddRecentFile(string? filePath)
    {
        if (string.IsNullOrWhiteSpace(filePath))
        {
            return;
        }

        string fullPath = Path.GetFullPath(filePath);
        _recentFilePaths.RemoveAll(path => path.Equals(fullPath, StringComparison.OrdinalIgnoreCase));
        _recentFilePaths.Insert(0, fullPath);

        if (_recentFilePaths.Count > AppSettingsStore.MaxRecentFiles)
        {
            _recentFilePaths.RemoveRange(
                AppSettingsStore.MaxRecentFiles,
                _recentFilePaths.Count - AppSettingsStore.MaxRecentFiles);
        }

        UpdateRecentFilesMenu();
        SavePersistentSettings();
    }

    private void UpdateRecentFilesMenu()
    {
        RecentMenuItem.Items.Clear();

        if (_recentFilePaths.Count == 0)
        {
            RecentMenuItem.Items.Add(new MenuItem
            {
                Header = UiText.Get("MenuNoRecentFiles"),
                IsEnabled = false
            });
            return;
        }

        foreach (string filePath in _recentFilePaths)
        {
            var item = new MenuItem
            {
                Header = filePath,
                ToolTip = filePath
            };
            item.Click += (_, _) => OpenRecentFile(filePath);
            RecentMenuItem.Items.Add(item);
        }
    }

    private void OpenRecentFile(string filePath)
    {
        if (!File.Exists(filePath))
        {
            _recentFilePaths.Remove(filePath);
            UpdateRecentFilesMenu();
            SavePersistentSettings();
            MessageBox.Show(this, UiText.Format("MessageOpenFileFailed", filePath), "Memojang", MessageBoxButton.OK, MessageBoxImage.Error);
            return;
        }

        if (!ConfirmDiscardUnsavedChanges())
        {
            return;
        }

        OpenDocument(filePath);
    }

    private string GetDocumentTabTitle(DocumentTabState tab)
    {
        string dirtyMarker = tab.HasUnsavedChanges ? "*" : string.Empty;
        return $"{dirtyMarker}{GetDocumentName(tab)}";
    }

    private string GetDocumentName(DocumentTabState tab)
    {
        return string.IsNullOrWhiteSpace(tab.FilePath)
            ? tab.UntitledName
            : Path.GetFileName(tab.FilePath);
    }

    private string CreateUntitledName(DocumentMode mode)
    {
        string defaultName = UiText.Get("UntitledFileName");
        string baseName = Path.GetFileNameWithoutExtension(defaultName);
        if (string.IsNullOrWhiteSpace(baseName))
        {
            baseName = "Untitled";
        }

        int documentNumber = _untitledDocumentCount++;
        string suffix = documentNumber == 1 ? string.Empty : $" {documentNumber}";
        return $"{baseName}{suffix}{DocumentModeDetector.GetDefaultExtension(mode)}";
    }

    private sealed class DocumentTabState
    {
        public string Text { get; set; } = string.Empty;
        public string UntitledName { get; init; } = "Untitled.txt";
        public string? FilePath { get; set; }
        public bool HasUnsavedChanges { get; set; }
        public TextFileEncoding FileEncoding { get; set; } = TextFileEncoding.Utf8;
        public string LineEndingName { get; set; } = LineEnding.Crlf;
        public DocumentMode Mode { get; set; } = DocumentMode.PlainText;
        public int CaretIndex { get; set; }
        public int SelectionStart { get; set; }
        public int SelectionLength { get; set; }
    }
}
