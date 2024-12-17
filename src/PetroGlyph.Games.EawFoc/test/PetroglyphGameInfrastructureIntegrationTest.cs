using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using AnakinRaW.CommonUtilities.Registry;
using EawModinfo.Model;
using EawModinfo.Spec;
using EawModinfo.Spec.Equality;
using EawModinfo.Utilities;
using Microsoft.Extensions.DependencyInjection;
using PG.StarWarsGame.Infrastructure.Games;
using PG.StarWarsGame.Infrastructure.Games.Registry;
using PG.StarWarsGame.Infrastructure.Mods;
using PG.StarWarsGame.Infrastructure.Services;
using PG.StarWarsGame.Infrastructure.Services.Dependencies;
using PG.StarWarsGame.Infrastructure.Services.Detection;
using PG.StarWarsGame.Infrastructure.Services.Steam;
using PG.StarWarsGame.Infrastructure.Testing;
using PG.StarWarsGame.Infrastructure.Testing.Game.Installation;
using PG.StarWarsGame.Infrastructure.Testing.Game.Registry;
using PG.StarWarsGame.Infrastructure.Testing.Mods;
using Semver;
using Xunit;

namespace PG.StarWarsGame.Infrastructure.Test;

public class PetroglyphGameInfrastructureIntegrationTest : CommonTestBase
{
    private readonly IRegistry _registry = new InMemoryRegistry(InMemoryRegistryCreationFlags.WindowsLike);

    private const string ExternalModPath = "externalMods/MyExternalMod";

    protected override void SetupServiceProvider(IServiceCollection sc)
    {
        base.SetupServiceProvider(sc);
        sc.AddSingleton(_registry);
    }

    [Theory]
    [MemberData(nameof(RealPlatforms))]
    public void FullWorkflow_WithGamesAndMultipleModsWithMultipleModsDependencies(GamePlatform platform)
    {
        // Init Mods uninitialized

        var eaw = FileSystem.InstallGame(new GameIdentity(GameType.Eaw, platform), ServiceProvider);
        var foc = FileSystem.InstallGame(new GameIdentity(GameType.Foc, platform), ServiceProvider);

        TestGameRegistrySetupData.Uninitialized(GameType.Eaw).Create(ServiceProvider); 
        TestGameRegistrySetupData.Uninitialized(GameType.Foc).Create(ServiceProvider);

        var registryFactory = ServiceProvider.GetRequiredService<IGameRegistryFactory>();
        var eawRegistry = registryFactory.CreateRegistry(GameType.Eaw);
        var focRegistry = registryFactory.CreateRegistry(GameType.Foc);

        // Test Game Creation
        
        var gameDetector = new RegistryGameDetector(eawRegistry, focRegistry, true, ServiceProvider);
        var initCount = 0;
        gameDetector.InitializationRequested += (_, args) =>
        {
            if (args.GameType == GameType.Eaw)
                TestGameRegistrySetupData.Installed(GameType.Eaw, eaw.Directory).Create(ServiceProvider);
            else
                TestGameRegistrySetupData.Installed(GameType.Foc, foc.Directory).Create(ServiceProvider);
            args.Handled = true;
            initCount++;
        };

        var eawFindResult = gameDetector.Detect(GameType.Eaw, platform);
        var focFindResult = gameDetector.Detect(GameType.Foc, platform);
        
        Assert.True(eawFindResult.Installed);
        Assert.True(focFindResult.Installed);
        Assert.Equal(2, initCount);

        var gameFactory = ServiceProvider.GetRequiredService<IGameFactory>();
        var actualEaw = gameFactory.CreateGame(eawFindResult, CultureInfo.CurrentCulture);
        var actualFoc = gameFactory.CreateGame(focFindResult, CultureInfo.CurrentCulture);
        AssertExpectedGame(eaw, actualEaw);
        AssertExpectedGame(foc, actualFoc);

        // Init Mods
        CreateExternalMod(actualFoc);
        if (platform == GamePlatform.SteamGold)
            CreateAndAddSteamScenario(actualFoc);
        Eaw_CreateModInModsDir(eaw);


        // Test Mod detection

        var modFinder = ServiceProvider.GetRequiredService<IModFinder>();

        var modsFromExternalSource = modFinder.FindMods(actualFoc, FileSystem.DirectoryInfo.New(ExternalModPath)).ToList();
        AssertModFinderResult(modsFromExternalSource,
        [
            new DetectedModReference(
                new ModReference(FileSystem.DirectoryInfo.New(ExternalModPath).FullName, ModType.Default),
                FileSystem.DirectoryInfo.New(ExternalModPath),
                null)
        ]);

        var eawModsSource = modFinder.FindMods(actualEaw).ToList();
        
        // Note that Raw and Raw Sub mod are not added, because they are known mod/have modinfo data
        AssertModFinderResult(eawModsSource,
        [
            new DetectedModReference(
                new ModReference("In-Mods-Dir", ModType.Default),
                FileSystem.DirectoryInfo.New(FileSystem.Path.Combine(actualEaw.ModsLocation.FullName, "In-Mods-Dir")),
                null)
        ]);

        var focModsSource = modFinder.FindMods(actualFoc).ToList();
        if (platform is not GamePlatform.SteamGold)
            AssertModFinderResult(focModsSource, []);
        else
        {
            var steamHelper = ServiceProvider.GetRequiredService<ISteamGameHelpers>();
            var steamDir = steamHelper.GetWorkshopsLocation(actualFoc).FullName;

            AssertModFinderResult(focModsSource,
            [
                new DetectedModReference(new ModReference("1129810972", ModType.Workshops), 
                    FileSystem.DirectoryInfo.New(FileSystem.Path.Combine(steamDir, "1129810972")), null),
                new DetectedModReference(new ModReference("123456", ModType.Workshops), 
                    FileSystem.DirectoryInfo.New(FileSystem.Path.Combine(steamDir, "123456")),
                    new ModinfoData("Republic at War Sub Mod")
                    {
                        Version = new SemVersion(1, 0, 0),
                        Dependencies = new DependencyList(new List<IModReference>
                        {
                            new ModReference("1129810972", ModType.Workshops)
                        }, DependencyResolveLayout.ResolveRecursive)
                    })
            ]);
        }


        // Test Mod Creation

        var externalModReference = modsFromExternalSource.First();
        var eawModRef = eawModsSource.First();

        var modFactory = ServiceProvider.GetRequiredService<IModFactory>();

        // External Mod
        var externalMod = modFactory.CreatePhysicalMod(actualFoc, externalModReference, CultureInfo.CurrentCulture);
        AssertModAddToGame(actualFoc, externalMod, m =>
        { 
            Assert.IsAssignableFrom<IPhysicalMod>(m);
            Assert.Null(m.ModInfo);
            Assert.Equal(externalMod.Directory.FullName, m.Identifier);
            Assert.Equal(externalMod.Directory.Name, m.Name);
            Assert.Equal(DependencyResolveStatus.Resolved, m.DependencyResolveStatus); // Auto-resolved cause it has no deps
            Assert.Equal(ModType.Default, m.Type);
        });
        // Check that the external mod could also be created for eaw
        Assert.Null(Record.Exception(() => modFactory.CreatePhysicalMod(actualEaw, externalModReference, CultureInfo.CurrentCulture)));

        // Eaw Mod in Mods dir
        Assert.Throws<ModException>(() => modFactory.CreatePhysicalMod(actualFoc, eawModRef, CultureInfo.CurrentCulture));
        var eawMod = modFactory.CreatePhysicalMod(actualEaw, eawModRef, CultureInfo.CurrentCulture);
        AssertModAddToGame(actualEaw, eawMod, m =>
        {
            Assert.IsAssignableFrom<IPhysicalMod>(m);
            Assert.Null(m.ModInfo);
            Assert.Equal(eawMod.Directory.Name, m.Identifier);
            Assert.Equal("In Mods Dir", m.Name);
            Assert.Equal(DependencyResolveStatus.Resolved, m.DependencyResolveStatus); // Auto-resolved cause it has no deps
            Assert.Equal(ModType.Default, m.Type);
        });

        // Foc Mods
        foreach (var modReference in focModsSource)
        {
            var focMod = modFactory.CreatePhysicalMod(actualFoc, modReference, CultureInfo.CurrentCulture);
            AssertModAddToGame(actualFoc, focMod, m =>
            {
                Assert.IsAssignableFrom<IPhysicalMod>(m);
                Assert.Equal(ModType.Workshops, m.Type);
                Assert.Equal(focMod.Directory.Name, m.Identifier);
                if (m.Identifier.Equals("1129810972"))
                {
                    Assert.Null(m.ModInfo);
                    Assert.Equal("Republic at War", m.Name);
                    Assert.Equal(DependencyResolveStatus.Resolved, m.DependencyResolveStatus); // Auto-resolved cause it has no deps
                }
                else if (m.Identifier == "123456")
                {
                    Assert.Same(modReference.Modinfo, focMod.ModInfo);
                    Assert.Equal("Republic at War Sub Mod", m.Name);
                    Assert.Equal(DependencyResolveStatus.None, m.DependencyResolveStatus);
                }
            });
        }


        // Assert Mod Dependency Resolve

        foreach (var mod in actualFoc.Mods.Concat(actualEaw.Mods)) 
            mod.ResolveDependencies();
        Assert.All(actualFoc.Mods.Concat(actualEaw.Mods), x => Assert.Equal(DependencyResolveStatus.Resolved, x.DependencyResolveStatus));

        AssertTraversedDependencies(externalMod, [externalMod]);
        AssertTraversedDependencies(eawMod, [eawMod]);

        if (platform is GamePlatform.SteamGold)
        {
            var raw = actualFoc.FindMod(new ModReference("1129810972", ModType.Workshops));
            Assert.NotNull(raw);
            AssertTraversedDependencies(raw, [raw]);

            var rawSubMod = actualFoc.FindMod(new ModReference("123456", ModType.Workshops));
            Assert.NotNull(rawSubMod);
            AssertTraversedDependencies(rawSubMod, [rawSubMod, raw]);
        }
    }

    private void CreateAndAddSteamScenario(IGame actualFoc)
    {
        Assert.Equal(GamePlatform.SteamGold, actualFoc.Platform);
        var steamHelper = ServiceProvider.GetRequiredService<ISteamGameHelpers>();

        var republicAtWarDir = FileSystem.Path.Combine(steamHelper.GetWorkshopsLocation(actualFoc).FullName, "1129810972"); // This is RaW's Steam ID
        actualFoc.InstallMod(FileSystem.DirectoryInfo.New(republicAtWarDir), true, new ModinfoData("NOT USED"), ServiceProvider);


        var rawSubModDir = FileSystem.Path.Combine(steamHelper.GetWorkshopsLocation(actualFoc).FullName, "123456"); // Just some ID
        var rawSubMod = actualFoc.InstallMod(FileSystem.DirectoryInfo.New(rawSubModDir), true,
            new ModinfoData("NOT USED"), ServiceProvider);


        var rawSubModModinfoContent = @"
{
    ""name"":""Republic at War Sub Mod"",
    ""version"":""1.0.0"",
    ""dependencies"": [
        {
            ""modtype"":1,
            ""identifier"":""1129810972""
        }
    ],
    ""steamdata"":{
        ""publishedfileid"": ""123456"",
        ""contentfolder"": ""folder"",
        ""visibility"": 1,
        ""title"": ""Raw Sub Mod"",
        ""tags"":[
          ""FOC"",
          ""Singleplayer""
        ],
    }
}";
        rawSubMod.InstallModinfoFile(ModinfoData.Parse(rawSubModModinfoContent));
    }

    private void AssertModFinderResult(ICollection<DetectedModReference> finderResult,
        ICollection<DetectedModReference> expectedResult)
    {
        Assert.Equal(expectedResult.Count, finderResult.Count);

        foreach (var expectedRef in expectedResult) 
            Assert.Contains(expectedRef, finderResult, DetectedModReferenceEqualityComparer.Instance);
    }


    private void AssertTraversedDependencies(IMod mod, IList<IMod> expectedDepList)
    {
        var traverser = ServiceProvider.GetRequiredService<IModDependencyTraverser>();
        var depList = traverser.Traverse(mod);
        Assert.Equal(expectedDepList, depList);
    }

    private void AssertExpectedGame(PetroglyphStarWarsGame expected, IGame actual)
    {
        Assert.Equal(expected.Directory.FullName, actual.Directory.FullName);
        Assert.Equal(expected.Type, actual.Type);
        Assert.Equal(expected.Platform, actual.Platform);
    }

    private void AssertModAddToGame(IGame game, IMod mod, Action<IMod> assertAction)
    {
        Assert.Null(game.FindMod(new ModReference(mod)));
        Assert.True(game.AddMod(mod));
        Assert.Same(mod, game.FindMod(new ModReference(mod)));

        assertAction(mod);
    }

    private void CreateExternalMod(IGame game)
    {
        var modinfo = new ModinfoData("NAME NOT TAKE CAUSE NO MODINFO"); 
        game.InstallMod(FileSystem.DirectoryInfo.New("externalMods/MyExternalMod"), false, modinfo, ServiceProvider);
    }


    private void Eaw_CreateModInModsDir(IGame game)
    {
        game.InstallMod("In-Mods-Dir", false, ServiceProvider);
    }

    private class DetectedModReferenceEqualityComparer : IEqualityComparer<DetectedModReference>
    {
        public static readonly DetectedModReferenceEqualityComparer Instance = new();

        public bool Equals(DetectedModReference? x, DetectedModReference? y)
        {
            if (ReferenceEquals(x, y)) 
                return true;
            if (x is null) 
                return false;
            if (y is null) 
                return false;
            if (!ModIdentityEqualityComparer.Default.Equals(x.Modinfo, y.Modinfo))
                return false;
            if (!x.Directory.FullName.Equals(y.Directory.FullName))
                return false;
            if (!x.ModReference.Equals(y.ModReference))
                return false;
            return true;
        }

        public int GetHashCode(DetectedModReference obj)
        {
            return HashCode.Combine(obj.Directory.FullName, ModIdentityEqualityComparer.Default.GetHashCode(obj.Modinfo), obj.ModReference);
        }
    }
}