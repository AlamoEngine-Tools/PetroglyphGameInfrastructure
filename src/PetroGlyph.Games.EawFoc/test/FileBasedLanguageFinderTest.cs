using System.Collections.Generic;
using System.IO.Abstractions;
using System.IO.Abstractions.TestingHelpers;
using EawModinfo.Model;
using EawModinfo.Spec;
using Moq;
using PetroGlyph.Games.EawFoc.Games;
using PetroGlyph.Games.EawFoc.Services.FileService;
using PetroGlyph.Games.EawFoc.Services.Language;
using Xunit;

namespace PetroGlyph.Games.EawFoc.Test
{
    public class FileBasedLanguageFinderTest
    {
        [Fact]
        public void TestMerge()
        {
            var finder = new FileBasedLanguageFinder();
            Assert.Empty(finder.Merge());

            var enumA = new List<ILanguageInfo>
            {
                new LanguageInfo("en", LanguageSupportLevel.FullLocalized),
                new LanguageInfo("de", LanguageSupportLevel.Speech | LanguageSupportLevel.Text),
                new LanguageInfo("fr", LanguageSupportLevel.Text),
            };
            Assert.Equal(enumA, finder.Merge(enumA));

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
            }, finder.Merge(enumA, enumB));
        }
        
        [Fact]
        public void TestFindText_None()
        {
            var fileService = new Mock<IPhysicalFileService>();
            var game = new Mock<IGame>();
            game.Setup(g => g.FileService).Returns(fileService.Object);

            var finder = new FileBasedLanguageFinder();
            var actual = finder.GetTextLocalizations(game.Object);

            Assert.Empty(actual);
        }

        [Fact]
        public void TestFindText_EnglishGerman()
        {
            var fs = new MockFileSystem();
            fs.AddFile("Game/Text/MasterTextFile_English.dat", MockFileData.NullObject);
            fs.AddFile("Game/Text/MASTERTEXTFILE_GERMAN.DAT", MockFileData.NullObject);

            var fileService = new Mock<IPhysicalFileService>();
            fileService.Setup(s => s.DataFiles("MasterTextFile_*.dat", "Text", false, false))
                .Returns(new List<IFileInfo>
                {
                    fs.FileInfo.FromFileName("Game/Text/MasterTextFile_English.dat"),
                    fs.FileInfo.FromFileName("Game/Text/MASTERTEXTFILE_GERMAN.DAT")
                });

            var game = new Mock<IGame>();
            game.Setup(g => g.FileService).Returns(fileService.Object);
            game.Setup(g => g.Directory).Returns(fs.DirectoryInfo.FromDirectoryName("Game"));

            var finder = new FileBasedLanguageFinder();
            var actual = finder.GetTextLocalizations(game.Object);

            Assert.Equal(new HashSet<ILanguageInfo>
            {
                new LanguageInfo("en", LanguageSupportLevel.Text),
                new LanguageInfo("de", LanguageSupportLevel.Text)
            }, actual);
        }

        [Fact]
        public void TestFindSFX_EnglishGerman()
        {
            var fs = new MockFileSystem();
            fs.AddFile("Game/Audio/SFX/sfx2d_english.meg", MockFileData.NullObject);
            fs.AddFile("Game/Audio/SFX/SFX2D_GERMAN.MEG", MockFileData.NullObject);
            fs.AddFile("Game/Audio/SFX/sfx2d_non_localized.meg", MockFileData.NullObject);

            var fileService = new Mock<IPhysicalFileService>();
            fileService.Setup(s => s.DataFiles("sfx2d_*.meg", "Audio/SFX", false, false))
                .Returns(new List<IFileInfo>
                {
                    fs.FileInfo.FromFileName("Game/Audio/SFX/sfx2d_english.meg"),
                    fs.FileInfo.FromFileName("Game/Audio/SFX/SFX2D_GERMAN.MEG"),
                    fs.FileInfo.FromFileName("Game/Audio/SFX/sfx2d_non_localized.meg")
                });

            var game = new Mock<IGame>();
            game.Setup(g => g.FileService).Returns(fileService.Object);
            game.Setup(g => g.FileSystem).Returns(fs);
            game.Setup(g => g.Directory).Returns(fs.DirectoryInfo.FromDirectoryName("Game"));

            var finder = new FileBasedLanguageFinder();
            var actual = finder.GetSfxMegLocalizations(game.Object);

            Assert.Equal(new HashSet<ILanguageInfo>
            {
                new LanguageInfo("en", LanguageSupportLevel.SFX),
                new LanguageInfo("de", LanguageSupportLevel.SFX)
            }, actual);
        }

        [Fact]
        public void TestFindSpeechMeg_EnglishGerman()
        {
            var fs = new MockFileSystem();
            fs.AddFile("Game/EnglishSpeech.meg", MockFileData.NullObject);
            fs.AddFile("Game/GERMANSPEECH.MEG", MockFileData.NullObject);
            fs.AddFile("Game/SomeSpeech.meg", MockFileData.NullObject);

            var fileService = new Mock<IPhysicalFileService>();
            fileService.Setup(s => s.DataFiles("*speech.meg", null, false, false))
                .Returns(new List<IFileInfo>
                {
                    fs.FileInfo.FromFileName("Game/EnglishSpeech.meg"),
                    fs.FileInfo.FromFileName("Game/GERMANSPEECH.MEG"),
                    fs.FileInfo.FromFileName("Game/SomeSpeech.meg")
                });

            var game = new Mock<IGame>();
            game.Setup(g => g.FileService).Returns(fileService.Object);
            game.Setup(g => g.FileSystem).Returns(fs);
            game.Setup(g => g.Directory).Returns(fs.DirectoryInfo.FromDirectoryName("Game"));

            var finder = new FileBasedLanguageFinder();
            var actual = finder.GetSpeechLocalizationsFromMegs(game.Object);

            Assert.Equal(new HashSet<ILanguageInfo>
            {
                new LanguageInfo("en", LanguageSupportLevel.Speech),
                new LanguageInfo("de", LanguageSupportLevel.Speech)
            }, actual);
        }

        [Fact]
        public void TestFindSpeechFolder_EnglishGerman()
        {
            var fs = new MockFileSystem();
            fs.AddDirectory("Game/Audio/Speech/English");
            fs.AddDirectory("Game/Audio/Speech/GERMAN");
            fs.AddDirectory("Game/Audio/Speech/Some");

            var fileService = new Mock<IPhysicalFileService>();
            fileService.Setup(s => s.DataDirectory("Audio/Speech", false))
                .Returns(fs.DirectoryInfo.FromDirectoryName("Game/Audio/Speech"));

            var game = new Mock<IGame>();
            game.Setup(g => g.FileService).Returns(fileService.Object);
            game.Setup(g => g.FileSystem).Returns(fs);
            game.Setup(g => g.Directory).Returns(fs.DirectoryInfo.FromDirectoryName("Game"));

            var finder = new FileBasedLanguageFinder();
            var actual = finder.GetSpeechLocalizationsFromFolder(game.Object);

            Assert.Equal(new HashSet<ILanguageInfo>
            {
                new LanguageInfo("en", LanguageSupportLevel.Speech),
                new LanguageInfo("de", LanguageSupportLevel.Speech)
            }, actual);
        }

        [Fact]
        public void TestFindSpeechFolder_NotExists()
        {
            var fs = new MockFileSystem();

            var fileService = new Mock<IPhysicalFileService>();
            fileService.Setup(s => s.DataDirectory("Audio/Speech", false))
                .Returns(fs.DirectoryInfo.FromDirectoryName("Game/Audio/Speech"));

            var game = new Mock<IGame>();
            game.Setup(g => g.FileService).Returns(fileService.Object);
            game.Setup(g => g.FileSystem).Returns(fs);
            game.Setup(g => g.Directory).Returns(fs.DirectoryInfo.FromDirectoryName("Game"));

            var finder = new FileBasedLanguageFinder();
            var actual = finder.GetSpeechLocalizationsFromFolder(game.Object);

            Assert.Empty(actual);
        }
    }
}