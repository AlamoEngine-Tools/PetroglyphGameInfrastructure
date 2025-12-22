using System;
using System.Diagnostics;
using System.IO.Abstractions;
using System.Runtime.InteropServices;
using Microsoft.Extensions.DependencyInjection;
using PG.StarWarsGame.Infrastructure.Clients;
using PG.StarWarsGame.Infrastructure.Clients.Processes;
using Xunit;

namespace PG.StarWarsGame.Infrastructure.Testing.Clients;

/// <summary>
/// Provides a test implementation for launching game processes and asserting the process creation in a controlled testing environment.
/// </summary>
/// <remarks>
/// This class is designed to simulate the behavior of a game process launcher, allowing for assertions and
/// controlled exceptions during testing scenarios. It ensures that the expected executable and process
/// information match the provided inputs.
/// </remarks>
public sealed class TestGameProcessLauncher : IGameProcessLauncher, IDisposable
{
    private Process? _process;

    /// <summary>
    /// Gets or sets a value indicating whether the <see cref="TestGameProcessLauncher"/> should throw
    /// a <see cref="GameStartException"/> when attempting to start a game process.
    /// </summary>
    /// <value>
    /// <see langword="true"/> if a <see cref="GameStartException"/> should be thrown during the game process start;
    /// otherwise, <see langword="false"/>.
    /// </value>
    /// <remarks>
    /// This property is primarily used in testing scenarios to simulate failure conditions
    /// when starting a game process.
    /// </remarks>
    public bool ThrowsGameStartException { get; set; }

    /// <summary>
    /// Gets or sets the expected executable file for the game process during testing.
    /// </summary>
    public IFileInfo ExpectedExecutable { get; set; } = null!;

    /// <summary>
    /// Gets or sets the expected process information used for validating the game process during testing.
    /// </summary>
    public GameProcessInfo ExpectedProcessInfo { get; set; } = null!;

    /// <summary>
    /// Registers the specified <see cref="TestGameProcessLauncher"/> instance as a singleton service  in the provided <see cref="IServiceCollection"/>.
    /// </summary>
    /// <param name="serviceCollection">The <see cref="IServiceCollection"/> to which the service will be added.</param>
    /// <param name="processLauncher">The <see cref="TestGameProcessLauncher"/> instance to register as the implementation of <see cref="IGameProcessLauncher"/>.</param>
    public static void RegisterAsService(IServiceCollection serviceCollection, TestGameProcessLauncher processLauncher)
    {
        serviceCollection.AddSingleton<IGameProcessLauncher>(processLauncher);
    }

    /// <summary>
    /// Initiates the execution of a fake game process using the provided executable.
    /// The inputs <paramref name="executable"/> and <paramref name="processInfo"/> are for asserted
    /// against <see cref="ExpectedExecutable"/> and <see cref="ExpectedProcessInfo"/>.
    /// </summary>
    /// <param name="executable">The executable file that will be launched to start the game process.</param>
    /// <param name="processInfo">An object containing details about the game process, such as the game instance, build type, and any additional arguments.</param>
    /// <returns>A new instance of <see cref="IGameProcess"/> representing the running game process.</returns>
    /// <exception cref="GameStartException">Thrown if <see cref="ThrowsGameStartException"/> is set to <see langword="true"/>.</exception>
    public IGameProcess StartGameProcess(IFileInfo executable, GameProcessInfo processInfo)
    {
        Assert.Equal(ExpectedExecutable.FullName, executable.FullName);
        Assert.Same(ExpectedProcessInfo.Game, processInfo.Game);
        Assert.Equal(ExpectedProcessInfo.BuildType, processInfo.BuildType);
        Assert.Equal(ExpectedProcessInfo.Arguments, processInfo.Arguments);

        if (ThrowsGameStartException)
            throw new GameStartException(processInfo.Game, "Some exception");

        var processName = RuntimeInformation.IsOSPlatform(OSPlatform.Windows) ? "cmd.exe" : "/bin/bash";
        var process = new Process();
        process.StartInfo.FileName = processName;
        process.StartInfo.CreateNoWindow = true;
        process.Start();
        _process = process;
        return new GameProcess(process, processInfo);
    }

    /// <summary>
    /// Releases the resources used by the <see cref="TestGameProcessLauncher"/> instance, including terminating
    /// and disposing of any associated game process.
    /// </summary>
    /// <remarks>
    /// This method ensures that any running game process started by this launcher is properly terminated
    /// and its resources are released. Exceptions during the disposal process are suppressed.
    /// </remarks>
    public void Dispose()
    {
        try
        {
            _process?.Kill();
            _process?.Dispose();
        }
        catch
        {
            // Ignore
        }
    }
}