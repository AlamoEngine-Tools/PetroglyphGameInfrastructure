using System;
using System.Diagnostics;
using System.IO.Abstractions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using PG.StarWarsGame.Infrastructure.Clients.Arguments;
using PG.StarWarsGame.Infrastructure.Clients.Arguments.CommandLine;

namespace PG.StarWarsGame.Infrastructure.Clients.Processes;

internal sealed class GameProcessLauncher(IServiceProvider serviceProvider) : IGameProcessLauncher
{
    private readonly ILogger? _logger = serviceProvider.GetService<ILoggerFactory>()?.CreateLogger(typeof(GameProcessLauncher));

    public IGameProcess StartGameProcess(IFileInfo executable, GameProcessInfo processInfo)
    {
        try
        {
            var arguments = ArgumentCommandLineBuilder.BuildCommandLine(processInfo.Arguments);

            _logger?.LogInformation($"Starting game '{processInfo.Game}' in '{processInfo.BuildType}' configuration and with launch arguments '{arguments}'");

            var processStartInfo = new ProcessStartInfo(executable.FullName)
            {
                Arguments = arguments,
                WorkingDirectory = processInfo.Game.Directory.FullName,
                UseShellExecute = false
            };
            var process = new Process
            {
                StartInfo = processStartInfo
            };
            process.Start();
            return new GameProcess(process, processInfo);
        }
        catch (GameArgumentException e)
        {
            throw new GameStartException(processInfo.Game, "Illegal argument(s) passed.", e);
        }
        catch (Exception e)
        {
            throw new GameStartException(processInfo.Game, "Unable to start the game", e);
        }
    }
}