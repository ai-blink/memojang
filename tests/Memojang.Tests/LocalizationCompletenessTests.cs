namespace Memojang.Tests;

using System.Collections;
using System.Globalization;
using System.Resources;
using System.Text.RegularExpressions;

public class LocalizationCompletenessTests
{
    private static readonly ResourceManager Strings =
        new("Memojang.App.Resources.Strings", typeof(Memojang.App.App).Assembly);

    public static TheoryData<string> Cultures => new() { "ar", "bn", "es", "fr", "hi", "id", "ja", "ko", "pt", "ru", "zh" };

    [Theory]
    [MemberData(nameof(Cultures))]
    public void Culture_TranslatesEveryKeyWithMatchingPlaceholdersAndAccessKeys(string cultureName)
    {
        Dictionary<string, string> neutral = ReadSet(CultureInfo.InvariantCulture);
        Dictionary<string, string> localized = ReadSet(CultureInfo.GetCultureInfo(cultureName));

        Assert.Empty(neutral.Keys.Except(localized.Keys));
        foreach ((string key, string source) in neutral)
        {
            string translated = localized[key];
            Assert.True(Placeholders(source).SetEquals(Placeholders(translated)), $"{cultureName}:{key} placeholders differ");
            Assert.True(
                source.Contains('_') == translated.Contains('_'),
                $"{cultureName}:{key} access-key marker mismatch");
        }
    }

    private static Dictionary<string, string> ReadSet(CultureInfo culture)
    {
        ResourceSet set = Strings.GetResourceSet(culture, createIfNotExists: true, tryParents: false)
            ?? throw new InvalidOperationException($"No resources for {culture.Name}");
        return set.Cast<DictionaryEntry>().ToDictionary(entry => (string)entry.Key, entry => (string)entry.Value!);
    }

    private static HashSet<string> Placeholders(string text) =>
        Regex.Matches(text, @"\{\d+\}").Select(match => match.Value).ToHashSet();
}
