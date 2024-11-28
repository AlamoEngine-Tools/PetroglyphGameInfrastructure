using System;
using System.Collections.Generic;
using EawModinfo.Spec;
using PG.StarWarsGame.Infrastructure.Mods;

namespace PG.StarWarsGame.Infrastructure.Test;

public static class ModTestScenarios
{
    public delegate IMod ModFactoryDelegate(
        string name,
        DependencyResolveLayout resolveLayout = DependencyResolveLayout.ResolveRecursive,
        params IList<IModReference> dependencies);

    public delegate IModReference ModReferenceDelegate(string name);

    public enum TestScenario
    {
        A,
        B,
        C,
        D,
        E,
        F,
        G,
        H,
        I,
        J,
        NoDep,
        SingleDep,
        SingleDepFullResolved,
        ResolveLastTriggered,
        ResolveLastNotTriggered,
        ResolveLastTransitiveTriggered,
        ResolveLastTransitiveNotTriggered
    }

    public enum CycleTestScenario
    {
        K,
        L, 
        M
    }

    public static IEnumerable<object[]> ValidScenarios()
    {
        yield return [TestScenario.A];
        yield return [TestScenario.B];
        yield return [TestScenario.C];
        yield return [TestScenario.D];
        yield return [TestScenario.E];
        yield return [TestScenario.F];
        yield return [TestScenario.G];
        yield return [TestScenario.H];
        yield return [TestScenario.I];
        yield return [TestScenario.J];
        yield return [TestScenario.NoDep];
        yield return [TestScenario.SingleDep];
        yield return [TestScenario.SingleDepFullResolved];
        yield return [TestScenario.ResolveLastTriggered];
        yield return [TestScenario.ResolveLastNotTriggered];
        yield return [TestScenario.ResolveLastTransitiveTriggered];
        yield return [TestScenario.ResolveLastTransitiveNotTriggered];
    }

    public static IEnumerable<object[]> CycleScenarios()
    {
        yield return [CycleTestScenario.K];
        yield return [CycleTestScenario.L];
        yield return [CycleTestScenario.M];
    }

    public static (IMod Mod, IList<IModReference>? ExpectedTraversedList) CreateTestScenario(
        TestScenario scenario,
        ModFactoryDelegate createRootFactory,
        ModFactoryDelegate createDepFactory)
    {
        return scenario switch
        {
            TestScenario.A => ScenarioA(createRootFactory, createDepFactory),
            TestScenario.B => ScenarioB(createRootFactory, createDepFactory),
            TestScenario.C => ScenarioC(createRootFactory, createDepFactory),
            TestScenario.D => ScenarioD(createRootFactory, createDepFactory),
            TestScenario.E => ScenarioE(createRootFactory, createDepFactory),
            TestScenario.F => ScenarioF(createRootFactory, createDepFactory),
            TestScenario.G => ScenarioG(createRootFactory, createDepFactory),
            TestScenario.H => ScenarioH(createRootFactory, createDepFactory),
            TestScenario.I => ScenarioI(createRootFactory, createDepFactory),
            TestScenario.J => ScenarioJ(createRootFactory, createDepFactory),
            TestScenario.NoDep => ScenarioNoDependencies(createRootFactory, createDepFactory),
            TestScenario.SingleDep => ScenarioSingleDependency(createRootFactory, createDepFactory),
            TestScenario.SingleDepFullResolved => ScenarioSingleDependencyFullResolved(createRootFactory, createDepFactory),
            TestScenario.ResolveLastTriggered => ScenarioResolveLast_WithDependency(createRootFactory, createDepFactory),
            TestScenario.ResolveLastNotTriggered => ScenarioResolveLast_WithOutDependency(createRootFactory, createDepFactory),
            TestScenario.ResolveLastTransitiveTriggered => ScenarioResolveLast_Transitive_WithDependency(createRootFactory, createDepFactory),
            TestScenario.ResolveLastTransitiveNotTriggered => ScenarioResolveLast_Transitive_WithOutDependency(createRootFactory, createDepFactory),
            _ => throw new ArgumentOutOfRangeException(nameof(scenario), scenario, null)
        };
    }

    public static (IMod Mod, IList<IModReference>? ExpectedTraversedList) CreateTestScenarioCycle(
       CycleTestScenario scenario,
       ModFactoryDelegate createRootFactory,
       ModFactoryDelegate createDepFactory,
       ModReferenceDelegate createReferenceFactory)
    {
        return scenario switch
        {
            CycleTestScenario.K => ScenarioK(createRootFactory, createDepFactory, createReferenceFactory),
            CycleTestScenario.L => ScenarioL(createRootFactory, createDepFactory, createReferenceFactory),
            CycleTestScenario.M => ScenarioM(createRootFactory, createDepFactory, createReferenceFactory),
            _ => throw new ArgumentOutOfRangeException(nameof(scenario), scenario, null)
        };
    }


    public static (IMod Mod, IList<IModReference>? ExpectedTraversedList) ScenarioA(
        ModFactoryDelegate createRootFactory,
        ModFactoryDelegate createDepFactory)
    {
        var e = createDepFactory("E");
        var c = createDepFactory("C", dependencies: e);
        var d = createDepFactory("D");
        var b = createDepFactory("B", dependencies: d);
        var a = createRootFactory("A", dependencies: [b, c]);
        return (a, new List<IModReference> { a, b, c, d, e });
    }

    public static (IMod Mod, IList<IModReference>? ExpectedTraversedList) ScenarioB(
        ModFactoryDelegate createRootFactory,
        ModFactoryDelegate createDepFactory)
    {
        var e = createDepFactory("E");
        var c = createDepFactory("C", dependencies: e);
        var d = createDepFactory("D");
        var b = createDepFactory("B", dependencies: d);
        var a = createRootFactory("A", dependencies: [c, b]); // Switched b & c compared to scenario A
        return (a, new List<IModReference> { a, c, b, e, d });
    }

    public static (IMod Mod, IList<IModReference>? ExpectedTraversedList) ScenarioC(
        ModFactoryDelegate createRootFactory,
        ModFactoryDelegate createDepFactory)
    {
        var e = createDepFactory("E");
        var d = createDepFactory("D", dependencies: e);
        var c = createDepFactory("C", dependencies: d);
        var b = createDepFactory("B", dependencies: d);
        var a = createRootFactory("A", dependencies: [b, c]);
        return (a, new List<IModReference> { a, b, c, d, e });
    }

    public static (IMod Mod, IList<IModReference>? ExpectedTraversedList) ScenarioD(
        ModFactoryDelegate createRootFactory,
        ModFactoryDelegate createDepFactory)
    {
        var e = createDepFactory("E");
        var d = createDepFactory("D", dependencies: e);
        var c = createDepFactory("C", dependencies: e);
        var b = createDepFactory("B", dependencies: e);
        var a = createRootFactory("A", dependencies: [b, c, d]);
        return (a, new List<IModReference> { a, b, c, d, e });
    }

    private static (IMod Mod, IList<IModReference>? ExpectedTraversedList) ScenarioE(
        ModFactoryDelegate createRootFactory,
        ModFactoryDelegate createDepFactory)
    {
        var d = createDepFactory("D");
        var e = createDepFactory("E", dependencies: d);
        var c = createDepFactory("C", dependencies: d);
        var b = createDepFactory("B", dependencies: e);
        var a = createRootFactory("A", dependencies: [b, c]);
        return (a, new List<IModReference> { a, b, c, e, d });
    }

    public static (IMod Mod, IList<IModReference>? ExpectedTraversedList) ScenarioF(
        ModFactoryDelegate createRootFactory,
        ModFactoryDelegate createDepFactory)
    {
        var d = createDepFactory("D");
        var e = createDepFactory("E", dependencies: d);
        var c = createDepFactory("C", dependencies: e);
        var b = createDepFactory("B", dependencies: d);
        var a = createRootFactory("A", dependencies: [b, c]);
        return (a, new List<IModReference> { a, b, c, e, d });
    }

    public static (IMod Mod, IList<IModReference>? ExpectedTraversedList) ScenarioG(
        ModFactoryDelegate createRootFactory,
        ModFactoryDelegate createDepFactory)
    {
        var g = createDepFactory("G");
        var f = createDepFactory("F");
        var e = createDepFactory("E");
        var d = createDepFactory("D");
        var c = createDepFactory("C");
        var b = createDepFactory("B");
        var a = createRootFactory("A", dependencies: [b, c, d, e, f, g]);
        return (a, new List<IModReference> { a, b, c, d, e, f, g });
    }

    public static (IMod Mod, IList<IModReference>? ExpectedTraversedList) ScenarioH(
        ModFactoryDelegate createRootFactory,
        ModFactoryDelegate createDepFactory)
    {
        var i = createDepFactory("I");
        var g = createDepFactory("G", dependencies: i);
        var f = createDepFactory("F", dependencies: i);
        var e = createDepFactory("E");
        var d = createDepFactory("D", dependencies: [e, f]);
        var c = createDepFactory("C", dependencies: g);
        var b = createDepFactory("B", dependencies: d);
        var a = createRootFactory("A", dependencies: [b, c]);
        return (a, new List<IModReference> { a, b, c, d, g, e, f, i });
    }

    public static (IMod Mod, IList<IModReference>? ExpectedTraversedList) ScenarioI(
        ModFactoryDelegate createRootFactory,
        ModFactoryDelegate createDepFactory)
    {
        var f = createDepFactory("F");
        var d = createDepFactory("D", dependencies: f);
        var x = createDepFactory("X", dependencies: [d, f]);
        var e = createDepFactory("E", dependencies: x);
        var c = createDepFactory("C", dependencies: d);
        var b = createDepFactory("B", dependencies: e);
        var a = createRootFactory("A", dependencies: [c, b]);
        return (a, new List<IModReference> { a, c, b, e, x, d, f });
    }

    public static (IMod Mod, IList<IModReference>? ExpectedTraversedList) ScenarioJ(
        ModFactoryDelegate createRootFactory,
        ModFactoryDelegate createDepFactory)
    {
        var f = createDepFactory("F");
        var x = createDepFactory("X");
        var e = createDepFactory("E", dependencies: [x, f]);
        var c = createDepFactory("C", dependencies: [x, f]);
        var b = createDepFactory("B", dependencies: x);
        var d = createDepFactory("D", dependencies: e);
        var a = createRootFactory("A", dependencies: [b, c, d]);
        return (a, new List<IModReference> { a, b, c, d, e, x, f });
    }

    public static (IMod Mod, IList<IModReference>? ExpectedTraversedList) ScenarioK(
        ModFactoryDelegate createRootFactory,
        ModFactoryDelegate createDepFactory,
        ModReferenceDelegate createReferenceFactory)
    {
        var refA = createReferenceFactory("A");
        var a = createRootFactory("A", dependencies: refA);
        return (a, null);
    }

    public static (IMod Mod, IList<IModReference>? ExpectedTraversedList) ScenarioL(
        ModFactoryDelegate createRootFactory,
        ModFactoryDelegate createDepFactory,
        ModReferenceDelegate createReferenceFactory)
    {
        var refA = createReferenceFactory("A");
        var b = createDepFactory("B", dependencies: refA);
        var a = createRootFactory("A", dependencies: b);
        return (a, null);
    }

    public static (IMod Mod, IList<IModReference>? ExpectedTraversedList) ScenarioM(
        ModFactoryDelegate createRootFactory,
        ModFactoryDelegate createDepFactory,
        ModReferenceDelegate createReferenceFactory)
    {
        var refA = createReferenceFactory("A");
        var e = createDepFactory("E", dependencies: refA);
        var d = createDepFactory("D", dependencies: e);
        var c = createDepFactory("C");
        var b = createDepFactory("B", dependencies: [c, d]);
        var a = createRootFactory("A", dependencies: b);
        return (a, null);
    }

    public static (IMod Mod, IList<IModReference>? ExpectedTraversedList) ScenarioNoDependencies(
        ModFactoryDelegate createRootFactory,
        ModFactoryDelegate createDepFactory)
    {
        var a = createRootFactory("A", dependencies: []);
        return (a, [a]);
    }

    public static (IMod Mod, IList<IModReference>? ExpectedTraversedList) ScenarioSingleDependency(
        ModFactoryDelegate createRootFactory,
        ModFactoryDelegate createDepFactory)
    {
        var b = createDepFactory("B");
        var a = createRootFactory("A", dependencies: b);
        return (a, [a, b]);
    }

    public static (IMod Mod, IList<IModReference>? ExpectedTraversedList) ScenarioSingleDependencyFullResolved(
        ModFactoryDelegate createRootFactory,
        ModFactoryDelegate createDepFactory)
    {
        var c = createDepFactory("C");
        var b = createDepFactory("B", dependencies: c);
        var a = createRootFactory("A", DependencyResolveLayout.FullResolved, dependencies: b);
        return (a, [a, b]);
    }

    public static (IMod Mod, IList<IModReference>? ExpectedTraversedList) ScenarioResolveLast_WithDependency(
        ModFactoryDelegate createRootFactory,
        ModFactoryDelegate createDepFactory)
    {
        var d = createDepFactory("D");
        var c = createDepFactory("C");
        var b = createDepFactory("B", dependencies: c);
        var a = createRootFactory("A", DependencyResolveLayout.ResolveLastItem, dependencies: [d, b]);
        return (a, [a, d, b, c]);
    }

    public static (IMod Mod, IList<IModReference>? ExpectedTraversedList) ScenarioResolveLast_WithOutDependency(
        ModFactoryDelegate createRootFactory,
        ModFactoryDelegate createDepFactory)
    {
        var d = createDepFactory("D");
        var c = createDepFactory("C");
        var b = createDepFactory("B", dependencies: c);
        var a = createRootFactory("A", DependencyResolveLayout.ResolveLastItem, dependencies: [b, d]);
        return (a, [a, b, d]);
    }

    public static (IMod Mod, IList<IModReference>? ExpectedTraversedList) ScenarioResolveLast_Transitive_WithDependency(
        ModFactoryDelegate createRootFactory,
        ModFactoryDelegate createDepFactory)
    {
        var e = createDepFactory("E");
        var d = createDepFactory("D");
        var c = createDepFactory("C", DependencyResolveLayout.FullResolved, d);
        var b = createDepFactory("B", DependencyResolveLayout.ResolveLastItem, e, c);
        var a = createRootFactory("A", DependencyResolveLayout.ResolveRecursive, b);
        return (a, [a, b, e, c, d]);
    }

    public static (IMod Mod, IList<IModReference>? ExpectedTraversedList) ScenarioResolveLast_Transitive_WithOutDependency(
        ModFactoryDelegate createRootFactory,
        ModFactoryDelegate createDepFactory)
    {
        var e = createDepFactory("E");
        var d = createDepFactory("D");
        var c = createDepFactory("C", DependencyResolveLayout.FullResolved, d);
        var b = createDepFactory("B", DependencyResolveLayout.ResolveLastItem, c, e);
        var a = createRootFactory("A", DependencyResolveLayout.ResolveRecursive, b);
        return (a, [a, b, c, e]);
    }
}