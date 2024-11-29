using System;
using System.Collections.Generic;
using System.Linq;
using EawModinfo.Model;
using EawModinfo.Spec;
using PG.StarWarsGame.Infrastructure.Mods;
using PG.StarWarsGame.Infrastructure.Games;
using PG.StarWarsGame.Infrastructure.Testing.Mods;
using PG.TestingUtilities;
using Xunit;
using PG.StarWarsGame.Infrastructure.Services.Dependencies;
using PG.StarWarsGame.Infrastructure.Testing;
using Semver;

namespace PG.StarWarsGame.Infrastructure.Test;

public abstract class ModBaseTest : PlayableModContainerTest
{
    protected IGame Game;

    protected ModBaseTest()
    {
        Game = CreateRandomGame();
    }

    protected abstract ModBase CreateMod(
        string name,
        DependencyResolveLayout layout = DependencyResolveLayout.FullResolved,
        params IList<IModReference> deps);

    protected IMod CreateOtherMod(
        string name, 
        DependencyResolveLayout layout = DependencyResolveLayout.FullResolved, 
        params IList<IModReference> deps)
    {
        return CreateAndAddMod(Game, name, new DependencyList(deps, layout));
    }

    [Fact]
    public void VersionRange_IsNull()
    {
        var mod = CreateMod("Mod");
        Assert.Null(mod.VersionRange);
    }

    [Theory]
    [MemberData(nameof(ModTestScenarios.ValidScenarios), MemberType = typeof(ModTestScenarios))]
    public void ResolveDependencies_ResolvesCorrectly(ModTestScenarios.TestScenario testScenario)
    {
        var mod = ModTestScenarios.CreateTestScenario(
                testScenario, 
                CreateMod, 
                CreateOtherMod)
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
        var notAddedDep = Game.InstallMod("NotAddedMod", false, ServiceProvider);

        var mod = CreateMod("Mod", TestHelpers.GetRandomEnum<DependencyResolveLayout>(), notAddedDep);
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
        var otherGameReference = new PetroglyphStarWarsGame(Game, Game.Directory, Game.Name, ServiceProvider);
        var wrongGameDep = otherGameReference.InstallAndAddMod("WrongGameRefMod", 
            GITestUtilities.GetRandomWorkshopFlag(otherGameReference), ServiceProvider);
        var mod = CreateMod("Mod", TestHelpers.GetRandomEnum<DependencyResolveLayout>(), wrongGameDep);
        
        var e = Assert.Throws<ModNotFoundException>(mod.ResolveDependencies);
        Assert.Same(Game, e.ModContainer);
        Assert.Equal(wrongGameDep, e.Mod);
        Assert.Equal(DependencyResolveStatus.Faulted, mod.DependencyResolveStatus);
    }

    [Fact]
    public void ResolveDependencies_VersionMismatch_Throws()
    {
        var ws = GITestUtilities.GetRandomWorkshopFlag(Game);
        var depLoc = FileSystem.DirectoryInfo.New(Game.GetModDirectory("B", ws, ServiceProvider));
        var dep = Game.InstallMod(depLoc, ws, new ModinfoData("B") { Version = new SemVersion(1) }, ServiceProvider);
        Game.AddMod(dep);

        var mod = CreateMod("Mod", deps: new ModReference(dep.Identifier, dep.Type, SemVersionRange.AtLeast(new SemVersion(2))));

        var e = Assert.Throws<VersionMismatchException>(mod.ResolveDependencies);
        Assert.Equal(new ModReference(dep), e.Mod);
        Assert.Equal(dep, e.Dependency);
        Assert.Equal(DependencyResolveStatus.Faulted, mod.DependencyResolveStatus);
    }

    [Fact]
    public void ResolveDependencies_VersionMatch_Throws()
    {
        var ws = GITestUtilities.GetRandomWorkshopFlag(Game);
        var bLoc = FileSystem.DirectoryInfo.New(Game.GetModDirectory("B", ws, ServiceProvider));
        var cLoc = FileSystem.DirectoryInfo.New(Game.GetModDirectory("C", ws, ServiceProvider));
        var b = Game.InstallMod(bLoc, ws, new ModinfoData("B") { Version = new SemVersion(3) }, ServiceProvider);
        var c = Game.InstallMod(cLoc, ws, new ModinfoData("C") { Version = null! }, ServiceProvider);
        Game.AddMod(b);
        Game.AddMod(c);

        var mod = CreateMod("Mod", deps:
            [
                new ModReference(b.Identifier, b.Type, SemVersionRange.AtLeast(new SemVersion(2))),
                new ModReference(c.Identifier, c.Type, SemVersionRange.AtLeast(new SemVersion(2)))
            ]
        );

        mod.ResolveDependencies();
        Assert.Equal([b, c], mod.Dependencies);
        Assert.Equal(DependencyResolveStatus.Resolved, mod.DependencyResolveStatus);
    }

    [Fact]
    public void ResolveDependencies_RaiseEvent()
    {
        var scenario = ModTestScenarios.CreateTestScenario(
            ModTestScenarios.TestScenario.SingleDepAndTransitive,
            CreateMod,
            CreateOtherMod);

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
        var mod = CreateMod("A");
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
        var depLoc = FileSystem.DirectoryInfo.New(Game.GetModDirectory("B", ws, ServiceProvider));
        var dep = Game.InstallMod(depLoc, ws, new ModinfoData("B") { Version = new SemVersion(1) }, ServiceProvider);
        Game.AddMod(dep);

        var mod = CreateMod("Mod", deps: new ModReference(dep.Identifier, dep.Type, SemVersionRange.AtLeast(new SemVersion(2))));

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

    public class ModBaseAbstractTest : CommonTestBaseWithRandomGame
    {
        [Fact]
        public void ResolveDependenciesCore_ReturnNull_Throws()
        {
            var dep = CreateAndAddMod("Dep");
            var modinfo = new ModinfoData("CustomMod")
            {
                Dependencies = new DependencyList(new List<IModReference> { dep }, DependencyResolveLayout.FullResolved)
            };
            var mod = new NullResolvingMod(Game, modinfo, ServiceProvider);

            Assert.Throws<PetroglyphException>(mod.ResolveDependencies);
            Assert.Equal(DependencyResolveStatus.Faulted, mod.DependencyResolveStatus);

            // Attempt to try again should try to resolve again, but we expect the same result.
            Assert.Throws<PetroglyphException>(mod.ResolveDependencies);
            Assert.Equal(DependencyResolveStatus.Faulted, mod.DependencyResolveStatus);
        }

        [Fact]
        public void ResolveDependencies_CalledTwice_Throws()
        {
            var dep = CreateAndAddMod("Dep");
            var modinfo = new ModinfoData("CustomMod")
            {
                Dependencies = new DependencyList(new List<IModReference> { dep }, DependencyResolveLayout.FullResolved)
            };
            var mod = new SelfResolvingMod(Game, modinfo, ServiceProvider);

            Assert.Throws<ModDependencyCycleException>(mod.ResolveDependencies);
            Assert.Equal(DependencyResolveStatus.Faulted, mod.DependencyResolveStatus);

            // Attempt to try again should try to resolve again, but we expect the same result.
            Assert.Throws<ModDependencyCycleException>(mod.ResolveDependencies);
            Assert.Equal(DependencyResolveStatus.Faulted, mod.DependencyResolveStatus);
        }

        private class NullResolvingMod(IGame game, IModinfo modinfo, IServiceProvider serviceProvider)
            : ModBase(game, ModType.Default, modinfo, serviceProvider)
        {
            public override string Identifier => "CustomMod";

            protected override IReadOnlyList<IMod> ResolveDependenciesCore()
            {
                Assert.Equal(DependencyResolveStatus.Resolving, DependencyResolveStatus);
                return null!;
            }
        }

        private class SelfResolvingMod(IGame game, IModinfo modinfo, IServiceProvider serviceProvider)
            : ModBase(game, ModType.Default, modinfo, serviceProvider)
        {
            public override string Identifier => "CustomMod";

            protected override IReadOnlyList<IMod> ResolveDependenciesCore()
            {
                Assert.Equal(DependencyResolveStatus.Resolving, DependencyResolveStatus);
                // We should not end up in a StackOverflowException, cause ModBase handles the reentry.
                ResolveDependencies();
                return [];
            }
        }
    }
}