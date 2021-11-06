using System;
using Microsoft.Extensions.DependencyInjection;
using PetroGlyph.Games.EawFoc.Clients.Arguments;
using PetroGlyph.Games.EawFoc.Clients.Processes;
using PetroGlyph.Games.EawFoc.Mods;

namespace PetroGlyph.Games.EawFoc.Clients;

public sealed class DefaultClient : ClientBase
{
    public DefaultClient(IServiceProvider serviceProvider) : base(serviceProvider)
    {
    }

    /// <summary>
    /// Starts the game bound to the given <paramref name="instance"/>.
    /// If <paramref name="instance"/> represents an <see cref="IMod"/> the mod's dependencies get added to the launch arguments.
    /// </summary>
    /// <param name="instance">The game or mod to play.</param>
    /// <returns>The game's process.</returns>
    public override IGameProcess Play(IPlayableObject instance)
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

    protected override IGameProcessLauncher GetGameLauncherService()
    { 
        return ServiceProvider.GetService<IGameProcessLauncher>() ?? new DefaultGameProcessLauncher();
    }
}