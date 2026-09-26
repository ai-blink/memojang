using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using Memojang.App.Localization;
using Memojang.App.Shell;

namespace Memojang.App;

public partial class MainWindow
{
    private void InitializeLineEndingAndEncodingControls()
    {
        // WPF TextBox always inserts CRLF on Enter and keeps pasted endings as-is; both would quietly turn an
        // LF file into a mixed one, so new line breaks follow the document's own ending.
        EditorTextBox.PreviewKeyDown += EditorTextBox_LineEndingPreviewKeyDown;
        DataObject.AddPastingHandler(EditorTextBox, EditorTextBox_Pasting);

        MakeStatusMenu(EncodingStatusText, "StatusEncodingHint", CreateEncodingMenu);
        MakeStatusMenu(LineEndingStatusText, "StatusLineEndingHint", CreateLineEndingMenu);
    }

    private void EditorTextBox_LineEndingPreviewKeyDown(object sender, KeyEventArgs e)
    {
        if (e.Key != Key.Enter || _lineEndingName == LineEnding.Crlf
            || Keyboard.Modifiers is not (ModifierKeys.None or ModifierKeys.Shift))
        {
            return;
        }

        string ending = GetLineEndingValue();
        int start = EditorTextBox.SelectionStart;
        EditorTextBox.SelectedText = ending;
        EditorTextBox.Select(start + ending.Length, 0);
        e.Handled = true;
    }

    private void EditorTextBox_Pasting(object sender, DataObjectPastingEventArgs e)
    {
        if (e.DataObject.GetData(DataFormats.UnicodeText) is string pasted)
        {
            string normalized = LineEnding.Normalize(pasted, _lineEndingName);
            if (normalized != pasted)
            {
                e.DataObject = new DataObject(DataFormats.UnicodeText, normalized);
            }
        }
    }

    private void MakeStatusMenu(FrameworkElement target, string hintKey, Func<ContextMenu> createMenu)
    {
        target.Cursor = Cursors.Hand;
        target.ToolTip = UiText.Get(hintKey);
        target.MouseLeftButtonUp += (_, _) =>
        {
            ContextMenu menu = createMenu();
            menu.PlacementTarget = target;
            menu.Placement = PlacementMode.Top;
            menu.IsOpen = true;
        };
    }

    private ContextMenu CreateEncodingMenu()
    {
        var menu = new ContextMenu();
        TextFileEncoding[] choices =
        [
            TextFileEncoding.Utf8,
            TextFileEncoding.Utf8Bom,
            TextFileEncoding.Utf16Le,
            TextFileEncoding.Utf16Be,
            _fileEncoding.IsUnicode ? TextFileEncoding.Ansi(TextFileCodec.SystemAnsiCodePage) : _fileEncoding
        ];

        foreach (TextFileEncoding choice in choices)
        {
            var item = new MenuItem
            {
                Header = UiText.Format("StatusSaveWithEncoding", choice.DisplayName),
                IsCheckable = true,
                IsChecked = choice == _fileEncoding
            };
            item.Click += (_, _) => SetSaveEncoding(choice);
            menu.Items.Add(item);
        }

        return menu;
    }

    // Changes how the next save writes bytes; the text itself is unchanged, so only the dirty flag moves.
    private void SetSaveEncoding(TextFileEncoding encoding)
    {
        if (encoding == _fileEncoding)
        {
            return;
        }

        _fileEncoding = encoding;
        _hasUnsavedChanges = true;
        PersistActiveDocumentState();
        UpdateTitle();
        UpdateStatusFromEditor();
    }

    private ContextMenu CreateLineEndingMenu()
    {
        var menu = new ContextMenu();
        foreach (string lineEnding in new[] { LineEnding.Crlf, LineEnding.Lf, LineEnding.Cr })
        {
            var item = new MenuItem
            {
                Header = UiText.Format("StatusConvertLineEnding", lineEnding),
                IsCheckable = true,
                IsChecked = lineEnding == _lineEndingName
            };
            item.Click += (_, _) => ConvertLineEndings(lineEnding);
            menu.Items.Add(item);
        }

        return menu;
    }

    private void ConvertLineEndings(string lineEnding)
    {
        string converted = LineEnding.Normalize(EditorTextBox.Text, lineEnding);
        if (converted == EditorTextBox.Text)
        {
            return;
        }

        // Whole-text selection replacement keeps the conversion as one undo step; TextChanged re-detects the ending.
        int caretIndex = EditorTextBox.CaretIndex;
        EditorTextBox.SelectAll();
        EditorTextBox.SelectedText = converted;
        EditorTextBox.Select(Math.Min(caretIndex, converted.Length), 0);
        EditorTextBox.Focus();
    }
}
