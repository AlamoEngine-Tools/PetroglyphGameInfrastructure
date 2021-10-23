using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO.Abstractions;
using System.Linq;
using System.Threading;
using EawModinfo.Model;
using EawModinfo.Spec;

namespace PetroGlyph.Games.EawFoc.Services.Language;

/// <inheritdoc cref="ILanguageFinder"/>
public sealed class SharedLanguageFinder : ILanguageFinder
{

    private static readonly IDictionary<string, CultureInfo>? NeutralCultures;

    private static readonly IDictionary<string, CultureInfo> AllCultures = LazyInitializer.EnsureInitialized(ref NeutralCultures!,
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

    /// <inheritdoc/>
    public ISet<ILanguageInfo> GetTextLocalizations(IPhysicalPlayableObject playableObject)
    {
        return TryGetLanguageFromFiles(
            () => playableObject.FileService.DataFiles("MasterTextFile_*.dat", "Text", false, false),
            GetTextLangName, LanguageSupportLevel.Text);

        string GetTextLangName(string textFileName)
        {
            textFileName = playableObject.Directory.FileSystem.Path.GetFileNameWithoutExtension(textFileName);
            const string cutOffPattern = "MasterTextFile_";
            return textFileName.Substring(cutOffPattern.Length);
        }
    }

    /// <inheritdoc/>
    public ISet<ILanguageInfo> GetSfxMegLocalizations(IPhysicalPlayableObject playableObject)
    {
        return TryGetLanguageFromFiles(
            () => playableObject.FileService.DataFiles("sfx2d_*.meg", "Audio/SFX", false, false),
            fileName => GetSfxLangName(fileName, playableObject.FileSystem), LanguageSupportLevel.SFX);

        static string? GetSfxLangName(string fileName, IFileSystem fs)
        {
            if (fileName.Equals("sfx2d_non_localized.meg", StringComparison.OrdinalIgnoreCase))
                return null;
            var cutOffIndex = fileName.LastIndexOf('_') + 1;
            if (cutOffIndex <= 0 || cutOffIndex == fileName.Length)
                return null;
            var langNameWithExtension = fileName.Substring(cutOffIndex);
            return fs.Path.GetFileNameWithoutExtension(langNameWithExtension);
        }
    }

    /// <inheritdoc/>
    public ISet<ILanguageInfo> GetSpeechLocalizationsFromMegs(IPhysicalPlayableObject playableObject)
    {
        // TODO: When merged into PG repo, try to get real path from megafiles.xml
        return TryGetLanguageFromFiles(
            () => playableObject.FileService.DataFiles("*speech.meg", null, false, false),
            GetSpeechLangName, LanguageSupportLevel.Speech);

        static string GetSpeechLangName(string megFileName)
        {
            var cutOffIndex = megFileName.IndexOf("speech.meg", StringComparison.OrdinalIgnoreCase);
            if (cutOffIndex < 0)
                throw new InvalidOperationException($"unable to get language name from {megFileName}");
            return megFileName.Substring(0, cutOffIndex);
        }
    }

    /// <inheritdoc/>
    public ISet<ILanguageInfo> GetSpeechLocalizationsFromFolder(IPhysicalPlayableObject playableObject)
    {
        var speechDir = playableObject.FileService.DataDirectory("Audio/Speech", false);
        if (!speechDir.Exists)
            return new HashSet<ILanguageInfo>();

        var langFolders = speechDir.EnumerateDirectories();

        var result = new HashSet<ILanguageInfo>();
        foreach (var folder in langFolders)
        {
            try
            {
                result.Add(LanguageNameToLanguageInfo(folder.Name, LanguageSupportLevel.Speech));
            }
            catch (CultureNotFoundException)
            {
            }
        }
        return result;
    }

    /// <summary>
    /// Converts a given language name and <see cref="LanguageSupportLevel"/> to an <see cref="ILanguageInfo"/>
    /// </summary>
    /// <param name="languageName">The english name of the language.</param>
    /// <param name="supportLevel">The desired support level.</param>
    /// <returns>A new instance of a <see cref="ILanguageInfo"/></returns>
    public static ILanguageInfo LanguageNameToLanguageInfo(string languageName, LanguageSupportLevel supportLevel)
    {
        languageName = languageName.ToLowerInvariant();
        if (!AllCultures.TryGetValue(languageName, out var culture))
            throw new CultureNotFoundException($"Unable to get culture for language {languageName}");
        return new LanguageInfo(culture.TwoLetterISOLanguageName, supportLevel);
    }

    /// <inheritdoc/>
    public ISet<ILanguageInfo> Merge(params IEnumerable<ILanguageInfo>[] setsToMerge)
    {
        if (!setsToMerge.Any())
            return new HashSet<ILanguageInfo>();
        if (setsToMerge.Length == 1)
            return new HashSet<ILanguageInfo>(setsToMerge[0]);

        var store = new Dictionary<string, LanguageSupportLevel>();
        foreach (var languageInfo in setsToMerge.SelectMany(x => x))
        {
            if (store.ContainsKey(languageInfo.Code))
                store[languageInfo.Code] |= languageInfo.Support;
            else
                store.Add(languageInfo.Code, languageInfo.Support);
        }
        return new HashSet<ILanguageInfo>(store.Select(pair =>
            (ILanguageInfo)new LanguageInfo(pair.Key, pair.Value)));

    }

    private static ISet<ILanguageInfo> TryGetLanguageFromFiles(
        Func<IEnumerable<IFileInfo>> fileEnumerator,
        Func<string, string?> languageNameFactory, LanguageSupportLevel supportLevel)
    {
        var files = fileEnumerator().ToList();
        var result = new HashSet<ILanguageInfo>();
        foreach (var languageName in files.Select(file => languageNameFactory(file.Name)).Where(languageName => languageName != null))
        {
            try
            {
                result.Add(LanguageNameToLanguageInfo(languageName!, supportLevel));
            }
            catch (CultureNotFoundException)
            {
                Console.WriteLine();
            }
        }
        return result;
    }
}