using System.Collections.Generic;
using System.IO.Abstractions;
using AET.SteamAbstraction.Games;
using AET.SteamAbstraction.Library;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Testably.Abstractions.Testing;
using Xunit;

namespace AET.SteamAbstraction.Test;

public class SteamGameFinderTest
{
    private readonly SteamGameFinder _service;
    private readonly MockFileSystem _fileSystem;
    private readonly Mock<ISteamLibraryFinder> _libraryFinder;

    public SteamGameFinderTest()
    {
            var sc = new ServiceCollection();
            _fileSystem = new MockFileSystem();
            _libraryFinder = new Mock<ISteamLibraryFinder>();
            sc.AddTransient<IFileSystem>(_ => _fileSystem);
            sc.AddTransient(_ => _libraryFinder.Object);
            _service = new SteamGameFinder(sc.BuildServiceProvider());
        }

    [Fact]
    public void TestGameNotFound()
    {
            _libraryFinder.Setup(l => l.FindLibraries()).Returns(new List<ISteamLibrary>());
            var app = _service.FindGame(456);
            Assert.Null(app);
        }

    [Fact]
    public void TestGameFound()
    {
            var lib1 = new Mock<ISteamLibrary>();
            var lib2 = new Mock<ISteamLibrary>();

            var manifestLoc = _fileSystem.FileInfo.New("Lib2\\manifest.txt");
            var gameLoc = _fileSystem.DirectoryInfo.New("Lib2\\Game123");
            var game = new SteamAppManifest(lib2.Object, manifestLoc, 123, "Game", gameLoc, SteamAppState.StateFullyInstalled,
                new HashSet<uint>());

            lib2.Setup(l => l.GetApps()).Returns(new List<SteamAppManifest>{game});

            _libraryFinder.Setup(l => l.FindLibraries()).Returns(new List<ISteamLibrary> { lib1.Object, lib2.Object });
            var app = _service.FindGame(123);
            Assert.NotNull(app);
            Assert.Equal(game, app);
        }
}