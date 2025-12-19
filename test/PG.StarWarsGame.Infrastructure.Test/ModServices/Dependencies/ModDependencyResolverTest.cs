using System;
using System.Collections.Generic;
using AET.Modinfo.Model;
using AET.Modinfo.Spec;
using PG.StarWarsGame.Infrastructure.Mods;
using PG.StarWarsGame.Infrastructure.Services.Dependencies;
using PG.StarWarsGame.Infrastructure.Testing.TestBases;
using Xunit;

namespace PG.StarWarsGame.Infrastructure.Test.ModServices.Dependencies;

public class ModDependencyResolverTest : GameInfrastructureTestBaseWithRandomGame
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
        var mod = GameInstallation.InstallAndAddMod("A").Mod;
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
        var mod = GameInstallation.InstallAndAddMod(modinfo, false).Mod;
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
        var mod = GameInstallation.InstallAndAddMod(modinfo, false).Mod;
        Assert.Throws<ModDependencyCycleException>(() => _resolver.Resolve(mod));
    }

    [Fact]
    public void Resolve_SingleDependency()
    {
        var dep = GameInstallation.InstallAndAddMod("B").Mod;
        var mod = InstallAndAddModWithDependencies("A", DependencyResolveLayout.FullResolved, new ModReference(dep)).Mod;

        var deps = _resolver.Resolve(mod);

        Assert.Single(deps);
        Assert.Equal(dep, deps[0]);
    }

    [Fact]
    public void Resolve_TwoDependency()
    {
        var b = GameInstallation.InstallAndAddMod("B").Mod;
        var c = GameInstallation.InstallAndAddMod("C").Mod;
        var mod = InstallAndAddModWithDependencies("A", DependencyResolveLayout.FullResolved, b, c).Mod;

        var deps = _resolver.Resolve(mod);

        Assert.Equal([b, c], deps);
    }

    [Fact]
    public void Resolve_ResolveCompleteChain_DependenciesHaveDeps_ButResolveLayoutIsFullResolved()
    {
        var b2 = GameInstallation.InstallAndAddMod("B2").Mod;
        var b = InstallAndAddModWithDependencies("B", DependencyResolveLayout.ResolveRecursive, b2).Mod;
        var c2 = GameInstallation.InstallAndAddMod("C2").Mod;
        var c = InstallAndAddModWithDependencies("C", DependencyResolveLayout.ResolveRecursive, c2).Mod;
        // Layout is recursive
        var mod = InstallAndAddModWithDependencies("A", DependencyResolveLayout.FullResolved, b, c).Mod;

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
        var b2 = GameInstallation.InstallAndAddMod("B2").Mod;
        var b = InstallAndAddModWithDependencies("B", DependencyResolveLayout.ResolveRecursive, b2).Mod;
        b.ResolveDependencies();
        var c2 = GameInstallation.InstallAndAddMod("C2").Mod;
        var c = InstallAndAddModWithDependencies("C", DependencyResolveLayout.ResolveRecursive, c2).Mod;
        c.ResolveDependencies();
        var mod = InstallAndAddModWithDependencies("A", DependencyResolveLayout.FullResolved, b, c).Mod;

        var deps = _resolver.Resolve(mod);

        // Only b and c, because a has layout FullResolved
        Assert.Equal([b, c], deps);
    }
}