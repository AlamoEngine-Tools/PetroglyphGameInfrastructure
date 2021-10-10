using System;
using System.Collections.Generic;
using System.IO.Abstractions;
using System.IO.Abstractions.TestingHelpers;
using Moq;
using PetroGlyph.Games.EawFoc.Games;
using PetroGlyph.Games.EawFoc.Services.Detection.Platform;
using Xunit;

namespace PetroGlyph.Games.EawFoc.Test.GameServices.Detection
{
    public class GamePlatformIdentifierTest
    {
        [Fact]
        public void TestNullArg_Throws()
        {
            Assert.Throws<ArgumentNullException>(() => new GamePlatformIdentifier(null!));
            var sp = new Mock<IServiceProvider>();
            var identifier = new GamePlatformIdentifier(sp.Object);
            IDirectoryInfo? nullRef = null;
            Assert.Throws<ArgumentNullException>(() => identifier.GetGamePlatform(default, ref nullRef!));
            Assert.Throws<ArgumentNullException>(() => identifier.GetGamePlatform(default, ref nullRef!, new List<GamePlatform>()));
            var fs = new MockFileSystem();
            var locRef = fs.DirectoryInfo.FromDirectoryName("Game");
            Assert.Throws<ArgumentNullException>(() => identifier.GetGamePlatform(default, ref locRef, null!));
        }

        [Fact]
        public void TestNoInput()
        {
            var sp = new Mock<IServiceProvider>();
            var identifier = new GamePlatformIdentifier(sp.Object);
            var fs = new MockFileSystem();
            var locRef = fs.DirectoryInfo.FromDirectoryName("Game");
            var actual = identifier.GetGamePlatform(default, ref locRef, new List<GamePlatform>());
            Assert.Equal(GamePlatform.Undefined, actual);
        }

        [Fact]
        public void TestUndefinedWhenNoKnownGameInput()
        {
            var sp = new Mock<IServiceProvider>();
            var identifier = new GamePlatformIdentifier(sp.Object);
            var fs = new MockFileSystem();
            var locRef = fs.DirectoryInfo.FromDirectoryName("Game");
            var actual = identifier.GetGamePlatform(default, ref locRef);
            Assert.Equal(GamePlatform.Undefined, actual);
        }

        [Fact]
        public void TestUndefinedWhenNoRequestedGameInput()
        {
            var sp = new Mock<IServiceProvider>();
            var identifier = new GamePlatformIdentifier(sp.Object);

            var locRef = Disk_Eaw();
            var lookup = new List<GamePlatform> { GamePlatform.SteamGold };

            var actual = identifier.GetGamePlatform(default, ref locRef, lookup);
            Assert.Equal(GamePlatform.Undefined, actual);
        }

        [Fact]
        public void TestEawDisk()
        {
            var sp = new Mock<IServiceProvider>();
            var identifier = new GamePlatformIdentifier(sp.Object);

            var locRef = Disk_Eaw();
            var lookup = new List<GamePlatform> { GamePlatform.Disk };
            const GameType type = GameType.EaW;

            var actual = identifier.GetGamePlatform(type, ref locRef, lookup);
            Assert.Equal(GamePlatform.Disk, actual);
        }

        [Fact]
        public void TestFocDisk()
        {
            var sp = new Mock<IServiceProvider>();
            var identifier = new GamePlatformIdentifier(sp.Object);

            var locRef = Disk_Foc();
            var lookup = new List<GamePlatform> { GamePlatform.Disk };
            const GameType type = GameType.Foc;

            var actual = identifier.GetGamePlatform(type, ref locRef, lookup);
            Assert.Equal(GamePlatform.Disk, actual);
        }

        [Fact]
        public void TestEawSteam()
        {
            var sp = new Mock<IServiceProvider>();
            var identifier = new GamePlatformIdentifier(sp.Object);

            var locRef = Steam_Eaw();
            var lookup = new List<GamePlatform> { GamePlatform.SteamGold };
            const GameType type = GameType.EaW;

            var actual = identifier.GetGamePlatform(type, ref locRef, lookup);
            Assert.Equal(GamePlatform.SteamGold, actual);
        }

        [Fact]
        public void TestFocSteam()
        {
            var sp = new Mock<IServiceProvider>();
            var identifier = new GamePlatformIdentifier(sp.Object);

            var locRef = Steam_Foc();
            var lookup = new List<GamePlatform> { GamePlatform.SteamGold };
            const GameType type = GameType.Foc;

            var actual = identifier.GetGamePlatform(type, ref locRef, lookup);
            Assert.Equal(GamePlatform.SteamGold, actual);
        }

        [Fact]
        public void TestEawGog()
        {
            var sp = new Mock<IServiceProvider>();
            var identifier = new GamePlatformIdentifier(sp.Object);

            var locRef = Gog_Eaw();
            var lookup = new List<GamePlatform> { GamePlatform.GoG };
            const GameType type = GameType.EaW;

            var actual = identifier.GetGamePlatform(type, ref locRef, lookup);
            Assert.Equal(GamePlatform.GoG, actual);
        }

        [Fact]
        public void TestFocGog()
        {
            var sp = new Mock<IServiceProvider>();
            var identifier = new GamePlatformIdentifier(sp.Object);

            var locRef = Gog_Foc();
            var lookup = new List<GamePlatform> { GamePlatform.GoG };
            const GameType type = GameType.Foc;

            var actual = identifier.GetGamePlatform(type, ref locRef, lookup);
            Assert.Equal(GamePlatform.GoG, actual);
        }

        [Fact]
        public void TestEawGold()
        {
            var sp = new Mock<IServiceProvider>();
            var identifier = new GamePlatformIdentifier(sp.Object);

            var locRef = DiskGold_Eaw();
            var lookup = new List<GamePlatform> { GamePlatform.DiskGold };
            const GameType type = GameType.EaW;

            var actual = identifier.GetGamePlatform(type, ref locRef, lookup);
            Assert.Equal(GamePlatform.DiskGold, actual);
        }

        [Fact]
        public void TestFocGold()
        {
            var sp = new Mock<IServiceProvider>();
            var identifier = new GamePlatformIdentifier(sp.Object);

            var locRef = DiskGold_Foc();
            var lookup = new List<GamePlatform> { GamePlatform.DiskGold };
            var type = GameType.Foc;

            var actual = identifier.GetGamePlatform(type, ref locRef, lookup);
            Assert.Equal(GamePlatform.DiskGold, actual);
        }

        [Fact]
        public void TestEawOrigin()
        {
            var sp = new Mock<IServiceProvider>();
            var identifier = new GamePlatformIdentifier(sp.Object);

            var locRef = Origin_Eaw();
            var lookup = new List<GamePlatform> { GamePlatform.Origin };
            const GameType type = GameType.EaW;

            var actual = identifier.GetGamePlatform(type, ref locRef, lookup);
            Assert.Equal(GamePlatform.Origin, actual);
        }

        [Fact]
        public void TestFocOrigin()
        {
            var sp = new Mock<IServiceProvider>();
            var identifier = new GamePlatformIdentifier(sp.Object);

            var locRef = Origin_Foc_Corrected();
            var lookup = new List<GamePlatform> { GamePlatform.Origin };
            var type = GameType.Foc;

            var actual = identifier.GetGamePlatform(type, ref locRef, lookup);
            Assert.Equal(GamePlatform.Origin, actual);
        }

        [Fact]
        public void TestFocOriginWithSanitization()
        {
            var sp = new Mock<IServiceProvider>();
            var identifier = new GamePlatformIdentifier(sp.Object);

            var locRef = Origin_Foc_Registry();
            var locStore = locRef;
            var lookup = new List<GamePlatform> { GamePlatform.Origin };
            var type = GameType.Foc;

            var actual = identifier.GetGamePlatform(type, ref locRef, lookup);
            Assert.Equal(GamePlatform.Origin, actual);
            Assert.NotEqual(locStore, locRef);
        }

        private static IDirectoryInfo Disk_Eaw()
        {
            var fs = new MockFileSystem();
            fs.AddFile("GameData/sweaw.exe", MockFileData.NullObject);
            return fs.DirectoryInfo.FromDirectoryName("GameData");
        }

        private static IDirectoryInfo Disk_Foc()
        {
            var fs = new MockFileSystem();
            fs.AddFile("Game/swfoc.exe", MockFileData.NullObject);
            return fs.DirectoryInfo.FromDirectoryName("Game");
        }

        private static IFileSystem SteamFs()
        {
            var fs = new MockFileSystem();
            fs.AddFile("Game/corruption/swfoc.exe", MockFileData.NullObject);
            fs.AddFile("Game/corruption/StarWarsG.exe", MockFileData.NullObject);
            fs.AddFile("Game/GameData/sweaw.exe", MockFileData.NullObject);
            fs.AddFile("Game/GameData/StarWarsG.exe", MockFileData.NullObject);
            fs.AddFile("Game/32470_install.vdf", MockFileData.NullObject);
            fs.AddFile("Game/32472_install.vdf", MockFileData.NullObject);
            fs.AddFile("Game/runme.dat", MockFileData.NullObject);
            fs.AddFile("Game/runm2.dat", MockFileData.NullObject);
            fs.AddFile("Game/runme.exe", MockFileData.NullObject);
            fs.AddFile("Game/runme2.exe", MockFileData.NullObject);
            return fs;
        }

        private static IDirectoryInfo Steam_Eaw()
        {
            return SteamFs().DirectoryInfo.FromDirectoryName("Game/GameData");
        }

        private static IDirectoryInfo Steam_Foc()
        {
            return SteamFs().DirectoryInfo.FromDirectoryName("Game/corruption");
        }

        private static IFileSystem GogFs()
        {
            var fs = new MockFileSystem();
            fs.AddFile("Game/EAWX/swfoc.exe", MockFileData.NullObject);
            fs.AddFile("Game/corruption/StarWarsG.exe", MockFileData.NullObject);
            fs.AddFile("Game/GameData/sweaw.exe", MockFileData.NullObject);
            fs.AddFile("Game/GameData/goggame-1421404887.dll", MockFileData.NullObject);
            fs.AddFile("Game/goggame.sdb", MockFileData.NullObject);
            fs.AddFile("Game/goggame-1421404887.hashdb", MockFileData.NullObject);
            fs.AddFile("Game/goggame-1421404887.info", MockFileData.NullObject);
            fs.AddFile("Game/Language.exe", MockFileData.NullObject);
            return fs;
        }

        private static IDirectoryInfo Gog_Eaw()
        {
            return GogFs().DirectoryInfo.FromDirectoryName("Game/GameData");
        }

        private static IDirectoryInfo Gog_Foc()
        {
            return GogFs().DirectoryInfo.FromDirectoryName("Game/EAWX");
        }

        private static IFileSystem DiskGoldFs()
        {
            var fs = new MockFileSystem();
            fs.AddFile("Game/Foc/swfoc.exe", MockFileData.NullObject);
            fs.AddFile("Game/Foc/fpupdate.exe", MockFileData.NullObject);
            fs.AddFile("Game/Foc/LaunchEAWX.exe", MockFileData.NullObject);
            fs.AddFile("Game/Foc/main.wav", MockFileData.NullObject);
            fs.AddDirectory("Game/Foc/Install");
            fs.AddDirectory("Game/Foc/Manuals");

            fs.AddFile("Game/Eaw/GameData/sweaw.exe", MockFileData.NullObject);
            fs.AddFile("Game/Eaw/GameData/fpupdate.exe", MockFileData.NullObject);
            fs.AddFile("Game/Eaw/GameData/MCELaunch.exe", MockFileData.NullObject);
            fs.AddFile("Game/Eaw/GameData/StubUpdate.exe", MockFileData.NullObject);
            fs.AddFile("Game/Eaw/LaunchEAW.exe", MockFileData.NullObject);
            fs.AddFile("Game/Eaw/main.wav", MockFileData.NullObject);
            return fs;
        }

        private static IDirectoryInfo DiskGold_Eaw()
        {
            return DiskGoldFs().DirectoryInfo.FromDirectoryName("Game/Eaw/GameData");
        }

        private static IDirectoryInfo DiskGold_Foc()
        {
            return DiskGoldFs().DirectoryInfo.FromDirectoryName("Game/Foc");
        }

        private static IFileSystem OriginFs()
        {
            var fs = new MockFileSystem();
            fs.AddFile("Game/EAWX/swfoc.exe", MockFileData.NullObject);
            fs.AddFile("Game/EAWX/EALaunchHelper.exe", MockFileData.NullObject);

            fs.AddFile("Game/GameData/sweaw.exe", MockFileData.NullObject);
            fs.AddDirectory("Game/Manuals");
            fs.AddDirectory("Game/__Installer");
            return fs;
        }

        private static IDirectoryInfo Origin_Eaw()
        {
            return OriginFs().DirectoryInfo.FromDirectoryName("Game/GameData");
        }

        private static IDirectoryInfo Origin_Foc_Corrected()
        {
            return OriginFs().DirectoryInfo.FromDirectoryName("Game/EAWX");
        }

        private static IDirectoryInfo Origin_Foc_Registry()
        {
            return OriginFs().DirectoryInfo.FromDirectoryName("Game/corruption");
        }
    }
}