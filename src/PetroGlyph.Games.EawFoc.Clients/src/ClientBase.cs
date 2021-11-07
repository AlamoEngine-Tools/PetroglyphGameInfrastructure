using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.DependencyInjection;
using PetroGlyph.Games.EawFoc.Clients.Arguments;
using PetroGlyph.Games.EawFoc.Clients.Processes;
using PetroGlyph.Games.EawFoc.Games;
using PetroGlyph.Games.EawFoc.Mods;
using Validation;

namespace PetroGlyph.Games.EawFoc.Clients;

public abstract class ClientBase : IGameClient
{
    public event EventHandler<IGameProcess>? GameStarted;
    public event EventHandler<GameStartingEventArgs>? GameStarting;
    public event EventHandler<IGameProcess>? GameClosed;

    private readonly ICollection<IGameProcess> _runningInstances = new List<IGameProcess>();
    private readonly object _syncObj = new();

    protected readonly IServiceProvider ServiceProvider;

    public virtual IGameArgumentCollection DefaultArguments => new EmptyArgumentsCollection();

    public IReadOnlyCollection<IPlayableObject> RunningInstances =>
        _runningInstances.Select(i => i.ProcessInfo.PlayedInstance).ToList();

    public ISet<GamePlatform> SupportedPlatforms =>
        new HashSet<GamePlatform>
        {
            GamePlatform.Disk,
            GamePlatform.DiskGold,
            GamePlatform.GoG,
            GamePlatform.Origin,
            GamePlatform.SteamGold
        };

    protected ClientBase(IServiceProvider serviceProvider)
    {
        Requires.NotNull(serviceProvider, nameof(serviceProvider));
        ServiceProvider = serviceProvider;
    }

    /// <summary>
    /// Starts the game bound to the given <paramref name="instance"/>.
    /// If <paramref name="instance"/> represents an <see cref="IMod"/> the mod's dependencies get added to the launch arguments.
    /// </summary>
    /// <param name="instance">The game or mod to play.</param>
    /// <returns>The game's process.</returns>
    public virtual IGameProcess Play(IPlayableObject instance)
    {
        var arguments = DefaultArguments;
        if (instance is IMod mod)
        {
            var argFactory = ServiceProvider.GetService<IModArgumentListFactory>() ?? new ModArgumentListFactory(ServiceProvider);
            var modArgs = argFactory.BuildArgumentList(mod);
            arguments = ArgumentCollection.Merge(DefaultArguments, modArgs);
        }
        return Play(instance, arguments);
    }

    public IGameProcess Play(IPlayableObject instance, IGameArgumentCollection arguments)
    {
        return StartGame(instance, arguments, GameBuildType.Release);
    }

    protected IGameProcess StartGame(IPlayableObject instance, IGameArgumentCollection arguments, GameBuildType type)
    {
        Requires.NotNull(instance, nameof(instance));
        Requires.NotNull(arguments, nameof(arguments));

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
        if (gameProcess.State == GameProcessState.Closed)
            return;
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

    private bool OnGameStartingInternal(IPlayableObject instance, IReadOnlyCollection<IGameArgument> arguments, GameBuildType type)
    {
        var gameStartingArgs = new GameStartingEventArgs(instance.Game, arguments, type);
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
    protected virtual bool OnGameStarting(IPlayableObject instance, IReadOnlyCollection<IGameArgument> arguments, GameBuildType type)
    {
        return true;
    }

    /// <summary>
    /// Gets called before right after the game process was started.
    /// </summary>
    /// <remarks>
    /// Important: Do not raise the <see cref="GameStarted"/> event. This will be done by the base class.
    /// </remarks>
    protected virtual void OnGameStarted(IGameProcess gameProcess)
    {
    }

    protected abstract IGameProcessLauncher GetGameLauncherService();
}