using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using AET.SteamAbstraction.Utilities;
using AET.Testing;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace AET.SteamAbstraction.Test;

public class ProcessHelperTest : TestBaseWithServiceProvider
{
    private readonly IProcessHelper _processHelper;

    public ProcessHelperTest()
    {
        _processHelper = ServiceProvider.GetRequiredService<IProcessHelper>();
    }

    protected override void SetupServices(IServiceCollection serviceCollection)
    {
        base.SetupServices(serviceCollection);
        SteamAbstractionLayer.InitializeServices(serviceCollection);
    }

    [Fact]
    public void IsProcessRunning_NotRunning()
    {
        Assert.False(_processHelper.IsProcessRunning(new Random().Next(int.MinValue, -1)));
    }

    [Fact]
    public void IsProcessRunning_IsRunning()
    {
        var thisProcess = Process.GetCurrentProcess();
        Assert.True(_processHelper.IsProcessRunning(thisProcess.Id));
    }

    [Fact]
    public void StartProcess()
    {
        var processName = RuntimeInformation.IsOSPlatform(OSPlatform.Windows) ? "cmd.exe" : "/bin/bash";

        var process = _processHelper.StartProcess(new ProcessStartInfo(processName)
        {
            UseShellExecute = false,
        });

        try
        {
            Assert.NotNull(process);
        }
        finally
        {
            try
            {
                process?.Kill();
            }
            catch (InvalidOperationException)
            {
            }
        }
    }
}