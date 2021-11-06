using System;
using System.Diagnostics;
using System.IO.Abstractions;
using System.Security;

namespace PetroGlyph.Games.EawFoc.Clients.Processes;

internal class DefaultGameProcessLauncher : IGameProcessLauncher
{
    public IGameProcess StartGameProcess(IFileInfo executable, GameProcessInfo processInfo)
    {
        try
        {
            var arguments = processInfo.Arguments.ToCommandlineString();
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
        catch (SecurityException e)
        {
            throw new GameStartException(processInfo.PlayedInstance, "Illegal argument(s) passed.", e);
        }
        catch (Exception e)
        {
            throw new GameStartException(processInfo.PlayedInstance, "Unable to start the game", e);
        }
    }
}