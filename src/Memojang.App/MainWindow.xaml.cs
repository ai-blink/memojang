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
    private bool _captureExclusionEnabled;
    private string? _currentFilePath;
    private string _encodingName = "UTF-8";
    private string _lineEndingName = LineEnding.Crlf;

    public MainWindow()
    {
        InitializeComponent();
        FlowDirection = UiText.FlowDirection;
        ApplyLocalizedText();
        InitializeDocumentModeAndPreview();
        SourceInitialized += MainWindow_SourceInitialized;
        StateChanged += MainWindow_StateChanged;
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
        SchedulePreviewUpdate();
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
        DocumentModeStatusText.Text = DocumentModeDetector.GetStatusText(_documentMode);
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

        AppThemeService.Apply(Application.Current, requestedTheme);
        UpdateThemeMenuState(requestedTheme);
        SchedulePreviewUpdate();
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
        e.Cancel = !ConfirmDiscardUnsavedChanges();
    }

    private void MainWindow_PreviewKeyDown(object sender, KeyEventArgs e)
    {
        bool isControl = Keyboard.Modifiers == ModifierKeys.Control;
        bool isControlShift = Keyboard.Modifiers == (ModifierKeys.Control | ModifierKeys.Shift);

        if (Keyboard.Modifiers == ModifierKeys.Alt && e.SystemKey == Key.Space)
        {
            ShowSystemMenu(new Point(0, 40));
            e.Handled = true;
        }
        else if (isControl && e.Key == Key.N)
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
        else if (isControl && e.Key == Key.V && TryPasteImageAttachment())
        {
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

    private void SetEditorText(string text, bool hasUnsavedChanges)
    {
        _isSettingEditorText = true;
        EditorTextBox.Text = text;
        EditorTextBox.CaretIndex = 0;
        _isSettingEditorText = false;

        _hasUnsavedChanges = hasUnsavedChanges;
        _encodingName = "UTF-8";
        _lineEndingName = LineEnding.Detect(text);
        UpdateDocumentModeFromPath();
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
        TabTitleText.Text = $"{dirtyMarker}{documentName}";
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

    private void ApplyLocalizedText()
    {
        FileMenuItem.Header = UiText.Get("MenuFile");
        NewMenuItem.Header = UiText.Get("MenuNew");
        OpenMenuItem.Header = UiText.Get("MenuOpen");
        SaveMenuItem.Header = UiText.Get("MenuSave");
        SaveAsMenuItem.Header = UiText.Get("MenuSaveAs");
        ExitMenuItem.Header = UiText.Get("MenuExit");
        EditMenuItem.Header = UiText.Get("MenuEdit");
        UndoMenuItem.Header = UiText.Get("MenuUndo");
        CutMenuItem.Header = UiText.Get("MenuCut");
        CopyMenuItem.Header = UiText.Get("MenuCopy");
        PasteMenuItem.Header = UiText.Get("MenuPaste");
        FindMenuItem.Header = UiText.Get("MenuFind");
        FindNextMenuItem.Header = UiText.Get("MenuFindNext");
        TimeDateMenuItem.Header = UiText.Get("MenuTimeDate");
        ViewMenuItem.Header = UiText.Get("MenuView");
        WordWrapMenuItem.Header = UiText.Get("MenuWordWrap");
        StatusBarMenuItem.Header = UiText.Get("MenuStatusBar");
        PreviewMenuItem.Header = UiText.Get("MenuMarkdownPreview");
        ThemeMenuItem.Header = UiText.Get("MenuAppTheme");
        ThemeSystemMenuItem.Header = UiText.Get("ThemeSystem");
        ThemeLightMenuItem.Header = UiText.Get("ThemeLight");
        ThemeDarkMenuItem.Header = UiText.Get("ThemeDark");
        MarkdownMenuItem.Header = UiText.Get("MenuMarkdown");
        InsertTableMenuItem.Header = UiText.Get("MenuInsertTable");
        AddTableRowMenuItem.Header = UiText.Get("MenuAddTableRow");
        InsertImageMenuItem.Header = UiText.Get("MenuInsertImage");
        PrivacyMenuItem.Header = UiText.Get("MenuPrivacy");
        CaptureExclusionMenuItem.Header = UiText.Get("MenuCaptureExclusion");
        HelpMenuItem.Header = UiText.Get("MenuHelp");
        AboutMenuItem.Header = UiText.Get("MenuAbout");

        FindLabelText.Text = UiText.Get("FindLabel");
        ReplaceLabelText.Text = UiText.Get("ReplaceLabel");
        FindNextButton.Content = UiText.Get("FindNextButton");
        ReplaceButton.Content = UiText.Get("ReplaceButton");
        ReplaceAllButton.Content = UiText.Get("ReplaceAllButton");
        CloseFindButton.Content = UiText.Get("CloseButton");
        ToolbarImageButton.Content = UiText.Get("ToolbarImage");
        ToolbarPreviewButton.Content = UiText.Get("ToolbarPreview");
        ToolbarPrivacyButton.Content = UiText.Get("ToolbarPrivacy");
        PrivacyStatusText.Text = UiText.Get("PrivacyStatusVisible");

        UpdateThemeMenuState(AppThemeService.RequestedTheme);
    }
}
