using System;
using System.Collections.Generic;
using EawModinfo.Model;
using EawModinfo.Spec;
using PG.StarWarsGame.Infrastructure.Mods;
using PG.StarWarsGame.Infrastructure.Services.Dependencies;
using PG.StarWarsGame.Infrastructure.Testing;
using PG.StarWarsGame.Infrastructure.Testing.Mods;
using PG.StarWarsGame.Infrastructure.Testing.TestBases;
using PG.TestingUtilities;
using Semver;
using Xunit;

namespace PG.StarWarsGame.Infrastructure.Test.ModServices.Dependencies;

public class ModDependencyGraphBuilderTest : CommonTestBaseWithRandomGame
{
    private readonly ModDependencyGraphBuilder _graphBuilder = new();

    [Fact]
    public void NullArgs_Throws()
    {
        Assert.Throws<ArgumentNullException>(() => _graphBuilder.Build(null!));
    }

    [Fact]
    public void Build_NoDependencies()
    {
        var mod = CreateAndAddMod("A");
        var graph = _graphBuilder.Build(mod);

        Assert.False(graph.HasCycle());
        Assert.Equal([new ModDependencyGraphVertex(mod, DependencyKind.Root)], graph.Vertices);
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
                new ModDependencyGraphVertex(mod, DependencyKind.Root),
                new ModDependencyGraphVertex(b, DependencyKind.DirectDependency)
            ]
            , graph.Vertices);

        Assert.Equivalent(new List<ModDependencyGraphEdge>
        {
            new(new ModDependencyGraphVertex(mod, DependencyKind.Root),
                new ModDependencyGraphVertex(b, DependencyKind.DirectDependency))
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
                new ModDependencyGraphVertex(mod, DependencyKind.Root),
                new ModDependencyGraphVertex(b, DependencyKind.DirectDependency)
            ]
            , graph.Vertices); // Mod C should not exist, cause A's layout is full resolved

        Assert.Equivalent(new List<ModDependencyGraphEdge>
        {
            new(new ModDependencyGraphVertex(mod, DependencyKind.Root),
                new ModDependencyGraphVertex(b, DependencyKind.DirectDependency))
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
            new ModDependencyGraphVertex(mod, DependencyKind.Root),
            new ModDependencyGraphVertex(b, DependencyKind.DirectDependency),
            new ModDependencyGraphVertex(c, DependencyKind.Transitive),
        ], graph.Vertices);
        Assert.Equivalent(new List<ModDependencyGraphEdge>
            {
                new(new ModDependencyGraphVertex(mod, DependencyKind.Root),
                    new ModDependencyGraphVertex(b, DependencyKind.DirectDependency)),
                new(new ModDependencyGraphVertex(b, DependencyKind.DirectDependency),
                    new ModDependencyGraphVertex(c, DependencyKind.Transitive))
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
            new ModDependencyGraphVertex(mod, DependencyKind.Root),
            new ModDependencyGraphVertex(b, DependencyKind.DirectDependency),
            new ModDependencyGraphVertex(d, DependencyKind.DirectDependency)
        ], graph.Vertices); // Mod C should not exist, cause B is not last item
        Assert.Equivalent(new List<ModDependencyGraphEdge>
            {
                new(new ModDependencyGraphVertex(mod, DependencyKind.Root),
                    new ModDependencyGraphVertex(b, DependencyKind.DirectDependency)),
                new(new ModDependencyGraphVertex(mod, DependencyKind.Root),
                    new ModDependencyGraphVertex(d, DependencyKind.DirectDependency))
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
            new ModDependencyGraphVertex(mod, DependencyKind.Root),
            new ModDependencyGraphVertex(d, DependencyKind.DirectDependency),
            new ModDependencyGraphVertex(b, DependencyKind.DirectDependency),
            new ModDependencyGraphVertex(c, DependencyKind.Transitive)
        ], graph.Vertices); // Mod C should exist, cause B is last item
        Assert.Equivalent(new List<ModDependencyGraphEdge>
            {
                new(new ModDependencyGraphVertex(mod, DependencyKind.Root),
                    new ModDependencyGraphVertex(b, DependencyKind.DirectDependency)),
                new(new ModDependencyGraphVertex(mod, DependencyKind.Root),
                    new ModDependencyGraphVertex(d, DependencyKind.DirectDependency)),
                new(new ModDependencyGraphVertex(b, DependencyKind.DirectDependency),
                    new ModDependencyGraphVertex(c, DependencyKind.Transitive)),
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
            new ModDependencyGraphVertex(mod, DependencyKind.Root),
            new ModDependencyGraphVertex(b, DependencyKind.DirectDependency),
            new ModDependencyGraphVertex(c, DependencyKind.Transitive),
            new ModDependencyGraphVertex(d, DependencyKind.Transitive)
        ], graph.Vertices);
        Assert.Equivalent(new List<ModDependencyGraphEdge>
            {
                new(new ModDependencyGraphVertex(mod, DependencyKind.Root),
                    new ModDependencyGraphVertex(b, DependencyKind.DirectDependency)),
                new(new ModDependencyGraphVertex(b, DependencyKind.DirectDependency),
                    new ModDependencyGraphVertex(c, DependencyKind.Transitive)),
                new(new ModDependencyGraphVertex(c, DependencyKind.Transitive),
                    new ModDependencyGraphVertex(d, DependencyKind.Transitive)),
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
            new ModDependencyGraphVertex(mod, DependencyKind.Root),
            new ModDependencyGraphVertex(b, DependencyKind.DirectDependency),
            new ModDependencyGraphVertex(e, DependencyKind.Transitive),
            new ModDependencyGraphVertex(c, DependencyKind.Transitive),
            new ModDependencyGraphVertex(d, DependencyKind.Transitive),
        ], graph.Vertices);
        Assert.Equivalent(new List<ModDependencyGraphEdge>
            {
                new(new ModDependencyGraphVertex(mod, DependencyKind.Root),
                    new ModDependencyGraphVertex(b, DependencyKind.DirectDependency)),
                new(new ModDependencyGraphVertex(b, DependencyKind.DirectDependency),
                    new ModDependencyGraphVertex(e, DependencyKind.Transitive)),
                new(new ModDependencyGraphVertex(b, DependencyKind.DirectDependency),
                    new ModDependencyGraphVertex(c, DependencyKind.Transitive)),
                new(new ModDependencyGraphVertex(c, DependencyKind.Transitive),
                    new ModDependencyGraphVertex(d, DependencyKind.Transitive)),
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
            new ModDependencyGraphVertex(mod, DependencyKind.Root),
            new ModDependencyGraphVertex(b, DependencyKind.DirectDependency),
            new ModDependencyGraphVertex(c, DependencyKind.Transitive),
            new ModDependencyGraphVertex(e, DependencyKind.Transitive),
        ], graph.Vertices);
        Assert.Equivalent(new List<ModDependencyGraphEdge>
            {
                new(new ModDependencyGraphVertex(mod, DependencyKind.Root),
                    new ModDependencyGraphVertex(b, DependencyKind.DirectDependency)),
                new(new ModDependencyGraphVertex(b, DependencyKind.DirectDependency),
                    new ModDependencyGraphVertex(c, DependencyKind.Transitive)),
                new(new ModDependencyGraphVertex(b, DependencyKind.DirectDependency),
                    new ModDependencyGraphVertex(e, DependencyKind.Transitive)),
            },
            graph.Edges, true);
    }

    [Fact]
    public void Build_DirectCycle()
    {
        var depA = new ModReference("A", ModType.Default);
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
        var depA = new ModReference("A", ModType.Default);

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
        var e = Assert.Throws<ModNotFoundException>(() => _graphBuilder.Build(mod));
        Assert.Same(Game, e.ModContainer);
        Assert.Equal(mod, e.Mod);
    }

    [Fact]
    public void Build_DirectDependencyNotFound_Throws()
    {
        var b = Game.InstallMod("B", false, ServiceProvider);
        var mod = CreateAndAddMod("A", TestHelpers.GetRandomEnum<DependencyResolveLayout>(), b);
        var e = Assert.Throws<ModNotFoundException>(() => _graphBuilder.Build(mod));
        Assert.Same(Game, e.ModContainer);
        Assert.Equal(b, e.Mod);
    }

    [Fact]
    public void Build_TransitiveDependencyNotFound_Throws()
    {
        var c = Game.InstallMod("C", false, ServiceProvider);
        var b = CreateAndAddMod("B", DependencyResolveLayout.FullResolved, c);
        var mod = CreateAndAddMod("A", DependencyResolveLayout.ResolveRecursive, b);
        var e = Assert.Throws<ModNotFoundException>(() => _graphBuilder.Build(mod));
        Assert.Same(Game, e.ModContainer);
        Assert.Equal(c, e.Mod);
    }


    public static IEnumerable<object[]> GetMatchingVersionRanges()
    {
        yield return [null!, SemVersionRange.All];
        yield return [null!, SemVersionRange.Empty];
        yield return [null!, SemVersionRange.Equals(new SemVersion(1, 2, 3))];
        yield return [new SemVersion(2, 0, 0), SemVersionRange.All];
        yield return [new SemVersion(1, 0, 0), SemVersionRange.AllRelease];
        yield return [new SemVersion(2, 0, 0), SemVersionRange.AtLeast(new SemVersion(1, 0, 0))];
        yield return [new SemVersion(1, 0, 0), SemVersionRange.AtMost(new SemVersion(2, 0, 0))];
        yield return [new SemVersion(1, 0, 0), SemVersionRange.Equals(new SemVersion(1, 0, 0))];
    }

    [Theory]
    [MemberData(nameof(GetMatchingVersionRanges))]
    public void Build_OneDependency_VersionRangeMatches(SemVersion? version, SemVersionRange ranges)
    {
        var b = CreateAndAddMod(new ModinfoData("B") { Version = version });
        Assert.Equal(version, b.Version);

        var mod = CreateAndAddMod("A", TestHelpers.GetRandomEnum<DependencyResolveLayout>(),
            new ModReference(b.Identifier, b.Type, ranges));

        var graph = _graphBuilder.Build(mod);

        Assert.False(graph.HasCycle());
        Assert.Equal(
            [
                new ModDependencyGraphVertex(mod, DependencyKind.Root),
                new ModDependencyGraphVertex(b, DependencyKind.DirectDependency)
            ]
            , graph.Vertices);

        Assert.Equivalent(new List<ModDependencyGraphEdge>
        {
            new(new ModDependencyGraphVertex(mod, DependencyKind.Root),
                new ModDependencyGraphVertex(b, DependencyKind.DirectDependency))
        }, graph.Edges, true);
    }


    public static IEnumerable<object[]> GetMismatchingVersionsAndRange()
    {
        yield return [
            new SemVersion(2, 0, 0),
            SemVersionRange.Equals(new SemVersion(1, 0, 0))];
        yield return [
            new SemVersion(1, 0, 0),
            SemVersionRange.Equals(new SemVersion(2, 0, 0))];
        yield return [
            new SemVersion(1, 0, 0, new List<string>{"BETA-1"}),
            SemVersionRange.AllRelease];
        yield return [
            new SemVersion(1, 1, 0),
            SemVersionRange.GreaterThan(new SemVersion(1, 2, 0))];
        yield return [
            new SemVersion(3, 0, 0), 
            SemVersionRange.Empty];
    }

    [Theory]
    [MemberData(nameof(GetMismatchingVersionsAndRange))]
    public void Build_OneDependency_VersionRangeDoesNotMatches_Throws(SemVersion version, SemVersionRange? range)
    {
        var b = CreateAndAddMod(new ModinfoData("B")
        {
            Version = version
        });
        Assert.Equal(version, b.Version);

        var mod = CreateAndAddMod("A", TestHelpers.GetRandomEnum<DependencyResolveLayout>(),
            new ModReference(b.Identifier, b.Type, range));

        var e = Assert.Throws<VersionMismatchException>(() => _graphBuilder.Build(mod));
        Assert.Equal(new ModReference(b), e.Mod);
        Assert.Equal(b, e.Dependency);
    }

    [Theory]
    [MemberData(nameof(GetMismatchingVersionsAndRange))]
    public void Build_TransitiveDependency_VersionRangeDoesNotMatches_Throws(SemVersion version, SemVersionRange? range)
    {
        var c = CreateAndAddMod(new ModinfoData("C")
        {
            Version = version
        });
        Assert.Equal(version, c.Version);

        var b = CreateAndAddMod("B", deps: new List<IModReference>{new ModReference(c.Identifier, c.Type, range)});

        var mod = CreateAndAddMod("A", DependencyResolveLayout.ResolveRecursive, b);

        var e = Assert.Throws<VersionMismatchException>(() => _graphBuilder.Build(mod));
        Assert.Equal(new ModReference(c), e.Mod);
        Assert.Equal(c, e.Dependency);
    }

    [Fact]
    public void Build_VersionMismatchInOnlyOneRef_WhereEqualRefHasMatchingVersion_Throws_Variant1()
    {
        /*
           A : B, C
           C : B
            (1.0.0)
           A ->B(1.0.0)
            \  | 
             \ | (2.0.0)
              \^
               C
         */
        var b = CreateAndAddMod(new ModinfoData("B") { Version = new SemVersion(1, 0, 0) });

        var c = CreateAndAddMod("C",
            deps: new ModReference(b.Identifier, b.Type, SemVersionRange.Equals(new SemVersion(2, 0, 0))));

        var mod = CreateAndAddMod("A", DependencyResolveLayout.ResolveRecursive, deps:
        [
            new ModReference(b.Identifier, b.Type, SemVersionRange.Equals(new SemVersion(1, 0, 0))),
            c
        ]);
        var e = Assert.Throws<VersionMismatchException>(() => _graphBuilder.Build(mod));
        Assert.Equal(new ModReference(b), e.Mod);
        Assert.Equal(b, e.Dependency);
    }

    [Fact]
    public void Build_VersionMismatchInOnlyOneRef_WhereEqualRefHasMatchingVersion_Throws_Variant2()
    {
        /*
           A : C, B
           C : B      
            (1.0.0)
           A -----> B(1.0.0)
             \   /
              \ /  (2.0.0)
           	   >
           	   C
         */
        var b = CreateAndAddMod(new ModinfoData("B") { Version = new SemVersion(1, 0, 0) });

        var c = CreateAndAddMod("C", DependencyResolveLayout.ResolveRecursive,
            deps: new ModReference(b.Identifier, b.Type, SemVersionRange.Equals(new SemVersion(2, 0, 0))));

        var mod = CreateAndAddMod("A", DependencyResolveLayout.ResolveRecursive, deps:
        [
            c, // Before b in this variant
            new ModReference(b.Identifier, b.Type, SemVersionRange.Equals(new SemVersion(1, 0, 0)))
        ]);
        var e = Assert.Throws<VersionMismatchException>(() => _graphBuilder.Build(mod));
        Assert.Equal(new ModReference(b), e.Mod);
        Assert.Equal(b, e.Dependency);
    }
}