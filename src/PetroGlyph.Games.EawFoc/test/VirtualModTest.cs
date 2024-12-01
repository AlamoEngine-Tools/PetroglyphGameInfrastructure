using System;
using System.Collections.Generic;
using System.Linq;
using EawModinfo.Model;
using EawModinfo.Spec;
using PG.StarWarsGame.Infrastructure.Games;
using PG.StarWarsGame.Infrastructure.Mods;
using PG.StarWarsGame.Infrastructure.Services.Dependencies;
using PG.StarWarsGame.Infrastructure.Testing;
using PG.StarWarsGame.Infrastructure.Testing.Mods;
using Semver;
using Xunit;

namespace PG.StarWarsGame.Infrastructure.Test;

public class VirtualModTest : ModBaseTest
{
    private VirtualMod CreateVirtualMod(
        string name,
        string? iconPath = null,
        ICollection<ILanguageInfo>? languages = null,
        DependencyResolveLayout layout = DependencyResolveLayout.FullResolved,
        params IList<IModReference> deps)
    {
        IModDependencyList depList;
        if (deps.Count == 0)
        {
            var dep = CreateOtherMod("dep");
            depList = new DependencyList(new List<IModReference> { dep }, layout);
        }
        else
        {
            depList = new DependencyList(deps, layout);
        }

        var modinfo = new ModinfoData(name)
        {
            Icon = iconPath,
            Languages = languages?.ToList() ?? new List<ILanguageInfo>(),
            Dependencies = depList
        };

        var mod = new VirtualMod(Game, "VirtualModId", modinfo, ServiceProvider);
        Game.AddMod(mod);
        return mod;
    }

    protected override ModBase CreateMod(
        string name,
        DependencyResolveLayout layout = DependencyResolveLayout.FullResolved, 
        params IList<IModReference> deps)
    {
        return CreateVirtualMod(name, layout: layout, deps: deps);
    }

    protected override IPlayableObject CreatePlayableObject(
        string? iconPath = null,
        ICollection<ILanguageInfo>? languages = null)
    {
        return CreateVirtualMod("Mod", iconPath, languages);
    }

    protected override PlayableModContainer CreateModContainer()
    {
        return CreateMod("Mod");
    }

    [Fact]
    public void InvalidCtor_ArgumentNull_Throws()
    {
        var dep = Game.InstallAndAddMod("Dep", GITestUtilities.GetRandomWorkshopFlag(Game), ServiceProvider);
        Assert.Throws<ArgumentNullException>(() => new VirtualMod(null!, "VirtualModId", new ModinfoData("Name"), ServiceProvider));
        Assert.Throws<ArgumentNullException>(() => new VirtualMod(Game, null!, "Name", new DependencyList(new List<IModReference>{dep}, DependencyResolveLayout.FullResolved), ServiceProvider));
        Assert.Throws<ArgumentNullException>(() => new VirtualMod(Game, "VirtualModId", null!, new DependencyList(new List<IModReference>{dep}, DependencyResolveLayout.FullResolved), ServiceProvider));

        Assert.Throws<ArgumentNullException>(() => new VirtualMod(Game, "VirtualModId", null!, ServiceProvider));
        Assert.Throws<ArgumentNullException>(() => new VirtualMod(null!, "VirtualModId", "name", new DependencyList(new List<IModReference> { dep }, DependencyResolveLayout.FullResolved), ServiceProvider));

        Assert.Throws<ArgumentNullException>(() => new VirtualMod(Game, "name", null!, ServiceProvider));

        Assert.Throws<ArgumentNullException>(() => new VirtualMod(Game, "VirtualModId", new ModinfoData("Name"), null!));
        Assert.Throws<ArgumentNullException>(() => new VirtualMod(Game, null!, new ModinfoData("Name"), ServiceProvider));
        Assert.Throws<ArgumentNullException>(() => new VirtualMod(Game, "VirtualModId", "name", new DependencyList(new List<IModReference> { dep }, DependencyResolveLayout.FullResolved), null!));
    }

    [Fact]
    public void Ctor_EmptyDependencies_Throws()
    {
        Assert.Throws<ModException>(() => new VirtualMod(Game, "VirtualModId", new ModinfoData("Name"), ServiceProvider));
        Assert.Throws<ModException>(() => new VirtualMod(Game, "VirtualModId", "name", DependencyList.EmptyDependencyList, ServiceProvider));
        var modInfo = new ModinfoData("Name")
        {
            Dependencies = new DependencyList(new List<IModReference>(), DependencyResolveLayout.FullResolved)
        };
        Assert.Throws<ModException>(() => new VirtualMod(Game, "VirtualModId", modInfo, ServiceProvider));
    }

    [Fact]
    public void Ctor_FromName_Properties()
    {
        var dep = CreateOtherMod("dep");

        var mod = new VirtualMod(Game,
            "VirtualModId",
            "VirtualMod",
            new DependencyList(new List<IModReference> { dep }, DependencyResolveLayout.FullResolved), ServiceProvider);
        
        Assert.Empty(mod.Dependencies);
        Assert.Single(((IModIdentity)mod).Dependencies);
        Assert.Equal(DependencyResolveLayout.FullResolved, mod.DependencyResolveLayout);
        Assert.Equal(DependencyResolveStatus.None, mod.DependencyResolveStatus);
        Assert.NotNull(mod.ModInfo);
        Assert.Equal("VirtualMod", mod.ModInfo.Name);
        Assert.Equal(ModType.Virtual, mod.Type);
        Assert.Empty(mod.Mods);
        Assert.Null(mod.Version);

        Assert.StartsWith("VirtualModId", mod.Identifier);
    }

    [Fact]
    public void Ctor_FromModinfo_Properties()
    {
        var dep = CreateOtherMod("dep");

        var modinfo = new ModinfoData("VirtualMod")
        {
            Icon = "IconPath",
            Version = new SemVersion(1, 0, 0),
            Languages = new List<ILanguageInfo>(),
            Dependencies = new DependencyList(new List<IModReference>
            {
                new ModReference(dep)
            }, DependencyResolveLayout.ResolveLastItem)
        };

        var mod = new VirtualMod(Game, "VirtualModId", modinfo, ServiceProvider);

        Assert.Empty(mod.Dependencies);
        Assert.Single(((IModIdentity)mod).Dependencies);
        Assert.Equal(DependencyResolveLayout.ResolveLastItem, mod.DependencyResolveLayout);
        Assert.Equal(DependencyResolveStatus.None, mod.DependencyResolveStatus);
        Assert.NotNull(mod.ModInfo);
        Assert.Equal("VirtualMod", mod.ModInfo.Name);
        Assert.Equal(modinfo.Version, mod.Version);
        Assert.Null(mod.VersionRange);
        Assert.Equal(modinfo.Icon, mod.IconFile);

        Assert.StartsWith("VirtualModId", mod.Identifier);
    }

    [Fact]
    public void Ctor_NonPhysicalDependencies_Throws()
    {
        var customDep = new CustomVirtualMod(Game, ServiceProvider);
        Game.AddMod(customDep);

        var modInfo = new ModinfoData("Name")
        {
            Dependencies = new DependencyList(new List<IModReference> { new ModReference(customDep) },
                DependencyResolveLayout.FullResolved)
        };

        var mod = new VirtualMod(Game, "VirtualModId", modInfo, ServiceProvider);
        Game.AddMod(mod);
        Assert.Throws<ModException>(mod.ResolveDependencies);
    }

    private class CustomVirtualMod(IGame game, IServiceProvider serviceProvider)
        : ModBase(game, "CustomVirtualMod", ModType.Virtual, "CustomVirtualMod", serviceProvider);
}