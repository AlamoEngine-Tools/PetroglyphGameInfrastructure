using System.IO.Abstractions;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using AET.Testing;
using AnakinRaW.CommonUtilities.Registry;
using AnakinRaW.CommonUtilities.Registry.Windows;
using Microsoft.Extensions.DependencyInjection;
using Testably.Abstractions;
using Xunit;

namespace AET.SteamAbstraction.Test;

public class SteamWrapperIntegrationTest : TestBaseWithServiceProvider
{
    private readonly ISteamWrapper _service;

    public SteamWrapperIntegrationTest()
    {
        _service = ServiceProvider.GetRequiredService<ISteamWrapperFactory>().CreateWrapper();
    }

    protected override void SetupServices(IServiceCollection serviceCollection)
    {
        base.SetupServices(serviceCollection);
        serviceCollection.AddSingleton<IFileSystem>(new RealFileSystem());
        SteamAbstractionLayer.InitializeServices(serviceCollection);
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            serviceCollection.AddSingleton<IRegistry>(new WindowsRegistry());
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
        Assert.True(running);
    }

    //[Fact]
    public async Task WaitRunning()
    {
        await _service.WaitSteamRunningAndLoggedInAsync(true);
    }
}