using System;
using System.Collections.Generic;
using System.IO.Abstractions;
using System.IO.Abstractions.TestingHelpers;
using System.Runtime.InteropServices;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Xunit;

namespace PetroGlyph.Games.EawFoc.Clients.Steam.Windows.Test.Steam
{
    public class SteamLibraryTest
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly MockFileSystem _fileSystem;
        private readonly Mock<ISteamAppManifestReader> _reader;

        public SteamLibraryTest()
        {
            var sc = new ServiceCollection();
            _fileSystem = new MockFileSystem();
            sc.AddTransient<IFileSystem>(_ => _fileSystem);
            _reader = new Mock<ISteamAppManifestReader>();
            sc.AddTransient(_ => _reader.Object);
            _serviceProvider = sc.BuildServiceProvider();
        }

        [Fact]
        public void TestProperties()
        {
            var lib = new SteamLibrary(_fileSystem.DirectoryInfo.New("Library"), _serviceProvider);
            Assert.NotNull(lib.CommonLocation);
            Assert.NotNull(lib.SteamAppsLocation);
            Assert.NotNull(lib.WorkshopsLocation);
        }

        [Fact]
        public void TestEquality()
        {
            var lib = new SteamLibrary(_fileSystem.DirectoryInfo.New("Library"), _serviceProvider);
            var other = new Mock<ISteamLibrary>();
            other.Setup(o => o.LibraryLocation)
                .Returns(_fileSystem.DirectoryInfo.New("OtherLib"));
            Assert.NotEqual(lib, other.Object);
            other.Setup(o => o.LibraryLocation)
                .Returns(_fileSystem.DirectoryInfo.New("Library"));
            Assert.Equal(lib, other.Object);

            var otherInst = new SteamLibrary(_fileSystem.DirectoryInfo.New("./Library"), _serviceProvider);
            Assert.Equal(lib, otherInst);
            Assert.Equal(lib.GetHashCode(), otherInst.GetHashCode());
        }

        [Fact]
        public void TestNoApps1()
        {
            var lib = new SteamLibrary(_fileSystem.DirectoryInfo.New("Library"), _serviceProvider);

            var app = new SteamAppManifest(lib, _fileSystem.FileInfo.New("file"), 123, "name",
                _fileSystem.DirectoryInfo.New("game"), SteamAppState.StateFullyInstalled,
                new HashSet<uint>());

            _reader.Setup(r => r.ReadManifest(It.IsAny<IFileInfo>(), lib))
                .Returns(app);

            var apps = lib.GetApps();

            Assert.Empty(apps);
        }

        [Fact]
        public void TestNoApps2()
        {
            _fileSystem.AddDirectory("Library/steamapps");
            var lib = new SteamLibrary(_fileSystem.DirectoryInfo.New("Library"), _serviceProvider);

            var app = new SteamAppManifest(lib, _fileSystem.FileInfo.New("file"), 123, "name",
                _fileSystem.DirectoryInfo.New("game"), SteamAppState.StateFullyInstalled,
                new HashSet<uint>());

            _reader.Setup(r => r.ReadManifest(It.IsAny<IFileInfo>(), lib))
                .Returns(app);

            var apps = lib.GetApps();

            Assert.Empty(apps);
        }

        [Fact]
        public void TestApps()
        {
            _fileSystem.AddFile("Library/steamapps/test.acf", new MockFileData(string.Empty));
            var lib = new SteamLibrary(_fileSystem.DirectoryInfo.New("Library"), _serviceProvider);

            var app = new SteamAppManifest(lib, _fileSystem.FileInfo.New("file"), 123, "name",
                _fileSystem.DirectoryInfo.New("game"), SteamAppState.StateFullyInstalled,
                new HashSet<uint>());

            _reader.Setup(r => r.ReadManifest(It.IsAny<IFileInfo>(), lib))
                .Returns(app);

            var apps = lib.GetApps();
            var result = Assert.Single(apps);
            Assert.Same(app, result);
        }

        [Fact]
        public void TestNoDuplicatesApps()
        {
            _fileSystem.AddFile("Library/steamapps/test1.acf", new MockFileData(string.Empty));
            _fileSystem.AddFile("Library/steamapps/test2.acf", new MockFileData(string.Empty));
            var lib = new SteamLibrary(_fileSystem.DirectoryInfo.New("Library"), _serviceProvider);

            var app = new SteamAppManifest(lib, _fileSystem.FileInfo.New("file"), 123, "name",
                _fileSystem.DirectoryInfo.New("game"), SteamAppState.StateFullyInstalled,
                new HashSet<uint>());

            _reader.Setup(r => r.ReadManifest(It.IsAny<IFileInfo>(), lib))
                .Returns(app);

            var apps = lib.GetApps();
            var result = Assert.Single(apps);
            Assert.Same(app, result);
        }

        [Fact]
        public void TestLocations()
        {
            var lib = new SteamLibrary(_fileSystem.DirectoryInfo.New("Library"), _serviceProvider);

            Assert.Equal(
                RuntimeInformation.IsOSPlatform(OSPlatform.Linux)
                    ? "/Library/steamapps/common"
                    : "C:\\Library\\steamapps\\common", lib.CommonLocation.FullName);
        }
    }
}
