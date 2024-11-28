using System;
using System.Collections.Generic;
using EawModinfo.Model;
using EawModinfo.Spec;
using Microsoft.Extensions.DependencyInjection;
using PG.StarWarsGame.Infrastructure.Mods;
using PG.StarWarsGame.Infrastructure.Services.Dependencies.New;
using PG.StarWarsGame.Infrastructure.Services.Detection;
using PG.StarWarsGame.Infrastructure.Testing;
using PG.StarWarsGame.Infrastructure.Testing.Mods;
using PG.TestingUtilities;
using Xunit;

namespace PG.StarWarsGame.Infrastructure.Test.ModServices;

public class ModReferenceDependencyGraphBuilderTest : CommonTestBaseWithRandomGame
{
    private readonly IModIdentifierBuilder _identifierBuilder;
    private readonly ModReferenceDependencyGraphBuilder _graphBuilder;

    public ModReferenceDependencyGraphBuilderTest()
    {
        _identifierBuilder = ServiceProvider.GetRequiredService<IModIdentifierBuilder>();
        _graphBuilder = new ModReferenceDependencyGraphBuilder(ServiceProvider);
    }

    [Fact]
    public void NullArgs_Throws()
    {
        Assert.Throws<ArgumentNullException>(() => new ModReferenceDependencyGraphBuilder(null!));
        Assert.Throws<ArgumentNullException>(() => _graphBuilder.Build(null!));
    }

    [Fact]
    public void Build_NoDependencies()
    {
        var mod = CreateAndAddMod("A");
        var graph = _graphBuilder.Build(mod);

        Assert.False(graph.HasCycle());
        Assert.Equal([new GraphModReference(mod, DependencyKind.Root)], graph.Vertices);
    }

    [Fact]
    public void Build_OneDependency()
    {
        var b = CreateAndAddMod("B");
        var mod = CreateAndAddMod("A", TestHelpers.GetRandomEnum<DependencyResolveLayout>(), b);
        var graph = _graphBuilder.Build(mod);

        Assert.False(graph.HasCycle());
        Assert.Equal(
            [
                new GraphModReference(mod, DependencyKind.Root),
                new GraphModReference(b, DependencyKind.DirectDependency)
            ]
            , graph.Vertices);

        Assert.Equivalent(new List<ModReferenceEdge>
        {
            new(new GraphModReference(mod, DependencyKind.Root),
                new GraphModReference(b, DependencyKind.DirectDependency))
        }, graph.Edges, true);
    }

    [Fact]
    public void Build_GraphContainsOnlyVerticesAsDefinedByLayout_FullResolved()
    {
        // C is not added to game. Building a graph should not throw, because it's not used.
        var c = Game.InstallMod("C", false, ServiceProvider);
        var b = CreateAndAddMod("B", DependencyResolveLayout.FullResolved, c);
        var mod = CreateAndAddMod("A", DependencyResolveLayout.FullResolved, b);
        var graph = _graphBuilder.Build(mod);

        Assert.False(graph.HasCycle());
        Assert.Equal(
            [
                new GraphModReference(mod, DependencyKind.Root),
                new GraphModReference(b, DependencyKind.DirectDependency)
            ]
            , graph.Vertices); // Mod C should not exist, cause A's layout is full resolved
        
        Assert.Equivalent(new List<ModReferenceEdge>
        {
            new(new GraphModReference(mod, DependencyKind.Root),
                new GraphModReference(b, DependencyKind.DirectDependency))
        }, graph.Edges, true); // B --> C should not exist
    }

    [Fact]
    public void Build_GraphContainsOnlyVerticesAsDefinedByLayout_ResolveRecursive()
    {
        var c = CreateAndAddMod("C");
        var b = CreateAndAddMod("B", DependencyResolveLayout.FullResolved, c);
        var mod = CreateAndAddMod("A", DependencyResolveLayout.ResolveRecursive, b);
        var graph = _graphBuilder.Build(mod);

        Assert.False(graph.HasCycle());
        Assert.Equal(
        [
            new GraphModReference(mod, DependencyKind.Root),
            new GraphModReference(b, DependencyKind.DirectDependency),
            new GraphModReference(c, DependencyKind.Transitive),
        ], graph.Vertices);
        Assert.Equivalent(new List<ModReferenceEdge>
            {
                new(new GraphModReference(mod, DependencyKind.Root),
                    new GraphModReference(b, DependencyKind.DirectDependency)),
                new(new GraphModReference(b, DependencyKind.DirectDependency),
                    new GraphModReference(c, DependencyKind.Transitive))
            },
            graph.Edges, true);
    }

    [Fact]
    public void Build_GraphContainsOnlyVerticesAsDefinedByLayout_ResolveLastItem_Variant1()
    {
        var d = CreateAndAddMod("D");
        var c = CreateAndAddMod("C");
        var b = CreateAndAddMod("B", DependencyResolveLayout.FullResolved, c);
        var mod = CreateAndAddMod("A", DependencyResolveLayout.ResolveLastItem, b, d);
        var graph = _graphBuilder.Build(mod);

        Assert.False(graph.HasCycle());
        Assert.Equal(
        [
            new GraphModReference(mod, DependencyKind.Root),
            new GraphModReference(b, DependencyKind.DirectDependency),
            new GraphModReference(d, DependencyKind.DirectDependency)
        ], graph.Vertices); // Mod C should not exist, cause B is not last item
        Assert.Equivalent(new List<ModReferenceEdge>
            {
                new(new GraphModReference(mod, DependencyKind.Root),
                    new GraphModReference(b, DependencyKind.DirectDependency)),
                new(new GraphModReference(mod, DependencyKind.Root),
                    new GraphModReference(d, DependencyKind.DirectDependency))
            },
            graph.Edges, true);
    }

    [Fact]
    public void Build_GraphContainsOnlyVerticesAsDefinedByLayout_ResolveLastItem_Variant2()
    {
        var d = CreateAndAddMod("D");
        var c = CreateAndAddMod("C");
        var b = CreateAndAddMod("B", DependencyResolveLayout.FullResolved, c);
        var mod = CreateAndAddMod("A", DependencyResolveLayout.ResolveLastItem, d, b);
        var graph = _graphBuilder.Build(mod);

        Assert.False(graph.HasCycle());
        Assert.Equal(
        [
            new GraphModReference(mod, DependencyKind.Root),
            new GraphModReference(d, DependencyKind.DirectDependency),
            new GraphModReference(b, DependencyKind.DirectDependency),
            new GraphModReference(c, DependencyKind.Transitive)
        ], graph.Vertices); // Mod C should exist, cause B is last item
        Assert.Equivalent(new List<ModReferenceEdge>
            {
                new(new GraphModReference(mod, DependencyKind.Root),
                    new GraphModReference(b, DependencyKind.DirectDependency)),
                new(new GraphModReference(mod, DependencyKind.Root),
                    new GraphModReference(d, DependencyKind.DirectDependency)),
                new(new GraphModReference(b, DependencyKind.DirectDependency),
                    new GraphModReference(c, DependencyKind.Transitive)),
            },
            graph.Edges, true);
    }

    [Fact]
    public void Build_ResolveLayoutFromTransitiveApplied_ResolveRecursive()
    {
        var d = CreateAndAddMod("D");
        var c = CreateAndAddMod("C", DependencyResolveLayout.FullResolved, d);
        var b = CreateAndAddMod("B", DependencyResolveLayout.ResolveRecursive, c);
        var mod = CreateAndAddMod("A", DependencyResolveLayout.ResolveRecursive, b);
        var graph = _graphBuilder.Build(mod);

        Assert.False(graph.HasCycle());
        Assert.Equal(
        [
            new GraphModReference(mod, DependencyKind.Root),
            new GraphModReference(b, DependencyKind.DirectDependency),
            new GraphModReference(c, DependencyKind.Transitive),
            new GraphModReference(d, DependencyKind.Transitive)
        ], graph.Vertices);
        Assert.Equivalent(new List<ModReferenceEdge>
            {
                new(new GraphModReference(mod, DependencyKind.Root),
                    new GraphModReference(b, DependencyKind.DirectDependency)),
                new(new GraphModReference(b, DependencyKind.DirectDependency),
                    new GraphModReference(c, DependencyKind.Transitive)),
                new(new GraphModReference(c, DependencyKind.Transitive),
                    new GraphModReference(d, DependencyKind.Transitive)),
            },
            graph.Edges, true);
    }

    [Fact]
    public void Build_ResolveLayoutFromTransitiveApplied_ResolveLastItem_Variant1()
    {
        var e = CreateAndAddMod("E");
        var d = CreateAndAddMod("D");
        var c = CreateAndAddMod("C", DependencyResolveLayout.FullResolved, d);
        var b = CreateAndAddMod("B", DependencyResolveLayout.ResolveLastItem, e, c);
        var mod = CreateAndAddMod("A", DependencyResolveLayout.ResolveRecursive, b);
        var graph = _graphBuilder.Build(mod);

        Assert.False(graph.HasCycle());
        Assert.Equal(
        [
            new GraphModReference(mod, DependencyKind.Root),
            new GraphModReference(b, DependencyKind.DirectDependency),
            new GraphModReference(e, DependencyKind.Transitive),
            new GraphModReference(c, DependencyKind.Transitive),
            new GraphModReference(d, DependencyKind.Transitive),
        ], graph.Vertices);
        Assert.Equivalent(new List<ModReferenceEdge>
            {
                new(new GraphModReference(mod, DependencyKind.Root),
                    new GraphModReference(b, DependencyKind.DirectDependency)),
                new(new GraphModReference(b, DependencyKind.DirectDependency),
                    new GraphModReference(e, DependencyKind.Transitive)),
                new(new GraphModReference(b, DependencyKind.DirectDependency),
                    new GraphModReference(c, DependencyKind.Transitive)),
                new(new GraphModReference(c, DependencyKind.Transitive),
                    new GraphModReference(d, DependencyKind.Transitive)),
            },
            graph.Edges, true);
    }

    [Fact]
    public void Build_ResolveLayoutFromTransitiveApplied_ResolveLastItem_Variant2()
    {
        var e = CreateAndAddMod("E");
        var d = CreateAndAddMod("D");
        var c = CreateAndAddMod("C", DependencyResolveLayout.FullResolved, d);
        var b = CreateAndAddMod("B", DependencyResolveLayout.ResolveLastItem, c, e);
        var mod = CreateAndAddMod("A", DependencyResolveLayout.ResolveRecursive, b);
        var graph = _graphBuilder.Build(mod);

        Assert.False(graph.HasCycle());
        Assert.Equal(
        [
            new GraphModReference(mod, DependencyKind.Root),
            new GraphModReference(b, DependencyKind.DirectDependency),
            new GraphModReference(c, DependencyKind.Transitive),
            new GraphModReference(e, DependencyKind.Transitive),
        ], graph.Vertices);
        Assert.Equivalent(new List<ModReferenceEdge>
            {
                new(new GraphModReference(mod, DependencyKind.Root),
                    new GraphModReference(b, DependencyKind.DirectDependency)),
                new(new GraphModReference(b, DependencyKind.DirectDependency),
                    new GraphModReference(c, DependencyKind.Transitive)),
                new(new GraphModReference(b, DependencyKind.DirectDependency),
                    new GraphModReference(e, DependencyKind.Transitive)),
            },
            graph.Edges, true);
    }

    [Fact]
    public void Build_DirectCycle()
    {
        var depA = _identifierBuilder.Normalize(new ModReference(FileSystem.Path.Combine(Game.ModsLocation.FullName, "A"), ModType.Default));
        var modinfo = new ModinfoData("A")
        {
            Dependencies = new DependencyList(new List<IModReference>
            {
                depA
            }, TestHelpers.GetRandomEnum<DependencyResolveLayout>())
        };
        var mod = Game.InstallAndAddMod(false, modinfo, ServiceProvider);

        var graph = _graphBuilder.Build(mod);
        Assert.True(graph.HasCycle());
    }

    [Fact]
    public void Build_TransitiveCycle()
    {
        var depA = _identifierBuilder.Normalize(new ModReference(FileSystem.Path.Combine(Game.ModsLocation.FullName, "A"), ModType.Default));

        var b = CreateAndAddMod("B", TestHelpers.GetRandomEnum<DependencyResolveLayout>(), depA);

        var modinfo = new ModinfoData("A")
        {
            Dependencies = new DependencyList(new List<IModReference>
            {
                b
            }, DependencyResolveLayout.ResolveRecursive)
        };
        var mod = Game.InstallAndAddMod(false, modinfo, ServiceProvider);

        var graph = _graphBuilder.Build(mod);
        Assert.True(graph.HasCycle());
    }

    [Fact]
    public void Build_SelfNotFound_Throws()
    {
        var mod = Game.InstallMod("A", GITestUtilities.GetRandomWorkshopFlag(Game), ServiceProvider);
        Assert.Throws<ModNotFoundException>(() => _graphBuilder.Build(mod));
    }

    [Fact]
    public void Build_DirectDependencyNotFound_Throws()
    {
        var b = Game.InstallMod("B", false, ServiceProvider);
        var mod = CreateAndAddMod("A", TestHelpers.GetRandomEnum<DependencyResolveLayout>(), b);
        Assert.Throws<ModNotFoundException>(() => _graphBuilder.Build(mod));
    }

    [Fact]
    public void Build_TransitiveDependencyNotFound_Throws()
    {
        var c = Game.InstallMod("C", false, ServiceProvider);
        var b = CreateAndAddMod("B", DependencyResolveLayout.FullResolved, c);
        var mod = CreateAndAddMod("A", DependencyResolveLayout.ResolveRecursive, b);
        Assert.Throws<ModNotFoundException>(() => _graphBuilder.Build(mod));
    }
}