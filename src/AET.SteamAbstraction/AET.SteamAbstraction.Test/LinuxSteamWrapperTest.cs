using System;
using System.IO.Abstractions;
using AET.SteamAbstraction.Games;
using AET.SteamAbstraction.Registry;
using AET.SteamAbstraction.Utilities;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Testably.Abstractions.Testing;

namespace AET.SteamAbstraction.Test;

public class LinuxSteamWrapperTest
{
    private readonly IServiceProvider _serviceProvider;
    private readonly Mock<ISteamRegistry> _steamRegistry;
    private readonly MockFileSystem _fileSystem;
    private readonly Mock<IProcessHelper> _processHelper;
    private readonly Mock<ISteamGameFinder> _gameFinder;

    private readonly LinuxSteamWrapper _steamWrapper;

    public LinuxSteamWrapperTest()
    {
        var sc = new ServiceCollection();
        _steamRegistry = new Mock<ISteamRegistry>();
        _fileSystem = new MockFileSystem();
        _gameFinder = new Mock<ISteamGameFinder>();
        _processHelper = new Mock<IProcessHelper>();
        sc.AddTransient(_ => _steamRegistry.Object);
        sc.AddTransient(_ => _processHelper.Object);
        sc.AddTransient(_ => _gameFinder.Object);
        sc.AddTransient<IFileSystem>(_ => _fileSystem);
        _serviceProvider = sc.BuildServiceProvider();

        _steamWrapper = new LinuxSteamWrapper(_steamRegistry.Object, _serviceProvider);
    }
}