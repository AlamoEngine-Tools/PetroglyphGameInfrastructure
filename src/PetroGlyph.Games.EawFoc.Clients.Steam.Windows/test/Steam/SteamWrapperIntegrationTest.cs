using System;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace PG.StarWarsGame.Infrastructure.Clients.Steam.Windows.Test.Steam;

public class SteamWrapperIntegrationTest
{
    private readonly SteamWrapper _service;
    private readonly IServiceProvider _sp;

    public SteamWrapperIntegrationTest()
    {
        var sc = new ServiceCollection();
        PetroglyphClientsLibrary.InitializeLibraryWithDefaultServices(sc); 
        PetroglyphWindowsSteamClientsLibrary.InitializeLibraryWithDefaultServices(sc);

        _sp = sc.BuildServiceProvider();
        _service = _sp.GetRequiredService<ISteamWrapper>() as SteamWrapper ?? throw new InvalidOperationException();
    }

    //[Fact]
    public void TestGameInstalled()
    {
        Assert.False(_service.IsGameInstalled(0, out _));
        Assert.True(_service.IsGameInstalled(32470, out _));
    }

    //[Fact]
    public void Running()
    {
        var running = _service.IsRunning;
    }

    //[Fact]
    public async Task WaitRunning()
    {
        await _service.WaitSteamRunningAndLoggedInAsync(false);
    }
}