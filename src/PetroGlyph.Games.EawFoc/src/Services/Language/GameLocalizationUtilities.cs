using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO.Abstractions;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using EawModinfo.Model;
using EawModinfo.Spec;
using PG.StarWarsGame.Infrastructure.Utilities;

namespace PG.StarWarsGame.Infrastructure.Services.Language;

/// <summary>
/// Finds installed languages based on well-known file locations and names.
/// </summary>
public static class GameLocalizationUtilities
{
    /// <summary>
    /// Searches the specified playable object for installed .DAT MasterText files.
    /// All returned language infos have <see cref="ILanguageInfo.Support"/> set to <see cref="LanguageSupportLevel.Text"/>.
    /// </summary>
    /// <param name="playableObject">The playable object to search MasterText .DAT files for.</param>
    /// <returns>A collection of languages infos which represents the found MasterText files.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="playableObject"/> is <see langword="null"/>.</exception>
    public static IReadOnlyCollection<ILanguageInfo> GetTextLocalizations(IPhysicalPlayableObject playableObject)
    {
        if (playableObject == null) 
            throw new ArgumentNullException(nameof(playableObject));

        return TryGetLanguageFromFiles(
            () => playableObject.DataFiles("MasterTextFile_*.dat", "Text", false),
            GetTextLangName, LanguageSupportLevel.Text);

        string GetTextLangName(string textFileName)
        {
            textFileName = playableObject.Directory.FileSystem.Path.GetFileNameWithoutExtension(textFileName);
            const string cutOffPattern = "MasterTextFile_";
            return textFileName.Substring(cutOffPattern.Length);
        }
    }

    /// <summary>
    /// Searches the specified playable object for installed and localized SFX2D MEG files.
    /// All returned language infos have <see cref="ILanguageInfo.Support"/> set to <see cref="LanguageSupportLevel.SFX"/>.
    /// </summary>
    /// <param name="playableObject">The playable object to search SFX2D .MEG files for.</param>
    /// <returns>A collection of languages infos which represents the found SFX2D files.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="playableObject"/> is <see langword="null"/>.</exception>
    public static IReadOnlyCollection<ILanguageInfo> GetSfxMegLocalizations(IPhysicalPlayableObject playableObject)
    {
        return TryGetLanguageFromFiles(
            () => playableObject.DataFiles("sfx2d_*.meg", "Audio/SFX", false),
            fileName => GetSfxLangName(fileName, playableObject.Directory.FileSystem), LanguageSupportLevel.SFX);

        static string? GetSfxLangName(string fileName, IFileSystem fs)
        {
            if (fileName.Equals("sfx2d_non_localized.meg", StringComparison.OrdinalIgnoreCase))
                return null;
            var cutOffIndex = fileName.LastIndexOf('_');
            Debug.Assert(cutOffIndex >= 0);
            var langNameWithExtension = fileName.Substring(cutOffIndex + 1);
            return fs.Path.GetFileNameWithoutExtension(langNameWithExtension);
        }
    }

    /// <summary>
    /// Searches the specified playable object for installed and localized Speech MEG files.
    /// All returned language infos have <see cref="ILanguageInfo.Support"/> set to <see cref="LanguageSupportLevel.Speech"/>.
    /// </summary>
    /// <param name="playableObject">The playable object to search Speech .MEG files for.</param>
    /// <returns>A collection of languages infos which represents the found Speech files.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="playableObject"/> is <see langword="null"/>.</exception>
    public static IReadOnlyCollection<ILanguageInfo> GetSpeechLocalizationsFromMegs(IPhysicalPlayableObject playableObject)
    {
        // TODO: When merged into PG repo, try to get real path from megafiles.xml
        return TryGetLanguageFromFiles(
            () => playableObject.DataFiles("*speech.meg", null, false),
            GetSpeechLangName, LanguageSupportLevel.Speech);

        static string GetSpeechLangName(string megFileName)
        {
            var cutOffIndex = megFileName.IndexOf("speech.meg", StringComparison.OrdinalIgnoreCase);
            Debug.Assert(cutOffIndex >= 0);
            return megFileName.Substring(0, cutOffIndex);
        }
    }

    /// <summary>
    /// Searches the specified playable object for localized Speech folders.
    /// All returned language infos have <see cref="ILanguageInfo.Support"/> set to <see cref="LanguageSupportLevel.Speech"/>.
    /// </summary>
    /// <remarks>
    /// The method does not check the contents of the found folder.
    /// </remarks>
    /// <param name="playableObject">The playable object to search Speech .MEG files for.</param>
    /// <returns>A collection of languages infos which represents the found Speech files.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="playableObject"/> is <see langword="null"/>.</exception>
    public static IReadOnlyCollection<ILanguageInfo> GetSpeechLocalizationsFromFolder(IPhysicalPlayableObject playableObject)
    {
        var speechDir = playableObject.DataDirectory("Audio/Speech");
        if (!speechDir.Exists)
            return new HashSet<ILanguageInfo>();

        var langFolders = speechDir.EnumerateDirectories();

        var result = new HashSet<ILanguageInfo>();
        foreach (var folder in langFolders)
        {
            var languageInfo = LanguageInfoUtilities.FromEnglishName(folder.Name, LanguageSupportLevel.Speech);
            if (languageInfo is not null) 
                result.Add(languageInfo);
        }
        return result;
    }

    /// <summary>
    /// Merges multiple languages into a unified collection so that each item of the returned list
    /// contains only one <see cref="ILanguageInfo"/> per language code.
    /// All language support levels of matching the same code get added together bitwise.
    /// </summary>
    /// <param name="languagesToMerge">The language infos to merge.</param>
    /// <returns>The merged collection language info.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="languagesToMerge"/> is <see langword="null"/>.</exception>
    public static IReadOnlyCollection<ILanguageInfo> Merge(params IEnumerable<ILanguageInfo>[] languagesToMerge)
    {
        if (languagesToMerge == null)
            throw new ArgumentNullException(nameof(languagesToMerge));

        if (languagesToMerge.Length == 0)
            return [];
        if (languagesToMerge.Length == 1)
            return new HashSet<ILanguageInfo>(languagesToMerge[0]);

        var store = new Dictionary<string, LanguageSupportLevel>();
        foreach (var languageInfo in languagesToMerge.SelectMany(x => x))
        {
            if (languageInfo is null)
                continue;

            if (store.ContainsKey(languageInfo.Code))
                store[languageInfo.Code] |= languageInfo.Support;
            else
                store.Add(languageInfo.Code, languageInfo.Support);
        }
        return new HashSet<ILanguageInfo>(store.Select(ILanguageInfo (pair) => new LanguageInfo(pair.Key, pair.Value)));

    }

    private static IReadOnlyCollection<ILanguageInfo> TryGetLanguageFromFiles(
        Func<IEnumerable<IFileInfo>> fileEnumerator,
        Func<string, string?> languageNameFactory,
        LanguageSupportLevel supportLevel)
    {
        var files = fileEnumerator().ToList();
        var result = new HashSet<ILanguageInfo>();
        foreach (var languageName in files.Select(file => languageNameFactory(file.Name)).Where(languageName => languageName is not null))
        {
            var languageInfo = LanguageInfoUtilities.FromEnglishName(languageName!, supportLevel);
            if (languageInfo is not null)
                result.Add(languageInfo);
        }
        return result;
    }
}