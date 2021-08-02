using System.IO.Abstractions.TestingHelpers;
using EawModinfo.Spec;
using Moq;
using PetroGlyph.Games.EawFoc.Games;
using PetroGlyph.Games.EawFoc.Services.Detection;
using Xunit;

namespace PetroGlyph.Games.EawFoc.Test.ModServices
{
    public class FileSystemModFinderTest
    {
        [Fact]
        public void GameNotExists_Throws()
        {
            var game = new Mock<IGame>();
            Assert.Throws<GameException>(() => new FileSystemModFinder().FindMods(game.Object));
        }

        [Fact]
        public void TestNoMods_Normal()
        {
            var fs = new MockFileSystem();
            fs.AddDirectory("Game/Mods");
            var game = new Mock<IGame>();
            game.Setup(g => g.Exists()).Returns(true);
            game.Setup(g => g.Platform).Returns(GamePlatform.Disk);
            game.Setup(g => g.Directory).Returns(fs.DirectoryInfo.FromDirectoryName("Game"));
            game.Setup(g => g.ModsLocation).Returns(fs.DirectoryInfo.FromDirectoryName("Game/Mods"));
            var mods = new FileSystemModFinder().FindMods(game.Object);
            Assert.Empty(mods);
        }

        [Fact]
        public void TestNoMods_Normal_NoFolder()
        {
            var fs = new MockFileSystem();
            fs.AddDirectory("Game");
            var game = new Mock<IGame>();
            game.Setup(g => g.Exists()).Returns(true);
            game.Setup(g => g.Platform).Returns(GamePlatform.Disk);
            game.Setup(g => g.Directory).Returns(fs.DirectoryInfo.FromDirectoryName("Game"));
            game.Setup(g => g.ModsLocation).Returns(fs.DirectoryInfo.FromDirectoryName("Game/Mods"));
            var mods = new FileSystemModFinder().FindMods(game.Object);
            Assert.Empty(mods);
        }

        [Fact]
        public void TestNoMods_Steam()
        {
            var fs = new MockFileSystem();
            fs.AddDirectory("Lib/Game/Eaw/Mods");
            var game = new Mock<IGame>();
            game.Setup(g => g.Exists()).Returns(true);
            game.Setup(g => g.Platform).Returns(GamePlatform.SteamGold);
            game.Setup(g => g.Directory).Returns(fs.DirectoryInfo.FromDirectoryName("Lib/Game/Eaw/Mods"));
            game.Setup(g => g.ModsLocation).Returns(fs.DirectoryInfo.FromDirectoryName("Lib/Game/Eaw/Mods"));
            var mods = new FileSystemModFinder().FindMods(game.Object);
            Assert.Empty(mods);
        }

        [Fact]
        public void TestOneMods_Normal()
        {
            var fs = new MockFileSystem();
            fs.AddDirectory("Game/Mods/ModA");
            var game = new Mock<IGame>();
            game.Setup(g => g.Exists()).Returns(true);
            game.Setup(g => g.Platform).Returns(GamePlatform.Disk);
            game.Setup(g => g.Directory).Returns(fs.DirectoryInfo.FromDirectoryName("Game"));
            game.Setup(g => g.ModsLocation).Returns(fs.DirectoryInfo.FromDirectoryName("Game/Mods"));
            var mods = new FileSystemModFinder().FindMods(game.Object);
            var mod = Assert.Single(mods);
            Assert.Equal("C:\\Game\\Mods\\ModA", mod.Identifier);
            Assert.Equal(ModType.Default, mod.Type);
        }

        [Fact]
        public void TestTwoMods_Normal()
        {
            var fs = new MockFileSystem();
            fs.AddDirectory("Game/Mods/ModA");
            fs.AddDirectory("Game/Mods/ModB");
            var game = new Mock<IGame>();
            game.Setup(g => g.Exists()).Returns(true);
            game.Setup(g => g.Platform).Returns(GamePlatform.Disk);
            game.Setup(g => g.Directory).Returns(fs.DirectoryInfo.FromDirectoryName("Game"));
            game.Setup(g => g.ModsLocation).Returns(fs.DirectoryInfo.FromDirectoryName("Game/Mods"));
            var mods = new FileSystemModFinder().FindMods(game.Object);
            Assert.Equal(2, mods.Count);
        }

        [Fact]
        public void TestOneDefaultMod_Steam()
        {
            var fs = new MockFileSystem();
            fs.AddDirectory("Lib/Game/Eaw/Mods/ModA");
            var game = new Mock<IGame>();
            game.Setup(g => g.Exists()).Returns(true);
            game.Setup(g => g.Platform).Returns(GamePlatform.SteamGold);
            game.Setup(g => g.Directory).Returns(fs.DirectoryInfo.FromDirectoryName("Lib/Game/Eaw/Mods"));
            game.Setup(g => g.ModsLocation).Returns(fs.DirectoryInfo.FromDirectoryName("Lib/Game/Eaw/Mods"));
            var mods = new FileSystemModFinder().FindMods(game.Object);
            var mod = Assert.Single(mods);
            Assert.Equal("C:\\Lib\\Game\\Eaw\\Mods\\ModA", mod.Identifier);
            Assert.Equal(ModType.Default, mod.Type);
        }

        [Fact]
        public void TestOneDefaultModOneWsMod_Steam()
        {
            var fs = new MockFileSystem();
            fs.AddDirectory("Lib/Game/Eaw/Mods/ModA");
            fs.AddDirectory("Lib/workshop/content/32470/12345678");
            var game = new Mock<IGame>();
            game.Setup(g => g.Exists()).Returns(true);
            game.Setup(g => g.Platform).Returns(GamePlatform.SteamGold);
            game.Setup(g => g.Directory).Returns(fs.DirectoryInfo.FromDirectoryName("Lib/Game/Eaw/Mods"));
            game.Setup(g => g.ModsLocation).Returns(fs.DirectoryInfo.FromDirectoryName("Lib/Game/Eaw/Mods"));

            var mods = new FileSystemModFinder().FindMods(game.Object);
            Assert.Equal(2, mods.Count);
        }

        [Fact]
        public void TestOneWsMod_Steam()
        {
            var fs = new MockFileSystem();
            fs.AddDirectory("Lib/workshop/content/32470/12345678");
            var game = new Mock<IGame>();
            game.Setup(g => g.Exists()).Returns(true);
            game.Setup(g => g.Platform).Returns(GamePlatform.SteamGold);
            game.Setup(g => g.Directory).Returns(fs.DirectoryInfo.FromDirectoryName("Lib/Game/Eaw/Mods"));
            game.Setup(g => g.ModsLocation).Returns(fs.DirectoryInfo.FromDirectoryName("Lib/Game/Eaw/Mods"));

            var mods = new FileSystemModFinder().FindMods(game.Object);
            var mod = Assert.Single(mods);
            Assert.Equal("12345678", mod.Identifier);
            Assert.Equal(ModType.Workshops, mod.Type);
        }
    }
}