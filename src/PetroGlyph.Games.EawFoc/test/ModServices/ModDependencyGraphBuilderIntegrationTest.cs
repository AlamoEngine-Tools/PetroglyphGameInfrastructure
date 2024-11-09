using System;
using System.Collections.Generic;
using System.IO.Abstractions;
using EawModinfo.Spec;
using Microsoft.Extensions.DependencyInjection;
using PG.StarWarsGame.Infrastructure.Games;
using PG.StarWarsGame.Infrastructure.Mods;
using PG.StarWarsGame.Infrastructure.Services.Dependencies;
using PG.StarWarsGame.Infrastructure.Testing;
using PG.StarWarsGame.Infrastructure.Testing.Game.Installation;
using PG.StarWarsGame.Infrastructure.Testing.Mods;
using Xunit;

namespace PG.StarWarsGame.Infrastructure.Test.ModServices;

public class ModDependencyGraphBuilderIntegrationTest : CommonTestBase
{
    [Theory]
    [MemberData(nameof(RealGameIdentities))]
    public void Test_OnlySelf(GameIdentity identity)
    {
        var game = FileSystem.InstallGame(identity, ServiceProvider);

        var targetMod = game.InstallAndAddMod("Target", false, ServiceProvider);

        var resolver = ServiceProvider.GetRequiredService<IDependencyResolver>();
        targetMod.ResolveDependencies(resolver, new DependencyResolverOptions());
        
        var graphBuilder = new ModDependencyGraphBuilder();

        var graph = graphBuilder.Build(targetMod);
        Assert.Single(graph);
    }

    //[Fact]
    //public void Test_Cycle1()
    //{
    //    var fs = new MockFileSystem();
    //    var sp = new Mock<IServiceProvider>();
    //    var game = SetupGame(fs, sp.Object);

    //    fs.Initialize()
    //        .WithSubdirectory("Game/Mods/Target");

    //    var targetMod = new TestMod(game, fs.DirectoryInfo.New("Game/Mods/Target"), false, "Name", sp.Object);
    //    targetMod.SetStatus(DependencyResolveStatus.Resolved);

    //    targetMod.DependencyAction(list => list.Add(new(targetMod)));

    //    var graphBuilder = new ModDependencyGraphBuilder();

    //    var graph = graphBuilder.Build(targetMod);
    //    Assert.Single(graph);
    //    Assert.True(graph.HasCycle());
    //}

    //[Fact]
    //public void Test_Cycle2()
    //{
    //    var fs = new MockFileSystem();
    //    var sp = new Mock<IServiceProvider>();
    //    var game = SetupGame(fs, sp.Object);

    //    fs.Initialize()
    //        .WithSubdirectory("Game/Mods/Target")
    //        .WithSubdirectory("Game/Mods/dep");

    //    var targetMod = new TestMod(game, fs.DirectoryInfo.New("Game/Mods/Target"), false, "Name", sp.Object);
    //    targetMod.SetStatus(DependencyResolveStatus.Resolved);

    //    var depMod = new TestMod(game, fs.DirectoryInfo.New("Game/Mods/dep"), false, "Name", sp.Object);
    //    depMod.SetStatus(DependencyResolveStatus.Resolved);
    //    depMod.DependencyAction(list => list.Add(new(targetMod)));

    //    // Cycle
    //    targetMod.DependencyAction(list => list.Add(new(depMod)));

    //    var graphBuilder = new ModDependencyGraphBuilder();

    //    var graph = graphBuilder.Build(targetMod);
    //    Assert.Equal(2, graph.Count());
    //    Assert.True(graph.HasCycle());
    //}

    //[Fact]
    //public void Test_LeftRightOrder1()
    //{
    //    var fs = new MockFileSystem();
    //    var sp = new Mock<IServiceProvider>();
    //    var game = SetupGame(fs, sp.Object);

    //    fs.Initialize()
    //        .WithSubdirectory("Game/Mods/Target")
    //        .WithSubdirectory("Game/Mods/dep1")
    //        .WithSubdirectory("Game/Mods/dep2")
    //        .WithSubdirectory("Game/Mods/dep3")
    //        .WithSubdirectory("Game/Mods/dep4");

    //    var dep1 = new TestMod(game, fs.DirectoryInfo.New("Game/Mods/dep1"), false, "Name", sp.Object);
    //    dep1.SetStatus(DependencyResolveStatus.Resolved);

    //    var dep2 = new TestMod(game, fs.DirectoryInfo.New("Game/Mods/dep2"), false, "Name", sp.Object);
    //    dep2.SetStatus(DependencyResolveStatus.Resolved);

    //    var dep3 = new TestMod(game, fs.DirectoryInfo.New("Game/Mods/dep3"), false, "Name", sp.Object);
    //    dep3.SetStatus(DependencyResolveStatus.Resolved);

    //    var dep4 = new TestMod(game, fs.DirectoryInfo.New("Game/Mods/dep4"), false, "Name", sp.Object);
    //    dep4.SetStatus(DependencyResolveStatus.Resolved);

    //    var targetMod = new TestMod(game, fs.DirectoryInfo.New("Game/Mods/Target"), false, "Name", sp.Object);
    //    targetMod.SetStatus(DependencyResolveStatus.Resolved);

    //    targetMod.DependencyAction(list =>
    //    {
    //        list.Add(new(dep1));
    //        list.Add(new(dep2));
    //        list.Add(new(dep3));
    //        list.Add(new(dep4));
    //    });

    //    var graphBuilder = new ModDependencyGraphBuilder();

    //    var graph = graphBuilder.Build(targetMod);
    //    Assert.Equal(5, graph.Count());
    //    var deps = graph.DependenciesOf(targetMod);

    //    Assert.Equal(new List<ModDependencyEntry> { new(dep1), new(dep2), new(dep3), new(dep4) }, deps);
    //}

    //[Fact]
    //public void Test_LeftRightOrder2()
    //{
    //    var fs = new MockFileSystem();
    //    var sp = new Mock<IServiceProvider>();
    //    var game = SetupGame(fs, sp.Object);

    //    fs.Initialize()
    //        .WithSubdirectory("Game/Mods/Target")
    //        .WithSubdirectory("Game/Mods/dep1")
    //        .WithSubdirectory("Game/Mods/dep2")
    //        .WithSubdirectory("Game/Mods/dep3")
    //        .WithSubdirectory("Game/Mods/dep4");

    //    var dep1 = new TestMod(game, fs.DirectoryInfo.New("Game/Mods/dep1"), false, "Name", sp.Object);
    //    dep1.SetStatus(DependencyResolveStatus.Resolved);

    //    var dep2 = new TestMod(game, fs.DirectoryInfo.New("Game/Mods/dep2"), false, "Name", sp.Object);
    //    dep2.SetStatus(DependencyResolveStatus.Resolved);

    //    var dep3 = new TestMod(game, fs.DirectoryInfo.New("Game/Mods/dep3"), false, "Name", sp.Object);
    //    dep3.SetStatus(DependencyResolveStatus.Resolved);

    //    var dep4 = new TestMod(game, fs.DirectoryInfo.New("Game/Mods/dep4"), false, "Name", sp.Object);
    //    dep4.SetStatus(DependencyResolveStatus.Resolved);


    //    var targetMod = new TestMod(game, fs.DirectoryInfo.New("Game/Mods/Target"), false, "Name", sp.Object);
    //    targetMod.SetStatus(DependencyResolveStatus.Resolved);

    //    targetMod.DependencyAction(list =>
    //    {
    //        list.Add(new(dep4));
    //        list.Add(new(dep3));
    //        list.Add(new(dep2));
    //        list.Add(new(dep1));
    //    });

    //    var graphBuilder = new ModDependencyGraphBuilder();

    //    var graph = graphBuilder.Build(targetMod);
    //    Assert.Equal(5, graph.Count());
    //    var deps = graph.DependenciesOf(targetMod);

    //    Assert.Equal(new List<ModDependencyEntry> { new(dep4), new(dep3), new(dep2), new(dep1) }, deps);
    //}

    //[Fact]
    //public void Test_LeftRightOrder3()
    //{
    //    var fs = new MockFileSystem();
    //    var sp = new Mock<IServiceProvider>();
    //    var game = SetupGame(fs, sp.Object);

    //    fs.Initialize()
    //        .WithSubdirectory("Game/Mods/Target")
    //        .WithSubdirectory("Game/Mods/dep1")
    //        .WithSubdirectory("Game/Mods/dep2")
    //        .WithSubdirectory("Game/Mods/dep3")
    //        .WithSubdirectory("Game/Mods/dep4")
    //        .WithSubdirectory("Game/Mods/dep5");

    //    // Target: 1, 2
    //    // 2: 3, 4, 5

    //    var dep1 = new TestMod(game, fs.DirectoryInfo.New("Game/Mods/dep1"), false, "Name", sp.Object);
    //    dep1.SetStatus(DependencyResolveStatus.Resolved);

    //    var dep2 = new TestMod(game, fs.DirectoryInfo.New("Game/Mods/dep2"), false, "Name", sp.Object);
    //    dep2.SetStatus(DependencyResolveStatus.Resolved);

    //    var dep3 = new TestMod(game, fs.DirectoryInfo.New("Game/Mods/dep3"), false, "Name", sp.Object);
    //    dep3.SetStatus(DependencyResolveStatus.Resolved);

    //    var dep4 = new TestMod(game, fs.DirectoryInfo.New("Game/Mods/dep4"), false, "Name", sp.Object);
    //    dep4.SetStatus(DependencyResolveStatus.Resolved);

    //    var dep5 = new TestMod(game, fs.DirectoryInfo.New("Game/Mods/dep5"), false, "Name", sp.Object);
    //    dep5.SetStatus(DependencyResolveStatus.Resolved);

    //    dep2.DependencyAction(list =>
    //    {
    //        list.Add(new(dep3));
    //        list.Add(new(dep4));
    //        list.Add(new(dep5));
    //    });

    //    var targetMod = new TestMod(game, fs.DirectoryInfo.New("Game/Mods/Target"), false, "Name", sp.Object);
    //    targetMod.SetStatus(DependencyResolveStatus.Resolved);

    //    targetMod.DependencyAction(list =>
    //    {
    //        list.Add(new(dep1));
    //        list.Add(new(dep2));
    //    });

    //    var graphBuilder = new ModDependencyGraphBuilder();

    //    var graph = graphBuilder.Build(targetMod);
    //    Assert.Equal(6, graph.Count());
    //    var depsOf2 = graph.DependenciesOf(dep2);

    //    Assert.Equal(new List<ModDependencyEntry> { new(dep3), new(dep4), new(dep5) }, depsOf2);
    //}


    //private static IGame SetupGame(MockFileSystem fileSystem, IServiceProvider sp)
    //{
    //    fileSystem.Initialize().WithFile("Game/swfoc.exe");
    //    var game = new PetroglyphStarWarsGame(new GameIdentity(GameType.Foc, GamePlatform.Disk),
    //        fileSystem.DirectoryInfo.New("Game"), "Foc", sp);
    //    return game;
    //}

    private class TestMod : Mod
    {
        private DependencyResolveLayout _layout;

        public override DependencyResolveLayout DependencyResolveLayout => _layout;

        public TestMod(IGame game, IDirectoryInfo modDirectory, bool workshop, IModinfo modInfoData, IServiceProvider serviceProvider) : base(game, modDirectory, workshop, modInfoData, serviceProvider)
        {
        }

        public TestMod(IGame game, IDirectoryInfo modDirectory, bool workshop, string name, IServiceProvider serviceProvider) : base(game, modDirectory, workshop, name, serviceProvider)
        {
        }

        public void DependencyAction(Action<IList<ModDependencyEntry>> action)
        {
            action(DependenciesInternal);
        }

        public void SetStatus(DependencyResolveStatus status)
        {
            DependencyResolveStatus = status;
        }

        public void SetLayout(DependencyResolveLayout layout)
        {
            _layout = layout;
        }
    }
}