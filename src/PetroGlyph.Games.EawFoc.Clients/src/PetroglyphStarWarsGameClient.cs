using System;
using AnakinRaW.CommonUtilities;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using PG.StarWarsGame.Infrastructure.Clients.Arguments;
using PG.StarWarsGame.Infrastructure.Clients.Processes;
using PG.StarWarsGame.Infrastructure.Clients.Utilities;
using PG.StarWarsGame.Infrastructure.Games;
using PG.StarWarsGame.Infrastructure.Mods;

namespace PG.StarWarsGame.Infrastructure.Clients;

/// <summary>
/// Represents a client for a Petroglyph Star Wars game which allows launching the game.
/// </summary>
public class PetroglyphStarWarsGameClient : DisposableObject, IGameClient
{
    /// <inheritdoc />
    public event EventHandler<IGameProcess>? GameStarted;

    /// <inheritdoc />
    public event EventHandler<GameStartingEventArgs>? GameStarting;

    private readonly IGameProcessLauncher _gameProcessLauncher;

    /// <summary>
    /// Returns the service provider of this instance.
    /// </summary>
    protected readonly IServiceProvider ServiceProvider;

    /// <summary>
    /// Returns the Logger of this instance or <see langword="null"/> if not logger is present.
    /// </summary>
    protected readonly ILogger? Logger;

    /// <inheritdoc />
    public IGame Game { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="PetroglyphStarWarsGameClient"/> class with the specified game.
    /// </summary>
    /// <param name="game">The game of the client.</param>
    /// <param name="serviceProvider">The service provider.</param>
    /// <exception cref="ArgumentNullException"><paramref name="game"/> or <paramref name="serviceProvider"/> is <see langword="null"/>.</exception>
    public PetroglyphStarWarsGameClient(IGame game, IServiceProvider serviceProvider)
    {
        Game = game ?? throw new ArgumentNullException(nameof(game));
        ServiceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
        Logger = serviceProvider.GetService<ILoggerFactory>()?.CreateLogger(GetType());
        _gameProcessLauncher = serviceProvider.GetRequiredService<IGameProcessLauncher>();
    }

    /// <inheritdoc />
    public bool IsDebugAvailable()
    {
        var debugExecutable = GameExecutableFileUtilities.GetExecutableForGame(Game, GameBuildType.Debug);
        return debugExecutable is not null && debugExecutable.Exists;
    }

    /// <inheritdoc />
    public IGameProcess Play()
    {
        return StartGame(ArgumentCollection.Empty, GameBuildType.Release);
    }

    /// <inheritdoc />
    public IGameProcess Play(IMod mod)
    {
        if (mod == null)
            throw new ArgumentNullException(nameof(mod));
        if (!ReferenceEquals(mod.Game, Game))
            throw new GameStartException(Game,
                "The game reference of the specified mod is not the same as the game reference of this client instance.");

        var modArgsFactory = new ModArgumentListFactory(ServiceProvider);
        var modArgs = modArgsFactory.BuildArgumentList(mod, true);
        var argumentsBuilder = new UniqueArgumentCollectionBuilder()
            .Add(modArgs);
        return StartGame(argumentsBuilder.Build(), GameBuildType.Release);
    }

    /// <inheritdoc />
    public IGameProcess Play(IArgumentCollection arguments)
    {
        return StartGame(arguments, GameBuildType.Release);
    }

    /// <inheritdoc />
    public IGameProcess Debug(IArgumentCollection arguments, bool fallbackToPlay)
    {
        var buildType = GameBuildType.Debug;
        if (!IsDebugAvailable())
        {
            if (!fallbackToPlay)
                throw new GameStartException(Game, "Debug version of the game could not be found.");
            buildType = GameBuildType.Release;
            Logger?.LogTrace($"Falling back to release configuration because debug executables of '{Game}' were not found.");
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
        var gameProcess = _gameProcessLauncher.StartGameProcess(executable, processInfo);
        OnGameStartedInternal(gameProcess);
        return gameProcess;
    }

    /// <summary>
    /// Invoked whenever <see cref="Game"/> has been requested to start.
    /// </summary>
    /// <param name="arguments">The requested arguments.</param>
    /// <param name="type">The requested build type.</param>
    protected virtual void OnGameStarting(IArgumentCollection arguments, GameBuildType type)
    {
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
        OnGameStarting(arguments, type);

        var gameStartingArgs = new GameStartingEventArgs(Game, arguments, type);
        GameStarting?.Invoke(this, gameStartingArgs);
        return !gameStartingArgs.Cancel;
    }
}