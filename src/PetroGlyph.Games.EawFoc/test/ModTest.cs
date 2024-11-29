using System;
using System.Collections.Generic;
using System.IO.Abstractions;
using System.Xml.Linq;
using EawModinfo.Model;
using EawModinfo.Spec;
using Microsoft.Extensions.DependencyInjection;
using PG.StarWarsGame.Infrastructure.Mods;
using PG.StarWarsGame.Infrastructure.Services.Dependencies;
using PG.StarWarsGame.Infrastructure.Services.Detection;
using PG.StarWarsGame.Infrastructure.Testing;
using PG.StarWarsGame.Infrastructure.Testing.Mods;
using Semver;
using Xunit;

namespace PG.StarWarsGame.Infrastructure.Test;

public class ModTest : ModBaseTest
{
    private readonly bool _isWorkshop;
    private readonly IModIdentifierBuilder _identifierBuilder;

    public ModTest()
    {
        _identifierBuilder = ServiceProvider.GetRequiredService<IModIdentifierBuilder>();
        _isWorkshop = GITestUtilities.GetRandomWorkshopFlag(Game);
    }

    private IDirectoryInfo CreateModDirectoryInfo(string name)
    {
        return FileSystem.DirectoryInfo.New(Game.GetModDirectory(name, _isWorkshop, ServiceProvider));
    }

    private IModReference CreateModRef(string name)
    {
        var id = _identifierBuilder.Build(CreateModDirectoryInfo(name), _isWorkshop);
        var modType = _isWorkshop ? ModType.Workshops : ModType.Default;
        return new ModReference(id, modType);
    }

    private Mod CreatePhysicalMod(
        string name,
        string? iconPath = null,
        ICollection<ILanguageInfo>? languages = null,
        DependencyResolveLayout layout = DependencyResolveLayout.FullResolved,
        params IList<IModReference> deps)
    {
        var modinfo = new ModinfoData(name)
        {
            Dependencies = new DependencyList(deps, layout)
        };

        var mod = Game.InstallMod(CreateModDirectoryInfo(name), _isWorkshop, modinfo, ServiceProvider);
        Game.AddMod(mod);

        if (languages is not null)
        {
            foreach (var languageInfo in languages) 
                mod.InstallLanguage(languageInfo);
        }

        if (iconPath is not null) 
            FileSystem.File.Create(FileSystem.Path.Combine(mod.Directory.FullName, iconPath));

        return mod;
    }

    protected override ModBase CreateMod(
        string name,
        DependencyResolveLayout layout = DependencyResolveLayout.FullResolved, 
        params IList<IModReference> deps)
    {
        return CreatePhysicalMod(name, layout: layout, deps: deps);
    }

    protected override IPlayableObject CreatePlayableObject(
        string? iconPath = null,
        ICollection<ILanguageInfo>? languages = null)
    {
        return CreatePhysicalMod("MyMod", iconPath, languages);
    }

    protected override PlayableModContainer CreateModContainer()
    {
        return CreateMod("MyMod");
    }

    [Fact]
    public void InvalidCtor_Throws()
    {
        Assert.Throws<ArgumentNullException>(() => new Mod(null!, FileSystem.DirectoryInfo.New("modPath"), false, new ModinfoData("Name"), ServiceProvider));
        Assert.Throws<ArgumentNullException>(() => new Mod(Game, null!, false, new ModinfoData("Name"), ServiceProvider));
        Assert.Throws<ArgumentNullException>(() => new Mod(Game, FileSystem.DirectoryInfo.New("modPath"), false, (IModinfo)null!, ServiceProvider));
        Assert.Throws<ArgumentNullException>(() => new Mod(Game, FileSystem.DirectoryInfo.New("modPath"), false, (string)null!, ServiceProvider));
        Assert.Throws<ArgumentException>(() => new Mod(Game, FileSystem.DirectoryInfo.New("modPath"), false, "", ServiceProvider));
    }

    [Fact]
    public void ValidCtor_Properties_FromName()
    {
        var ws = GITestUtilities.GetRandomWorkshopFlag(Game);

        var mod = Game.InstallAndAddMod("Mod", ws, ServiceProvider);

        Assert.Equal("Mod", mod.Name);
        Assert.Equal(ws ? ModType.Workshops : ModType.Default, mod.Type);
        Assert.Empty(mod.Dependencies);
        Assert.Empty(((IModIdentity)mod).Dependencies);
        Assert.Equal(DependencyResolveLayout.ResolveRecursive, mod.DependencyResolveLayout);

        // Resolved, because there are no dependencies defined.
        Assert.Equal(DependencyResolveStatus.Resolved, mod.DependencyResolveStatus);
        Assert.Empty(mod.Mods);
        Assert.Null(mod.Version);

        if (ws)
            Assert.True(uint.TryParse(mod.Identifier, out _));
        else
            Assert.Equal(mod.Directory.FullName.ToUpperInvariant(), mod.Identifier);

        Assert.Null(mod.ModInfo);
    }

    [Fact]
    public void ValidCtor_Properties_FromModinfo_WithoutDependencies()
    {
        var ws = GITestUtilities.GetRandomWorkshopFlag(Game);

        var modInfo = new ModinfoData("Mod")
        {
            Icon = "IconPath",
            Version = new SemVersion(1, 0, 0),
            Languages = new List<ILanguageInfo>(),
        };
        var mod = Game.InstallAndAddMod(ws, modInfo,ServiceProvider);

        Assert.Equal("Mod", mod.Name);
        Assert.Equal(ws ? ModType.Workshops : ModType.Default, mod.Type);

        Assert.Empty(mod.Dependencies);
        Assert.Empty(((IModIdentity)mod).Dependencies);
        Assert.Equal(modInfo.Dependencies.ResolveLayout, mod.DependencyResolveLayout);

        // Resolved, because there are no dependencies defined.
        Assert.Equal(DependencyResolveStatus.Resolved, mod.DependencyResolveStatus);
        Assert.Empty(mod.Mods);
        Assert.Equal(modInfo.Version, mod.Version);

        if (ws)
            Assert.True(uint.TryParse(mod.Identifier, out _));
        else
            Assert.Equal(mod.Directory.FullName.ToUpperInvariant(), mod.Identifier);

        Assert.Same(modInfo, mod.ModInfo);
    }

    [Fact]
    public void ValidCtor_Properties_FromModinfo_WithDependencies()
    {
        var dep = CreateOtherMod("dep");

        var ws = GITestUtilities.GetRandomWorkshopFlag(Game);

        var modInfo = new ModinfoData("Mod")
        {
            Dependencies = new DependencyList(new List<IModReference>{ dep }, DependencyResolveLayout.ResolveLastItem)
        };
        var mod = Game.InstallAndAddMod(ws, modInfo, ServiceProvider);

        Assert.Empty(mod.Dependencies);
        Assert.Single(((IModIdentity)mod).Dependencies);
        Assert.Equal(modInfo.Dependencies.ResolveLayout, mod.DependencyResolveLayout);

        // None, because there we have dependencies defined and they are not resolved on initialization.
        Assert.Equal(DependencyResolveStatus.None, mod.DependencyResolveStatus);
        Assert.Empty(mod.Mods);
    }

    [Fact]
    public void ResolveDependencies_NoDependenciesIsNOP()
    {
        var mod = CreateMod("Mod"); 
        // Should not throw or anything else
        mod.ResolveDependencies();
        Assert.Empty(mod.Dependencies);
        Assert.Equal(DependencyResolveStatus.Resolved, mod.DependencyResolveStatus);
    }

    [Theory]
    [MemberData(nameof(ModTestScenarios.CycleScenarios), MemberType = typeof(ModTestScenarios))]
    public void ResolveDependencies_ResolvesCycle_Throws(ModTestScenarios.CycleTestScenario testScenario)
    {
        var mod = ModTestScenarios.CreateTestScenarioCycle(
                testScenario,
                CreateMod,
                CreateOtherMod,
                CreateModRef)
            .Mod;

        var e = Assert.Throws<ModDependencyCycleException>(mod.ResolveDependencies);
        Assert.Equal(mod, e.Mod);
        Assert.Equal(mod, e.Dependency);
        Assert.Equal(DependencyResolveStatus.Faulted, mod.DependencyResolveStatus);
    }
}