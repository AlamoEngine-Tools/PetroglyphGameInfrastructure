using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using AET.SteamAbstraction.Utilities;
using AnakinRaW.CommonUtilities;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace AET.SteamAbstraction.Test;

public class ProcessHelperTest
{
    private readonly IProcessHelper _processHelper;

    public ProcessHelperTest()
    {
        var sc = new ServiceCollection();
        SteamAbstractionLayer.InitializeServices(sc);
        _processHelper = sc.BuildServiceProvider().GetRequiredService<IProcessHelper>();
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
    public async Task StartProcess()
    {
        var processName = RuntimeInformation.IsOSPlatform(OSPlatform.Windows) ? "cmd.exe" : "/bin/bash";
        var expectedExitCode = RuntimeInformation.IsOSPlatform(OSPlatform.Windows) ? -1 : 128 + 9; // https://stackoverflow.com/a/1041309

        var process = _processHelper.StartProcess(new ProcessStartInfo(processName)
        {
            UseShellExecute = false
        });

        Assert.NotNull(process);
        var exitTask = process.WaitForExitAsync();
        process.Kill();
        await exitTask;
        Assert.Equal(expectedExitCode, process.ExitCode);
    }
}