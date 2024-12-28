namespace PG.StarWarsGame.Infrastructure.Clients.Processes;

internal interface IGameProcessLauncherFactory
{
    IGameProcessLauncher CreateGameProcessLauncher(bool isSteam);
}