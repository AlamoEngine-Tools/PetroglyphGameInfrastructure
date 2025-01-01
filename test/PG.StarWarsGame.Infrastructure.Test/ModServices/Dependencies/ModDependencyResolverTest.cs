using System;
using System.Collections.Generic;
using EawModinfo.Model;
using EawModinfo.Spec;
using PG.StarWarsGame.Infrastructure.Mods;
using PG.StarWarsGame.Infrastructure.Services.Dependencies;
using PG.StarWarsGame.Infrastructure.Testing;
using PG.StarWarsGame.Infrastructure.Testing.Mods;
using PG.StarWarsGame.Infrastructure.Testing.TestBases;
using Xunit;

namespace PG.StarWarsGame.Infrastructure.Test.ModServices.Dependencies;

public class ModDependencyResolverTest : CommonTestBaseWithRandomGame
{
    private readonly ModDependencyResolver _resolver;

    public ModDependencyResolverTest()
    {
        _resolver = new ModDependencyResolver(ServiceProvider);
    }

    [Fact]
    public void InvalidArgs_Throws()
    {
        Assert.Throws<ArgumentNullException>(() => new ModDependencyResolver(null!));
        Assert.Throws<ArgumentNullException>(() => new ModDependencyResolver(ServiceProvider).Resolve(null!));
    }

    [Fact]
    public void Resolve_NoDependencies()
    {
        var mod = CreateMod("A");
        var dependencies = _resolver.Resolve(mod);
        Assert.Empty(dependencies);
    }

    [Fact]
    public void Resolve_DependencyNot_Found()
    {
        var modinfo = new ModinfoData("A")
        {
            Dependencies = new DependencyList(new List<IModReference>
            {
                new ModReference("B", ModType.Default)
            }, DependencyResolveLayout.FullResolved)
        };
        var mod = Game.InstallAndAddMod(false, modinfo, ServiceProvider);
        Assert.Throws<ModNotFoundException>(() => _resolver.Resolve(mod));
    }

    [Fact]
    public void Resolve_SelfDependency_WithCycleCheck_Throws()
    {
        var depA = new ModReference("A", ModType.Default);

        var modinfo = new ModinfoData("A")
        {
            Dependencies = new DependencyList(new List<IModReference>
            {
                depA
            }, DependencyResolveLayout.FullResolved)
        };
        var mod = Game.InstallAndAddMod(false, modinfo, ServiceProvider);
        Assert.Throws<ModDependencyCycleException>(() => _resolver.Resolve(mod));
    }

    [Fact]
    public void Resolve_SingleDependency()
    {
        var dep = CreateMod("B");
        var mod = CreateMod("A", DependencyResolveLayout.FullResolved, new ModReference(dep));

        var deps = _resolver.Resolve(mod);

        Assert.Single(deps);
        Assert.Equal(dep, deps[0]);
    }

    [Fact]
    public void Resolve_TwoDependency()
    {
        var b = CreateMod("B");
        var c = CreateMod("C");
        var mod = CreateMod("A", DependencyResolveLayout.FullResolved, b, c);

        var deps = _resolver.Resolve(mod);

        Assert.Equal([b, c], deps);
    }

    [Fact]
    public void Resolve_ResolveCompleteChain_DependenciesHaveDeps_ButResolveLayoutIsFullResolved()
    {
        var b2 = CreateMod("B2");
        var b = CreateMod("B", DependencyResolveLayout.ResolveRecursive, b2);
        var c2 = CreateMod("C2");
        var c = CreateMod("C", DependencyResolveLayout.ResolveRecursive, c2);
        // Layout is recursive
        var mod = CreateMod("A", DependencyResolveLayout.FullResolved, b, c);

        var deps = _resolver.Resolve(mod);

        // Only b and c, because a has layout FullResolved
        Assert.Equal([b, c], deps);

        // ResolveCompleteChain should not have done anything
        Assert.Equal(DependencyResolveStatus.None, b.DependencyResolveStatus);
        Assert.Equal(DependencyResolveStatus.None, c.DependencyResolveStatus);
    }

    [Fact]
    public void Resolve_ResolveCompleteChain_DependenciesHaveResolvedDeps_ButResolveLayoutIsFullResolved()
    {
        var b2 = CreateMod("B2");
        var b = CreateMod("B", DependencyResolveLayout.ResolveRecursive, b2);
        b.ResolveDependencies();
        var c2 = CreateMod("C2");
        var c = CreateMod("C", DependencyResolveLayout.ResolveRecursive, c2);
        c.ResolveDependencies();
        var mod = CreateMod("A", DependencyResolveLayout.FullResolved, b, c);

        var deps = _resolver.Resolve(mod);

        // Only b and c, because a has layout FullResolved
        Assert.Equal([b, c], deps);
    }

    private IMod CreateMod(string name, DependencyResolveLayout layout = DependencyResolveLayout.FullResolved, params IModReference[] deps)
    {
        if (deps.Length == 0)
            return Game.InstallAndAddMod(name, GITestUtilities.GetRandomWorkshopFlag(Game), ServiceProvider);

        var modinfo = new ModinfoData("A")
        {
            Dependencies = new DependencyList(deps, layout)
        };

        return Game.InstallAndAddMod(GITestUtilities.GetRandomWorkshopFlag(Game), modinfo, ServiceProvider);
    }
}