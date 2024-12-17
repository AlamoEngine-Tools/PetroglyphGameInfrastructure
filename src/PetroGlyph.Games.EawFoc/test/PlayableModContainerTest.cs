using System;
using EawModinfo.Model;
using PG.StarWarsGame.Infrastructure.Games;
using PG.StarWarsGame.Infrastructure.Mods;
using PG.StarWarsGame.Infrastructure.Testing;
using PG.StarWarsGame.Infrastructure.Testing.Mods;
using Xunit;
#if NET5_0_OR_GREATER
using System.Collections.Generic;
#else
using PG.TestingUtilities;
#endif

namespace PG.StarWarsGame.Infrastructure.Test;

public abstract class PlayableModContainerTest : PlayableObjectTest
{
    private readonly string _randomModName;

    protected abstract PlayableModContainer CreateModContainer();

    protected PlayableModContainerTest()
    {
        var i = new Random().Next(0, 100);
        _randomModName = $"Mod-{i}";
    }

    [Fact]
    public void Mods_NoMods()
    {
        var obj = CreateModContainer();
        Assert.Empty(obj.Mods);
    }

    [Fact]
    public void AddMod_RemoveMod()
    {
        var container = CreateModContainer();
        var game = container.Game;

        var mod = game.InstallMod(_randomModName, false, ServiceProvider);
        var modSame = game.InstallMod(_randomModName, false, ServiceProvider);

        Assert.DoesNotContain(mod, container.Mods);
        Assert.DoesNotContain(modSame, container.Mods);
        Assert.DoesNotContain(mod, container);
        Assert.DoesNotContain(modSame, container);
        Assert.DoesNotContain(mod, container.Game.Mods);

        Assert.True(container.AddMod(mod));
        Assert.Contains(mod, container.Mods);
        Assert.Contains(mod, container);
        Assert.Contains(mod, container.Game.Mods);
        Assert.Contains(mod, container.Mods, ReferenceEqualityComparer.Instance);
        Assert.Contains(mod, container, ReferenceEqualityComparer.Instance);
        Assert.Contains(mod, container.Game.Mods, ReferenceEqualityComparer.Instance);

        Assert.False(container.AddMod(mod));
        Assert.Contains(mod, container.Mods);
        Assert.Contains(mod, container);
        Assert.Contains(mod, container.Game.Mods);
        Assert.Contains(mod, container.Mods, ReferenceEqualityComparer.Instance);
        Assert.Contains(mod, container, ReferenceEqualityComparer.Instance);
        Assert.Contains(mod, container.Game.Mods, ReferenceEqualityComparer.Instance);

        Assert.False(container.AddMod(modSame));
        Assert.DoesNotContain(modSame, container.Mods, ReferenceEqualityComparer.Instance);
        Assert.DoesNotContain(modSame, container, ReferenceEqualityComparer.Instance);
        Assert.DoesNotContain(modSame, container.Game.Mods, ReferenceEqualityComparer.Instance);

        Assert.True(container.RemoveMod(modSame));
        Assert.DoesNotContain(mod, container.Mods);
        Assert.DoesNotContain(mod, container);
        if (container is IMod)
            Assert.Contains(mod, container.Game.Mods); // Should not remove from game

        Assert.False(container.RemoveMod(mod));

        Assert.DoesNotContain(mod, container.Mods);
        Assert.DoesNotContain(mod, container);
        if (container is IMod)
            Assert.Contains(mod, container.Game.Mods); // Should not remove from game
    }

    [Fact]
    public void AddMod_DifferentGameRef_Throws()
    {
        var container = CreateModContainer();
        var game = container.Game;

        var otherGame = new PetroglyphStarWarsGame(game, game.Directory, game.Name, ServiceProvider);
        var mod = otherGame.InstallAndAddMod(_randomModName, GITestUtilities.GetRandomWorkshopFlag(game), ServiceProvider);
        Assert.Throws<ModException>(() => container.AddMod(mod));
    }

    [Fact]
    public void AddMod_ShouldNotAddSelf()
    {
        var game = CreateModContainer().Game;

        var isWs = GITestUtilities.GetRandomWorkshopFlag(game);
        var mod = game.InstallAndAddMod(_randomModName, isWs, ServiceProvider);
        var sameMod = new Mod(game, mod.Identifier, mod.Directory, isWs, mod.Name, ServiceProvider);

        Assert.False(mod.AddMod(mod));
        Assert.False(mod.AddMod(sameMod));
    }

    [Fact]
    public void AddMod_RemoveMod_RaiseEvent()
    {
        var container = CreateModContainer();
        var game = container.Game;

        var mod = game.InstallMod(_randomModName, GITestUtilities.GetRandomWorkshopFlag(game), ServiceProvider);

        var evtAdd = Assert.Raises<ModCollectionChangedEventArgs>(
            a => container.ModsCollectionModified += a,
            a => container.ModsCollectionModified -= a,
            () => { container.AddMod(mod); }
        );

        Assert.NotNull(evtAdd);
        Assert.Equal(container, evtAdd.Sender);
        Assert.Equal(mod, evtAdd.Arguments.Mod);
        Assert.Equal(ModCollectionChangedAction.Add, evtAdd.Arguments.Action);

        var evtRemove = Assert.Raises<ModCollectionChangedEventArgs>(
            a => container.ModsCollectionModified += a,
            a => container.ModsCollectionModified -= a,
            () => { container.RemoveMod(mod); }
        );

        Assert.NotNull(evtRemove);
        Assert.Equal(container, evtRemove.Sender);
        Assert.Equal(mod, evtRemove.Arguments.Mod);
        Assert.Equal(ModCollectionChangedAction.Remove, evtRemove.Arguments.Action);
    }


    [Fact]
    public void RemoveMod_NotExisting_DoesNotRaiseEvent()
    {
        var container = CreateModContainer();
        var game = container.Game;

        var mod = game.InstallMod(_randomModName, GITestUtilities.GetRandomWorkshopFlag(game), ServiceProvider);

        var containerRaised = false;
        container.ModsCollectionModified += (_, _) =>
        {
            containerRaised = true;
        };

        var gameRaised = false;
        container.Game.ModsCollectionModified += (_, _) =>
        {
            gameRaised = true;
        };

        game.RemoveMod(mod);
        Assert.False(containerRaised);
        Assert.False(gameRaised);
    }

    [Fact]
    public void AddMod_Existing_DoesNotRaiseEvent()
    {
        var container = CreateModContainer();
        var game = container.Game;

        var mod = game.InstallMod(_randomModName, GITestUtilities.GetRandomWorkshopFlag(game), ServiceProvider);
        container.AddMod(mod);

        var containerRaised = false;
        container.ModsCollectionModified += (_, _) =>
        {
            containerRaised = true;
        };

        var gameRaised = false;
        container.Game.ModsCollectionModified += (_, _) =>
        {
            gameRaised = true;
        };

        container.AddMod(mod);
        Assert.False(containerRaised);
        Assert.False(gameRaised);
    }

    [Fact]
    public void AddMod_ExistingFromContainer_ShouldBeAlreadyInGame()
    {
        var container = CreateModContainer();
        var game = container.Game;

        var mod = game.InstallMod(_randomModName, GITestUtilities.GetRandomWorkshopFlag(game), ServiceProvider);
        Assert.True(container.AddMod(mod));

        Assert.False(game.AddMod(mod));
    }

    [Fact]
    public void FindMod()
    {
        var container = CreateModContainer();
        var game = container.Game;

        var mod = game.InstallMod(_randomModName, GITestUtilities.GetRandomWorkshopFlag(game), ServiceProvider);
        
        Assert.True(container.AddMod(mod));

        Assert.Same(mod, container.FindMod(mod));
        Assert.Same(mod, container.FindMod(new ModReference(mod.Identifier, mod.Type)));

        Assert.Null(container.FindMod(new ModReference("other", mod.Type)));
    }

    [Fact]
    public void ModContainerMethod_NullArgs_Throws()
    {
        var container = CreateModContainer();
        Assert.Throws<ArgumentNullException>(() => container.AddMod(null!));
        Assert.Throws<ArgumentNullException>(() => container.RemoveMod(null!));
        Assert.Throws<ArgumentNullException>(() => container.FindMod(null!));
    }
}