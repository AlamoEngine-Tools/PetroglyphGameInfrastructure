using System.IO.Abstractions;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using AnakinRaW.CommonUtilities.Registry;
using AnakinRaW.CommonUtilities.Registry.Windows;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace AET.SteamAbstraction.Test;

public class SteamWrapperIntegrationTest
{
    private readonly ISteamWrapper _service;

    public SteamWrapperIntegrationTest()
    {
        var sc = new ServiceCollection();
        // Use actual FS
        sc.AddSingleton<IFileSystem>(new FileSystem());
        SteamAbstractionLayer.InitializeServices(sc);

        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            sc.AddSingleton<IRegistry>(new WindowsRegistry());
        else
            sc.AddSingleton<IRegistry>(new InMemoryRegistry());

        _service = sc.BuildServiceProvider().GetRequiredService<ISteamWrapperFactory>().CreateWrapper();
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