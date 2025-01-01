using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO.Abstractions;
using System.Linq;
using AnakinRaW.CommonUtilities;
using EawModinfo.Spec;
using PG.StarWarsGame.Infrastructure.Clients.Arguments.GameArguments;
using PG.StarWarsGame.Infrastructure.Mods;

namespace PG.StarWarsGame.Infrastructure.Clients.Arguments;

/// <summary>
/// Builder service to create an <see cref="ArgumentCollection"/>. Arguments of the same name will be replaced.
/// </summary>
public class GameArgumentsBuilder : DisposableObject
{
    private readonly Dictionary<string, GameArgument> _argumentDict = new();
    private List<IMod>? _mods;
    private IDirectoryInfo? _gameDirForMods;

    /// <summary>
    /// Initializes a new instance of the <see cref="GameArgumentsBuilder"/> class with no initial arguments.
    /// </summary>
    public GameArgumentsBuilder()
    {
    }

    /// <summary>
    /// Adds a game argument to the <see cref="GameArgumentsBuilder"/>.
    /// </summary>
    /// <remarks>
    /// If the argument is already present in the <see cref="GameArgumentsBuilder"/>, the value old argument is overwritten by <paramref name="argument"/>.
    /// </remarks>
    /// <param name="argument">The argument to add.</param>
    /// <returns></returns>
    /// <exception cref="ArgumentNullException"><paramref name="argument"/> is <see langword="null"/>.</exception>
    /// <exception cref="NotSupportedException"><paramref name="argument"/> is a <see cref="ModArgumentList"/> or <see cref="ModArgument"/>.</exception>
    /// <exception cref="ObjectDisposedException">The <see cref="GameArgumentsBuilder"/> is disposed.</exception>
    public GameArgumentsBuilder Add(GameArgument argument)
    {
        ThrowIfDisposed();
        if (argument == null) 
            throw new ArgumentNullException(nameof(argument));

        if (argument is ModArgument or ModArgumentList)
            throw new NotSupportedException(
                $"Adding an instance of {nameof(ModArgument)} or {nameof(ModArgumentList)} directly is not supported. Use AddSingleMod() or AddMods()");

        _argumentDict[argument.Name] = argument;
        return this;
    }

    /// <summary>
    /// Adds a single mod to the <see cref="GameArgumentsBuilder"/>.
    /// </summary>
    /// <remarks>
    /// Previous mods added to the <see cref="GameArgumentsBuilder"/> are removed.
    /// </remarks>
    /// <param name="mod">The mod add.</param>
    /// <returns>This instance.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="mod"/> is <see langword="null"/>.</exception>
    /// <exception cref="ObjectDisposedException">The <see cref="GameArgumentsBuilder"/> is disposed.</exception>
    public GameArgumentsBuilder AddSingleMod(IPhysicalMod mod)
    {
        ThrowIfDisposed();
        if (mod == null)
            throw new ArgumentNullException(nameof(mod));
        return AddSingleMod(mod, mod.Game.Directory);
    }

    /// <summary>
    /// Adds a single mod to the <see cref="GameArgumentsBuilder"/> with an alternate game directory.
    /// </summary>
    /// <remarks>
    /// Previous mods added to the <see cref="GameArgumentsBuilder"/> are removed.
    /// </remarks>
    /// <param name="mod">The mod add.</param>
    /// <param name="gameDir">The directory of the target game.</param>
    /// <returns>This instance.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="mod"/> is <see langword="null"/>.</exception>
    /// <exception cref="ObjectDisposedException">The <see cref="GameArgumentsBuilder"/> is disposed.</exception>
    public GameArgumentsBuilder AddSingleMod(IPhysicalMod mod, IDirectoryInfo gameDir)
    {
        ThrowIfDisposed();
        if (mod == null)
            throw new ArgumentNullException(nameof(mod));
        if (gameDir == null) 
            throw new ArgumentNullException(nameof(gameDir));
        return AddMods([mod], gameDir);
    }

    /// <summary>
    /// Adds a list of mods to the <see cref="GameArgumentsBuilder"/>.
    /// </summary>
    /// <remarks>
    /// Previous mods added to the <see cref="GameArgumentsBuilder"/> are removed.
    /// </remarks>
    /// <param name="mods">The list of mods to add.</param>
    /// <returns>This instance.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="mods"/> is <see langword="null"/>.</exception>
    /// <exception cref="ArgumentException"><paramref name="mods"/> contains a null reference.</exception>
    /// <exception cref="ObjectDisposedException">The <see cref="GameArgumentsBuilder"/> is disposed.</exception>
    public GameArgumentsBuilder AddMods(IList<IMod> mods)
    {
        ThrowIfDisposed();
        if (mods == null) 
            throw new ArgumentNullException(nameof(mods));

        ClearMods();

        if (mods.Count == 0)
            return this;

        var gameDir = mods.First().Game.Directory;
        return AddMods(mods, gameDir);
    }

    /// <summary>
    /// Adds a list of mods to the <see cref="GameArgumentsBuilder"/> with an alternate game directory.
    /// </summary>
    /// <remarks>
    /// Previous mods added to the <see cref="GameArgumentsBuilder"/> are removed.
    /// </remarks>
    /// <param name="mods">The list of mods to add.</param>
    /// <param name="gameDir">The directory of the target game.</param>
    /// <returns>This instance.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="mods"/> is <see langword="null"/>.</exception>
    /// <exception cref="ArgumentException"><paramref name="mods"/> contains a null reference.</exception>
    /// <exception cref="ObjectDisposedException">The <see cref="GameArgumentsBuilder"/> is disposed.</exception>
    public GameArgumentsBuilder AddMods(IList<IMod> mods, IDirectoryInfo gameDir)
    {
        ThrowIfDisposed();
        if (mods == null) 
            throw new ArgumentNullException(nameof(mods));
        if (gameDir == null) 
            throw new ArgumentNullException(nameof(gameDir));

        ClearMods();

        if (mods.Count == 0)
            return this;
        
        var modList = new List<IMod>(mods.Count);
        foreach (var mod in mods)
        {
            if (mod is null)
                throw new ArgumentException("The mod list contains a null reference.", nameof(mods));
            modList.Add(mod);
        }
        _mods = modList;
        _gameDirForMods = gameDir;

        return this;
    }

    /// <summary>
    /// Removes the argument if present from the <see cref="GameArgumentsBuilder"/>.
    /// </summary>
    /// <param name="argument">The argument to remove.</param>
    /// <returns>This instance.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="argument"/> is <see langword="null"/>.</exception>
    /// <exception cref="ObjectDisposedException">The <see cref="GameArgumentsBuilder"/> is disposed.</exception>
    public GameArgumentsBuilder Remove(GameArgument argument)
    {
        ThrowIfDisposed();
        if (argument == null)
            throw new ArgumentNullException(nameof(argument));
        return Remove(argument.Name);
    }

    /// <summary>
    /// Removes the argument with the specified <paramref name="name"/> from the <see cref="GameArgumentsBuilder"/>.
    /// </summary>
    /// <param name="name">The name of the argument to remove.</param>
    /// <returns>This instance.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="name"/> is <see langword="null"/>.</exception>
    /// <exception cref="ArgumentException"><paramref name="name"/> is empty.</exception>
    /// <exception cref="ObjectDisposedException">The <see cref="GameArgumentsBuilder"/> is disposed.</exception>
    public GameArgumentsBuilder Remove(string name)
    {
        ThrowIfDisposed();
        AnakinRaW.CommonUtilities.ThrowHelper.ThrowIfNullOrEmpty(name);
        _argumentDict.Remove(name);
        return this;
    }

    /// <summary>
    /// Removes all mods from the <see cref="GameArgumentsBuilder"/>.
    /// </summary>
    /// <returns>This instance.</returns>
    /// <exception cref="ObjectDisposedException">The <see cref="GameArgumentsBuilder"/> is disposed.</exception>
    public GameArgumentsBuilder RemoveMods()
    {
        ThrowIfDisposed();
        ClearMods();
        return this;
    }

    /// <summary>
    /// Builds an <see cref="ArgumentCollection"/> from the <see cref="GameArgumentsBuilder"/>.
    /// </summary>
    /// <returns>The created argument collection.</returns>
    /// <exception cref="GameArgumentException">One or more arguments of the <see cref="GameArgumentsBuilder"/> are not valid.</exception>
    /// <exception cref="ObjectDisposedException">The <see cref="GameArgumentsBuilder"/> is disposed.</exception>
    public ArgumentCollection Build()
    {
        ThrowIfDisposed();
        if (_argumentDict.Keys.Count == 0 && (_mods is null || _mods.Count == 0))
            return ArgumentCollection.Empty;

        var arguments = new List<GameArgument>();
        
        foreach (var argumentPair in _argumentDict)
        {
            var argument = argumentPair.Value;
            if (!argument.IsValid(out var reason))
                throw new GameArgumentException(argument, $"Unable to create argument list: The argument '{argument}' is not valid: '{reason.GetInvalidArgMessage()}'");
            arguments.Add(argument);
        }

        var modArgs = BuildModArgumentList(_mods);
        if (modArgs.Value.Count > 0)
            arguments.Add(modArgs);

        return new ArgumentCollection(arguments);
    }

    /// <inheritdoc />
    protected override void DisposeManagedResources()
    {
        base.DisposeManagedResources();
        ClearMods();
        _argumentDict.Clear();
    }

    private ModArgumentList BuildModArgumentList(List<IMod>? mods)
    {
        if (mods is null || mods.Count == 0)
            return ModArgumentList.Empty;

        Debug.Assert(_gameDirForMods is not null);

        var dependencyArgs = new List<ModArgument>(mods.Count);
        foreach (var dependency in mods)
        {
            // Virtual and non-physical mods are just "placeholders" we cannot add them to the argument list.
            if (dependency.Type == ModType.Virtual || dependency is not IPhysicalMod physicalMod)
                continue;
            var isWorkshop = physicalMod.Type == ModType.Workshops;
            dependencyArgs.Add(new ModArgument(physicalMod.Directory, _gameDirForMods!, isWorkshop));
        }

        return new ModArgumentList(dependencyArgs);
    }

    private void ClearMods()
    {
        _mods?.Clear();
        _mods = null;
        _gameDirForMods = null;
    }
}