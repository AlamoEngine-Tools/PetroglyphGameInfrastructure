using System;
using System.Collections.Generic;
using System.Linq;
using AET.Modinfo.Model;
using AET.Modinfo.Spec;
using AnakinRaW.CommonUtilities.Testing.Extensions;
using PG.StarWarsGame.Infrastructure.Mods;
using PG.StarWarsGame.Infrastructure.Games;
using Xunit;
using PG.StarWarsGame.Infrastructure.Services.Dependencies;
using PG.StarWarsGame.Infrastructure.Testing;
using Semver;
using PG.StarWarsGame.Infrastructure.Testing.TestBases;
using PG.StarWarsGame.Infrastructure.Testing.Installations.Mods;
using PG.StarWarsGame.Infrastructure.Testing.Installations;
using PG.StarWarsGame.Infrastructure.Testing.Installations.Game;

namespace PG.StarWarsGame.Infrastructure.Test;

public abstract class ModBaseTest : PlayableModContainerTest
{
    protected IGame Game => GameInstallation.Game;
    protected ITestingGameInstallation GameInstallation { get; }

    protected ModBaseTest()
    {
        // ReSharper disable once VirtualMemberCallInConstructor
        GameInstallation = GetOrCreateGameInstallation();
    }

    protected abstract ITestingModInstallation CreateAndAddModInstallation(
        string name,
        DependencyResolveLayout layout = DependencyResolveLayout.FullResolved,
        params IList<IModReference> deps);


    [Fact]
    public void VersionRange_IsNull()
    {
        var mod = CreateAndAddModInstallation("Mod").Mod;
        Assert.Null(mod.VersionRange);
    }

    [Theory]
    [MemberData(nameof(ModTestScenarios.ValidScenarios), MemberType = typeof(ModTestScenarios))]
    public void ResolveDependencies_ResolvesCorrectly(ModTestScenarios.TestScenario testScenario)
    {
        var mod = ModTestScenarios.CreateTestScenario(
                testScenario,
                (name, layout, dependencies) => CreateAndAddModInstallation(name, layout, dependencies).Mod,
                (name, layout, dependencies) => InstallAndAddModWithDependencies(name, layout, dependencies).Mod)
            .Mod;

        var expectedDirectDeps = mod.ModInfo!.Dependencies.Select(d =>
        {
            var dep = Game.FindMod(d);
            Assert.NotNull(dep);
            return dep;
        });

        mod.ResolveDependencies();

        Assert.Equal(expectedDirectDeps, mod.Dependencies);
        Assert.Equal(DependencyResolveStatus.Resolved, mod.DependencyResolveStatus);

        AssertDependenciesResolved(mod);
    }

    [Fact]
    public void ResolveDependencies_DepNotAdded_Throws_ThenAddingModResolvesCorrectly()
    {
        // Do not add mod to game
        var notAddedDep = GameInstallation.InstallMod("NotAddedMod", false).Mod;

        var mod = CreateAndAddModInstallation("Mod", Random.Enum<DependencyResolveLayout>(), notAddedDep).Mod;
        Game.AddMod(mod);
        var e = Assert.Throws<ModNotFoundException>(mod.ResolveDependencies);
        Assert.Same(Game, e.ModContainer);
        Assert.Equal(notAddedDep, e.Mod);
        Assert.Equal(DependencyResolveStatus.Faulted, mod.DependencyResolveStatus);

        // Add dep to game
        Game.AddMod(notAddedDep);

        mod.ResolveDependencies();

        Assert.Equal([notAddedDep], mod.Dependencies);
        Assert.Equal(DependencyResolveStatus.Resolved, mod.DependencyResolveStatus);
    }

    [Fact]
    public void ResolveDependencies_DepOfWrongGame_Throws()
    {
        var otherGameInstallRef = GameInfrastructureTesting.Game(Game, ServiceProvider);
        var wrongGameDep = otherGameInstallRef.InstallAndAddMod("WrongGameRefMod");

        var mod = CreateAndAddModInstallation("Mod", Random.Enum<DependencyResolveLayout>(), wrongGameDep.Mod).Mod;
        
        var e = Assert.Throws<ModNotFoundException>(mod.ResolveDependencies);
        Assert.Same(Game, e.ModContainer);
        Assert.Equal(wrongGameDep.Mod, e.Mod);
        Assert.Equal(DependencyResolveStatus.Faulted, mod.DependencyResolveStatus);
    }

    [Fact]
    public void ResolveDependencies_VersionMismatch_Throws()
    {
        var ws = GITestUtilities.GetRandomWorkshopFlag(Game);
        var depLoc = GameInstallation.GetModDirectory("B", ws);
        var dep = GameInstallation.InstallMod(new ModinfoData("B") { Version = new SemVersion(1) }, depLoc, ws).Mod;
        Game.AddMod(dep);

        var mod = CreateAndAddModInstallation("Mod", deps: new ModReference(dep.Identifier, dep.Type, SemVersionRange.AtLeast(new SemVersion(2)))).Mod;

        var e = Assert.Throws<VersionMismatchException>(mod.ResolveDependencies);
        Assert.Equal(new ModReference(dep), e.Mod);
        Assert.Equal(dep, e.Dependency);
        Assert.Equal(DependencyResolveStatus.Faulted, mod.DependencyResolveStatus);
    }

    [Fact]
    public void ResolveDependencies_VersionMatch_Throws()
    {
        var ws = GITestUtilities.GetRandomWorkshopFlag(Game);
        var bLoc = GameInstallation.GetModDirectory("B", ws);
        var cLoc = GameInstallation.GetModDirectory("C", ws);
        var b = GameInstallation.InstallMod(new ModinfoData("B") { Version = new SemVersion(3) }, bLoc, ws).Mod;
        var c = GameInstallation.InstallMod(new ModinfoData("C") { Version = null! }, cLoc, ws).Mod;
        Game.AddMod(b);
        Game.AddMod(c);

        var mod = CreateAndAddModInstallation("Mod", deps:
            [
                new ModReference(b.Identifier, b.Type, SemVersionRange.AtLeast(new SemVersion(2))),
                new ModReference(c.Identifier, c.Type, SemVersionRange.AtLeast(new SemVersion(2)))
            ]
        ).Mod;

        mod.ResolveDependencies();
        Assert.Equal([b, c], mod.Dependencies);
        Assert.Equal(DependencyResolveStatus.Resolved, mod.DependencyResolveStatus);
    }

    [Fact]
    public void ResolveDependencies_RaiseEvent()
    {
        var scenario = ModTestScenarios.CreateTestScenario(
            ModTestScenarios.TestScenario.SingleDepAndTransitive,
            (name, layout, dependencies) => CreateAndAddModInstallation(name, layout, dependencies).Mod,
            (name, layout, dependencies) => InstallAndAddModWithDependencies(name, layout, dependencies).Mod);

        var mod = scenario.Mod;
        var b = Game.FindMod(scenario.ExpectedTraversedList![1])!;

        var bRaised = false;
        var modRaised = false;
        b.DependenciesResolved += (sender, _) =>
        {
            Assert.Equal(b, sender);
            bRaised = true;
        };
        mod.DependenciesResolved += (sender, _) =>
        {
            Assert.Equal(mod, sender);
            modRaised = true;
        };

        mod.ResolveDependencies();

        Assert.True(bRaised);
        Assert.True(modRaised);
    }

    [Fact]
    public void ResolveDependencies_AlreadyResolved_DoesNotRaiseEvent()
    {
        var mod = CreateAndAddModInstallation("A").Mod;
        mod.ResolveDependencies();
        Assert.Equal(DependencyResolveStatus.Resolved, mod.DependencyResolveStatus);

        var depsResolvedRaised = false;
        mod.DependenciesResolved += (_, _) =>
        {
            depsResolvedRaised = true;
        };
        
        mod.ResolveDependencies();

        Assert.False(depsResolvedRaised);
    }

    [Fact]
    public void ResolveDependencies_Faulted_DoesNotRaiseEvent()
    {
        var ws = GITestUtilities.GetRandomWorkshopFlag(Game);
        var depLoc = GameInstallation.GetModDirectory("B", ws);
        var dep = GameInstallation.InstallMod(new ModinfoData("B") { Version = new SemVersion(1) }, depLoc, ws).Mod;
        Game.AddMod(dep);

        var mod = CreateAndAddModInstallation("Mod", deps: new ModReference(dep.Identifier, dep.Type, SemVersionRange.AtLeast(new SemVersion(2)))).Mod;

        var depsResolvedRaised = false;
        mod.DependenciesResolved += (_, _) =>
        {
            depsResolvedRaised = true;
        };

        Assert.Throws<VersionMismatchException>(mod.ResolveDependencies);
        Assert.Equal(DependencyResolveStatus.Faulted, mod.DependencyResolveStatus);

        Assert.False(depsResolvedRaised);
    }

    private static void AssertDependenciesResolved(IMod mod)
    {
        if (mod.DependencyResolveLayout == DependencyResolveLayout.FullResolved)
            return;

        Assert.Equal(DependencyResolveStatus.Resolved, mod.DependencyResolveStatus);

        for (var i = 0; i < mod.Dependencies.Count; i++)
        {
            var dependency = mod.Dependencies[i];
            if (mod.DependencyResolveLayout == DependencyResolveLayout.ResolveRecursive ||
                mod.DependencyResolveLayout == DependencyResolveLayout.ResolveLastItem && i == mod.Dependencies.Count - 1)
                AssertDependenciesResolved(dependency);
        }
    }

    [Fact]
    public void EqualsHashCode()
    {
        var dep = GameInstallation.InstallAndAddMod("B").Mod;
        var mod = CreateAndAddModInstallation("A").Mod;
        var samish = CreateAndAddModInstallation("A").Mod;
        var otherA = CreateAndAddModInstallation("A", deps: dep).Mod;

        ModBase custom = mod.ModInfo is not null 
            ? new CustomMod(Game, mod.Identifier, mod.Type, mod.ModInfo, ServiceProvider) 
            : new CustomMod(Game, mod.Identifier, mod.Type, mod.Name, ServiceProvider);

        Assert.False(mod.Equals(null!));
        Assert.False(mod.Equals((object)null!));
        Assert.False(mod.Equals((IModIdentity)null!));
        Assert.False(mod.Equals((IModReference)null!));

        Assert.True(mod.Equals(mod));
        Assert.True(mod.Equals((object)mod));
        Assert.True(mod.Equals((IModIdentity)mod));
        Assert.True(mod.Equals((IModReference)mod));
        Assert.Equal(mod.GetHashCode(), mod.GetHashCode());
        
        Assert.True(mod.Equals(samish), $"{mod} AND {samish}");
        Assert.True(mod.Equals((object)samish));
        Assert.True(mod.Equals((IModIdentity)samish));
        Assert.True(mod.Equals((IModReference)samish));
        Assert.Equal(mod.GetHashCode(), samish.GetHashCode());

        Assert.False(mod.Equals(otherA));
        Assert.False(mod.Equals((object)otherA));
        Assert.False(mod.Equals((IModIdentity)otherA));
        Assert.True(mod.Equals((IModReference)otherA));
        Assert.NotEqual(mod.GetHashCode(), otherA.GetHashCode());

        Assert.True(mod.Equals(custom));
        Assert.False(mod.Equals((object)custom));
        Assert.True(mod.Equals((IModIdentity)custom));
        Assert.True(mod.Equals((IModReference)custom));
    }

    public class ModBaseAbstractTest : GameInfrastructureTestBaseWithRandomGame
    {
        [Fact]
        public void ResolveDependencies_CalledTwice_Throws()
        {
            var dep = GameInstallation.InstallAndAddMod("Dep").Mod;
            var modinfo = new ModinfoData("CustomMod")
            {
                Dependencies = new DependencyList(new List<IModReference> { dep }, DependencyResolveLayout.FullResolved)
            };
            var mod = new SelfResolvingMod(Game, modinfo, ServiceProvider);
            Game.AddMod(mod);

            Assert.Throws<ModDependencyCycleException>(mod.ResolveDependencies);
            Assert.Equal(DependencyResolveStatus.Faulted, mod.DependencyResolveStatus);

            // Attempt to try again should try to resolve again, but we expect the same result.
            Assert.Throws<ModDependencyCycleException>(mod.ResolveDependencies);
            Assert.Equal(DependencyResolveStatus.Faulted, mod.DependencyResolveStatus);
        }

        private class SelfResolvingMod(IGame game, IModinfo modinfo, IServiceProvider serviceProvider)
            : ModBase(game, "CustomMod", ModType.Default, modinfo, serviceProvider)
        {
            protected override void OnDependenciesResolved()
            {
                ResolveDependencies();
                base.OnDependenciesResolved();
            }
        }
    }

    private class CustomMod : ModBase
    {
        public CustomMod(IGame game, string identifier, ModType type, string name, IServiceProvider serviceProvider) : base(game, identifier, type, name, serviceProvider)
        {
        }

        public CustomMod(IGame game, string identifier, ModType type, IModinfo modinfo, IServiceProvider serviceProvider) : base(game, identifier, type, modinfo, serviceProvider)
        {
        }
    }
}