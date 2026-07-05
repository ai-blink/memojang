using System.Globalization;
using System.Reflection;
using System.Resources;
using System.Windows;

namespace Memojang.App.Localization;

public static class UiText
{
    private static readonly ResourceManager ResourceManager =
        new("Memojang.App.Resources.Strings", Assembly.GetExecutingAssembly());

    public static string Get(string key)
    {
        return ResourceManager.GetString(key, CultureInfo.CurrentUICulture) ?? key;
    }

    public static string Format(string key, params object[] args)
    {
        return string.Format(CultureInfo.CurrentUICulture, Get(key), args);
    }

    public static FlowDirection FlowDirection =>
        CultureInfo.CurrentUICulture.TextInfo.IsRightToLeft ? FlowDirection.RightToLeft : FlowDirection.LeftToRight;
}
