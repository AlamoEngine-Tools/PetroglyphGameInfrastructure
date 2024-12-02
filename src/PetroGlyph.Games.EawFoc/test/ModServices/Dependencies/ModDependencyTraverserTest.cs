using System;
using EawModinfo.Spec;
using PG.StarWarsGame.Infrastructure.Mods;
using PG.StarWarsGame.Infrastructure.Services.Dependencies;
using PG.StarWarsGame.Infrastructure.Testing;
using PG.StarWarsGame.Infrastructure.Testing.Mods;
using PG.TestingUtilities;
using Xunit;

namespace PG.StarWarsGame.Infrastructure.Test.ModServices.Dependencies;

public class ModDependencyTraverserTest : CommonTestBaseWithRandomGame
{
    private readonly ModDependencyTraverser _traverser;

    // Note that we cannot test the cycle scenarios here,
    // as these cases already get handled when resolving a mod's dependencies.
    public ModDependencyTraverserTest()
    {
        _traverser = new ModDependencyTraverser(ServiceProvider);
    }

    [Theory]
    [MemberData(nameof(ModTestScenarios.ValidScenarios), MemberType = typeof(ModTestScenarios))]
    public void Traverse_ValidScenarios(ModTestScenarios.TestScenario testScenario)
    {

        var scenario = ModTestScenarios.CreateTestScenario(
            testScenario,
            CreateAndAddMod,
            CreateAndAddMod);

        var mod = scenario.Mod;
        mod.ResolveDependencies();

        var traversedList = _traverser.Traverse(mod);

        Assert.Equal(scenario.ExpectedTraversedList, traversedList);
    }
    
    [Fact]
    public void Traverse_FaultedResolvedMod_Throws()
    {
        // Do not add to provoke faulted
        var dep = Game.InstallMod("B", GITestUtilities.GetRandomWorkshopFlag(Game), ServiceProvider);

        var mod = CreateAndAddMod("Mod", TestHelpers.GetRandomEnum<DependencyResolveLayout>(), dep);

        try
        {
            mod.ResolveDependencies();
        }
        catch (ModNotFoundException)
        {
            // Ignore
        }

        Assert.Equal(DependencyResolveStatus.Faulted, mod.DependencyResolveStatus);
        Assert.Throws<InvalidOperationException>(() => _traverser.Traverse(mod));
    }

    [Fact]
    public void Traverse_NotResolvedMod_Throws()
    {
        // Do not add to provoke faulted
        var dep = Game.InstallMod("B", GITestUtilities.GetRandomWorkshopFlag(Game), ServiceProvider);
        var mod = CreateAndAddMod("Mod", TestHelpers.GetRandomEnum<DependencyResolveLayout>(), dep);
        
        Assert.Equal(DependencyResolveStatus.None, mod.DependencyResolveStatus);
        Assert.Throws<InvalidOperationException>(() => _traverser.Traverse(mod));
    }
}