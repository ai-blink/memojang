using System.IO;
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
        EnsureStableWorkingDirectory();
        DispatcherUnhandledException += App_DispatcherUnhandledException;
        AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;
        AppDiagnostics.Log($"Startup args=[{string.Join(", ", e.Args)}]");
        AppCultureService.ApplySystemCulture();
        AppThemeService.Apply(this, AppTheme.System);

        var mainWindow = new MainWindow();
        MainWindow = mainWindow;
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

    // 파일 연결/"열기"로 실행되면 프로세스 작업 디렉토리(CWD)가 문서 폴더로 상속돼,
    // 앱이 살아있는 동안 그 폴더 삭제가 막힌다(디렉토리는 자기 CWD를 삭제할 수 없음).
    // 시작 시 앱 기준 폴더로 되돌려 임의 문서 폴더를 인질로 잡지 않게 한다.
    private static void EnsureStableWorkingDirectory()
    {
        try
        {
            Directory.SetCurrentDirectory(AppContext.BaseDirectory);
        }
        catch (Exception ex)
        {
            AppDiagnostics.Log($"EnsureStableWorkingDirectory failed {ex}");
        }
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
