using System.Windows;
using System.Windows.Documents;
using System.Windows.Media;

namespace Memojang.App.Shell;

public static class MarkdownPreviewDocumentFactory
{
    public static FlowDocument Create(string markdown)
    {
        var document = new FlowDocument
        {
            FontFamily = new FontFamily("Segoe UI"),
            FontSize = 14,
            PagePadding = new Thickness(0)
        };

        foreach (MarkdownPreviewBlock block in MarkdownPreviewParser.Parse(markdown))
        {
            document.Blocks.Add(CreatePreviewBlock(block));
        }

        return document;
    }

    private static Block CreatePreviewBlock(MarkdownPreviewBlock block)
    {
        return block.Kind switch
        {
            MarkdownPreviewBlockKind.Heading => new Paragraph(new Run(block.Text))
            {
                FontSize = block.HeadingLevel == 1 ? 24 : 20,
                FontWeight = FontWeights.SemiBold,
                Margin = new Thickness(0, 0, 0, 10)
            },
            MarkdownPreviewBlockKind.Table => CreateTablePreview(block.TableRows ?? []),
            _ => new Paragraph(new Run(block.Text))
            {
                Margin = new Thickness(0, 0, 0, 8)
            }
        };
    }

    private static Block CreateTablePreview(IReadOnlyList<IReadOnlyList<string>> rows)
    {
        var table = new Table
        {
            CellSpacing = 0,
            Margin = new Thickness(0, 4, 0, 12)
        };

        int columnCount = rows.Count == 0 ? 0 : rows.Max(row => row.Count);
        for (int i = 0; i < columnCount; i++)
        {
            table.Columns.Add(new TableColumn());
        }

        var group = new TableRowGroup();
        table.RowGroups.Add(group);

        for (int rowIndex = 0; rowIndex < rows.Count; rowIndex++)
        {
            var tableRow = new TableRow();
            group.Rows.Add(tableRow);

            for (int columnIndex = 0; columnIndex < columnCount; columnIndex++)
            {
                string text = columnIndex < rows[rowIndex].Count ? rows[rowIndex][columnIndex] : string.Empty;
                tableRow.Cells.Add(new TableCell(new Paragraph(new Run(text)))
                {
                    BorderBrush = Brushes.Gainsboro,
                    BorderThickness = new Thickness(0, 0, 1, 1),
                    FontWeight = rowIndex == 0 ? FontWeights.SemiBold : FontWeights.Normal,
                    Padding = new Thickness(6, 4, 6, 4)
                });
            }
        }

        return table;
    }
}
