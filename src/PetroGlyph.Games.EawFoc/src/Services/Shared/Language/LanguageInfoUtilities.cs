using System.Collections.Generic;
using System.Globalization;
using System.Threading;
using EawModinfo.Model;
using EawModinfo.Spec;

namespace PG.StarWarsGame.Infrastructure.Services.Language;

/// <summary>
/// Utilities for <see cref="ILanguageInfo"/>.
/// </summary>
public static class LanguageInfoUtilities
{
    private static IDictionary<string, CultureInfo>? _culturesByName;

    private static IDictionary<string, CultureInfo> CulturesByName
    {
        get
        {
            var cultures = LazyInitializer.EnsureInitialized(ref _culturesByName,
                () =>
                {
                    var dictionary = new Dictionary<string, CultureInfo>();
                    foreach (var culture in CultureInfo.GetCultures(CultureTypes.NeutralCultures))
                    {
                        var englishName = culture.EnglishName.ToLowerInvariant();
                        if (!dictionary.ContainsKey(englishName))
                            dictionary.Add(englishName, culture);
                    }
                    return dictionary;
                });
            return cultures!;
        }
    }

    private static IDictionary<string, CultureInfo>? _culturesByCode;

    private static IDictionary<string, CultureInfo> CulturesByCode
    {
        get
        {
            var cultures = LazyInitializer.EnsureInitialized(ref _culturesByCode,
                () =>
                {
                    var dictionary = new Dictionary<string, CultureInfo>();
                    foreach (var culture in CultureInfo.GetCultures(CultureTypes.NeutralCultures))
                    {
                        var tln = culture.TwoLetterISOLanguageName.ToLowerInvariant();
                        if (!dictionary.ContainsKey(tln))
                            dictionary.Add(tln, culture);
                    }
                    return dictionary;
                });
            return cultures!;
        }
    }

    /// <summary>
    /// Gets the english language name of a given <see cref="ILanguageInfo"/>.
    /// </summary>
    /// <param name="languageInfo">The info to get the english name from.</param>
    /// <returns>The english language name.</returns>
    /// <exception cref="CultureNotFoundException">If the language could not be determined.</exception>
    public static string GetEnglishName(ILanguageInfo languageInfo)
    {
        var code = languageInfo.Code.ToLowerInvariant();
        if (!CulturesByCode.TryGetValue(code, out var culture))
            throw new CultureNotFoundException($"Unable to get culture for language {code}");
        return culture.EnglishName;
    }


    /// <summary>
    /// Converts a given language name and <see cref="LanguageSupportLevel"/> to an <see cref="ILanguageInfo"/>
    /// </summary>
    /// <param name="englishLanguageName">The english name of the language.</param>
    /// <param name="supportLevel">The desired support level.</param>
    /// <returns>A new instance of a <see cref="ILanguageInfo"/></returns>
    public static ILanguageInfo FromEnglishName(string englishLanguageName, LanguageSupportLevel supportLevel)
    {
        englishLanguageName = englishLanguageName.ToLowerInvariant();
        if (!CulturesByName.TryGetValue(englishLanguageName, out var culture))
            throw new CultureNotFoundException($"Unable to get culture for language {englishLanguageName}");
        return new LanguageInfo(culture.TwoLetterISOLanguageName, supportLevel);
    }


}