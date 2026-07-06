using System.Windows;
using Memojang.App.Localization;
using Memojang.App.Theme;

namespace Memojang.App;

public partial class App : Application
{
    static App()
    {
        EnsureWindowsDirectoryEnvironment();
    }

    protected override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);
        AppCultureService.ApplySystemCulture();

        var mainWindow = new MainWindow();
        MainWindow = mainWindow;
        AppThemeService.Apply(this, AppTheme.System);
        mainWindow.Show();
    }

    private static void EnsureWindowsDirectoryEnvironment()
    {
        string windowsDirectory = Environment.GetFolderPath(Environment.SpecialFolder.Windows);
        if (string.IsNullOrWhiteSpace(windowsDirectory))
        {
            windowsDirectory = Environment.GetEnvironmentVariable("SystemRoot") ?? @"C:\Windows";
        }

        if (string.IsNullOrWhiteSpace(Environment.GetEnvironmentVariable("WINDIR")))
        {
            Environment.SetEnvironmentVariable("WINDIR", windowsDirectory);
        }

        if (string.IsNullOrWhiteSpace(Environment.GetEnvironmentVariable("WinDir")))
        {
            Environment.SetEnvironmentVariable("WinDir", windowsDirectory);
        }
    }
}
