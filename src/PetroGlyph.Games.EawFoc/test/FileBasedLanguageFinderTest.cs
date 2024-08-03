using System.Collections.Generic;
using System.IO.Abstractions;
using EawModinfo.Model;
using EawModinfo.Spec;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using PG.StarWarsGame.Infrastructure.Games;
using PG.StarWarsGame.Infrastructure.Services.Language;
using PG.StarWarsGame.Infrastructure.Utilities;
using Testably.Abstractions.Testing;
using Xunit;

namespace PG.StarWarsGame.Infrastructure.Test;

public class FileBasedLanguageFinderTest
{
    private readonly FileBasedLanguageFinder _languageFinder;
    private readonly Mock<IPlayableObjectFileService> _fileService = new();
    private readonly MockFileSystem _fileSystem = new();

    public FileBasedLanguageFinderTest()
    {
        var sc = new ServiceCollection();
        sc.AddSingleton(_fileService.Object);
        sc.AddSingleton<IFileSystem>(_fileSystem);
        _languageFinder = new FileBasedLanguageFinder(sc.BuildServiceProvider());
    }

    [Fact]
    public void TestMerge()
    {
        Assert.Empty(_languageFinder.Merge());

        var enumA = new List<ILanguageInfo>
        {
            new LanguageInfo("en", LanguageSupportLevel.FullLocalized),
            new LanguageInfo("de", LanguageSupportLevel.Speech | LanguageSupportLevel.Text),
            new LanguageInfo("fr", LanguageSupportLevel.Text),
        };
        Assert.Equal(enumA, _languageFinder.Merge(enumA));

        var enumB = new List<ILanguageInfo>
        {
            new LanguageInfo("en", LanguageSupportLevel.Text),
            new LanguageInfo("de", LanguageSupportLevel.SFX),
            new LanguageInfo("fr", LanguageSupportLevel.Speech),
        };

        Assert.Equal(new HashSet<ILanguageInfo>
        {
            new LanguageInfo("en", LanguageSupportLevel.FullLocalized),
            new LanguageInfo("de", LanguageSupportLevel.FullLocalized),
            new LanguageInfo("fr", LanguageSupportLevel.Text | LanguageSupportLevel.Speech),
        }, _languageFinder.Merge(enumA, enumB));
    }
        
    [Fact]
    public void TestFindText_None()
    {
        var game = new Mock<IGame>();
        var actual = _languageFinder.GetTextLocalizations(game.Object);
        Assert.Empty(actual);
    }

    [Fact]
    public void TestFindText_EnglishGerman()
    {
        var game = new Mock<IGame>();
        game.Setup(g => g.Directory).Returns(_fileSystem.DirectoryInfo.New("Game"));

        _fileSystem.Initialize()
            .WithFile("Game/Text/MasterTextFile_English.da")
            .WithFile("Game/Text/MASTERTEXTFILE_GERMAN.DAT");

        _fileService.Setup(s => s.DataFiles(game.Object, "MasterTextFile_*.dat", "Text", false, false))
            .Returns(new List<IFileInfo>
            {
                _fileSystem.FileInfo.New("Game/Text/MasterTextFile_English.dat"),
                _fileSystem.FileInfo.New("Game/Text/MASTERTEXTFILE_GERMAN.DAT")
            });

        var actual = _languageFinder.GetTextLocalizations(game.Object);

        Assert.Equal(new HashSet<ILanguageInfo>
        {
            new LanguageInfo("en", LanguageSupportLevel.Text),
            new LanguageInfo("de", LanguageSupportLevel.Text)
        }, actual);
    }

    [Fact]
    public void TestFindSFX_EnglishGerman()
    {
        var game = new Mock<IGame>();
        game.Setup(g => g.FileSystem).Returns(_fileSystem);
        game.Setup(g => g.Directory).Returns(_fileSystem.DirectoryInfo.New("Game"));

        _fileSystem.Initialize()
            .WithFile("Game/Audio/SFX/sfx2d_english.meg")
            .WithFile("Game/Audio/SFX/SFX2D_GERMAN.MEG")
            .WithFile("Game/Audio/SFX/sfx2d_non_localized.meg");

        _fileService.Setup(s => s.DataFiles(game.Object, "sfx2d_*.meg", "Audio/SFX", false, false))
            .Returns(new List<IFileInfo>
            {
                _fileSystem.FileInfo.New("Game/Audio/SFX/sfx2d_english.meg"),
                _fileSystem.FileInfo.New("Game/Audio/SFX/SFX2D_GERMAN.MEG"),
                _fileSystem.FileInfo.New("Game/Audio/SFX/sfx2d_non_localized.meg")
            });

       var actual = _languageFinder.GetSfxMegLocalizations(game.Object);

        Assert.Equal(new HashSet<ILanguageInfo>
        {
            new LanguageInfo("en", LanguageSupportLevel.SFX),
            new LanguageInfo("de", LanguageSupportLevel.SFX)
        }, actual);
    }

    [Fact]
    public void TestFindSpeechMeg_EnglishGerman()
    {
        var game = new Mock<IGame>();
        game.Setup(g => g.FileSystem).Returns(_fileSystem);
        game.Setup(g => g.Directory).Returns(_fileSystem.DirectoryInfo.New("Game"));

        var fs = new MockFileSystem();
        fs.Initialize()
            .WithFile("Game/EnglishSpeech.meg")
            .WithFile("Game/GERMANSPEECH.MEG")
            .WithFile("Game/SomeSpeech.meg");

        _fileService.Setup(s => s.DataFiles(game.Object, "*speech.meg", null, false, false))
            .Returns(new List<IFileInfo>
            {
                fs.FileInfo.New("Game/EnglishSpeech.meg"),
                fs.FileInfo.New("Game/GERMANSPEECH.MEG"),
                fs.FileInfo.New("Game/SomeSpeech.meg")
            });

      var actual = _languageFinder.GetSpeechLocalizationsFromMegs(game.Object);

        Assert.Equal(new HashSet<ILanguageInfo>
        {
            new LanguageInfo("en", LanguageSupportLevel.Speech),
            new LanguageInfo("de", LanguageSupportLevel.Speech)
        }, actual);
    }

    [Fact]
    public void TestFindSpeechFolder_EnglishGerman()
    {
        var game = new Mock<IGame>();
        game.Setup(g => g.FileSystem).Returns(_fileSystem);
        game.Setup(g => g.Directory).Returns(_fileSystem.DirectoryInfo.New("Game"));

        _fileSystem.Initialize()
            .WithSubdirectory("Game/Audio/Speech/English")
            .WithSubdirectory("Game/Audio/Speech/GERMAN")
            .WithSubdirectory("Game/Audio/Speech/Some");

        _fileService.Setup(s => s.DataDirectory(game.Object, "Audio/Speech", false))
            .Returns(_fileSystem.DirectoryInfo.New("Game/Audio/Speech"));

        var actual = _languageFinder.GetSpeechLocalizationsFromFolder(game.Object);

        Assert.Equal(new HashSet<ILanguageInfo>
        {
            new LanguageInfo("en", LanguageSupportLevel.Speech),
            new LanguageInfo("de", LanguageSupportLevel.Speech)
        }, actual);
    }

    [Fact]
    public void TestFindSpeechFolder_NotExists()
    {
        var game = new Mock<IGame>();
        game.Setup(g => g.FileSystem).Returns(_fileSystem);
        game.Setup(g => g.Directory).Returns(_fileSystem.DirectoryInfo.New("Game"));

        _fileService.Setup(s => s.DataDirectory(game.Object, "Audio/Speech", false))
            .Returns(_fileSystem.DirectoryInfo.New("Game/Audio/Speech"));

        var actual = _languageFinder.GetSpeechLocalizationsFromFolder(game.Object);

        Assert.Empty(actual);
    }
}