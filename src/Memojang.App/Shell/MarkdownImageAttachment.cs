using System.IO;

namespace Memojang.App.Shell;

public sealed record MarkdownImageAttachment(
    string SourcePath,
    string TargetPath,
    string MarkdownText);

public static class MarkdownImageAttachmentService
{
    private static readonly HashSet<string> SupportedImageExtensions = new(StringComparer.OrdinalIgnoreCase)
    {
        ".bmp",
        ".gif",
        ".jpeg",
        ".jpg",
        ".png",
        ".webp"
    };

    public static bool IsSupportedImagePath(string filePath)
    {
        return SupportedImageExtensions.Contains(Path.GetExtension(filePath));
    }

    public static IReadOnlyList<MarkdownImageAttachment> CopyImagesToSidecar(
        string documentPath,
        IEnumerable<string> sourcePaths,
        DateTime timestamp)
    {
        var copiedTargets = new List<string>();
        var attachments = new List<MarkdownImageAttachment>();

        try
        {
            foreach (string sourcePath in sourcePaths.Where(IsSupportedImagePath))
            {
                string targetPath = GetAvailableImagePath(documentPath, Path.GetExtension(sourcePath), timestamp);
                Directory.CreateDirectory(Path.GetDirectoryName(targetPath)!);
                File.Copy(sourcePath, targetPath);
                copiedTargets.Add(targetPath);

                string altText = CreateAltText(Path.GetFileNameWithoutExtension(sourcePath));
                attachments.Add(new MarkdownImageAttachment(
                    sourcePath,
                    targetPath,
                    CreateMarkdownImageLink(documentPath, targetPath, altText)));
            }

            return attachments;
        }
        catch
        {
            foreach (string copiedTarget in copiedTargets)
            {
                TryDelete(copiedTarget);
            }

            throw;
        }
    }

    public static string GetSidecarDirectory(string documentPath)
    {
        string directory = Path.GetDirectoryName(documentPath) ?? Directory.GetCurrentDirectory();
        string documentName = Path.GetFileNameWithoutExtension(documentPath);
        return Path.Combine(directory, $"{documentName}.assets");
    }

    public static string GetAvailableImagePath(string documentPath, string extension, DateTime timestamp)
    {
        string safeExtension = string.IsNullOrWhiteSpace(extension) ? ".png" : extension;
        string directory = GetSidecarDirectory(documentPath);
        string baseName = $"image-{timestamp:yyyyMMdd-HHmmss}";
        string candidate = Path.Combine(directory, $"{baseName}{safeExtension}");

        for (int suffix = 2; File.Exists(candidate); suffix++)
        {
            candidate = Path.Combine(directory, $"{baseName}-{suffix}{safeExtension}");
        }

        return candidate;
    }

    public static string CreateMarkdownImageLink(string documentPath, string targetPath, string altText)
    {
        string documentDirectory = Path.GetDirectoryName(documentPath) ?? Directory.GetCurrentDirectory();
        string relativePath = Path.GetRelativePath(documentDirectory, targetPath).Replace('\\', '/');
        return $"![{CreateAltText(altText)}]({EncodeMarkdownPath(relativePath)})";
    }

    private static string CreateAltText(string value)
    {
        return value.Replace("[", string.Empty, StringComparison.Ordinal)
            .Replace("]", string.Empty, StringComparison.Ordinal)
            .Trim();
    }

    private static string EncodeMarkdownPath(string relativePath)
    {
        return string.Join(
            "/",
            relativePath.Split('/').Select(segment => Uri.EscapeDataString(segment)));
    }

    private static void TryDelete(string path)
    {
        try
        {
            File.Delete(path);
        }
        catch (IOException)
        {
        }
        catch (UnauthorizedAccessException)
        {
        }
    }
}
