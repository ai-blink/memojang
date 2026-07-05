using System.Windows;
using Memojang.App.Localization;
using Memojang.App.Theme;

namespace Memojang.App;

public partial class App : Application
{
    protected override void OnStartup(StartupEventArgs e)
    {
        AppCultureService.ApplySystemCulture();
        AppThemeService.Apply(this, AppTheme.System);

        var mainWindow = new MainWindow();
        MainWindow = mainWindow;
        mainWindow.Show();

        base.OnStartup(e);
    }
}
