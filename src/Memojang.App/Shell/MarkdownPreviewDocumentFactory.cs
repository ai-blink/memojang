using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Text.RegularExpressions;

namespace Memojang.App.Shell;

public static class MarkdownPreviewDocumentFactory
{
    private static readonly Regex BoldRegex = new(
        @"\*\*(?<text>.+?)\*\*",
        RegexOptions.Compiled);

    private static readonly Brush InlineAccentBrush = CreateInlineAccentBrush();

    public static FlowDocument Create(string markdown, string? documentPath = null)
    {
        var document = new FlowDocument
        {
            FontFamily = new FontFamily("Segoe UI"),
            FontSize = 14,
            PagePadding = new Thickness(0)
        };

        foreach (MarkdownPreviewBlock block in MarkdownPreviewParser.Parse(markdown))
        {
            document.Blocks.Add(CreatePreviewBlock(block, documentPath));
        }

        return document;
    }

    private static Block CreatePreviewBlock(MarkdownPreviewBlock block, string? documentPath)
    {
        return block.Kind switch
        {
            MarkdownPreviewBlockKind.CodeBlock => CreateCodeBlock(block, documentPath),
            MarkdownPreviewBlockKind.EmptyHeading => CreateEmptyHeading(block.Text),
            MarkdownPreviewBlockKind.Heading => CreateTextParagraph(
                block.Text,
                new Thickness(0, 0, 0, 10),
                block.HeadingLevel == 1 ? 24 : 20,
                FontWeights.SemiBold),
            MarkdownPreviewBlockKind.Image => CreateImagePreview(block, documentPath),
            MarkdownPreviewBlockKind.Table => CreateTablePreview(block.TableRows ?? []),
            MarkdownPreviewBlockKind.ThematicBreak => CreateThematicBreak(),
            MarkdownPreviewBlockKind.UnorderedList => CreateUnorderedList(block.ListItems ?? []),
            _ => CreateTextParagraph(block.Text, new Thickness(0, 0, 0, 8))
        };
    }

    private static Block CreateCodeBlock(MarkdownPreviewBlock block, string? documentPath)
    {
        return IsMarkdownCodeBlock(block.CodeLanguage)
            ? CreateMarkdownBox(block.Text, documentPath)
            : CreateRawCodeBlock(block);
    }

    private static Block CreateMarkdownBox(string markdown, string? documentPath)
    {
        var section = new Section
        {
            Background = new SolidColorBrush(Color.FromRgb(43, 43, 43)),
            BorderBrush = new SolidColorBrush(Color.FromRgb(90, 90, 90)),
            BorderThickness = new Thickness(1),
            Padding = new Thickness(10),
            Margin = new Thickness(0, 6, 0, 12)
        };

        foreach (MarkdownPreviewBlock innerBlock in MarkdownPreviewParser.Parse(markdown))
        {
            section.Blocks.Add(CreatePreviewBlock(innerBlock, documentPath));
        }

        return section;
    }

    private static Block CreateRawCodeBlock(MarkdownPreviewBlock block)
    {
        var panel = new StackPanel();

        if (!string.IsNullOrWhiteSpace(block.CodeLanguage))
        {
            panel.Children.Add(new TextBlock
            {
                Text = block.CodeLanguage,
                Foreground = Brushes.Silver,
                FontSize = 12,
                Margin = new Thickness(0, 0, 0, 6)
            });
        }

        panel.Children.Add(new TextBlock
        {
            Text = block.Text,
            FontFamily = new FontFamily("Consolas"),
            FontSize = 13,
            Foreground = Brushes.WhiteSmoke,
            TextWrapping = TextWrapping.Wrap
        });

        return new BlockUIContainer(new Border
        {
            Background = new SolidColorBrush(Color.FromRgb(43, 43, 43)),
            BorderBrush = new SolidColorBrush(Color.FromRgb(90, 90, 90)),
            BorderThickness = new Thickness(1),
            CornerRadius = new CornerRadius(4),
            Padding = new Thickness(10),
            Child = panel
        })
        {
            Margin = new Thickness(0, 6, 0, 12)
        };
    }

    private static bool IsMarkdownCodeBlock(string? language)
    {
        return language?.Trim().ToLowerInvariant() is "md" or "markdown";
    }

    private static Paragraph CreateEmptyHeading(string marker)
    {
        var paragraph = new Paragraph
        {
            FontSize = 13,
            FontWeight = FontWeights.SemiBold,
            Foreground = Brushes.Gray,
            Margin = new Thickness(0, 2, 0, 6)
        };

        paragraph.Inlines.Add(new Run(marker));
        return paragraph;
    }

    private static Paragraph CreateTextParagraph(
        string text,
        Thickness margin,
        double? fontSize = null,
        FontWeight? fontWeight = null)
    {
        var paragraph = new Paragraph
        {
            Margin = margin
        };

        if (fontSize is not null)
        {
            paragraph.FontSize = fontSize.Value;
        }

        if (fontWeight is not null)
        {
            paragraph.FontWeight = fontWeight.Value;
        }

        AddInlineMarkdown(paragraph.Inlines, text);
        return paragraph;
    }

    private static void AddInlineMarkdown(InlineCollection inlines, string text)
    {
        int currentIndex = 0;
        foreach (Match match in BoldRegex.Matches(text).Cast<Match>())
        {
            if (match.Index > currentIndex)
            {
                inlines.Add(new Run(text[currentIndex..match.Index]));
            }

            string boldText = match.Groups["text"].Value;
            var run = new Run(boldText)
            {
                FontWeight = FontWeights.Bold
            };

            if (boldText.Equals("skillhooks", StringComparison.OrdinalIgnoreCase))
            {
                run.Foreground = InlineAccentBrush;
            }

            inlines.Add(run);
            currentIndex = match.Index + match.Length;
        }

        if (currentIndex < text.Length)
        {
            inlines.Add(new Run(text[currentIndex..]));
        }
    }

    private static Block CreateUnorderedList(IReadOnlyList<MarkdownPreviewListItem> items)
    {
        var section = new Section
        {
            Margin = new Thickness(0, 2, 0, 8)
        };

        AddListItems(section.Blocks, items);
        return section;
    }

    private static void AddListItems(BlockCollection blocks, IReadOnlyList<MarkdownPreviewListItem> items)
    {
        foreach (MarkdownPreviewListItem item in items)
        {
            var paragraph = new Paragraph
            {
                Margin = new Thickness((item.Level - 1) * 18, 0, 0, 3),
                LineHeight = 20
            };

            paragraph.Inlines.Add(new Run($"{GetListMarker(item)} ")
            {
                FontWeight = FontWeights.SemiBold
            });
            AddInlineMarkdown(paragraph.Inlines, item.Text);
            blocks.Add(paragraph);

            if (item.Children.Count > 0)
            {
                AddListItems(blocks, item.Children);
            }
        }
    }

    private static string GetListMarker(MarkdownPreviewListItem item)
    {
        if (item.Children.Count > 0)
        {
            return "▶";
        }

        return item.Level switch
        {
            1 or 2 => "●",
            3 => "○",
            _ => "▪"
        };
    }

    private static Block CreateThematicBreak()
    {
        return new BlockUIContainer(new Border
        {
            Height = 1,
            Background = Brushes.Gray,
            Opacity = 0.65
        })
        {
            Margin = new Thickness(0, 8, 0, 14)
        };
    }

    private static Block CreateImagePreview(MarkdownPreviewBlock block, string? documentPath)
    {
        string source = block.ImageSource ?? string.Empty;
        MarkdownImagePathResolution resolution = MarkdownImagePathResolver.Resolve(source, documentPath);

        if (resolution.Exists && resolution.LocalPath is not null)
        {
            try
            {
                var image = new Image
                {
                    Source = LoadBitmap(resolution.LocalPath),
                    MaxWidth = 720,
                    Stretch = Stretch.Uniform,
                    HorizontalAlignment = HorizontalAlignment.Left,
                    ToolTip = resolution.Source
                };

                return new BlockUIContainer(image)
                {
                    Margin = new Thickness(0, 4, 0, 12)
                };
            }
            catch (IOException)
            {
            }
            catch (NotSupportedException)
            {
            }
        }

        string label = resolution.IsRemote
            ? $"Remote image not loaded: {source}"
            : $"Missing image: {CreateImageLabel(block, source)}";

        var placeholder = new Border
        {
            BorderBrush = Brushes.Gainsboro,
            BorderThickness = new Thickness(1),
            Padding = new Thickness(10),
            Child = new TextBlock
            {
                Text = label,
                Foreground = Brushes.Gray,
                TextWrapping = TextWrapping.Wrap
            }
        };

        return new BlockUIContainer(placeholder)
        {
            Margin = new Thickness(0, 4, 0, 12)
        };
    }

    private static BitmapSource LoadBitmap(string path)
    {
        var bitmap = new BitmapImage();
        bitmap.BeginInit();
        bitmap.CacheOption = BitmapCacheOption.OnLoad;
        bitmap.UriSource = new Uri(path, UriKind.Absolute);
        bitmap.DecodePixelWidth = 1400;
        bitmap.EndInit();
        bitmap.Freeze();
        return bitmap;
    }

    private static string CreateImageLabel(MarkdownPreviewBlock block, string source)
    {
        return string.IsNullOrWhiteSpace(block.ImageAltText)
            ? source
            : $"{block.ImageAltText} ({source})";
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
                tableRow.Cells.Add(new TableCell(CreateTextParagraph(text, new Thickness(0)))
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

    private static Brush CreateInlineAccentBrush()
    {
        var brush = new SolidColorBrush(Color.FromRgb(76, 194, 255));
        brush.Freeze();
        return brush;
    }
}
