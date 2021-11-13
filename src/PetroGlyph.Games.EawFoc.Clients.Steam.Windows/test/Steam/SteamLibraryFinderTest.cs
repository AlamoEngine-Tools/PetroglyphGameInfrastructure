using System.Collections.Generic;
using System.IO.Abstractions;
using System.IO.Abstractions.TestingHelpers;
using System.Linq;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Xunit;

namespace PetroGlyph.Games.EawFoc.Clients.Steam.Windows.Test.Steam
{
    public class SteamLibraryFinderTest
    {
        private readonly SteamLibraryFinder _service;
        private readonly Mock<ILibraryConfigReader> _reader;
        private readonly Mock<ISteamRegistry> _registry;
        private readonly MockFileSystem _fileSystem;


        public SteamLibraryFinderTest()
        {
            var sc = new ServiceCollection();
            _fileSystem = new MockFileSystem();
            sc.AddSingleton<IFileSystem>(_fileSystem);
            sc.AddSingleton<ISteamRegistry>(sp => new SteamRegistry(sp));
            _reader = new Mock<ILibraryConfigReader>();
            sc.AddTransient(_ => _reader.Object);
            _registry = new Mock<ISteamRegistry>();
            sc.AddTransient(_ => _registry.Object);
            _service = new SteamLibraryFinder(sc.BuildServiceProvider());
        }

        [Fact]
        public void TestNoSteam_Throws()
        {
            Assert.Throws<SteamException>(() => _service.FindLibraries());
        }

        [Fact]
        public void TestNoLibrariesFound()
        {
            _fileSystem.AddDirectory("Steam");
            _registry.Setup(r => r.InstallationDirectory)
                .Returns(_fileSystem.DirectoryInfo.FromDirectoryName("Steam"));

            var libraries = _service.FindLibraries();

            Assert.Empty(libraries);
        }

        [Fact]
        public void TestNoLibrary_MissingSteamDll()
        {
            var libraryLocation = _fileSystem.DirectoryInfo.FromDirectoryName("Steam");
            _fileSystem.AddFile("Steam/steamapps/libraryfolders.vdf", MockFileData.NullObject);
            _registry.Setup(r => r.InstallationDirectory)
                .Returns(_fileSystem.DirectoryInfo.FromDirectoryName("Steam"));
            _reader.Setup(r => r.ReadLibraryLocationsFromConfig(It.IsAny<IFileInfo>()))
                .Returns(new List<IDirectoryInfo> { libraryLocation });

            var libraries = _service.FindLibraries();

            Assert.Empty(libraries);
        }

        [Fact]
        public void TestNoLibrary_NoConfigFile()
        {
            var libraryLocation = _fileSystem.DirectoryInfo.FromDirectoryName("Steam");
            _fileSystem.AddFile("Steam/noValidConfigPath/libraryfolders.vdf", MockFileData.NullObject);
            _fileSystem.AddFile("Steam/steam.dll", MockFileData.NullObject);
            _registry.Setup(r => r.InstallationDirectory)
                .Returns(_fileSystem.DirectoryInfo.FromDirectoryName("Steam"));
            _reader.Setup(r => r.ReadLibraryLocationsFromConfig(It.IsAny<IFileInfo>()))
                .Returns(new List<IDirectoryInfo> { libraryLocation });

            var libraries = _service.FindLibraries();

            Assert.Empty(libraries);
        }

        [Fact]
        public void TestSingleLibrary()
        {
            var libraryLocation = _fileSystem.DirectoryInfo.FromDirectoryName("Steam");
            _fileSystem.AddFile("Steam/steamapps/libraryfolders.vdf", MockFileData.NullObject);
            _fileSystem.AddFile("Steam/steam.dll", MockFileData.NullObject);
            _registry.Setup(r => r.InstallationDirectory)
                .Returns(_fileSystem.DirectoryInfo.FromDirectoryName("Steam"));
            _reader.Setup(r => r.ReadLibraryLocationsFromConfig(It.IsAny<IFileInfo>()))
                .Returns(new List<IDirectoryInfo> { libraryLocation });

            var libraries = _service.FindLibraries();

            Assert.Single(libraries);
            Assert.Equal(libraryLocation, libraries.First().LibraryLocation);
        }

        [Fact]
        public void TestSingleAlternateConfig()
        {
            var libraryLocation = _fileSystem.DirectoryInfo.FromDirectoryName("Steam");
            _fileSystem.AddFile("Steam/config/libraryfolders.vdf", MockFileData.NullObject);
            _fileSystem.AddFile("Steam/steam.dll", MockFileData.NullObject);
            _registry.Setup(r => r.InstallationDirectory)
                .Returns(_fileSystem.DirectoryInfo.FromDirectoryName("Steam"));
            _reader.Setup(r => r.ReadLibraryLocationsFromConfig(It.IsAny<IFileInfo>()))
                .Returns(new List<IDirectoryInfo> { libraryLocation });

            var libraries = _service.FindLibraries();

            Assert.Single(libraries);
            Assert.Equal(libraryLocation, libraries.First().LibraryLocation);
        }

        [Fact]
        public void TestUniqueResult()
        {
            var libraryLocation = _fileSystem.DirectoryInfo.FromDirectoryName("Steam");
            _fileSystem.AddFile("Steam/config/libraryfolders.vdf", MockFileData.NullObject);
            _fileSystem.AddFile("Steam/steam.dll", MockFileData.NullObject);
            _registry.Setup(r => r.InstallationDirectory)
                .Returns(_fileSystem.DirectoryInfo.FromDirectoryName("Steam"));
            _reader.Setup(r => r.ReadLibraryLocationsFromConfig(It.IsAny<IFileInfo>()))
                .Returns(new List<IDirectoryInfo> { libraryLocation, libraryLocation });

            var libraries = _service.FindLibraries();

            Assert.Single(libraries);
        }
    }
}
