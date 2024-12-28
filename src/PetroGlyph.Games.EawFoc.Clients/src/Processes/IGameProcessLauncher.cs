using System.IO.Abstractions;

namespace PG.StarWarsGame.Infrastructure.Clients.Processes;

internal interface IGameProcessLauncher
{
    IGameProcess StartGameProcess(IFileInfo executable, GameProcessInfo processInfo);
}