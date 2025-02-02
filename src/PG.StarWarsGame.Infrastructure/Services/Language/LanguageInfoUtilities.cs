using System;
using System.Collections.Generic;
using System.Globalization;
using AET.Modinfo.Model;
using AET.Modinfo.Spec;

namespace PG.StarWarsGame.Infrastructure.Services.Language;

/// <summary>
/// Utilities for <see cref="ILanguageInfo"/>.
/// </summary>
public static class LanguageInfoUtilities
{
    private static readonly IDictionary<string, CultureInfo> CulturesByCode;
    private static readonly IDictionary<string, CultureInfo> CulturesByEnglishName;

    static LanguageInfoUtilities()
    {
        var neutralCultures = CultureInfo.GetCultures(CultureTypes.NeutralCultures);

        var twoLetterCodeDict = new Dictionary<string, CultureInfo>(neutralCultures.Length, StringComparer.OrdinalIgnoreCase);
        var englishNameDict = new Dictionary<string, CultureInfo>(neutralCultures.Length, StringComparer.OrdinalIgnoreCase);

        foreach (var culture in neutralCultures)
        {
            var tln = culture.TwoLetterISOLanguageName;
            if (!twoLetterCodeDict.ContainsKey(tln))
                twoLetterCodeDict.Add(tln, culture);
            var englishName = culture.EnglishName;
            if (!englishNameDict.ContainsKey(englishName))
                englishNameDict.Add(englishName, culture);
        }

        CulturesByCode = twoLetterCodeDict;
        CulturesByEnglishName = englishNameDict;
    }

    /// <summary>
    /// Gets the english language name of a given <see cref="ILanguageInfo"/>,
    /// or <see langword="null"/> if the language info does not have an english name or has an invalid name.
    /// </summary>
    /// <param name="languageInfo">The info to get the english name from.</param>
    /// <returns>The english language name.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="languageInfo"/> is <see langinfo="null"/>.</exception>
    public static string? GetEnglishName(ILanguageInfo languageInfo)
    {
        if (languageInfo == null)
            throw new ArgumentNullException(nameof(languageInfo));

        return !CulturesByCode.TryGetValue(languageInfo.Code, out var culture)
            ? null
            : culture.EnglishName;
    }


    /// <summary>
    /// Returns a language info from the specified english language name and <see cref="LanguageSupportLevel"/>,
    /// or <see langword="null"/> if no language info could be created from the specified language name.
    /// </summary>
    /// <param name="englishLanguageName">The english name of the language.</param>
    /// <param name="supportLevel">The support level to use for the returned language info.</param>
    /// <returns>
    /// The <see cref="ILanguageInfo"/> from <paramref name="englishLanguageName"/> and <paramref name="supportLevel"/>,
    /// or <see langword="null"/> if no language info could be created from <paramref name="englishLanguageName"/>.
    /// </returns>
    /// <exception cref="ArgumentNullException"><paramref name="englishLanguageName"/> is <see langinfo="null"/>.</exception>
    public static ILanguageInfo? FromEnglishName(string englishLanguageName, LanguageSupportLevel supportLevel)
    {
        if (englishLanguageName == null) 
            throw new ArgumentNullException(nameof(englishLanguageName));

        return !CulturesByEnglishName.TryGetValue(englishLanguageName, out var culture)
            ? null
            : new LanguageInfo(culture.TwoLetterISOLanguageName, supportLevel);
    }
}