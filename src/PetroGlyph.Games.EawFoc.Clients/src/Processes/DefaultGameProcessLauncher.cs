﻿using System;
using System.Diagnostics;
using System.IO.Abstractions;
using Microsoft.Extensions.DependencyInjection;
using PetroGlyph.Games.EawFoc.Clients.Arguments;
using Validation;

namespace PetroGlyph.Games.EawFoc.Clients.Processes;

internal class DefaultGameProcessLauncher : IGameProcessLauncher
{
    private readonly IServiceProvider _serviceProvider;

    public DefaultGameProcessLauncher(IServiceProvider serviceProvider)
    {
        Requires.NotNull(serviceProvider, nameof(serviceProvider));
        _serviceProvider = serviceProvider;
    }

    public IGameProcess StartGameProcess(IFileInfo executable, GameProcessInfo processInfo)
    {
        try
        {
            var arguments = _serviceProvider.GetRequiredService<IArgumentCommandLineBuilder>()
                .BuildCommandLine(processInfo.Arguments);
            var processStartInfo = new ProcessStartInfo(executable.FullName)
            {
                Arguments = arguments,
                WorkingDirectory = processInfo.PlayedInstance.Game.Directory.FullName,
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
            throw new GameStartException(processInfo.PlayedInstance, "Illegal argument(s) passed.", e);
        }
        catch (Exception e)
        {
            throw new GameStartException(processInfo.PlayedInstance, "Unable to start the game", e);
        }
    }
}