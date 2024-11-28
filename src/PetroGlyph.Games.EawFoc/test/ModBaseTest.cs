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

        Assert.Equal(expectedDirectDeps, mod.Dependencies.Select(x => x.Mod));
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
        Assert.Throws<ModNotFoundException>(mod.ResolveDependencies);
        Assert.Equal(DependencyResolveStatus.Faulted, mod.DependencyResolveStatus);

        // Add dep to game
        Game.AddMod(notAddedDep);

        mod.ResolveDependencies();

        Assert.Equal([notAddedDep], mod.Dependencies.Select(x => x.Mod));
        Assert.Equal(DependencyResolveStatus.Resolved, mod.DependencyResolveStatus);
    }

    [Fact]
    public void ResolveDependencies_DepOfWrongGame_Throws()
    {
        var otherGameReference = new PetroglyphStarWarsGame(Game, Game.Directory, Game.Name, ServiceProvider);
        var wrongGameDep = otherGameReference.InstallAndAddMod("WrongGameRefMod", 
            GITestUtilities.GetRandomWorkshopFlag(otherGameReference), ServiceProvider);
        var mod = CreateMod("Mod", TestHelpers.GetRandomEnum<DependencyResolveLayout>(), wrongGameDep);
        
        Assert.Throws<ModNotFoundException>(mod.ResolveDependencies);
        Assert.Equal(DependencyResolveStatus.Faulted, mod.DependencyResolveStatus);
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
                AssertDependenciesResolved(dependency.Mod);
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

            protected override IReadOnlyList<ModDependencyEntry> ResolveDependenciesCore()
            {
                Assert.Equal(DependencyResolveStatus.Resolving, DependencyResolveStatus);
                return null!;
            }
        }

        private class SelfResolvingMod(IGame game, IModinfo modinfo, IServiceProvider serviceProvider)
            : ModBase(game, ModType.Default, modinfo, serviceProvider)
        {
            public override string Identifier => "CustomMod";

            protected override IReadOnlyList<ModDependencyEntry> ResolveDependenciesCore()
            {
                Assert.Equal(DependencyResolveStatus.Resolving, DependencyResolveStatus);
                // We should not end up in a StackOverflowException, cause ModBase handles the reentry.
                ResolveDependencies();
                return [];
            }
        }
    }
}