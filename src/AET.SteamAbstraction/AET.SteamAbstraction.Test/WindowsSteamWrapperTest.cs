using System;
using System.Diagnostics;
using System.IO.Abstractions;
using AET.SteamAbstraction.Games;
using AET.SteamAbstraction.Utilities;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using PG.TestingUtilities;
using Testably.Abstractions.Testing;
using Xunit;

namespace AET.SteamAbstraction.Test;

public class WindowsSteamWrapperTest
{
    private readonly IServiceProvider _serviceProvider;
    private readonly Mock<IWindowsSteamRegistry> _steamRegistry;
    private readonly MockFileSystem _fileSystem;
    private readonly Mock<IProcessHelper> _processHelper;
    private readonly Mock<ISteamGameFinder> _gameFinder;

    private readonly WindowsSteamWrapper _steamWrapper;

    public WindowsSteamWrapperTest()
    {
        var sc = new ServiceCollection();
        _steamRegistry = new Mock<IWindowsSteamRegistry>();
        _fileSystem = new MockFileSystem();
        _gameFinder = new Mock<ISteamGameFinder>();
        _processHelper = new Mock<IProcessHelper>();
        sc.AddTransient(_ => _steamRegistry.Object);
        sc.AddTransient(_ => _processHelper.Object);
        sc.AddTransient(_ => _gameFinder.Object);
        sc.AddTransient<IFileSystem>(_ => _fileSystem);
        _serviceProvider = sc.BuildServiceProvider();

        _steamWrapper = new WindowsSteamWrapper(_steamRegistry.Object, _serviceProvider);
    }

    [PlatformSpecificFact(TestPlatformIdentifier.Windows)]
    public void TestRunning()
    {
        _steamRegistry.SetupSequence(r => r.ProcessId)
            .Returns((int?)null)
            .Returns(0)
            .Returns(123)
            .Returns(123);

        Assert.False(_steamWrapper.IsRunning);
        Assert.False(_steamWrapper.IsRunning);
        Assert.False(_steamWrapper.IsRunning);

        _processHelper.SetupSequence(h => h.GetProcessByPid(It.IsAny<int>()))
            .Returns(Process.GetCurrentProcess);

        Assert.True(_steamWrapper.IsRunning);
    }
}