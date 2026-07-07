using System.IO;

namespace Memojang.App.Shell;

public static class StartupDocumentPath
{
    public static string? FromArgs(IEnumerable<string> args)
    {
        foreach (string arg in args)
        {
            string path = arg.Trim().Trim('"');
            if (!string.IsNullOrWhiteSpace(path) && File.Exists(path))
            {
                return path;
            }
        }

        return null;
    }
}
