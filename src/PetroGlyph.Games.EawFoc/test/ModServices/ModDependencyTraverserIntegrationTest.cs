using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO.Abstractions;
using System.Linq;
using EawModinfo.Spec;
using Microsoft.Extensions.DependencyInjection;
using PG.StarWarsGame.Infrastructure.Games;
using PG.StarWarsGame.Infrastructure.Mods;
using PG.StarWarsGame.Infrastructure.Services.Dependencies;
using PG.StarWarsGame.Infrastructure.Services.Detection;
using PG.StarWarsGame.Infrastructure.Services.Steam;
using Testably.Abstractions.Testing;
using Xunit;

namespace PG.StarWarsGame.Infrastructure.Test.ModServices;

public class ModDependencyTraverserIntegrationTest
{
    private readonly MockFileSystem _fileSystem;
    private readonly IGame _game;
    private readonly IServiceProvider _serviceProvider;

    public ModDependencyTraverserIntegrationTest()
    {
        _fileSystem = new MockFileSystem();
        var sc = new ServiceCollection();
        sc.AddSingleton<IModIdentifierBuilder>(sp => new ModIdentifierBuilder(sp));
        sc.AddSingleton<IFileSystem>(_fileSystem);
        sc.AddSingleton<ISteamGameHelpers>(sp => new SteamGameHelpers(sp));
        _serviceProvider = sc.BuildServiceProvider();
        _game = SetupGame(_fileSystem, _serviceProvider);
    }


    [Fact]
    public void TestTraverse_LayoutLastItem()
    {
        var targetMod = CreateMod("Target");
        var depMod = CreateMod("dep");
        var dep2Mod = CreateMod("dep2");
        var subDepMod = CreateMod("subdep");
        var subDep2Mod = CreateMod("SubDep2");

        targetMod.SetDependencies(depMod, dep2Mod);
        targetMod.SetLayout(DependencyResolveLayout.ResolveLastItem);

        depMod.SetDependencies(subDepMod);
        dep2Mod.SetDependencies(subDep2Mod);

        var traverser = new ModDependencyTraverser(_serviceProvider);
        var actual = traverser.Traverse(targetMod);

        var expected = new List<IMod> { targetMod, depMod, dep2Mod, subDep2Mod }.Select(m => new ModDependencyEntry(m)).ToList();

        Assert.Equal(expected, actual);
    }

    [Fact]
    public void TestTraverse_LayoutFullResolved()
    {
        var targetMod = CreateMod("Target");
        var depMod = CreateMod("dep");
        var dep2Mod = CreateMod("dep2");
        var subDepMod = CreateMod("subdep");

        targetMod.SetDependencies(depMod, dep2Mod);
        targetMod.SetLayout(DependencyResolveLayout.FullResolved);
        depMod.SetDependencies(subDepMod);

        var traverser = new ModDependencyTraverser(_serviceProvider);
        var actual = traverser.Traverse(targetMod);

        var expected = new List<IMod> { targetMod, depMod, dep2Mod }.Select(m => new ModDependencyEntry(m)).ToList();

        Assert.Equal(expected, actual);
    }

    [Fact]
    public void TestTraverse_LayoutRecursive_ModinfoSpec_A()
    {
        var a = CreateMod("A");
        var b = CreateMod("B");
        var c = CreateMod("C");
        var d = CreateMod("D");
        var e = CreateMod("E");

        a.SetDependencies(b, c);
        b.SetDependencies(d);
        c.SetDependencies(e);

        var traverser = new ModDependencyTraverser(_serviceProvider);
        var actual = traverser.Traverse(a);

        var expected = new List<IMod> { a, b, c, d, e }.Select(m => new ModDependencyEntry(m)).ToList();

        Assert.Equal(expected, actual);
    }

    [Fact]
    public void TestTraverse_LayoutRecursive_ModinfoSpec_B()
    {
        var a = CreateMod("A");
        var b = CreateMod("B");
        var c = CreateMod("C");
        var d = CreateMod("D");
        var e = CreateMod("E");

        a.SetDependencies(c, b);
        b.SetDependencies(d);
        c.SetDependencies(e);

        var traverser = new ModDependencyTraverser(_serviceProvider);
        var actual = traverser.Traverse(a);

        var expected = new List<IMod> { a, c, b, e, d }.Select(m => new ModDependencyEntry(m)).ToList();

        Assert.Equal(expected, actual);
    }

    [Fact]
    public void TestTraverse_LayoutRecursive_ModinfoSpec_C()
    {
        var a = CreateMod("A");
        var b = CreateMod("B");
        var c = CreateMod("C");
        var d = CreateMod("D");
        var e = CreateMod("E");

        a.SetDependencies(b, c);
        b.SetDependencies(d);
        c.SetDependencies(d);
        d.SetDependencies(e);

        var traverser = new ModDependencyTraverser(_serviceProvider);
        var actual = traverser.Traverse(a);

        var expected = new List<IMod> { a, b, c, d, e }.Select(m => new ModDependencyEntry(m)).ToList();
        Assert.Equal(expected, actual);
    }

    [Fact]
    public void TestTraverse_LayoutRecursive_ModinfoSpec_D()
    {
        var a = CreateMod("A");
        var b = CreateMod("B");
        var c = CreateMod("C");
        var d = CreateMod("D");
        var e = CreateMod("E");

        a.SetDependencies(b, c, d);
        b.SetDependencies(e);
        c.SetDependencies(e);
        d.SetDependencies(e);

        var traverser = new ModDependencyTraverser(_serviceProvider);
        var actual = traverser.Traverse(a);

        var expected = new List<IMod> { a, b, c, d, e }.Select(m => new ModDependencyEntry(m)).ToList();
        Assert.Equal(expected, actual);
    }

    [Fact]
    public void TestTraverse_LayoutRecursive_ModinfoSpec_E()
    {
        var a = CreateMod("A");
        var b = CreateMod("B");
        var c = CreateMod("C");
        var d = CreateMod("D");
        var e = CreateMod("E");

        a.SetDependencies(b, c, d);
        b.SetDependencies(e);
        c.SetDependencies(d);
        e.SetDependencies(d);

        var traverser = new ModDependencyTraverser(_serviceProvider);
        var actual = traverser.Traverse(a);

        var expected = new List<IMod> { a, b, c, e, d }.Select(m => new ModDependencyEntry(m)).ToList();
        Assert.Equal(expected, actual);
    }

    [Fact]
    public void TestTraverse_LayoutRecursive_ModinfoSpec_F()
    {
        var a = CreateMod("A");
        var b = CreateMod("B");
        var c = CreateMod("C");
        var d = CreateMod("D");
        var e = CreateMod("E");

        a.SetDependencies(b, c, d);
        b.SetDependencies(d);
        c.SetDependencies(e);
        e.SetDependencies(d);

        var traverser = new ModDependencyTraverser(_serviceProvider);
        var actual = traverser.Traverse(a);

        var expected = new List<IMod> { a, b, c, e, d }.Select(m => new ModDependencyEntry(m)).ToList();
        Assert.Equal(expected, actual);
    }

    [Fact]
    public void TestTraverse_LayoutRecursive_ModinfoSpec_G()
    {
        var a = CreateMod("A");
        var b = CreateMod("B");
        var c = CreateMod("C");
        var d = CreateMod("D");
        var e = CreateMod("E");
        var f = CreateMod("F");
        var g = CreateMod("G");

        a.SetDependencies(b, c, d, e, f, g);

        var traverser = new ModDependencyTraverser(_serviceProvider);
        var actual = traverser.Traverse(a);

        var expected = new List<IMod> { a, b, c, d, e, f, g }.Select(m => new ModDependencyEntry(m)).ToList();
        Assert.Equal(expected, actual);
    }

    [Fact]
    public void TestTraverse_LayoutRecursive_ModinfoSpec_H()
    {
        var a = CreateMod("A");
        var b = CreateMod("B");
        var c = CreateMod("C");
        var d = CreateMod("D");
        var e = CreateMod("E");
        var f = CreateMod("F");
        var g = CreateMod("G");
        var i = CreateMod("I");

        a.SetDependencies(b, c);
        b.SetDependencies(d);
        c.SetDependencies(g);
        d.SetDependencies(e, f);
        g.SetDependencies(i);
        f.SetDependencies(i);

        var traverser = new ModDependencyTraverser(_serviceProvider);
        var actual = traverser.Traverse(a);

        var expected = new List<IMod> { a, b, c, d, g, e, f, i }.Select(m => new ModDependencyEntry(m)).ToList();
        Assert.Equal(expected, actual);
    }

    [Fact]
    public void TestTraverse_LayoutRecursive_ModinfoSpec_I()
    {
        var a = CreateMod("A");
        var b = CreateMod("B");
        var c = CreateMod("C");
        var d = CreateMod("D");
        var e = CreateMod("E");
        var f = CreateMod("F");
        var x = CreateMod("X");

        a.SetDependencies(c, b);
        b.SetDependencies(e);
        c.SetDependencies(d);
        d.SetDependencies(f);
        e.SetDependencies(x);
        x.SetDependencies(d, f);

        var traverser = new ModDependencyTraverser(_serviceProvider);
        var actual = traverser.Traverse(a);

        var expected = new List<IMod> { a, c, b, e, x, d, f }.Select(m => new ModDependencyEntry(m)).ToList();
        Assert.Equal(expected, actual);
    }

    [Fact]
    public void TestTraverse_LayoutRecursive_ModinfoSpec_J()
    {
        var a = CreateMod("A");
        var b = CreateMod("B");
        var c = CreateMod("C");
        var d = CreateMod("D");
        var e = CreateMod("E");
        var f = CreateMod("F");
        var x = CreateMod("X");

        a.SetDependencies(b, c, d);
        b.SetDependencies(x);
        c.SetDependencies(x, f);
        d.SetDependencies(e);
        e.SetDependencies(x, f);

        var traverser = new ModDependencyTraverser(_serviceProvider);
        var actual = traverser.Traverse(a);

        var expected = new List<IMod> { a, b, c, d, e, x, f }.Select(m => new ModDependencyEntry(m)).ToList();
        Assert.Equal(expected, actual);
    }

    [Fact]
    public void TestTraverse_LayoutRecursive_ModinfoSpec_K_Throws()
    {
        var a = CreateMod("A");
        a.SetDependencies(a);
        var traverser = new ModDependencyTraverser(_serviceProvider);
        Assert.Throws<ModDependencyCycleException>(() => traverser.Traverse(a));
    }

    [Fact]
    public void TestTraverse_LayoutRecursive_ModinfoSpec_L_Throws()
    {
        var a = CreateMod("A");
        var b = CreateMod("B");
        a.SetDependencies(b);
        b.SetDependencies(a);
        var traverser = new ModDependencyTraverser(_serviceProvider);
        Assert.Throws<ModDependencyCycleException>(() => traverser.Traverse(a));
    }

    [Fact]
    public void TestTraverse_LayoutRecursive_ModinfoSpec_M_Throws()
    {
        var a = CreateMod("A");
        var b = CreateMod("B");
        var c = CreateMod("C");
        var d = CreateMod("D");
        var e = CreateMod("E");
        a.SetDependencies(b);
        b.SetDependencies(c, d);
        d.SetDependencies(e);
        e.SetDependencies(a);
        var traverser = new ModDependencyTraverser(_serviceProvider);
        Assert.Throws<ModDependencyCycleException>(() => traverser.Traverse(a));
    }

    private TestMod CreateMod(string name)
    {
        _fileSystem.Initialize().WithSubdirectory($"Game/Mods/{name}");
        return new TestMod(_game, _fileSystem.DirectoryInfo.New($"Game/Mods/{name}"), false, name, _serviceProvider);
    }

    private static IGame SetupGame(MockFileSystem fileSystem, IServiceProvider sp)
    {
        fileSystem.Initialize().WithFile("Game/swfoc.exe");
        var game = new PetroglyphStarWarsGame(new GameIdentity(GameType.Foc, GamePlatform.Disk),
            fileSystem.DirectoryInfo.New("Game"), "Foc", sp);
        return game;
    }

    [DebuggerDisplay("Name = {Name}")]
    private class TestMod : Mod
    {
        private DependencyResolveLayout _layout;

        public override DependencyResolveLayout DependencyResolveLayout => _layout;

        public TestMod(IGame game, IDirectoryInfo modDirectory, bool workshop, string name, IServiceProvider serviceProvider) : base(game, modDirectory, workshop, name, serviceProvider)
        {
            DependencyResolveStatus = DependencyResolveStatus.Resolved;
        }

        private void DependencyAction(Action<IList<ModDependencyEntry>> action)
        {
            action(DependenciesInternal);
        }

        public void SetLayout(DependencyResolveLayout layout)
        {
            _layout = layout;
        }

        public void SetDependencies(params IMod[] mods)
        {
            DependencyAction(dl =>
            {
                foreach (var dep in mods)
                {
                    dl.Add(new ModDependencyEntry(dep));
                }
            });
        }
    }
}