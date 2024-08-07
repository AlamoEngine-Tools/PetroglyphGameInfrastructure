﻿using System.Collections.Generic;
using System.IO.Abstractions;
using System.Linq;
using AET.SteamAbstraction.Library;
using AET.SteamAbstraction.Registry;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Testably.Abstractions.Testing;
using Xunit;

namespace AET.SteamAbstraction.Test;

public class SteamLibraryFinderTest
{
    private readonly SteamLibraryFinder _service;
    private readonly Mock<ISteamVdfReader> _reader;
    private readonly Mock<ISteamRegistry> _registry;
    private readonly MockFileSystem _fileSystem;

    public SteamLibraryFinderTest()
    {
        var sc = new ServiceCollection();
        _fileSystem = new MockFileSystem();
        sc.AddSingleton<IFileSystem>(_fileSystem);

        _registry = new Mock<ISteamRegistry>();

        var regFactory = new Mock<ISteamRegistryFactory>();
        regFactory.Setup(f => f.CreateRegistry()).Returns(_registry.Object);

        sc.AddSingleton(sp => regFactory.Object);
        _reader = new Mock<ISteamVdfReader>();
        sc.AddTransient(_ => _reader.Object);
        
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
        _fileSystem.Initialize().WithSubdirectory("Steam");
        _registry.Setup(r => r.InstallationDirectory)
            .Returns(_fileSystem.DirectoryInfo.New("Steam"));

        var libraries = _service.FindLibraries();

        Assert.Empty(libraries);
    }

    [Fact]
    public void TestNoLibrary_MissingSteamDll()
    {
        _fileSystem.Initialize().WithFile("Steam/steamapps/libraryfolders.vdf");
        var libraryLocation = _fileSystem.DirectoryInfo.New("Steam");
        _registry.Setup(r => r.InstallationDirectory)
            .Returns(_fileSystem.DirectoryInfo.New("Steam"));
        _reader.Setup(r => r.ReadLibraryLocationsFromConfig(It.IsAny<IFileInfo>()))
            .Returns(new List<IDirectoryInfo> { libraryLocation });

        var libraries = _service.FindLibraries();

        Assert.Empty(libraries);
    }

    [Fact]
    public void TestNoLibrary_NoConfigFile()
    {
        _fileSystem.Initialize()
            .WithFile("Steam/noValidConfigPath/libraryfolders.vdf")
            .WithFile("Steam/steam.dll");

        var libraryLocation = _fileSystem.DirectoryInfo.New("Steam");
        _registry.Setup(r => r.InstallationDirectory)
            .Returns(_fileSystem.DirectoryInfo.New("Steam"));
        _reader.Setup(r => r.ReadLibraryLocationsFromConfig(It.IsAny<IFileInfo>()))
            .Returns(new List<IDirectoryInfo> { libraryLocation });

        var libraries = _service.FindLibraries();

        Assert.Empty(libraries);
    }

    [Fact]
    public void TestSingleLibrary()
    {
        _fileSystem.Initialize()
            .WithFile("Steam/steamapps/libraryfolders.vdf")
            .WithFile("Steam/steam.dll");

        var libraryLocation = _fileSystem.DirectoryInfo.New("Steam");
        _registry.Setup(r => r.InstallationDirectory)
            .Returns(_fileSystem.DirectoryInfo.New("Steam"));
        _reader.Setup(r => r.ReadLibraryLocationsFromConfig(It.IsAny<IFileInfo>()))
            .Returns(new List<IDirectoryInfo> { libraryLocation });

        var libraries = _service.FindLibraries();

        Assert.Single(libraries);
        Assert.Equal(libraryLocation, libraries.First().LibraryLocation);
    }

    [Fact]
    public void TestSingleAlternateConfig()
    {
        _fileSystem.Initialize()
            .WithFile("Steam/config/libraryfolders.vdf")
            .WithFile("Steam/steam.dll");

        var libraryLocation = _fileSystem.DirectoryInfo.New("Steam");
        _registry.Setup(r => r.InstallationDirectory)
            .Returns(_fileSystem.DirectoryInfo.New("Steam"));
        _reader.Setup(r => r.ReadLibraryLocationsFromConfig(It.IsAny<IFileInfo>()))
            .Returns(new List<IDirectoryInfo> { libraryLocation });

        var libraries = _service.FindLibraries();

        Assert.Single(libraries);
        Assert.Equal(libraryLocation, libraries.First().LibraryLocation);
    }

    [Fact]
    public void TestUniqueResult()
    {
        _fileSystem.Initialize()
            .WithFile("Steam/config/libraryfolders.vdf")
            .WithFile("Steam/steam.dll");

        var libraryLocation = _fileSystem.DirectoryInfo.New("Steam");
        _registry.Setup(r => r.InstallationDirectory)
            .Returns(_fileSystem.DirectoryInfo.New("Steam"));
        _reader.Setup(r => r.ReadLibraryLocationsFromConfig(It.IsAny<IFileInfo>()))
            .Returns(new List<IDirectoryInfo> { libraryLocation, libraryLocation });

        var libraries = _service.FindLibraries();

        Assert.Single(libraries);
    }
}