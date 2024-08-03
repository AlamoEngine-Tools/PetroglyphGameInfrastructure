using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO.Abstractions;
using System.Linq;
using EawModinfo.Model;
using EawModinfo.Spec;
using Microsoft.Extensions.DependencyInjection;
using PG.StarWarsGame.Infrastructure.Utilities;

namespace PG.StarWarsGame.Infrastructure.Services.Language;

/// <summary>
/// Finds installed languages based on well-known file locations and names.
/// </summary>
public sealed class FileBasedLanguageFinder(IServiceProvider serviceProvider) : ILanguageFinder
{
    private readonly IPlayableObjectFileService _fileService = serviceProvider.GetRequiredService<IPlayableObjectFileService>();

    /// <inheritdoc/>
    public ISet<ILanguageInfo> GetTextLocalizations(IPhysicalPlayableObject playableObject)
    {
        return TryGetLanguageFromFiles(
            () => _fileService.DataFiles(playableObject, "MasterTextFile_*.dat", "Text", false, false),
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
            () => _fileService.DataFiles(playableObject, "sfx2d_*.meg", "Audio/SFX", false, false),
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
            () => _fileService.DataFiles(playableObject, "*speech.meg", null, false, false),
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
        var speechDir = _fileService.DataDirectory(playableObject, "Audio/Speech");
        if (!speechDir.Exists)
            return new HashSet<ILanguageInfo>();

        var langFolders = speechDir.EnumerateDirectories();

        var result = new HashSet<ILanguageInfo>();
        foreach (var folder in langFolders)
        {
            try
            {
                result.Add(LanguageInfoUtilities.FromEnglishName(folder.Name, LanguageSupportLevel.Speech));
            }
            catch (CultureNotFoundException)
            {
            }
        }
        return result;
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
                result.Add(LanguageInfoUtilities.FromEnglishName(languageName!, supportLevel));
            }
            catch (CultureNotFoundException)
            {
                Console.WriteLine();
            }
        }
        return result;
    }
}