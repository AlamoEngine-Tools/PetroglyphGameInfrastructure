using System;
using System.Collections.Generic;
using EawModinfo.Model;
using EawModinfo.Spec;
using Microsoft.Extensions.DependencyInjection;
using PG.StarWarsGame.Infrastructure.Games;
using PG.StarWarsGame.Infrastructure.Mods;
using PG.StarWarsGame.Infrastructure.Services.Dependencies.New;
using PG.StarWarsGame.Infrastructure.Services.Detection;
using PG.StarWarsGame.Infrastructure.Testing;
using PG.StarWarsGame.Infrastructure.Testing.Mods;
using PG.TestingUtilities;
using Xunit;

namespace PG.StarWarsGame.Infrastructure.Test.ModServices;

public class ModReferenceDependencyGraphBuilderTest : CommonTestBase
{
    private readonly IGame _game;
    private readonly IModIdentifierBuilder _identifierBuilder;
    private readonly ModReferenceDependencyGraphBuilder _graphBuilder;

    public ModReferenceDependencyGraphBuilderTest()
    {
        _game = CreateRandomGame();
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
        var mod = CreateMod("A");
        var graph = _graphBuilder.Build(mod);

        Assert.False(graph.HasCycle());
        Assert.Equal([new GraphModReference(mod, DependencyKind.Root)], graph.Vertices);
    }

    [Fact]
    public void Build_OneDependency()
    {
        var b = CreateMod("B");
        var mod = CreateMod("A", TestHelpers.GetRandomEnum<DependencyResolveLayout>(), b);
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
    public void Build_DirectDependencyNotFound_Throws()
    {
        var b = _game.InstallMod("B", false, ServiceProvider);
        var mod = CreateMod("A", TestHelpers.GetRandomEnum<DependencyResolveLayout>(), b);
        Assert.Throws<ModNotFoundException>(() => _graphBuilder.Build(mod));
    }

    [Fact]
    public void Build_TransitiveDependencyNotFound_Throws()
    {
        var c = _game.InstallMod("C", false, ServiceProvider);
        var b = CreateMod("B", DependencyResolveLayout.FullResolved, c);
        var mod = CreateMod("A", DependencyResolveLayout.ResolveRecursive, b);
        Assert.Throws<ModNotFoundException>(() => _graphBuilder.Build(mod));
    }

    [Fact]
    public void Build_GraphContainsOnlyVerticesAsDefinedByLayout_FullResolved()
    {
        // C is not added to game. Building a graph should not throw, because it's not used.
        var c = _game.InstallMod("C", false, ServiceProvider);
        var b = CreateMod("B", DependencyResolveLayout.FullResolved, c);
        var mod = CreateMod("A", DependencyResolveLayout.FullResolved, b);
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
        var c = CreateMod("C");
        var b = CreateMod("B", DependencyResolveLayout.FullResolved, c);
        var mod = CreateMod("A", DependencyResolveLayout.ResolveRecursive, b);
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
        var d = CreateMod("D");
        var c = CreateMod("C");
        var b = CreateMod("B", DependencyResolveLayout.FullResolved, c);
        var mod = CreateMod("A", DependencyResolveLayout.ResolveLastItem, b, d);
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
        var d = CreateMod("D");
        var c = CreateMod("C");
        var b = CreateMod("B", DependencyResolveLayout.FullResolved, c);
        var mod = CreateMod("A", DependencyResolveLayout.ResolveLastItem, d, b);
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
        var d = CreateMod("D");
        var c = CreateMod("C", DependencyResolveLayout.FullResolved, d);
        var b = CreateMod("B", DependencyResolveLayout.ResolveRecursive, c);
        var mod = CreateMod("A", DependencyResolveLayout.ResolveRecursive, b);
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
        var e = CreateMod("E");
        var d = CreateMod("D");
        var c = CreateMod("C", DependencyResolveLayout.FullResolved, d);
        var b = CreateMod("B", DependencyResolveLayout.ResolveLastItem, e, c);
        var mod = CreateMod("A", DependencyResolveLayout.ResolveRecursive, b);
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
        var e = CreateMod("E");
        var d = CreateMod("D");
        var c = CreateMod("C", DependencyResolveLayout.FullResolved, d);
        var b = CreateMod("B", DependencyResolveLayout.ResolveLastItem, c, e);
        var mod = CreateMod("A", DependencyResolveLayout.ResolveRecursive, b);
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
        var depA = _identifierBuilder.Normalize(new ModReference(FileSystem.Path.Combine(_game.ModsLocation.FullName, "A"), ModType.Default));
        var modinfo = new ModinfoData("A")
        {
            Dependencies = new DependencyList(new List<IModReference>
            {
                depA
            }, TestHelpers.GetRandomEnum<DependencyResolveLayout>())
        };
        var mod = _game.InstallAndAddMod(false, modinfo, ServiceProvider);

        var graph = _graphBuilder.Build(mod);
        Assert.True(graph.HasCycle());
    }

    [Fact]
    public void Build_TransitiveCycle()
    {
        var depA = _identifierBuilder.Normalize(new ModReference(FileSystem.Path.Combine(_game.ModsLocation.FullName, "A"), ModType.Default));

        var b = CreateMod("B", TestHelpers.GetRandomEnum<DependencyResolveLayout>(), depA);

        var modinfo = new ModinfoData("A")
        {
            Dependencies = new DependencyList(new List<IModReference>
            {
                b
            }, DependencyResolveLayout.ResolveRecursive)
        };
        var mod = _game.InstallAndAddMod(false, modinfo, ServiceProvider);

        var graph = _graphBuilder.Build(mod);
        Assert.True(graph.HasCycle());
    }

    private IMod CreateMod(string name, DependencyResolveLayout layout = DependencyResolveLayout.FullResolved, params IModReference[] deps)
    {
        if (deps.Length == 0)
            return _game.InstallAndAddMod(name, GITestUtilities.GetRandomWorkshopFlag(_game), ServiceProvider);

        var modinfo = new ModinfoData(name)
        {
            Dependencies = new DependencyList(deps, layout)
        };

        return _game.InstallAndAddMod(GITestUtilities.GetRandomWorkshopFlag(_game), modinfo, ServiceProvider);
    }
}