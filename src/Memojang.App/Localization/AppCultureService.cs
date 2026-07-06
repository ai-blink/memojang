using System.Globalization;

namespace Memojang.App.Localization;

public static class AppCultureService
{
    private static readonly HashSet<string> SupportedLanguages = new(StringComparer.OrdinalIgnoreCase)
    {
        "ar",
        "bn",
        "en",
        "es",
        "fr",
        "hi",
        "id",
        "ja",
        "ko",
        "pt",
        "ru",
        "zh"
    };

    public static CultureInfo ApplySystemCulture()
    {
        CultureInfo culture = SelectSupportedCulture(CultureInfo.CurrentUICulture);
        CultureInfo.CurrentCulture = culture;
        CultureInfo.CurrentUICulture = culture;
        Thread.CurrentThread.CurrentCulture = culture;
        Thread.CurrentThread.CurrentUICulture = culture;
        return culture;
    }

    public static CultureInfo SelectSupportedCulture(CultureInfo requestedCulture)
    {
        for (CultureInfo? culture = requestedCulture; culture is not null && !string.IsNullOrEmpty(culture.Name); culture = culture.Parent)
        {
            if (SupportedLanguages.Contains(culture.Name))
            {
                return culture;
            }

            if (SupportedLanguages.Contains(culture.TwoLetterISOLanguageName))
            {
                return requestedCulture.IsNeutralCulture
                    ? CultureInfo.CreateSpecificCulture(culture.TwoLetterISOLanguageName)
                    : requestedCulture;
            }
        }

        return CultureInfo.GetCultureInfo("en-US");
    }
}
