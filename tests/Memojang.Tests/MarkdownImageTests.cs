namespace Memojang.Tests;

using Memojang.App.Shell;

public class MarkdownImageTests
{
    [Fact]
    public void MarkdownPreviewParser_ParsesStandaloneImage()
    {
        var blocks = MarkdownPreviewParser.Parse("![Diagram](example.assets/image.png)");

        var block = Assert.Single(blocks);
        Assert.Equal(MarkdownPreviewBlockKind.Image, block.Kind);
        Assert.Equal("Diagram", block.ImageAltText);
        Assert.Equal("example.assets/image.png", block.ImageSource);
    }

    [Fact]
    public void MarkdownImagePathResolver_ResolvesRelativePathFromDocument()
    {
        using var workspace = new TempWorkspace();
        string documentPath = workspace.WriteText("notes/example.md", "# Example");
        string imagePath = workspace.WriteBytes("notes/example.assets/image.png", [1, 2, 3]);

        MarkdownImagePathResolution resolution =
            MarkdownImagePathResolver.Resolve("example.assets/image.png", documentPath);

        Assert.True(resolution.Exists);
        Assert.Equal(Path.GetFullPath(imagePath), resolution.LocalPath);
    }

    [Fact]
    public void MarkdownImageAttachmentService_CopiesImageToSidecarAndCreatesRelativeLink()
    {
        using var workspace = new TempWorkspace();
        string documentPath = workspace.WriteText("notes/example.md", "# Example");
        string sourcePath = workspace.WriteBytes("source.png", [1, 2, 3]);
        var timestamp = new DateTime(2026, 7, 7, 12, 0, 0);

        IReadOnlyList<MarkdownImageAttachment> attachments =
            MarkdownImageAttachmentService.CopyImagesToSidecar(documentPath, [sourcePath], timestamp);

        MarkdownImageAttachment attachment = Assert.Single(attachments);
        Assert.True(File.Exists(attachment.TargetPath));
        Assert.EndsWith(Path.Combine("example.assets", "image-20260707-120000.png"), attachment.TargetPath);
        Assert.Equal("![source](example.assets/image-20260707-120000.png)", attachment.MarkdownText);
    }

    [Fact]
    public void MarkdownImageAttachmentService_AvoidsTargetNameCollision()
    {
        using var workspace = new TempWorkspace();
        string documentPath = workspace.WriteText("notes/example.md", "# Example");
        string sourcePath = workspace.WriteBytes("source.png", [1, 2, 3]);
        workspace.WriteBytes("notes/example.assets/image-20260707-120000.png", [9]);
        var timestamp = new DateTime(2026, 7, 7, 12, 0, 0);

        IReadOnlyList<MarkdownImageAttachment> attachments =
            MarkdownImageAttachmentService.CopyImagesToSidecar(documentPath, [sourcePath], timestamp);

        MarkdownImageAttachment attachment = Assert.Single(attachments);
        Assert.EndsWith(Path.Combine("example.assets", "image-20260707-120000-2.png"), attachment.TargetPath);
        Assert.Equal("![source](example.assets/image-20260707-120000-2.png)", attachment.MarkdownText);
    }

    private sealed class TempWorkspace : IDisposable
    {
        private readonly string _root = Path.Combine(Path.GetTempPath(), $"memojang-tests-{Guid.NewGuid():N}");

        public string WriteText(string relativePath, string text)
        {
            string path = GetPath(relativePath);
            Directory.CreateDirectory(Path.GetDirectoryName(path)!);
            File.WriteAllText(path, text);
            return path;
        }

        public string WriteBytes(string relativePath, byte[] bytes)
        {
            string path = GetPath(relativePath);
            Directory.CreateDirectory(Path.GetDirectoryName(path)!);
            File.WriteAllBytes(path, bytes);
            return path;
        }

        public void Dispose()
        {
            if (Directory.Exists(_root))
            {
                Directory.Delete(_root, recursive: true);
            }
        }

        private string GetPath(string relativePath)
        {
            return Path.Combine(_root, relativePath.Replace('/', Path.DirectorySeparatorChar));
        }
    }
}
