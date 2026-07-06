using System.IO;

namespace Memojang.App.Shell;

public sealed record MarkdownImagePathResolution(
    string Source,
    string? LocalPath,
    bool Exists,
    bool IsRemote);

public static class MarkdownImagePathResolver
{
    public static MarkdownImagePathResolution Resolve(string source, string? documentPath)
    {
        string normalizedSource = source.Trim();
        if (normalizedSource.Length == 0)
        {
            return new MarkdownImagePathResolution(normalizedSource, null, Exists: false, IsRemote: false);
        }

        if (Uri.TryCreate(normalizedSource, UriKind.Absolute, out Uri? uri)
            && (uri.Scheme == Uri.UriSchemeHttp || uri.Scheme == Uri.UriSchemeHttps))
        {
            return new MarkdownImagePathResolution(normalizedSource, null, Exists: false, IsRemote: true);
        }

        string decodedSource = Uri.UnescapeDataString(normalizedSource.Replace('/', Path.DirectorySeparatorChar));
        string localPath = Path.IsPathRooted(decodedSource)
            ? decodedSource
            : ResolveRelativePath(decodedSource, documentPath);

        return new MarkdownImagePathResolution(
            normalizedSource,
            localPath,
            File.Exists(localPath),
            IsRemote: false);
    }

    private static string ResolveRelativePath(string source, string? documentPath)
    {
        if (string.IsNullOrWhiteSpace(documentPath))
        {
            return Path.GetFullPath(source);
        }

        string? documentDirectory = Path.GetDirectoryName(documentPath);
        return Path.GetFullPath(Path.Combine(documentDirectory ?? Directory.GetCurrentDirectory(), source));
    }
}
