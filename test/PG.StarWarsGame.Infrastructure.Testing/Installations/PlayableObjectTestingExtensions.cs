using System.IO.Abstractions;
using AET.Modinfo.Spec;
using PG.StarWarsGame.Infrastructure.Services.Language;
using PG.StarWarsGame.Infrastructure.Utilities;
using Xunit;

namespace PG.StarWarsGame.Infrastructure.Testing.Installations;

internal static class PlayableObjectTestingUtilities
{
    public static void InstallLanguage(IPhysicalPlayableObject obj, ILanguageInfo language, IFileSystem fileSystem)
    {
        var languageName = LanguageInfoUtilities.GetEnglishName(language);
        var dataDir = obj.DataDirectory();
        Assert.NotNull(languageName);
        if (language.Support.HasFlag(LanguageSupportLevel.Text))
            InstallText(dataDir, languageName, fileSystem);
        if (language.Support.HasFlag(LanguageSupportLevel.SFX))
            InstallSfx(dataDir, languageName, fileSystem);
        if (language.Support.HasFlag(LanguageSupportLevel.Speech))
            InstallSpeech(dataDir, languageName, fileSystem);
    }

    private static void InstallText(IDirectoryInfo dataDir, string languageName, IFileSystem fileSystem)
    {
        var masterTextFileName = $"MasterTextFile_{languageName}.dat";

        var dir = fileSystem.Path.Combine(dataDir.FullName, "Text");
        fileSystem.Directory.CreateDirectory(dir);

        var filePath = fileSystem.Path.Combine(dir, masterTextFileName);
        using var _ = fileSystem.File.Create(filePath);
    }

    private static void InstallSfx(IDirectoryInfo dataDir, string languageName, IFileSystem fileSystem)
    {
        var sfxFileName = $"SFX2D_{languageName}.meg";

        var dir = fileSystem.Path.Combine(dataDir.FullName, "Audio", "SFX");
        fileSystem.Directory.CreateDirectory(dir);

        var filePath = fileSystem.Path.Combine(dir, sfxFileName);
        using var _ = fileSystem.File.Create(filePath);
    }

    private static void InstallSpeech(IDirectoryInfo dataDir, string languageName, IFileSystem fileSystem)
    {
        var sfxFileName = $"{languageName}Speech.meg";
        var filePath = fileSystem.Path.Combine(dataDir.FullName, sfxFileName);
        using var _ = fileSystem.File.Create(filePath);
    }
}