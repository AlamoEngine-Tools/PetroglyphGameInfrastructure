using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO.Abstractions;
using System.IO.Abstractions.TestingHelpers;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using PetroGlyph.Games.EawFoc.Clients.Processes;
using PetroGlyph.Games.EawFoc.Clients.Steam;
using Xunit;

namespace PetroGlyph.Games.EawFoc.Clients.Windows.Test.Steam
{
    public class SteamWrapperTest
    {
        private readonly SteamWrapper _service;
        private readonly Mock<ISteamRegistry> _steamRegistry;
        private readonly MockFileSystem _fileSystem;
        private readonly Mock<IProcessHelper> _processHelper;
        private readonly Mock<ISteamGameFinder> _gameFinder;

        public SteamWrapperTest()
        {
            var sc = new ServiceCollection();
            _steamRegistry = new Mock<ISteamRegistry>();
            _fileSystem = new MockFileSystem();
            _processHelper = new Mock<IProcessHelper>();
            _gameFinder = new Mock<ISteamGameFinder>();
            sc.AddTransient(_ => _steamRegistry.Object);
            sc.AddTransient(_ => _processHelper.Object);
            sc.AddTransient(_ => _gameFinder.Object);
            sc.AddTransient<IFileSystem>(_ => _fileSystem);
            _service = new SteamWrapper(sc.BuildServiceProvider());
        }

        [Fact]
        public void TestInvalidArgs_Throws()
        {
            Assert.Throws<ArgumentNullException>(() => new SteamWrapper(null));
        }

        [Fact]
        public void TestRunning()
        {
            _processHelper.SetupSequence(h => h.GetProcessByPid(It.IsAny<int>()))
                .Returns((Process)null)
                .Returns(Process.GetCurrentProcess);
            _steamRegistry.SetupSequence(r => r.ProcessId)
                .Returns((int?)null)
                .Returns(0)
                .Returns(123)
                .Returns(123);

            Assert.False(_service.IsRunning);
            Assert.False(_service.IsRunning);
            Assert.False(_service.IsRunning);
            Assert.True(_service.IsRunning);
        }

        [Fact]
        public void TestInstalled()
        {
            _steamRegistry.SetupSequence(r => r.ExeFile)
                .Returns((IFileInfo?)null)
                .Returns(_fileSystem.FileInfo.FromFileName("steam.exe"))
                .Returns(_fileSystem.FileInfo.FromFileName("steam.exe"));

            Assert.False(_service.Installed);
            Assert.False(_service.Installed);

            _fileSystem.AddFile("steam.exe", MockFileData.NullObject);

            Assert.True(_service.Installed);
        }

        [Fact]
        public void TestUserLoggedIn()
        {
            _steamRegistry.SetupSequence(r => r.ActiveUserId)
                .Returns((int?)null)
                .Returns(0)
                .Returns(123);

            Assert.False(_service.IsUserLoggedIn);
            Assert.False(_service.IsUserLoggedIn);
            Assert.True(_service.IsUserLoggedIn);
        }

        [Fact]
        public void TestGameInstalled()
        {
            var reg = SetupInstalledRegistry();
            var mFile = _fileSystem.FileInfo.FromFileName("manifest.acf");
            _gameFinder.SetupSequence(f => f.FindGame(It.IsAny<IDirectoryInfo>(), It.IsAny<uint>()))
                .Returns((SteamAppManifest?)null)
                .Returns(new SteamAppManifest(mFile, 1234, "name", _fileSystem.DirectoryInfo.FromDirectoryName("Game"),
                    SteamAppState.StateFullyInstalled, new HashSet<uint>()));
            reg.Setup(r => r.InstalledApps).Returns(new HashSet<uint> { 1, 2, 3 });
            reg.Setup(r => r.InstallationDirectory).Returns(_fileSystem.DirectoryInfo.FromDirectoryName("Steam"));

            Assert.False(_service.IsGameInstalled(0, out _));
            Assert.False(_service.IsGameInstalled(1, out _));
            Assert.True(_service.IsGameInstalled(1, out _));
        }

        [Fact]
        public void TestWantsOffline()
        {
            var reg = SetupInstalledRegistry();
           
            reg.Setup(r => r.InstallationDirectory).Returns(_fileSystem.DirectoryInfo.FromDirectoryName("."));

            Assert.Null(_service.WantOfflineMode);

            _fileSystem.AddFile("config/loginusers.vdf", MockFileData.NullObject);
            Assert.Null(_service.WantOfflineMode);

            _fileSystem.AddFile("config/loginusers.vdf", WantsNotOffline());
            Assert.False(_service.WantOfflineMode);

            _fileSystem.AddFile("config/loginusers.vdf", WantsOffline());
            Assert.True(_service.WantOfflineMode);
        }
        
        private Mock<ISteamRegistry> SetupInstalledRegistry()
        {
            _fileSystem.AddFile("steam.exe", MockFileData.NullObject);
            _steamRegistry.Setup(r => r.ExeFile).Returns(_fileSystem.FileInfo.FromFileName("steam.exe"));
            return _steamRegistry;
        }

        private static string WantsNotOffline()
        {
            return "\"users\"\n{\n\t\"123\"\n\t{\n\t\t\"AccountName\"\t\t\"user_name\"\n\t\t\"PersonaName\"\t\t\"User Name\"\n\t\t\"RememberPassword\"\t\t\"1\"\n\t\t\"MostRecent\"\t\t\"1\"\n\t\t\"Timestamp\"\t\t\"0000000000\"\n\t\t\"WantsOfflineMode\"\t\t\"0\"\n\t\t\"SkipOfflineModeWarning\"\t\t\"0\"\n\t}\n}";
        }

        private static string WantsOffline()
        {
            return "\"users\"\n{\n\t\"123\"\n\t{\n\t\t\"AccountName\"\t\t\"user_name\"\n\t\t\"PersonaName\"\t\t\"User Name\"\n\t\t\"RememberPassword\"\t\t\"1\"\n\t\t\"MostRecent\"\t\t\"1\"\n\t\t\"Timestamp\"\t\t\"0000000000\"\n\t\t\"WantsOfflineMode\"\t\t\"1\"\n\t\t\"SkipOfflineModeWarning\"\t\t\"0\"\n\t}\n}";
        }
    }
}