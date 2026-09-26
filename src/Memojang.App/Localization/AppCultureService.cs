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

    public const string OverrideVariable = "MEMOJANG_UI_CULTURE";

    public static CultureInfo ApplySystemCulture()
    {
        CultureInfo culture = SelectSupportedCulture(
            ResolveRequestedCulture(Environment.GetEnvironmentVariable(OverrideVariable), CultureInfo.CurrentUICulture));
        CultureInfo.CurrentCulture = culture;
        CultureInfo.CurrentUICulture = culture;
        Thread.CurrentThread.CurrentCulture = culture;
        Thread.CurrentThread.CurrentUICulture = culture;
        return culture;
    }

    // Lets testers view any locale (e.g. RTL Arabic) without changing the Windows display language.
    public static CultureInfo ResolveRequestedCulture(string? overrideName, CultureInfo systemCulture)
    {
        if (string.IsNullOrWhiteSpace(overrideName))
        {
            return systemCulture;
        }

        try
        {
            // ICU accepts almost any tag, so only honor overrides for languages Memojang actually ships.
            CultureInfo candidate = CultureInfo.GetCultureInfo(overrideName.Trim());
            return SupportedLanguages.Contains(candidate.TwoLetterISOLanguageName) ? candidate : systemCulture;
        }
        catch (CultureNotFoundException)
        {
            return systemCulture;
        }
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
