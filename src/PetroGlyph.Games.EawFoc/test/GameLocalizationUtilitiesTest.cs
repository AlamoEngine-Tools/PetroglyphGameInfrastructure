using System;
using System.Collections.Generic;
using EawModinfo.Model;
using EawModinfo.Spec;
using PG.StarWarsGame.Infrastructure.Games;
using PG.StarWarsGame.Infrastructure.Services.Language;
using PG.StarWarsGame.Infrastructure.Testing;
using Xunit;

namespace PG.StarWarsGame.Infrastructure.Test;

public class GameLocalizationUtilitiesTest : CommonTestBase
{
    [Fact]
    public void ArgumentNull_Throws()
    {
        IGame game = null!;
        Assert.Throws<ArgumentNullException>(() => GameLocalizationUtilities.GetTextLocalizations(game));
        Assert.Throws<ArgumentNullException>(() => GameLocalizationUtilities.GetSfxMegLocalizations(game));
        Assert.Throws<ArgumentNullException>(() => GameLocalizationUtilities.GetSpeechLocalizationsFromMegs(game));
        Assert.Throws<ArgumentNullException>(() => GameLocalizationUtilities.GetSpeechLocalizationsFromFolder(game));
    }

    [Fact]
    public void Merge()
    {
        Assert.Throws<ArgumentNullException>(() => GameLocalizationUtilities.Merge(null!));

        Assert.Empty(GameLocalizationUtilities.Merge());

        var enumA = new List<ILanguageInfo>
        {
            new LanguageInfo("en", LanguageSupportLevel.FullLocalized),
            new LanguageInfo("de", LanguageSupportLevel.Speech | LanguageSupportLevel.Text),
            new LanguageInfo("fr", LanguageSupportLevel.Text)
        };
        Assert.Equal(enumA, GameLocalizationUtilities.Merge(enumA));

        var enumB = new List<ILanguageInfo>
        {
            new LanguageInfo("en", LanguageSupportLevel.Text),
            new LanguageInfo("de", LanguageSupportLevel.SFX),
            new LanguageInfo("fr", LanguageSupportLevel.Speech),
            null! // Should be ignored
        };

        Assert.Equivalent(new HashSet<ILanguageInfo>
        {
            new LanguageInfo("en", LanguageSupportLevel.FullLocalized),
            new LanguageInfo("de", LanguageSupportLevel.FullLocalized),
            new LanguageInfo("fr", LanguageSupportLevel.Text | LanguageSupportLevel.Speech),
        }, GameLocalizationUtilities.Merge(enumA, enumB), true);
    }

    [Fact]
    public void GetTextLocalizations_None()
    {
        var game = CreateRandomGame();
        var actual = GameLocalizationUtilities.GetTextLocalizations(game);
        Assert.Empty(actual);
    }

    [Fact]
    public void GetSfxMegLocalizations_None()
    {
        var game = CreateRandomGame();
        var actual = GameLocalizationUtilities.GetSfxMegLocalizations(game);
        Assert.Empty(actual);
    }

    [Fact]
    public void GetSpeechLocalizationsFromFolder_None()
    {
        var game = CreateRandomGame();
        var actual = GameLocalizationUtilities.GetSpeechLocalizationsFromFolder(game);
        Assert.Empty(actual);
    }

    [Fact]
    public void GetSpeechLocalizationsFromMegs_None()
    {
        var game = CreateRandomGame();
        var actual = GameLocalizationUtilities.GetSpeechLocalizationsFromMegs(game);
        Assert.Empty(actual);
    }

    [Fact]
    public void GetTextLocalizations()
    {
        var game = CreateRandomGame();
        var dir = FileSystem.Path.Combine(game.Directory.FullName, "Data", "Text");
        FileSystem.Directory.CreateDirectory(dir);

        FileSystem.File.Create(FileSystem.Path.Combine(dir, "MasterTextFile_English.txt"));
        FileSystem.File.Create(FileSystem.Path.Combine(dir, "MASTERTEXTFILE_GERMAN.DAT"));
        FileSystem.File.Create(FileSystem.Path.Combine(dir, "mastertextfile_spanish.DAT"));
        FileSystem.File.Create(FileSystem.Path.Combine(dir, "mastertextfile_en.dat"));
        FileSystem.File.Create(FileSystem.Path.Combine(game.Directory.FullName, "Data", "mastertextfile_eng.dat"));

        var actual = GameLocalizationUtilities.GetTextLocalizations(game);

        Assert.Equivalent(new HashSet<ILanguageInfo>
        {
            new LanguageInfo("es", LanguageSupportLevel.Text),
            new LanguageInfo("de", LanguageSupportLevel.Text)
        }, actual, true);
    }

    [Fact]
    public void GetSfxMegLocalizations()
    {
        var game = CreateRandomGame();
        var dir = FileSystem.Path.Combine(game.Directory.FullName, "Data", "Audio", "SFX");
        FileSystem.Directory.CreateDirectory(dir);

        FileSystem.File.Create(FileSystem.Path.Combine(dir, "sfx2d_english.txt"));
        FileSystem.File.Create(FileSystem.Path.Combine(dir, "sfx2d_german.meg"));
        FileSystem.File.Create(FileSystem.Path.Combine(dir, "SFX2D_SPANISH.meg"));
        FileSystem.File.Create(FileSystem.Path.Combine(dir, "SFX2Denglish.meg"));
        FileSystem.File.Create(FileSystem.Path.Combine(dir, "SFX2D_en.meg"));
        FileSystem.File.Create(FileSystem.Path.Combine(dir, "SFX2D_.meg"));
        FileSystem.File.Create(FileSystem.Path.Combine(dir, "sfx2d_non_localized.meg"));

        var actual = GameLocalizationUtilities.GetSfxMegLocalizations(game);

        Assert.Equivalent(new HashSet<ILanguageInfo>
        {
            new LanguageInfo("es", LanguageSupportLevel.SFX),
            new LanguageInfo("de", LanguageSupportLevel.SFX)
        }, actual, true);
    }

    [Fact]
    public void GetSpeechLocalizationsFromMegs()
    {
        var game = CreateRandomGame();
        var dir = FileSystem.Path.Combine(game.Directory.FullName, "Data");
        FileSystem.Directory.CreateDirectory(dir);

        FileSystem.File.Create(FileSystem.Path.Combine(dir, "EnglishSpeech.txt"));
        FileSystem.File.Create(FileSystem.Path.Combine(dir, "English.meg"));
        FileSystem.File.Create(FileSystem.Path.Combine(dir, "Speech.meg"));
        FileSystem.File.Create(FileSystem.Path.Combine(dir, "GermanSpeech.meg"));
        FileSystem.File.Create(FileSystem.Path.Combine(dir, "SPANISHSPEECH.MEG"));

        var actual = GameLocalizationUtilities.GetSpeechLocalizationsFromMegs(game);

        Assert.Equivalent(new HashSet<ILanguageInfo>
        {
            new LanguageInfo("es", LanguageSupportLevel.Speech),
            new LanguageInfo("de", LanguageSupportLevel.Speech)
        }, actual, true);
    }

    [Fact]
    public void GetSpeechLocalizationsFromFolder()
    {
        var game = CreateRandomGame();
        var dir = FileSystem.Path.Combine(game.Directory.FullName, "Data", "Audio", "Speech");
        FileSystem.Directory.CreateDirectory(dir);

        FileSystem.Directory.CreateDirectory(FileSystem.Path.Combine(dir, "Eng"));
        FileSystem.Directory.CreateDirectory(FileSystem.Path.Combine(dir, "German"));
        FileSystem.Directory.CreateDirectory(FileSystem.Path.Combine(dir, "Spanish"));

        var actual = GameLocalizationUtilities.GetSpeechLocalizationsFromFolder(game);

        Assert.Equivalent(new HashSet<ILanguageInfo>
        {
            new LanguageInfo("es", LanguageSupportLevel.Speech),
            new LanguageInfo("de", LanguageSupportLevel.Speech)
        }, actual, true);
    }
}