using System;
using Microsoft.Extensions.DependencyInjection;
using PG.StarWarsGame.Infrastructure.Clients.Arguments;
using PG.StarWarsGame.Infrastructure.Clients.Processes;
using PG.StarWarsGame.Infrastructure.Clients.Utilities;
using PG.StarWarsGame.Infrastructure.Games;
using PG.StarWarsGame.Infrastructure.Mods;

namespace PG.StarWarsGame.Infrastructure.Clients;

internal abstract class ClientBase(IGame game, IServiceProvider serviceProvider) : IGameClient
{
    public event EventHandler<IGameProcess>? GameStarted;
    public event EventHandler<GameStartingEventArgs>? GameStarting;

    private readonly IServiceProvider _serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
    private readonly IGameProcessLauncherFactory _gameProcessLauncherFactory = serviceProvider.GetRequiredService<IGameProcessLauncherFactory>();

    public abstract bool SupportsDebug { get; }

    public abstract bool IsSteamClient { get; }

    public IGame Game { get; } = game ?? throw new ArgumentNullException(nameof(game));

    public bool IsDebugAvailable()
    {
        var debugExecutable = GameExecutableFileUtilities.GetExecutableForGame(Game, GameBuildType.Debug);
        return debugExecutable is not null && debugExecutable.Exists;
    }

    public IGameProcess Play()
    {
        return StartGame(ArgumentCollection.Empty, GameBuildType.Release);
    }

    public IGameProcess Play(IMod mod)
    {
        if (mod == null)
            throw new ArgumentNullException(nameof(mod));
        if (!ReferenceEquals(mod.Game, Game))
            throw new GameStartException(Game,
                "The game reference of the specified mod is not the same as the game reference of this client instance.");

        var modArgsFactory = new ModArgumentListFactory(_serviceProvider);
        var modArgs = modArgsFactory.BuildArgumentList(mod, true);
        var argumentsBuilder = new UniqueArgumentCollectionBuilder()
            .Add(modArgs);
        return StartGame(argumentsBuilder.Build(), GameBuildType.Release);
    }

    public IGameProcess Play(IArgumentCollection arguments)
    {
        return StartGame(arguments, GameBuildType.Release);
    }

    public IGameProcess Debug(IArgumentCollection arguments, bool fallbackToPlay)
    {
        var buildType = GameBuildType.Debug;
        if (!IsDebugAvailable())
        {
            if (!fallbackToPlay)
                throw new GameStartException(Game, "Debug version of the game could not be found.");
            buildType = GameBuildType.Release;
        }
        return StartGame(arguments, buildType);
    }

    private IGameProcess StartGame(IArgumentCollection arguments, GameBuildType type)
    {
        if (arguments == null)
            throw new ArgumentNullException(nameof(arguments));

        if (!OnGameStartingInternal(arguments, type))
            throw new GameStartException(Game, "Starting the game was cancelled by event handler.");

        var executable = GameExecutableFileUtilities.GetExecutableForGame(Game, type);
        if (executable is null || !executable.Exists)
            throw new GameStartException(Game, $"Executable files for {Game} could not be found.");

        var processInfo = new GameProcessInfo(Game, type, arguments);
        var gameLauncher = _gameProcessLauncherFactory.CreateGameProcessLauncher(IsSteamClient);
        var gameProcess = gameLauncher.StartGameProcess(executable, processInfo);
        OnGameStartedInternal(gameProcess);
        return gameProcess;
    }

    private void OnGameStartedInternal(IGameProcess gameProcess)
    {
        // Quit immediately if process was already terminated.
        if (gameProcess.State == GameProcessState.Closed)
            return;
        GameStarted?.Invoke(this, gameProcess);
    }

    private bool OnGameStartingInternal(IArgumentCollection arguments, GameBuildType type)
    {
        var gameStartingArgs = new GameStartingEventArgs(Game, arguments, type);
        GameStarting?.Invoke(this, gameStartingArgs);
        return !gameStartingArgs.Cancel;
    }
}