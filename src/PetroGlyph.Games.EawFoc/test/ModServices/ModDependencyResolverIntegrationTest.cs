using System;
using System.Collections.Generic;
using System.IO.Abstractions;
using System.Linq;
using EawModinfo.Model;
using EawModinfo.Spec;
using Microsoft.Extensions.DependencyInjection;
using PetroGlyph.Games.EawFoc;
using PetroGlyph.Games.EawFoc.Games;
using PetroGlyph.Games.EawFoc.Mods;
using PetroGlyph.Games.EawFoc.Services.Dependencies;
using Semver;
using Testably.Abstractions.Testing;
using Xunit;

namespace PG.StarWarsGame.Infrastructure.Test.ModServices;

public class ModDependencyResolverIntegrationTest
{
    private readonly IGame _game;
    private readonly MockFileSystem _fileSystem;
    private readonly IServiceProvider _serviceProvider;

    public ModDependencyResolverIntegrationTest()
    {
        _fileSystem = new MockFileSystem();
        var sc = new ServiceCollection();
        sc.AddSingleton<IFileSystem>(_fileSystem);
        sc.AddSingleton<IModDependencyGraphBuilder>(new ModDependencyGraphBuilder());
        PetroglyphGameInfrastructureLibrary.InitializeLibraryWithDefaultServices(sc);
        _serviceProvider = sc.BuildServiceProvider();
        _game = SetupGame(_fileSystem, _serviceProvider);
    }

    [Fact]
    public void TestNoDependencies()
    {
        _fileSystem.Initialize().WithFile("Game/Mods/Target");
        var targetMod = new TestMod(_game, _fileSystem.DirectoryInfo.New("Game/Mods/Target"), false, "Name", _serviceProvider);

        var resolver = new ModDependencyResolver(_serviceProvider);

        Assert.Equal(DependencyResolveStatus.None, targetMod.DependencyResolveStatus);

        targetMod.ResolveDependencies(resolver, new DependencyResolverOptions());

        Assert.Empty(targetMod.Dependencies);
        Assert.Equal(DependencyResolveStatus.Resolved, targetMod.DependencyResolveStatus);
        Assert.Equal(DependencyResolveLayout.ResolveRecursive, targetMod.DependencyResolveLayout);
    }


    [Fact]
    public void TestResolveDependency()
    {
        _fileSystem.Initialize()
            .WithSubdirectory("Game/Mods/Target")
            .WithSubdirectory("Game/Mods/dep");

        var targetInfo = new ModinfoData("Name")
        {
            Dependencies = new DependencyList(new List<IModReference>
            {
                new ModReference(_fileSystem.Path.GetFullPath("Game/Mods/dep"), ModType.Default, SemVersionRange.Parse("1.*"))
            }, DependencyResolveLayout.ResolveRecursive)
        };

        var targetMod = new TestMod(_game, _fileSystem.DirectoryInfo.New("Game/Mods/Target"), false, targetInfo, _serviceProvider);

        var depMod = new TestMod(_game, _fileSystem.DirectoryInfo.New("Game/Mods/dep"), false, "Name", _serviceProvider);


        _game.AddMod(targetMod);
        _game.AddMod(depMod);

        var resolver = new ModDependencyResolver(_serviceProvider);

        Assert.Equal(DependencyResolveStatus.None, targetMod.DependencyResolveStatus);

        targetMod.ResolveDependencies(resolver, new DependencyResolverOptions());

        Assert.Single(targetMod.Dependencies);
        Assert.Equal(depMod, targetMod.Dependencies.First().Mod);
        Assert.Equal(SemVersionRange.Parse("1.*"), targetMod.Dependencies.First().VersionRange);
        Assert.Equal(DependencyResolveStatus.Resolved, targetMod.DependencyResolveStatus);
        Assert.Equal(DependencyResolveLayout.ResolveRecursive, targetMod.DependencyResolveLayout);

        Assert.Equal(DependencyResolveStatus.None, depMod.DependencyResolveStatus);
    }

    [Fact]
    public void TestResolveDependencyIgnoreCycle()
    {
        _fileSystem.Initialize()
            .WithSubdirectory("Game/Mods/Target")
            .WithSubdirectory("Game/Mods/dep");

        var targetInfo = new ModinfoData("Name")
        {
            Dependencies = new DependencyList(new List<IModReference>
            {
                new ModReference(_fileSystem.Path.GetFullPath("Game/Mods/dep"), ModType.Default, SemVersionRange.Parse("1.*"))
            }, DependencyResolveLayout.ResolveRecursive)
        };

        var targetMod = new TestMod(_game, _fileSystem.DirectoryInfo.New("Game/Mods/Target"), false, targetInfo, _serviceProvider);

        var depMod = new TestMod(_game, _fileSystem.DirectoryInfo.New("Game/Mods/dep"), false, "Name", _serviceProvider);
        depMod.SetStatus(DependencyResolveStatus.Resolved);
        depMod.DependencyAction(l => l.Add(new ModDependencyEntry(targetMod)));


        _game.AddMod(targetMod);
        _game.AddMod(depMod);

        var resolver = new ModDependencyResolver(_serviceProvider);

        Assert.Equal(DependencyResolveStatus.None, targetMod.DependencyResolveStatus);

        targetMod.ResolveDependencies(resolver, new DependencyResolverOptions { ResolveCompleteChain = true });

        Assert.Single(targetMod.Dependencies);
        Assert.Equal(DependencyResolveStatus.Resolved, targetMod.DependencyResolveStatus);
        Assert.Equal(DependencyResolveLayout.ResolveRecursive, targetMod.DependencyResolveLayout);
    }

    [Fact]
    public void TestResolveDependencyCycle_Throws()
    {
        _fileSystem.Initialize()
            .WithSubdirectory("Game/Mods/Target")
            .WithSubdirectory("Game/Mods/dep");

        var targetInfo = new ModinfoData("Name")
        {
            Dependencies = new DependencyList(new List<IModReference>
            {
                new ModReference(_fileSystem.Path.GetFullPath("Game/Mods/dep"), ModType.Default, SemVersionRange.Parse("1.*"))
            }, DependencyResolveLayout.ResolveRecursive)
        };

        var targetMod = new TestMod(_game, _fileSystem.DirectoryInfo.New("Game/Mods/Target"), false, targetInfo, _serviceProvider);

        var depMod = new TestMod(_game, _fileSystem.DirectoryInfo.New("Game/Mods/dep"), false, "Name", _serviceProvider);
        depMod.SetStatus(DependencyResolveStatus.Resolved);
        depMod.DependencyAction(l => l.Add(new ModDependencyEntry(targetMod)));


        _game.AddMod(targetMod);
        _game.AddMod(depMod);

        var resolver = new ModDependencyResolver(_serviceProvider);

        Assert.Equal(DependencyResolveStatus.None, targetMod.DependencyResolveStatus);

        Assert.Throws<ModDependencyCycleException>(() => targetMod.ResolveDependencies(resolver, new DependencyResolverOptions { CheckForCycle = true }));

        Assert.Empty(targetMod.Dependencies);
        Assert.Equal(DependencyResolveStatus.Faulted, targetMod.DependencyResolveStatus);
        Assert.Equal(DependencyResolveLayout.ResolveRecursive, targetMod.DependencyResolveLayout);
    }

    [Fact]
    public void TestResolveDependencyChain()
    {
        _fileSystem.Initialize()
            .WithSubdirectory("Game/Mods/Target")
            .WithSubdirectory("Game/Mods/dep")
            .WithSubdirectory("Game/Mods/subdep");

        var targetInfo = new ModinfoData("Name")
        {
            Dependencies = new DependencyList(new List<IModReference>
            {
                new ModReference(_fileSystem.Path.GetFullPath("Game/Mods/dep"), ModType.Default, SemVersionRange.Parse("1.*"))
            }, DependencyResolveLayout.ResolveRecursive)
        };

        var targetMod = new TestMod(_game, _fileSystem.DirectoryInfo.New("Game/Mods/Target"), false, targetInfo, _serviceProvider);

        var depInfo = new ModinfoData("Dep")
        {
            Dependencies = new DependencyList(new List<IModReference>
            {
                new ModReference(_fileSystem.Path.GetFullPath("Game/Mods/subdep"), ModType.Default)
            }, DependencyResolveLayout.ResolveRecursive)
        };

        var depMod = new TestMod(_game, _fileSystem.DirectoryInfo.New("Game/Mods/dep"), false, depInfo, _serviceProvider);

        var subDepMod = new TestMod(_game, _fileSystem.DirectoryInfo.New("Game/Mods/subdep"), false, "SubDep", _serviceProvider);


        _game.AddMod(targetMod);
        _game.AddMod(depMod);
        _game.AddMod(subDepMod);

        var resolver = new ModDependencyResolver(_serviceProvider);

        Assert.Equal(DependencyResolveStatus.None, targetMod.DependencyResolveStatus);

        targetMod.ResolveDependencies(resolver, new DependencyResolverOptions { ResolveCompleteChain = true });

        Assert.Single(targetMod.Dependencies);
        Assert.Equal(depMod, targetMod.Dependencies.First().Mod);
        Assert.Equal(SemVersionRange.Parse("1.*"), targetMod.Dependencies.First().VersionRange);
        Assert.Equal(DependencyResolveStatus.Resolved, targetMod.DependencyResolveStatus);
        Assert.Equal(DependencyResolveLayout.ResolveRecursive, targetMod.DependencyResolveLayout);

        Assert.Equal(DependencyResolveStatus.Resolved, depMod.DependencyResolveStatus);
        Assert.Single(depMod.Dependencies);
        Assert.Equal(subDepMod, depMod.Dependencies.First().Mod);

        Assert.Equal(DependencyResolveStatus.Resolved, subDepMod.DependencyResolveStatus);
    }

    [Fact]
    public void TestResolveDependencyChainLayoutLastItem()
    {
        _fileSystem.Initialize()
            .WithSubdirectory("Game/Mods/Target")
            .WithSubdirectory("Game/Mods/dep")
            .WithSubdirectory("Game/Mods/dep2")
            .WithSubdirectory("Game/Mods/subdep");

        var targetInfo = new ModinfoData("Name")
        {
            Dependencies = new DependencyList(new List<IModReference>
            {
                new ModReference(_fileSystem.Path.GetFullPath("Game/Mods/dep"), ModType.Default, SemVersionRange.Parse("1.*")),
                new ModReference(_fileSystem.Path.GetFullPath("Game/Mods/dep2"), ModType.Default)
            }, DependencyResolveLayout.ResolveLastItem)
        };

        var targetMod = new TestMod(_game, _fileSystem.DirectoryInfo.New("Game/Mods/Target"), false, targetInfo, _serviceProvider);
        targetMod.SetLayout(DependencyResolveLayout.ResolveLastItem);

        IModinfo depInfo = new ModinfoData("Dep")
        {
            Dependencies = new DependencyList(new List<IModReference>
            {
                new ModReference(_fileSystem.Path.GetFullPath("Game/Mods/subdep"), ModType.Default)
            }, DependencyResolveLayout.ResolveRecursive)
        };

        var depMod = new TestMod(_game, _fileSystem.DirectoryInfo.New("Game/Mods/dep"), false, depInfo, _serviceProvider);

        var dep2Mod = new TestMod(_game, _fileSystem.DirectoryInfo.New("Game/Mods/dep2"), false, "Dep2", _serviceProvider);

        var subDepMod = new TestMod(_game, _fileSystem.DirectoryInfo.New("Game/Mods/subdep"), false, "SubDep", _serviceProvider);


        _game.AddMod(targetMod);
        _game.AddMod(depMod);
        _game.AddMod(dep2Mod);
        _game.AddMod(subDepMod);

        var resolver = new ModDependencyResolver(_serviceProvider);

        Assert.Equal(DependencyResolveStatus.None, targetMod.DependencyResolveStatus);

        targetMod.ResolveDependencies(resolver, new DependencyResolverOptions { ResolveCompleteChain = true });

        Assert.Equal(2, targetMod.Dependencies.Count);
        Assert.Equal(depMod, targetMod.Dependencies.First().Mod);
        Assert.Equal(SemVersionRange.Parse("1.*"), targetMod.Dependencies.First().VersionRange);
        Assert.Equal(DependencyResolveStatus.Resolved, targetMod.DependencyResolveStatus);
        Assert.Equal(DependencyResolveLayout.ResolveLastItem, targetMod.DependencyResolveLayout);

        Assert.Equal(DependencyResolveStatus.Resolved, dep2Mod.DependencyResolveStatus);
        Assert.Equal(DependencyResolveStatus.None, depMod.DependencyResolveStatus);
        Assert.Equal(DependencyResolveStatus.None, subDepMod.DependencyResolveStatus);
    }

    [Fact]
    public void TestResolveDependencyChainLayoutFullResolved()
    {
        _fileSystem.Initialize()
            .WithSubdirectory("Game/Mods/Target")
            .WithSubdirectory("Game/Mods/dep")
            .WithSubdirectory("Game/Mods/dep2")
            .WithSubdirectory("Game/Mods/subdep");

        var targetInfo = new ModinfoData("Name")
        {
            Dependencies = new DependencyList(new List<IModReference>
            {
                new ModReference(_fileSystem.Path.GetFullPath("Game/Mods/dep"), ModType.Default, SemVersionRange.Parse("1.*")),
                new ModReference(_fileSystem.Path.GetFullPath("Game/Mods/dep2"), ModType.Default)
            }, DependencyResolveLayout.FullResolved)
        };

        var targetMod = new TestMod(_game, _fileSystem.DirectoryInfo.New("Game/Mods/Target"), false, targetInfo, _serviceProvider);
        targetMod.SetLayout(DependencyResolveLayout.FullResolved);

        var depInfo = new ModinfoData("Dep")
        {
            Dependencies = new DependencyList(new List<IModReference>
            {
                new ModReference(_fileSystem.Path.GetFullPath("Game/Mods/subdep"), ModType.Default)
            }, DependencyResolveLayout.ResolveRecursive)
        };

        var depMod = new TestMod(_game, _fileSystem.DirectoryInfo.New("Game/Mods/dep"), false, depInfo, _serviceProvider);

        var dep2Mod = new TestMod(_game, _fileSystem.DirectoryInfo.New("Game/Mods/dep2"), false, "Dep2", _serviceProvider);

        var subDepMod = new TestMod(_game, _fileSystem.DirectoryInfo.New("Game/Mods/subdep"), false, "SubDep", _serviceProvider);


        _game.AddMod(targetMod);
        _game.AddMod(depMod);
        _game.AddMod(dep2Mod);
        _game.AddMod(subDepMod);

        var resolver = new ModDependencyResolver(_serviceProvider);

        Assert.Equal(DependencyResolveStatus.None, targetMod.DependencyResolveStatus);

        targetMod.ResolveDependencies(resolver, new DependencyResolverOptions { ResolveCompleteChain = true });

        Assert.Equal(2, targetMod.Dependencies.Count);
        Assert.Equal(depMod, targetMod.Dependencies.First().Mod);
        Assert.Equal(SemVersionRange.Parse("1.*"), targetMod.Dependencies.First().VersionRange);
        Assert.Equal(DependencyResolveStatus.Resolved, targetMod.DependencyResolveStatus);
        Assert.Equal(DependencyResolveLayout.FullResolved, targetMod.DependencyResolveLayout);

        Assert.Equal(DependencyResolveStatus.None, dep2Mod.DependencyResolveStatus);
        Assert.Equal(DependencyResolveStatus.None, depMod.DependencyResolveStatus);
        Assert.Equal(DependencyResolveStatus.None, subDepMod.DependencyResolveStatus);
    }


    private static IGame SetupGame(MockFileSystem fileSystem, IServiceProvider sp)
    {
        fileSystem.Initialize()
            .WithFile("Game/swfoc.exe");
        var game = new PetroglyphStarWarsGame(new GameIdentity(GameType.Foc, GamePlatform.Disk),
            fileSystem.DirectoryInfo.New("Game"), "Foc", sp);
        return game;
    }

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