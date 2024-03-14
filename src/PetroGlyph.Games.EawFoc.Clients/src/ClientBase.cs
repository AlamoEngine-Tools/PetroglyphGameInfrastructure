using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.DependencyInjection;
using PetroGlyph.Games.EawFoc;
using PetroGlyph.Games.EawFoc.Games;
using PetroGlyph.Games.EawFoc.Mods;
using PG.StarWarsGame.Infrastructure.Clients.Arguments;
using PG.StarWarsGame.Infrastructure.Clients.Processes;

namespace PG.StarWarsGame.Infrastructure.Clients;

/// <summary>
/// Base implementation for an <see cref="IGameClient"/> which handles registration and raising of the game events. 
/// </summary>
public abstract class ClientBase : IGameClient
{
    /// <inheritdoc/>
    public event EventHandler<IGameProcess>? GameStarted;
    /// <inheritdoc/>
    public event EventHandler<GameStartingEventArgs>? GameStarting;
    /// <inheritdoc/>
    public event EventHandler<IGameProcess>? GameClosed;

    private readonly ICollection<IGameProcess> _runningInstances = new List<IGameProcess>();
    private readonly object _syncObj = new();

    /// <summary>
    /// Service provider for this instance.
    /// </summary>
    protected readonly IServiceProvider ServiceProvider;

    /// <summary>
    /// By default this gives an empty <see cref="IArgumentCollection"/>.
    /// </summary>
    public virtual IArgumentCollection DefaultArguments => ArgumentCollection.Empty;

    /// <inheritdoc/>
    public IReadOnlyCollection<IGameProcess> RunningInstances
    {
        get
        {
            lock (_syncObj)
                return _runningInstances.ToList();
        }
    }

    /// <inheritdoc/>
    public abstract ISet<GamePlatform> SupportedPlatforms { get; }

    /// <summary>
    /// Initializes a new client.
    /// </summary>
    /// <param name="serviceProvider">The service provider.</param>
    protected ClientBase(IServiceProvider serviceProvider)
    {
        ServiceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
    }

    /// <inheritdoc/>
    public IGameProcess Play(IPlayableObject instance)
    {
        return StartGame(instance, GameBuildType.Release);
    }

    /// <inheritdoc/>
    public IGameProcess Play(IPlayableObject instance, IArgumentCollection arguments)
    {
        return StartGame(instance, arguments, GameBuildType.Release);
    }

    /// <summary>
    /// Plays the given <paramref name="instance"/> with <see cref="DefaultArguments"/>.
    /// <para>
    /// If <paramref name="instance"/> in an <see cref="IMod"/> the arguments passed
    /// will be <see cref="DefaultArguments"/> merged with the dependency chain of the given mod.
    /// </para>
    /// </summary>
    /// <param name="instance">The game or mod to start.</param>
    /// <param name="buildType">The requested build type to start.</param>
    /// <returns>The process of the started game.</returns>
    /// <exception cref="GameStartException">
    /// The game's platform is not supported by this client.
    /// OR
    /// Starting the game was cancelled by one <see cref="GameStarting"/> handler.
    /// OR
    /// The executable file of the game was not found.
    /// OR
    /// An internal error occurred.
    /// </exception>
    protected IGameProcess StartGame(IPlayableObject instance, GameBuildType buildType)
    {
        var arguments = DefaultArguments;
        if (instance is IMod mod)
        {
            var modArgFactory = ServiceProvider.GetRequiredService<IModArgumentListFactory>();
            var modArgs = modArgFactory.BuildArgumentList(mod, false);
            var builder = ServiceProvider.GetRequiredService<IArgumentCollectionBuilder>();
            arguments = builder.AddAll(DefaultArguments).Add(modArgs).Build();
        }
        return StartGame(instance, arguments, buildType);
    }

    /// <summary>
    /// Starts a <paramref name="instance"/> with the given <paramref name="arguments"/> and the expected <paramref name="type"/>.
    /// </summary>
    /// <param name="instance">The instance to start.</param>
    /// <param name="arguments">The requested arguments.</param>
    /// <param name="type">The requested <see cref="GameBuildType"/>.</param>
    /// <returns>The game process.</returns>
    /// <exception cref="GameStartException">
    /// The game's platform is not supported by this client.
    /// OR
    /// Starting the game was cancelled by one <see cref="GameStarting"/> handler.
    /// OR
    /// The executable file of the game was not found.
    /// OR
    /// An internal error occurred.
    /// </exception>
    protected IGameProcess StartGame(IPlayableObject instance, IArgumentCollection arguments, GameBuildType type)
    {
        if (instance == null)
            throw new ArgumentNullException(nameof(instance));
        if (arguments == null) 
            throw new ArgumentNullException(nameof(arguments));

        if (!SupportedPlatforms.Contains(instance.Game.Platform))
            throw new GameStartException(instance,
                $"Unable to start {instance}, because its platform '{instance.Game.Platform}' is not supported by this client.");

        if (!OnGameStartingInternal(instance, arguments, type))
            throw new GameStartException(instance, "Game starting was aborted.");

        var exeService = ServiceProvider.GetRequiredService<IGameExecutableFileService>();
        var executable = exeService.GetExecutableForGame(instance.Game, type);
        if (executable is null || !executable.Exists)
            throw new GameStartException(instance, $"Executable for {instance} could not be found.");

        var processInfo = new GameProcessInfo(instance, type, arguments);
        var gameLauncher = GetGameLauncherService();
        var gameProcess = gameLauncher.StartGameProcess(executable, processInfo);
        OnGameStartedInternal(gameProcess);
        return gameProcess;
    }
    
    private void OnGameStartedInternal(IGameProcess gameProcess)
    {
        OnGameStarted(gameProcess);
        // Quit immediately if process was already terminated.
        if (gameProcess.State == GameProcessState.Closed)
            return;
        GameStarted?.Invoke(this, gameProcess);
        lock (_syncObj)
        {
            gameProcess.Closed += OnGameProcessClosed;
            if (gameProcess.State == GameProcessState.Closed)
                return;
            _runningInstances.Add(gameProcess);
        }
    }

    private void OnGameProcessClosed(object? sender, EventArgs e)
    {
        if (sender is not IGameProcess gameProcess)
            return;
        lock (_syncObj)
            _runningInstances.Remove(gameProcess);
        try
        {
            GameClosed?.Invoke(this, gameProcess);
        }
        finally
        {
            gameProcess.Closed -= OnGameProcessClosed;
        }
    }

    private bool OnGameStartingInternal(IPlayableObject instance,IArgumentCollection arguments, GameBuildType type)
    {
        var gameStartingArgs = new GameStartingEventArgs(instance, arguments, type);
        if (!OnGameStarting(instance, arguments, type))
            return false;
        GameStarting?.Invoke(this, gameStartingArgs);
        return !gameStartingArgs.Cancel;
    }

    /// <summary>
    /// Gets called before the game get's started.
    /// </summary>
    /// <remarks>
    /// Important: Do not raise the <see cref="GameStarting"/> event. This will be done by the base class.
    /// When this method returns <see langword="false"/> <see cref="GameStarting"/> will not get raised
    /// and the game will not get started.
    /// </remarks>
    /// <param name="instance">The instance that was requested to play.</param>
    /// <param name="arguments">The requested argument collection</param>
    /// <param name="type">The game build type.</param>
    /// <returns><see langword="true"/> if the game starting procedure shall continue; <see langword="false"/> otherwise.</returns>
    protected internal virtual bool OnGameStarting(IPlayableObject instance, IArgumentCollection arguments, GameBuildType type)
    {
        return true;
    }

    /// <summary>
    /// Gets called before right after the game process was started.
    /// </summary>
    /// <remarks>
    /// This method races with the <paramref name="gameProcess"/> lifetime.
    /// It is possible that the process terminates before this method is finished.
    /// Important: Do not raise the <see cref="GameStarted"/> event. This will be done by the base class.
    /// </remarks>
    protected internal virtual void OnGameStarted(IGameProcess gameProcess)
    {
    }

    /// <summary>
    /// Retrieves an <see cref="IGameProcessLauncher"/> for this instance to start the game process.
    /// </summary>
    /// <returns>The <see cref="IGameProcessLauncher"/> instance.</returns>
    protected internal abstract IGameProcessLauncher GetGameLauncherService();
}