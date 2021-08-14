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
            var sp = new Mock<IServiceProvider>();
            var reg = SetupInstalledRegistry();
            reg.Setup(r => r.InstalledApps).Returns(new HashSet<uint> { 1, 2, 3 });

            var steam = new SteamWrapper(reg.Object, sp.Object);

            Assert.False(steam.IsGameInstalled(0, out _));
            Assert.True(steam.IsGameInstalled(1, out _));
        }

        private Mock<ISteamRegistry> SetupInstalledRegistry()
        {
            var fs = new MockFileSystem();
            fs.AddFile("steam.exe", MockFileData.NullObject);
            var reg = new Mock<ISteamRegistry>();
            reg.Setup(r => r.ExeFile).Returns(fs.FileInfo.FromFileName("steam.exe"));
            return reg;
        }
    }
}