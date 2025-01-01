using EawModinfo.Spec;
using PG.StarWarsGame.Infrastructure.Services.Language;
using PG.StarWarsGame.Infrastructure.Utilities;

namespace PG.StarWarsGame.Infrastructure.Testing;

public static class PlayableObjectTestingExtensions
{
    public static void InstallLanguage(this IPhysicalPlayableObject obj, ILanguageInfo language)
    {
        var languageName = LanguageInfoUtilities.GetEnglishName(language);
        Assert.NotNull(languageName);
        if (language.Support.HasFlag(LanguageSupportLevel.Text))
            obj.InstallText(languageName);
        if (language.Support.HasFlag(LanguageSupportLevel.SFX))
            obj.InstallSfx(languageName);
        if (language.Support.HasFlag(LanguageSupportLevel.Speech))
            obj.InstallSpeech(languageName);
    }

    private static void InstallText(this IPhysicalPlayableObject obj, string languageName)
    {
        var fs = obj.Directory.FileSystem;
        var masterTextFileName = $"MasterTextFile_{languageName}.dat";

        var dir = fs.Path.Combine(obj.DataDirectory().FullName, "Text");
        fs.Directory.CreateDirectory(dir);

        var filePath = fs.Path.Combine(dir, masterTextFileName);
        using var _ = fs.File.Create(filePath);
    }

    private static void InstallSfx(this IPhysicalPlayableObject obj, string languageName)
    {
        var fs = obj.Directory.FileSystem;
        var sfxFileName = $"SFX2D_{languageName}.meg";

        var dir = fs.Path.Combine(obj.DataDirectory().FullName, "Audio", "SFX");
        fs.Directory.CreateDirectory(dir);

        var filePath = fs.Path.Combine(dir, sfxFileName);
        using var _ = fs.File.Create(filePath);
    }

    private static void InstallSpeech(this IPhysicalPlayableObject obj, string languageName)
    {
        var fs = obj.Directory.FileSystem;
        var sfxFileName = $"{languageName}Speech.meg";
        var filePath = fs.Path.Combine(obj.DataDirectory().FullName, sfxFileName);
        using var _ = fs.File.Create(filePath);
    }
}