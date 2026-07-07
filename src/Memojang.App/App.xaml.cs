using System.Windows;
using System.Windows.Threading;
using Memojang.App.Localization;
using Memojang.App.Shell;
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
        DispatcherUnhandledException += App_DispatcherUnhandledException;
        AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;
        AppDiagnostics.Log($"Startup args=[{string.Join(", ", e.Args)}]");
        AppCultureService.ApplySystemCulture();

        var mainWindow = new MainWindow();
        MainWindow = mainWindow;
        AppThemeService.Apply(this, AppTheme.System);
        mainWindow.Show();
        mainWindow.Activate();
        AppDiagnostics.Log("MainWindow shown");

        string? startupDocumentPath = StartupDocumentPath.FromArgs(e.Args);
        AppDiagnostics.Log($"Startup document path={startupDocumentPath ?? "<none>"}");
        if (!string.IsNullOrWhiteSpace(startupDocumentPath))
        {
            mainWindow.Dispatcher.BeginInvoke(
                async () => await mainWindow.OpenDocumentAsync(startupDocumentPath),
                DispatcherPriority.ApplicationIdle);
        }
    }

    private static void App_DispatcherUnhandledException(object sender, DispatcherUnhandledExceptionEventArgs e)
    {
        AppDiagnostics.Log($"DispatcherUnhandledException {e.Exception}");
    }

    private static void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
    {
        AppDiagnostics.Log($"UnhandledException {e.ExceptionObject}");
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
