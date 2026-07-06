using System.Windows;
using Microsoft.Win32;

namespace Memojang.App.Theme;

public static class AppThemeService
{
    private const string PersonalizeKeyPath = @"Software\Microsoft\Windows\CurrentVersion\Themes\Personalize";
    private const string AppsUseLightThemeValue = "AppsUseLightTheme";

    public static AppTheme RequestedTheme { get; private set; } = AppTheme.System;

    public static bool IsDarkThemeActive { get; private set; }

    public static void Apply(Application application, AppTheme requestedTheme)
    {
        RequestedTheme = requestedTheme;
        IsDarkThemeActive = ResolveDarkTheme(requestedTheme);

        var dictionaries = application.Resources.MergedDictionaries;
        for (int i = dictionaries.Count - 1; i >= 0; i--)
        {
            if (dictionaries[i].Source?.OriginalString.Contains("/Themes/", StringComparison.OrdinalIgnoreCase) == true)
            {
                dictionaries.RemoveAt(i);
            }
        }

        string themeName = IsDarkThemeActive ? "Dark" : "Light";
        dictionaries.Add(new ResourceDictionary
        {
            Source = new Uri($"pack://application:,,,/Themes/{themeName}.xaml", UriKind.Absolute)
        });
    }

    private static bool ResolveDarkTheme(AppTheme requestedTheme)
    {
        return requestedTheme switch
        {
            AppTheme.Dark => true,
            AppTheme.Light => false,
            _ => IsSystemAppThemeDark()
        };
    }

    private static bool IsSystemAppThemeDark()
    {
        object? value = Registry.CurrentUser.OpenSubKey(PersonalizeKeyPath)?.GetValue(AppsUseLightThemeValue);
        return value is int appsUseLightTheme && appsUseLightTheme == 0;
    }
}
