using System;
using System.Collections.Generic;
using System.IO.Abstractions;
using System.IO.Abstractions.TestingHelpers;
using System.Linq;
using EawModinfo.Model;
using EawModinfo.Spec;
using Microsoft.Extensions.DependencyInjection;
using PetroGlyph.Games.EawFoc.Games;
using PetroGlyph.Games.EawFoc.Mods;
using PetroGlyph.Games.EawFoc.Services.Dependencies;
using Xunit;
using Range = SemanticVersioning.Range;

namespace PetroGlyph.Games.EawFoc.Test.ModServices
{
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
            PetroglyphGameInfrastructureLibrary.InitializeLibraryWithDefaultServices(sc);
            _serviceProvider = sc.BuildServiceProvider();
            _game = SetupGame(_fileSystem, _serviceProvider);
        }

        [Fact]
        public void TestNoDependencies()
        {
            _fileSystem.AddDirectory("Game/Mods/Target");
            var targetMod = new TestMod(_game, _fileSystem.DirectoryInfo.FromDirectoryName("Game/Mods/Target"), false, "Name", _serviceProvider);

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
            _fileSystem.AddDirectory("Game/Mods/Target");

            IModinfo targetInfo;
            if (TestUtils.IsUnixLikePlatform)
            {
                targetInfo = new ModinfoData("Name")
                {
                    Dependencies = new DependencyList(new List<IModReference>()
                    {
                        new ModReference("/Game/Mods/dep", ModType.Default, Range.Parse("1.x"))
                    }, DependencyResolveLayout.ResolveRecursive)
                };
            }
            else
            {
                targetInfo = new ModinfoData("Name")
                {
                    Dependencies = new DependencyList(new List<IModReference>()
                    {
                        new ModReference("C:\\Game\\Mods\\dep", ModType.Default, Range.Parse("1.x"))
                    }, DependencyResolveLayout.ResolveRecursive)
                };
            }

            var targetMod = new TestMod(_game, _fileSystem.DirectoryInfo.FromDirectoryName("Game/Mods/Target"), false, targetInfo, _serviceProvider);

            _fileSystem.AddDirectory("Game/Mods/dep");
            var depMod = new TestMod(_game, _fileSystem.DirectoryInfo.FromDirectoryName("Game/Mods/dep"), false, "Name", _serviceProvider);


            _game.AddMod(targetMod);
            _game.AddMod(depMod);

            var resolver = new ModDependencyResolver(_serviceProvider);

            Assert.Equal(DependencyResolveStatus.None, targetMod.DependencyResolveStatus);

            targetMod.ResolveDependencies(resolver, new DependencyResolverOptions());

            Assert.Single(targetMod.Dependencies);
            Assert.Equal(depMod, targetMod.Dependencies.First().Mod);
            Assert.Equal(Range.Parse("1.x"), targetMod.Dependencies.First().VersionRange);
            Assert.Equal(DependencyResolveStatus.Resolved, targetMod.DependencyResolveStatus);
            Assert.Equal(DependencyResolveLayout.ResolveRecursive, targetMod.DependencyResolveLayout);

            Assert.Equal(DependencyResolveStatus.None, depMod.DependencyResolveStatus);
        }

        [Fact]
        public void TestResolveDependencyIgnoreCycle()
        {
            _fileSystem.AddDirectory("Game/Mods/Target");

            IModinfo targetInfo;
            if (TestUtils.IsUnixLikePlatform)
            {
                targetInfo = new ModinfoData("Name")
                {
                    Dependencies = new DependencyList(new List<IModReference>
                    {
                        new ModReference("/Game/Mods/dep", ModType.Default, Range.Parse("1.x"))
                    }, DependencyResolveLayout.ResolveRecursive)
                };
            }
            else
            {
                targetInfo = new ModinfoData("Name")
                {
                    Dependencies = new DependencyList(new List<IModReference>
                    {
                        new ModReference("C:\\Game\\Mods\\dep", ModType.Default, Range.Parse("1.x"))
                    }, DependencyResolveLayout.ResolveRecursive)
                };
            }

            var targetMod = new TestMod(_game, _fileSystem.DirectoryInfo.FromDirectoryName("Game/Mods/Target"), false, targetInfo, _serviceProvider);

            _fileSystem.AddDirectory("Game/Mods/dep");
            var depMod = new TestMod(_game, _fileSystem.DirectoryInfo.FromDirectoryName("Game/Mods/dep"), false, "Name", _serviceProvider);
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
            _fileSystem.AddDirectory("Game/Mods/Target");
            IModinfo targetInfo;
            if (TestUtils.IsUnixLikePlatform)
            {
                targetInfo = new ModinfoData("Name")
                {
                    Dependencies = new DependencyList(new List<IModReference>
                    {
                        new ModReference("/Game/Mods/dep", ModType.Default, Range.Parse("1.x"))
                    }, DependencyResolveLayout.ResolveRecursive)
                };
            }
            else
            {
                targetInfo = new ModinfoData("Name")
                {
                    Dependencies = new DependencyList(new List<IModReference>
                    {
                        new ModReference("C:\\Game\\Mods\\dep", ModType.Default, Range.Parse("1.x"))
                    }, DependencyResolveLayout.ResolveRecursive)
                };
            }
            var targetMod = new TestMod(_game, _fileSystem.DirectoryInfo.FromDirectoryName("Game/Mods/Target"), false, targetInfo, _serviceProvider);

            _fileSystem.AddDirectory("Game/Mods/dep");
            var depMod = new TestMod(_game, _fileSystem.DirectoryInfo.FromDirectoryName("Game/Mods/dep"), false, "Name", _serviceProvider);
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
            _fileSystem.AddDirectory("Game/Mods/Target");
            IModinfo targetInfo;
            if (TestUtils.IsUnixLikePlatform)
            {
                targetInfo = new ModinfoData("Name")
                {
                    Dependencies = new DependencyList(new List<IModReference>
                    {
                        new ModReference("/Game/Mods/dep", ModType.Default, Range.Parse("1.x"))
                    }, DependencyResolveLayout.ResolveRecursive)
                };
            }
            else
            {
                targetInfo = new ModinfoData("Name")
                {
                    Dependencies = new DependencyList(new List<IModReference>
                    {
                        new ModReference("C:\\Game\\Mods\\dep", ModType.Default, Range.Parse("1.x"))
                    }, DependencyResolveLayout.ResolveRecursive)
                };
            }
            var targetMod = new TestMod(_game, _fileSystem.DirectoryInfo.FromDirectoryName("Game/Mods/Target"), false, targetInfo, _serviceProvider);

            _fileSystem.AddDirectory("Game/Mods/dep");
            IModinfo depInfo;
            if (TestUtils.IsUnixLikePlatform)
            {
                depInfo = new ModinfoData("Dep")
                {
                    Dependencies = new DependencyList(new List<IModReference>
                    {
                        new ModReference("/Game/Mods/subdep", ModType.Default)
                    }, DependencyResolveLayout.ResolveRecursive)
                };
            }
            else
            {
                depInfo = new ModinfoData("Dep")
                {
                    Dependencies = new DependencyList(new List<IModReference>
                    {
                        new ModReference("C:\\Game\\Mods\\subdep", ModType.Default)
                    }, DependencyResolveLayout.ResolveRecursive)
                };
            }
            var depMod = new TestMod(_game, _fileSystem.DirectoryInfo.FromDirectoryName("Game/Mods/dep"), false, depInfo, _serviceProvider);

            _fileSystem.AddDirectory("Game/Mods/subdep");
            var subDepMod = new TestMod(_game, _fileSystem.DirectoryInfo.FromDirectoryName("Game/Mods/subdep"), false, "SubDep", _serviceProvider);


            _game.AddMod(targetMod);
            _game.AddMod(depMod);
            _game.AddMod(subDepMod);

            var resolver = new ModDependencyResolver(_serviceProvider);

            Assert.Equal(DependencyResolveStatus.None, targetMod.DependencyResolveStatus);

            targetMod.ResolveDependencies(resolver, new DependencyResolverOptions { ResolveCompleteChain = true });

            Assert.Single(targetMod.Dependencies);
            Assert.Equal(depMod, targetMod.Dependencies.First().Mod);
            Assert.Equal(Range.Parse("1.x"), targetMod.Dependencies.First().VersionRange);
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
            _fileSystem.AddDirectory("Game/Mods/Target");
            IModinfo targetInfo;
            if (TestUtils.IsUnixLikePlatform)
            {
                targetInfo = new ModinfoData("Name")
                {
                    Dependencies = new DependencyList(new List<IModReference>
                    {
                        new ModReference("/Game/Mods/dep", ModType.Default, Range.Parse("1.x")),
                        new ModReference("/Game/Mods/dep2", ModType.Default)
                    }, DependencyResolveLayout.ResolveLastItem)
                };
            }
            else
            {
                targetInfo = new ModinfoData("Name")
                {
                    Dependencies = new DependencyList(new List<IModReference>
                    {
                        new ModReference("C:\\Game\\Mods\\dep", ModType.Default, Range.Parse("1.x")),
                        new ModReference("C:\\Game\\Mods\\dep2", ModType.Default)
                    }, DependencyResolveLayout.ResolveLastItem)
                };
            }
            var targetMod = new TestMod(_game, _fileSystem.DirectoryInfo.FromDirectoryName("Game/Mods/Target"), false, targetInfo, _serviceProvider);
            targetMod.SetLayout(DependencyResolveLayout.ResolveLastItem);

            _fileSystem.AddDirectory("Game/Mods/dep");
            IModinfo depInfo;
            if (TestUtils.IsUnixLikePlatform)
            {
                depInfo = new ModinfoData("Dep")
                {
                    Dependencies = new DependencyList(new List<IModReference>
                    {
                        new ModReference("/Game/Mods/subdep", ModType.Default)
                    }, DependencyResolveLayout.ResolveRecursive)
                };
            }
            else
            {
                depInfo = new ModinfoData("Dep")
                {
                    Dependencies = new DependencyList(new List<IModReference>
                    {
                        new ModReference("C:\\Game\\Mods\\subdep", ModType.Default)
                    }, DependencyResolveLayout.ResolveRecursive)
                };
            }
            var depMod = new TestMod(_game, _fileSystem.DirectoryInfo.FromDirectoryName("Game/Mods/dep"), false, depInfo, _serviceProvider);

            _fileSystem.AddDirectory("Game/Mods/dep2");
            var dep2Mod = new TestMod(_game, _fileSystem.DirectoryInfo.FromDirectoryName("Game/Mods/dep2"), false, "Dep2", _serviceProvider);

            _fileSystem.AddDirectory("Game/Mods/subdep");
            var subDepMod = new TestMod(_game, _fileSystem.DirectoryInfo.FromDirectoryName("Game/Mods/subdep"), false, "SubDep", _serviceProvider);


            _game.AddMod(targetMod);
            _game.AddMod(depMod);
            _game.AddMod(dep2Mod);
            _game.AddMod(subDepMod);

            var resolver = new ModDependencyResolver(_serviceProvider);

            Assert.Equal(DependencyResolveStatus.None, targetMod.DependencyResolveStatus);

            targetMod.ResolveDependencies(resolver, new DependencyResolverOptions { ResolveCompleteChain = true });

            Assert.Equal(2, targetMod.Dependencies.Count);
            Assert.Equal(depMod, targetMod.Dependencies.First().Mod);
            Assert.Equal(Range.Parse("1.x"), targetMod.Dependencies.First().VersionRange);
            Assert.Equal(DependencyResolveStatus.Resolved, targetMod.DependencyResolveStatus);
            Assert.Equal(DependencyResolveLayout.ResolveLastItem, targetMod.DependencyResolveLayout);

            Assert.Equal(DependencyResolveStatus.Resolved, dep2Mod.DependencyResolveStatus);
            Assert.Equal(DependencyResolveStatus.None, depMod.DependencyResolveStatus);
            Assert.Equal(DependencyResolveStatus.None, subDepMod.DependencyResolveStatus);
        }

        [Fact]
        public void TestResolveDependencyChainLayoutFullResolved()
        {
            _fileSystem.AddDirectory("Game/Mods/Target");
            IModinfo targetInfo;
            if (TestUtils.IsUnixLikePlatform)
            {
                targetInfo = new ModinfoData("Name")
                {
                    Dependencies = new DependencyList(new List<IModReference>
                    {
                        new ModReference("/Game/Mods/dep", ModType.Default, Range.Parse("1.x")),
                        new ModReference("/Game/Mods/dep2", ModType.Default)
                    }, DependencyResolveLayout.FullResolved)
                };
            }
            else
            {
                targetInfo = new ModinfoData("Name")
                {
                    Dependencies = new DependencyList(new List<IModReference>
                    {
                        new ModReference("C:\\Game\\Mods\\dep", ModType.Default, Range.Parse("1.x")),
                        new ModReference("C:\\Game\\Mods\\dep2", ModType.Default)
                    }, DependencyResolveLayout.FullResolved)
                };
            }
            var targetMod = new TestMod(_game, _fileSystem.DirectoryInfo.FromDirectoryName("Game/Mods/Target"), false, targetInfo, _serviceProvider);
            targetMod.SetLayout(DependencyResolveLayout.FullResolved);

            _fileSystem.AddDirectory("Game/Mods/dep");
            IModinfo depInfo;
            if (TestUtils.IsUnixLikePlatform)
            {
                depInfo = new ModinfoData("Dep")
                {
                    Dependencies = new DependencyList(new List<IModReference>
                    {
                        new ModReference("/Game/Mods/subdep", ModType.Default)
                    }, DependencyResolveLayout.ResolveRecursive)
                };
            }
            else
            {
                depInfo = new ModinfoData("Dep")
                {
                    Dependencies = new DependencyList(new List<IModReference>
                    {
                        new ModReference("C:\\Game\\Mods\\subdep", ModType.Default)
                    }, DependencyResolveLayout.ResolveRecursive)
                };
            }
            var depMod = new TestMod(_game, _fileSystem.DirectoryInfo.FromDirectoryName("Game/Mods/dep"), false, depInfo, _serviceProvider);

            _fileSystem.AddDirectory("Game/Mods/dep2");
            var dep2Mod = new TestMod(_game, _fileSystem.DirectoryInfo.FromDirectoryName("Game/Mods/dep2"), false, "Dep2", _serviceProvider);

            _fileSystem.AddDirectory("Game/Mods/subdep");
            var subDepMod = new TestMod(_game, _fileSystem.DirectoryInfo.FromDirectoryName("Game/Mods/subdep"), false, "SubDep", _serviceProvider);


            _game.AddMod(targetMod);
            _game.AddMod(depMod);
            _game.AddMod(dep2Mod);
            _game.AddMod(subDepMod);

            var resolver = new ModDependencyResolver(_serviceProvider);

            Assert.Equal(DependencyResolveStatus.None, targetMod.DependencyResolveStatus);

            targetMod.ResolveDependencies(resolver, new DependencyResolverOptions { ResolveCompleteChain = true });

            Assert.Equal(2, targetMod.Dependencies.Count);
            Assert.Equal(depMod, targetMod.Dependencies.First().Mod);
            Assert.Equal(Range.Parse("1.x"), targetMod.Dependencies.First().VersionRange);
            Assert.Equal(DependencyResolveStatus.Resolved, targetMod.DependencyResolveStatus);
            Assert.Equal(DependencyResolveLayout.FullResolved, targetMod.DependencyResolveLayout);

            Assert.Equal(DependencyResolveStatus.None, dep2Mod.DependencyResolveStatus);
            Assert.Equal(DependencyResolveStatus.None, depMod.DependencyResolveStatus);
            Assert.Equal(DependencyResolveStatus.None, subDepMod.DependencyResolveStatus);
        }


        private static IGame SetupGame(IMockFileDataAccessor fileSystem, IServiceProvider sp)
        {
            fileSystem.AddFile("Game/swfoc.exe", MockFileData.NullObject);
            var game = new PetroglyphStarWarsGame(new GameIdentity(GameType.Foc, GamePlatform.Disk),
                fileSystem.DirectoryInfo.FromDirectoryName("Game"), "Foc", sp);
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
}