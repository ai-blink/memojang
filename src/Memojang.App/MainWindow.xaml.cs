using System.IO;
using System.Windows;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Media;
using Microsoft.Win32;
using Memojang.App.Localization;
using Memojang.App.Shell;
using Memojang.App.Theme;
using CancelEventArgs = System.ComponentModel.CancelEventArgs;
using MenuItem = System.Windows.Controls.MenuItem;

namespace Memojang.App;

public partial class MainWindow : Window
{
    private bool _hasUnsavedChanges;
    private bool _isSettingEditorText;
    private bool _captureExclusionRequested;
    private string? _currentFilePath;
    private TextFileEncoding _fileEncoding = TextFileEncoding.Utf8;
    private string _lineEndingName = LineEnding.Crlf;

    public MainWindow()
    {
        InitializeComponent();
        FlowDirection = UiText.FlowDirection;
        ApplyLocalizedText();
        InitializeViewControls();
        InitializeDocumentModeAndPreview();
        InitializePersistentSettings();
        InitializeTableEditing();
        InitializeLineEndingAndEncodingControls();
        InitializeTabs();
        SourceInitialized += MainWindow_SourceInitialized;
        StateChanged += MainWindow_StateChanged;
        Activated += MainWindow_Activated;
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
            PersistActiveDocumentState();
            UpdateTitle();
        }

        UpdateStatusFromEditor();
        SchedulePreviewUpdate();
    }

    private void EditorTextBox_SelectionChanged(object sender, RoutedEventArgs e)
    {
        PersistActiveDocumentSelection();
        UpdateStatusFromEditor();
    }

    private void UpdateStatusFromEditor()
    {
        (int line, int column) = TextLines.GetPosition(EditorTextBox.Text, EditorTextBox.CaretIndex);

        UpdateStatus(new EditorStatus(line, column, _hasUnsavedChanges, _fileEncoding.DisplayName, _lineEndingName));
    }

    private void UpdateStatus(EditorStatus status)
    {
        CaretStatusText.Text = status.CaretText;
        ChangeStatusText.Text = status.ChangeText;
        CharacterCountStatusText.Text = UiText.Format("StatusCharacterCount", EditorTextBox.Text.Length);
        EncodingStatusText.Text = status.EncodingName;
        LineEndingStatusText.Text = status.LineEndingName;
        DocumentModeStatusText.Text = DocumentModeDetector.GetStatusText(_documentMode);
    }

    private void NewTabMenuItem_Click(object sender, RoutedEventArgs e)
    {
        CreateNewDocumentTab(DocumentMode.PlainText);
    }

    private void NewTextMenuItem_Click(object sender, RoutedEventArgs e)
    {
        CreateNewDocumentTab(DocumentMode.PlainText);
    }

    private void NewMarkdownMenuItem_Click(object sender, RoutedEventArgs e)
    {
        CreateNewDocumentTab(DocumentMode.Markdown);
    }

    private void TimeDateMenuItem_Click(object sender, RoutedEventArgs e)
    {
        InsertTimeDate();
    }

    private void PreviewMenuItem_Click(object sender, RoutedEventArgs e)
    {
        if (_documentMode != DocumentMode.Markdown)
        {
            PreviewMenuItem.IsChecked = false;
            return;
        }

        bool isPreviewVisible = PreviewMenuItem.IsChecked;
        PreviewColumn.Width = isPreviewVisible ? new GridLength(1, GridUnitType.Star) : new GridLength(0);
        PreviewPane.Visibility = isPreviewVisible ? Visibility.Visible : Visibility.Collapsed;
        SchedulePreviewUpdate();
    }

    private void ToolbarPreviewButton_Click(object sender, RoutedEventArgs e)
    {
        PreviewMenuItem.IsChecked = !PreviewMenuItem.IsChecked;
        PreviewMenuItem_Click(sender, e);
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
            MessageBox.Show(this, UiText.Get("MessageCursorInsideTable"), "Memojang", MessageBoxButton.OK, MessageBoxImage.Information);
            return;
        }

        int caretIndex = EditorTextBox.CaretIndex;
        EditorTextBox.Text = updatedText;
        EditorTextBox.CaretIndex = Math.Min(EditorTextBox.Text.Length, caretIndex);
        EditorTextBox.Focus();
    }

    private void ExitMenuItem_Click(object sender, RoutedEventArgs e)
    {
        Close();
    }

    private void MinimizeWindowButton_Click(object sender, RoutedEventArgs e)
    {
        SystemCommands.MinimizeWindow(this);
    }

    private void MaximizeRestoreWindowButton_Click(object sender, RoutedEventArgs e)
    {
        ToggleMaximizeRestore();
    }

    private void TitleBar_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
    {
        if (IsInsideButton(e.OriginalSource as DependencyObject))
        {
            return;
        }

        if (e.ClickCount == 2)
        {
            ToggleMaximizeRestore();
            e.Handled = true;
            return;
        }

        try
        {
            DragMove();
            e.Handled = true;
        }
        catch (InvalidOperationException)
        {
            // DragMove can throw if the mouse button state changes during routing.
        }
    }

    private void TitleBar_MouseRightButtonUp(object sender, MouseButtonEventArgs e)
    {
        ShowSystemMenu(e.GetPosition(this));
        e.Handled = true;
    }

    private static bool IsInsideButton(DependencyObject? source)
    {
        for (DependencyObject? current = source; current is not null; current = VisualTreeHelper.GetParent(current))
        {
            if (current is ButtonBase)
            {
                return true;
            }
        }

        return false;
    }

    private void MainWindow_StateChanged(object? sender, EventArgs e)
    {
        MaximizeRestoreButton.Content = WindowState == WindowState.Maximized ? "❐" : "□";
    }

    private void ToggleMaximizeRestore()
    {
        if (WindowState == WindowState.Maximized)
        {
            SystemCommands.RestoreWindow(this);
            return;
        }

        SystemCommands.MaximizeWindow(this);
    }

    private void ThemeMenuItem_Click(object sender, RoutedEventArgs e)
    {
        AppTheme requestedTheme = sender == ThemeDarkMenuItem
            ? AppTheme.Dark
            : sender == ThemeLightMenuItem
                ? AppTheme.Light
                : AppTheme.System;

        ApplyTheme(requestedTheme);
    }

    private void UpdateThemeMenuState(AppTheme requestedTheme)
    {
        foreach (MenuItem item in new[] { ThemeSystemMenuItem, ThemeLightMenuItem, ThemeDarkMenuItem })
        {
            item.IsChecked = false;
        }

        MenuItem selectedItem = requestedTheme switch
        {
            AppTheme.Dark => ThemeDarkMenuItem,
            AppTheme.Light => ThemeLightMenuItem,
            _ => ThemeSystemMenuItem
        };
        selectedItem.IsChecked = true;
    }

    private void MainWindow_Closing(object? sender, CancelEventArgs e)
    {
        e.Cancel = !ConfirmCloseAllTabs();
        if (!e.Cancel)
        {
            SavePersistentSettings();
        }
    }

    private void MainWindow_PreviewKeyDown(object sender, KeyEventArgs e)
    {
        bool isControl = Keyboard.Modifiers == ModifierKeys.Control;
        bool isControlShift = Keyboard.Modifiers == (ModifierKeys.Control | ModifierKeys.Shift);
        bool isShift = Keyboard.Modifiers == ModifierKeys.Shift;
        bool isControlAlt = Keyboard.Modifiers == (ModifierKeys.Control | ModifierKeys.Alt);

        if (Keyboard.Modifiers == ModifierKeys.Alt && e.SystemKey == Key.Space)
        {
            ShowSystemMenu(new Point(0, 40));
            e.Handled = true;
        }
        else if (isControl && e.Key == Key.T)
        {
            NewTabMenuItem_Click(sender, e);
            e.Handled = true;
        }
        else if (isControl && e.Key == Key.N)
        {
            NewTextMenuItem_Click(sender, e);
            e.Handled = true;
        }
        else if (isControlShift && e.Key == Key.N)
        {
            NewMarkdownMenuItem_Click(sender, e);
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
        else if (isControlAlt && e.Key == Key.S)
        {
            SaveAllMenuItem_Click(sender, e);
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
        else if (isControl && e.Key == Key.H)
        {
            ShowFindPanel();
            ReplaceTextBox.Focus();
            e.Handled = true;
        }
        else if (isControl && e.Key == Key.W && _activeDocumentTab is not null)
        {
            CloseDocumentTab(_activeDocumentTab);
            e.Handled = true;
        }
        else if ((isControl || isControlShift) && e.Key == Key.Tab)
        {
            SwitchDocumentTab(isControlShift ? -1 : 1);
            e.Handled = true;
        }
        else if (isControl && e.Key == Key.G)
        {
            ShowGoToLineDialog();
            e.Handled = true;
        }
        else if (isControl && e.Key == Key.A)
        {
            EditorTextBox.SelectAll();
            e.Handled = true;
        }
        else if (isControl && e.Key == Key.V && TryPasteImageAttachment())
        {
            e.Handled = true;
        }
        else if (isControl && (e.Key == Key.Add || e.Key == Key.OemPlus))
        {
            AdjustZoom(10);
            e.Handled = true;
        }
        else if (isControl && (e.Key == Key.Subtract || e.Key == Key.OemMinus))
        {
            AdjustZoom(-10);
            e.Handled = true;
        }
        else if (isControl && (e.Key == Key.D0 || e.Key == Key.NumPad0))
        {
            ResetZoom();
            e.Handled = true;
        }
        else if (isShift && e.Key == Key.F3)
        {
            FindPrevious();
            e.Handled = true;
        }
        else if (e.Key == Key.F3)
        {
            FindNext();
            e.Handled = true;
        }
    }

    private void ShowSystemMenu(Point clientPoint)
    {
        SystemCommands.ShowSystemMenu(this, PointToScreen(clientPoint));
    }

    private void MainWindow_SourceInitialized(object? sender, EventArgs e)
    {
        ApplyCaptureExclusion();
    }

    private void SetEditorText(string text, bool hasUnsavedChanges, TextFileEncoding fileEncoding)
    {
        _isSettingEditorText = true;
        EditorTextBox.Text = text;
        EditorTextBox.CaretIndex = 0;
        _isSettingEditorText = false;

        _hasUnsavedChanges = hasUnsavedChanges;
        _fileEncoding = fileEncoding;
        _lineEndingName = LineEnding.Detect(text);
        UpdateDocumentModeFromPath();
        PersistActiveDocumentState();
        UpdateTitle();
        UpdateStatusFromEditor();
        UpdatePreview();
        EditorTextBox.Focus();
    }

    private void UpdateTitle()
    {
        string documentName = _activeDocumentTab is null
            ? string.IsNullOrWhiteSpace(_currentFilePath) ? "Untitled" : Path.GetFileName(_currentFilePath)
            : GetDocumentName(_activeDocumentTab);
        string dirtyMarker = _hasUnsavedChanges ? "*" : string.Empty;

        Title = $"{dirtyMarker}{documentName} - Memojang";
        UpdateTabStrip();
    }

    private bool ConfirmDiscardUnsavedChanges()
    {
        if (!_hasUnsavedChanges)
        {
            return true;
        }

        var result = MessageBox.Show(
            this,
            UiText.Get("PromptSaveChanges"),
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

    private string GetLineEndingValue() => LineEnding.ToValue(_lineEndingName);

    private void InsertTimeDate()
    {
        string timestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm");
        int selectionStart = EditorTextBox.SelectionStart;
        EditorTextBox.SelectedText = timestamp;
        EditorTextBox.CaretIndex = selectionStart + timestamp.Length;
        EditorTextBox.Focus();
    }

    private void UpdatePreview()
    {
        if (!PreviewMenuItem.IsChecked)
        {
            return;
        }

        var document = MarkdownPreviewDocumentFactory.Create(EditorTextBox.Text, _currentFilePath);
        if (TryFindResource("AppTextBrush") is Brush textBrush)
        {
            document.Foreground = textBrush;
        }

        PreviewViewer.Document = document;
    }

}
