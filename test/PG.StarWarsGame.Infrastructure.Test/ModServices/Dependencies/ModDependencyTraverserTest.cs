using System;
using AET.Modinfo.Spec;
using AET.Testing.Extensions;
using PG.StarWarsGame.Infrastructure.Mods;
using PG.StarWarsGame.Infrastructure.Services.Dependencies;
using PG.StarWarsGame.Infrastructure.Testing.TestBases;
using Xunit;

namespace PG.StarWarsGame.Infrastructure.Test.ModServices.Dependencies;

public class ModDependencyTraverserTest : GameInfrastructureTestBaseWithRandomGame
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
            (name, layout, dependencies) => InstallAndAddModWithDependencies(name, layout, dependencies).Mod,
            (name, layout, dependencies) => InstallAndAddModWithDependencies(name, layout, dependencies).Mod);

        var mod = scenario.Mod;
        mod.ResolveDependencies();

        var traversedList = _traverser.Traverse(mod);

        Assert.Equal(scenario.ExpectedTraversedList, traversedList);
    }

    [Fact]
    public void Traverse_FaultedResolvedMod_Throws()
    {
        // Do not add to provoke faulted
        var dep = GameInstallation.InstallMod("B").Mod;

        var mod = InstallAndAddModWithDependencies("Mod", Random.Enum<DependencyResolveLayout>(), dep).Mod;

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
        var dep = GameInstallation.InstallMod("B").Mod;
        var mod = InstallAndAddModWithDependencies("Mod", Random.Enum<DependencyResolveLayout>(), dep).Mod;
        
        Assert.Equal(DependencyResolveStatus.None, mod.DependencyResolveStatus);
        Assert.Throws<InvalidOperationException>(() => _traverser.Traverse(mod));
    }
}