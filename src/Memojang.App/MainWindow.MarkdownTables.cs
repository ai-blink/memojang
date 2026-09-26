using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Memojang.App.Localization;
using Memojang.App.Shell;

namespace Memojang.App;

public partial class MainWindow
{
    private MenuItem? _tableContextMenuItem;

    private void InitializeTableEditing()
    {
        EditorTextBox.PreviewKeyDown += EditorTextBox_TablePreviewKeyDown;
        EditorTextBox.PreviewMouseRightButtonDown += EditorTextBox_PreviewMouseRightButtonDown;
        EditorTextBox.ContextMenu = CreateEditorContextMenu();
        EditorTextBox.ContextMenuOpening += (_, _) =>
        {
            _tableContextMenuItem!.Visibility = IsCaretInMarkdownTable() ? Visibility.Visible : Visibility.Collapsed;
        };
    }

    // Tab moves between cells only inside a Markdown table; everywhere else it still types a tab like Notepad.
    private void EditorTextBox_TablePreviewKeyDown(object sender, KeyEventArgs e)
    {
        if (e.Key != Key.Tab || !IsCaretInMarkdownTable())
        {
            return;
        }

        if (Keyboard.Modifiers is ModifierKeys.None or ModifierKeys.Shift)
        {
            e.Handled = ApplyTableCommand(
                Keyboard.Modifiers == ModifierKeys.Shift ? TableCommand.PreviousCell : TableCommand.NextCell);
        }
    }

    private void EditorTextBox_PreviewMouseRightButtonDown(object sender, MouseButtonEventArgs e)
    {
        // WPF keeps the old caret on right-click; move it so table commands act on the clicked cell.
        int index = EditorTextBox.GetCharacterIndexFromPoint(e.GetPosition(EditorTextBox), snapToText: true);
        if (index >= 0 && EditorTextBox.SelectionLength == 0)
        {
            EditorTextBox.CaretIndex = index;
        }
    }

    private bool IsCaretInMarkdownTable() =>
        _documentMode == DocumentMode.Markdown && MarkdownTableEditor.IsInTable(EditorTextBox.Text, EditorTextBox.CaretIndex);

    private bool ApplyTableCommand(TableCommand command)
    {
        if (!MarkdownTableEditor.TryApply(EditorTextBox.Text, EditorTextBox.CaretIndex, command, out TableEdit edit))
        {
            return false;
        }

        // Replacing through the selection keeps the edit as a single undo step.
        if (EditorTextBox.Text.Substring(edit.Start, edit.Length) != edit.Replacement)
        {
            EditorTextBox.Select(edit.Start, edit.Length);
            EditorTextBox.SelectedText = edit.Replacement;
        }

        EditorTextBox.Select(edit.CaretIndex, 0);
        EditorTextBox.Focus();
        return true;
    }

    private ContextMenu CreateEditorContextMenu()
    {
        var menu = new ContextMenu();
        menu.Items.Add(new MenuItem { Header = UiText.Get("MenuCut"), Command = ApplicationCommands.Cut });
        menu.Items.Add(new MenuItem { Header = UiText.Get("MenuCopy"), Command = ApplicationCommands.Copy });
        menu.Items.Add(new MenuItem { Header = UiText.Get("MenuPaste"), Command = ApplicationCommands.Paste });
        menu.Items.Add(new MenuItem { Header = UiText.Get("MenuSelectAll"), Command = ApplicationCommands.SelectAll });

        _tableContextMenuItem = new MenuItem { Header = UiText.Get("ContextTable") };
        (string Key, TableCommand Command)[] tableItems =
        [
            ("TableAddRowBelow", TableCommand.AddRowBelow),
            ("TableDeleteRow", TableCommand.DeleteRow),
            ("TableInsertColumnLeft", TableCommand.InsertColumnLeft),
            ("TableInsertColumnRight", TableCommand.InsertColumnRight),
            ("TableDeleteColumn", TableCommand.DeleteColumn),
            ("TableAlignLeft", TableCommand.AlignLeft),
            ("TableAlignCenter", TableCommand.AlignCenter),
            ("TableAlignRight", TableCommand.AlignRight),
            ("TableFormat", TableCommand.Format)
        ];

        foreach ((string key, TableCommand command) in tableItems)
        {
            if (key is "TableInsertColumnLeft" or "TableAlignLeft" or "TableFormat")
            {
                _tableContextMenuItem.Items.Add(new Separator());
            }

            var item = new MenuItem { Header = UiText.Get(key) };
            item.Click += (_, _) => ApplyTableCommand(command);
            _tableContextMenuItem.Items.Add(item);
        }

        menu.Items.Add(new Separator());
        menu.Items.Add(_tableContextMenuItem);
        return menu;
    }
}
