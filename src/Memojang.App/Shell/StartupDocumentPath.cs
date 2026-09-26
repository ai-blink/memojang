using System.IO;

namespace Memojang.App.Shell;

public static class StartupDocumentPath
{
    // Resolved to absolute paths at launch: relative args depend on the launching CWD, which the app resets
    // and which the primary instance never sees when the paths are forwarded.
    public static IReadOnlyList<string> FromArgs(IEnumerable<string> args)
    {
        var paths = new List<string>();
        foreach (string arg in args)
        {
            string path = arg.Trim().Trim('"');
            if (string.IsNullOrWhiteSpace(path) || !File.Exists(path))
            {
                continue;
            }

            string fullPath = Path.GetFullPath(path);
            if (!paths.Contains(fullPath, StringComparer.OrdinalIgnoreCase))
            {
                paths.Add(fullPath);
            }
        }

        return paths;
    }
}
