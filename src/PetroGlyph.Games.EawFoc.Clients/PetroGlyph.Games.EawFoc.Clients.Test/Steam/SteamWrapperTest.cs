using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO.Abstractions;
using System.IO.Abstractions.TestingHelpers;
using Moq;
using PetroGlyph.Games.EawFoc.Clients.Steam;
using Xunit;

namespace PetroGlyph.Games.EawFoc.Clients.Test.Steam
{
    public class SteamWrapperTest
    {
        [Fact]
        public void TestInvalidArgs_Throws()
        {
            Assert.Throws<ArgumentNullException>(() => new SteamWrapper(null, null));
            var sp = new Mock<IServiceProvider>();
            Assert.Throws<ArgumentNullException>(() => new SteamWrapper(null, sp.Object));
            var reg = new Mock<ISteamRegistry>();
            Assert.Throws<ArgumentNullException>(() => new SteamWrapper(reg.Object, null));
        }

        [Fact]
        public void TestRunning()
        {
            var processHelper = new Mock<Processes.IProcessHelper>();
            processHelper.SetupSequence(h => h.GetProcessByPid(It.IsAny<int>()))
                .Returns((Process)null)
                .Returns(Process.GetCurrentProcess);
            var sp = new Mock<IServiceProvider>();
            sp.Setup(p => p.GetService(typeof(Processes.IProcessHelper))).Returns(processHelper.Object);
            var reg = new Mock<ISteamRegistry>();
            reg.SetupSequence(r => r.ProcessId)
                .Returns((int?)null)
                .Returns(0)
                .Returns(123)
                .Returns(123);

            var steam = new SteamWrapper(reg.Object, sp.Object);
            Assert.False(steam.IsRunning);
            Assert.False(steam.IsRunning);
            Assert.False(steam.IsRunning);
            Assert.True(steam.IsRunning);
        }

        [Fact]
        public void TestInstalled()
        {
            var fs = new MockFileSystem();
            var sp = new Mock<IServiceProvider>();
            var reg = new Mock<ISteamRegistry>();
            reg.SetupSequence(r => r.ExeFile)
                .Returns((IFileInfo?)null)
                .Returns(fs.FileInfo.FromFileName("steam.exe"))
                .Returns(fs.FileInfo.FromFileName("steam.exe"));

            var steam = new SteamWrapper(reg.Object, sp.Object);
            Assert.False(steam.Installed);
            Assert.False(steam.Installed);

            fs.AddFile("steam.exe", MockFileData.NullObject);

            Assert.True(steam.Installed);
        }

        [Fact]
        public void TestUserLoggedIn()
        {
            var sp = new Mock<IServiceProvider>();
            var reg = new Mock<ISteamRegistry>();
            reg.SetupSequence(r => r.ActiveUserId)
                .Returns((int?)null)
                .Returns(0)
                .Returns(123);

            var steam = new SteamWrapper(reg.Object, sp.Object);

            Assert.False(steam.IsUserLoggedIn);
            Assert.False(steam.IsUserLoggedIn);
            Assert.True(steam.IsUserLoggedIn);
        }

        [Fact]
        public void TestGameInstalled()
        {
            var reg = SetupInstalledRegistry(out var fs);
            var finder = new Mock<ISteamGameLocationFinder>();
            finder.SetupSequence(f => f.FindGame(It.IsAny<IDirectoryInfo>(), It.IsAny<uint>()))
                .Returns((IDirectoryInfo?)null)
                .Returns(fs.DirectoryInfo.FromDirectoryName("Game"));
            var sp = new Mock<IServiceProvider>();
            sp.Setup(p => p.GetService(typeof(ISteamGameLocationFinder))).Returns(finder.Object);
            reg.Setup(r => r.InstalledApps).Returns(new HashSet<uint> { 1, 2, 3 });
            reg.Setup(r => r.InstallationDirectory).Returns(fs.DirectoryInfo.FromDirectoryName("Steam"));

            var steam = new SteamWrapper(reg.Object, sp.Object);

            Assert.False(steam.IsGameInstalled(0, out _));
            Assert.False(steam.IsGameInstalled(1, out _));
            Assert.True(steam.IsGameInstalled(1, out _));
        }

        [Fact]
        public void TestWantsOffline()
        {
            var reg = SetupInstalledRegistry(out var fs);
            var sp = new Mock<IServiceProvider>();
            sp.Setup(p => p.GetService(typeof(IFileSystem))).Returns(fs);
            reg.Setup(r => r.InstallationDirectory).Returns(fs.DirectoryInfo.FromDirectoryName("."));

            var steam = new SteamWrapper(reg.Object, sp.Object);

            Assert.Null(steam.WantOfflineMode);

            fs.AddFile("config/loginusers.vdf", MockFileData.NullObject);
            Assert.Null(steam.WantOfflineMode);

            fs.AddFile("config/loginusers.vdf", WantsNotOffline());
            Assert.False(steam.WantOfflineMode);

            fs.AddFile("config/loginusers.vdf", WantsOffline());
            Assert.True(steam.WantOfflineMode);
        }
        
        private static Mock<ISteamRegistry> SetupInstalledRegistry(out MockFileSystem fileSystem)
        {
            fileSystem = new MockFileSystem();
            fileSystem.AddFile("steam.exe", MockFileData.NullObject);
            var reg = new Mock<ISteamRegistry>();
            reg.Setup(r => r.ExeFile).Returns(fileSystem.FileInfo.FromFileName("steam.exe"));
            return reg;
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