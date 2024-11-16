using System;
using System.Collections.Generic;
using EawModinfo.Model;
using EawModinfo.Spec;
using PG.StarWarsGame.Infrastructure.Games;
using PG.StarWarsGame.Infrastructure.Mods;
using PG.StarWarsGame.Infrastructure.Services.Dependencies;
using PG.StarWarsGame.Infrastructure.Testing;
using PG.StarWarsGame.Infrastructure.Testing.Mods;
using Xunit;

namespace PG.StarWarsGame.Infrastructure.Test;

public class VirtualModTest : ModBaseTest
{
    private VirtualMod CreateVirtualMod(
        string? iconPath = null,
        ICollection<ILanguageInfo>? languages = null)
    {
        var game = CreateRandomGame();
        var dep = game.InstallAndAddMod("dep", GITestUtilities.GetRandomWorkshopFlag(game), ServiceProvider);

        if (languages is not null)
        {
            foreach (var languageInfo in languages) 
                dep.InstallLanguage(languageInfo);
        }

        var modinfo = new ModinfoData("virtualMod")
        {
            Icon = iconPath,
            Dependencies = new DependencyList(new List<IModReference>
            {
                new ModReference(dep)
            }, DependencyResolveLayout.FullResolved)
        };

        return new VirtualMod(game, modinfo, ServiceProvider);
    }

    protected override IPlayableObject CreatePlayableObject(
        string? iconPath = null,
        ICollection<ILanguageInfo>? languages = null)
    {
        return CreateVirtualMod(iconPath, languages);
    }

    protected override PlayableModContainer CreateModContainer()
    {
        return CreateVirtualMod();
    }

    protected override ModBase CreateMod(
        string? iconPath = null,
        ICollection<ILanguageInfo>? languages = null)
    {
        return CreateVirtualMod();
    }

    [Fact]
    public void InvalidCtor_ArgumentNull_Throws()
    {
        var game = CreateRandomGame();
        var dep = game.InstallAndAddMod("Dep", GITestUtilities.GetRandomWorkshopFlag(game), ServiceProvider);
        Assert.Throws<ArgumentNullException>(() => new VirtualMod(null!, new ModinfoData("Name"), ServiceProvider));
        Assert.Throws<ArgumentNullException>(() => new VirtualMod(null!, game, new List<ModDependencyEntry> { new(dep) }, DependencyResolveLayout.FullResolved, ServiceProvider));

        Assert.Throws<ArgumentNullException>(() => new VirtualMod(game, null!, ServiceProvider));
        Assert.Throws<ArgumentNullException>(() => new VirtualMod("name", null!, new List<ModDependencyEntry>{new(dep)}, DependencyResolveLayout.FullResolved, ServiceProvider));

        Assert.Throws<ArgumentNullException>(() => new VirtualMod("name", game, null!, DependencyResolveLayout.FullResolved, ServiceProvider));

        Assert.Throws<ArgumentNullException>(() => new VirtualMod(game, new ModinfoData("Name"), null!));
        Assert.Throws<ArgumentNullException>(() => new VirtualMod("name", game, new List<ModDependencyEntry> { new(dep) }, DependencyResolveLayout.FullResolved, null!));
    }

    [Fact]
    public void Ctor_EmptyDependencies_Throws()
    {
        var game = CreateRandomGame();

        // Empty dependency lists
        Assert.Throws<PetroglyphException>(() => new VirtualMod(game, new ModinfoData("Name"), ServiceProvider));
        Assert.Throws<PetroglyphException>(() => new VirtualMod("name", game, new List<ModDependencyEntry>(), DependencyResolveLayout.FullResolved, ServiceProvider));
        var modInfo = new ModinfoData("Name")
        {
            Dependencies = new DependencyList(new List<IModReference>(), DependencyResolveLayout.FullResolved)
        };
        Assert.Throws<PetroglyphException>(() => new VirtualMod(game, modInfo, ServiceProvider));
    }

    [Fact]
    public void Ctor_DepNotAdded_Throws()
    {
        var game = CreateRandomGame();
        var notAddedDep = game.InstallMod("NotAddedMod", false, ServiceProvider);

        var modInfo = new ModinfoData("Name")
        {
            Dependencies = new DependencyList(new List<IModReference>{new ModReference(notAddedDep) }, DependencyResolveLayout.FullResolved)
        };
        Assert.Throws<ModNotFoundException>(() => new VirtualMod(game, modInfo, ServiceProvider));
    }

    [Fact]
    public void Ctor_DepOfWrongGame_Throws()
    {
        var game = CreateRandomGame();
        var otherGameReference = new PetroglyphStarWarsGame(game, game.Directory, game.Name, ServiceProvider);

        var wrongGameDep = otherGameReference.InstallAndAddMod("NotAddedMod", false, ServiceProvider);

        Assert.Throws<PetroglyphException>(() => new VirtualMod("VirtualMod", game,
            new List<ModDependencyEntry> { new(wrongGameDep) }, DependencyResolveLayout.FullResolved, ServiceProvider));
    }

    [Fact]
    public void Ctor_NonPhysicalDependencies_Throws()
    {
        var game = CreateRandomGame();
        var customDep = new CustomVirtualMod(game, ServiceProvider);
        game.AddMod(customDep);

        Assert.Throws<ModException>(() => new VirtualMod("VirtualMod", game,
            new List<ModDependencyEntry> { new(customDep) }, DependencyResolveLayout.FullResolved, ServiceProvider));

        var modInfo = new ModinfoData("Name")
        {
            Dependencies = new DependencyList(new List<IModReference> { new ModReference(customDep) }, DependencyResolveLayout.FullResolved)
        };
        Assert.Throws<ModException>(() => new VirtualMod(game, modInfo, ServiceProvider));
    }

    [Fact]
    public void Ctor_FromName_Properties()
    {
        var game = CreateRandomGame();
        var dep = game.InstallAndAddMod("dep", GITestUtilities.GetRandomWorkshopFlag(game), ServiceProvider);
        
        var mod = new VirtualMod("VirtualMod", game, new List<ModDependencyEntry>(new List<ModDependencyEntry>
        {
            new(dep)
        }), DependencyResolveLayout.FullResolved, ServiceProvider);
        
        Assert.Single(mod.Dependencies);
        Assert.Equal(DependencyResolveLayout.FullResolved, mod.DependencyResolveLayout);
        Assert.Equal(DependencyResolveStatus.Resolved, mod.DependencyResolveStatus);
        Assert.NotNull(mod.ModInfo);
        Assert.Equal("VirtualMod", mod.ModInfo.Name);
    }

    [Fact]
    public void Ctor_FromModinfo_Properties()
    {
        var game = CreateRandomGame();
        var dep = game.InstallAndAddMod("dep", GITestUtilities.GetRandomWorkshopFlag(game), ServiceProvider);

        var modinfo = new ModinfoData("VirtualMod")
        {
            Dependencies = new DependencyList(new List<IModReference>
            {
                new ModReference(dep)
            }, DependencyResolveLayout.ResolveLastItem)
        };

        var mod = new VirtualMod(game, modinfo, ServiceProvider);

        Assert.Single(mod.Dependencies);
        Assert.Equal(DependencyResolveLayout.ResolveLastItem, mod.DependencyResolveLayout);
        Assert.Equal(DependencyResolveStatus.Resolved, mod.DependencyResolveStatus);
        Assert.NotNull(mod.ModInfo);
        Assert.Equal("VirtualMod", mod.ModInfo.Name);
    }

    [Fact]
    public void ResolveDependencies_NotSupportedOperation()
    {
        var mod = CreateMod();
        Assert.Throws<NotSupportedException>(() => mod.ResolveDependencies(new ModDependencyResolver(ServiceProvider), new DependencyResolverOptions()));
    }

    private class CustomVirtualMod(IGame game, IServiceProvider serviceProvider)
        : ModBase(game, ModType.Virtual, "CustomVirtualMod", serviceProvider)
    {
        public override string Identifier => "CustomVirtualMod";
    }
}