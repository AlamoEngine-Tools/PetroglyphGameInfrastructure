using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.IO.Abstractions;
using System.Text.Json;
using AET.Modinfo;
using AET.Modinfo.Model;
using AET.Modinfo.Spec;
using AET.Modinfo.Spec.Steam;
using AET.Modinfo.Utilities;
using AET.Testing.Extensions;
using Microsoft.Extensions.DependencyInjection;
using PG.StarWarsGame.Infrastructure.Games;
using PG.StarWarsGame.Infrastructure.Mods;
using PG.StarWarsGame.Infrastructure.Services;
using PG.StarWarsGame.Infrastructure.Services.Name;
using PG.StarWarsGame.Infrastructure.Testing;
using PG.StarWarsGame.Infrastructure.Testing.Installations;
using PG.StarWarsGame.Infrastructure.Testing.Installations.Game;
using PG.StarWarsGame.Infrastructure.Testing.TestBases;
using Semver;
using Xunit;

namespace PG.StarWarsGame.Infrastructure.Test.ModServices;

public class ModFactoryTest : GameInfrastructureTestBase
{
    private const string DefaultModName = "Default Mod Name";
    private const string SteamModName = "Steam Mod Name";

    private readonly ModFactory _factory;
    private readonly CustomModNameResolver _nameResolver = new();

    public ModFactoryTest()
    {
        _factory = new ModFactory(ServiceProvider);
    }

    protected override void SetupServices(IServiceCollection serviceCollection)
    {
        base.SetupServices(serviceCollection);
        serviceCollection.AddSingleton<IModNameResolver>(_ => _nameResolver);
    }

    #region CreatePhysicalMod

    [Fact]
    public void CreatePhysicalMod_NullArgs_Throws()
    {
        var gameInstallation = GetOrCreateGameInstallation();
        var game = gameInstallation.Game;
        var modData = CreateDetectedModReference(gameInstallation, FileSystem.DirectoryInfo.New("path"), null, null);

        Assert.Throws<ArgumentNullException>(() => _factory.CreatePhysicalMod(null!, modData, CultureInfo.CurrentCulture));
        Assert.Throws<ArgumentNullException>(() => _factory.CreatePhysicalMod(game, null!, CultureInfo.CurrentCulture));
        Assert.Throws<ArgumentNullException>(() => _factory.CreatePhysicalMod(game, modData, null!));
    }

    [Fact]
    public void CreatePhysicalMod_VirtualMod_Throws()
    {
        var game = GetOrCreateGameInstallation().Game;
        var modDir = FileSystem.DirectoryInfo.New("path");
        modDir.Create();
        var modData = new DetectedModReference(new ModReference("SOME_ID", ModType.Virtual), modDir, null);
        Assert.Throws<NotSupportedException>(() => _factory.CreatePhysicalMod(game, modData, CultureInfo.CurrentCulture));
    }

    [Fact]
    public void CreatePhysicalMod_ModDirectoryNotFound_Throws()
    {
        var gameInstallation = GetOrCreateGameInstallation();
        var game = gameInstallation.Game;
        var modData = CreateDetectedModReference(gameInstallation, FileSystem.DirectoryInfo.New("path"), null, null);
        modData.Directory.Delete(true);
        Assert.Throws<DirectoryNotFoundException>(() => _factory.CreatePhysicalMod(game, modData, CultureInfo.CurrentCulture));
    }

    [Theory]
    [MemberData(nameof(GITestUtilities.RealGameIdentities), MemberType = typeof(GITestUtilities))]
    public void CreatePhysicalMod_FromModsDir_WithoutModinfo(GameIdentity gameIdentity)
    {
        var gameInstallation = GetOrCreateGameInstallation(gameIdentity);
        var game = gameInstallation.Game;
        var modDir = gameInstallation.GetModDirectory("Mod_Name", false);

        var modData = CreateDetectedModReference(gameInstallation, modDir, false, null);
        var mod = _factory.CreatePhysicalMod(game, modData, CultureInfo.CurrentCulture);

        Assert.Null(mod.ModInfo);
        Assert.Equal(modData.ModReference.Identifier, mod.Identifier);
        Assert.Equal(modData.ModReference.Type, mod.Type);
        Assert.Equal(DefaultModName, mod.Name);
    }

    [Theory]
    [MemberData(nameof(GITestUtilities.RealGameIdentities), MemberType = typeof(GITestUtilities))]
    public void CreatePhysicalMod_FromModsDir_WithModinfo(GameIdentity gameIdentity)
    {
        var gameInstallation = GetOrCreateGameInstallation(gameIdentity);
        var game = gameInstallation.Game;
        var modDir = gameInstallation.GetModDirectory("Mod_Name", false);

        var modinfo = new ModinfoData("MyMod");

        var modData = CreateDetectedModReference(gameInstallation, modDir, false, modinfo);
        var mod = _factory.CreatePhysicalMod(game, modData, CultureInfo.CurrentCulture);

        Assert.Same(modinfo, mod.ModInfo);
        Assert.Equal(modData.ModReference.Identifier, mod.Identifier);
        Assert.Equal(modData.ModReference.Type, mod.Type);
        Assert.Equal("MyMod", mod.Name);
    }

    [Theory]
    [InlineData(GameType.Eaw)]
    [InlineData(GameType.Foc)]
    public void CreatePhysicalMod_Steam_WithoutModinfo(GameType gameType)
    {
        var gameInstallation = GetOrCreateGameInstallation(new GameIdentity(gameType, GamePlatform.SteamGold));
        var game = gameInstallation.Game;
        var modDir = gameInstallation.GetModDirectory("Mod_Name", true);

        var modinfo = new ModinfoData("MyMod");

        var modRef = CreateDetectedModReference(gameInstallation, modDir, true, modinfo);
        var mod = _factory.CreatePhysicalMod(game, modRef, CultureInfo.CurrentCulture);

        Assert.Same(modinfo, mod.ModInfo);
        Assert.Equal(modRef.ModReference.Identifier, mod.Identifier);
        Assert.Equal(modRef.ModReference.Type, mod.Type);
        Assert.Equal(modinfo.Name, mod.Name);
    }

    [Theory]
    [InlineData(GameType.Eaw)]
    [InlineData(GameType.Foc)]
    public void CreatePhysicalMod_Steam_WithModinfo(GameType gameType)
    {
        var gameInstallation = GetOrCreateGameInstallation(new GameIdentity(gameType, GamePlatform.SteamGold));
        var game = gameInstallation.Game;
        var modDir = gameInstallation.GetModDirectory("Mod_Name", true);

        var modRef = CreateDetectedModReference(gameInstallation, modDir, true, null);
        var mod = _factory.CreatePhysicalMod(game, modRef, CultureInfo.CurrentCulture);

        Assert.Null(mod.ModInfo);
        Assert.Equal(modRef.ModReference.Identifier, mod.Identifier);
        Assert.Equal(modRef.ModReference.Type, mod.Type);
        Assert.Equal(SteamModName, mod.Name);
    }

    [Theory]
    [MemberData(nameof(GITestUtilities.RealGameIdentities), MemberType = typeof(GITestUtilities))]
    public void CreatePhysicalMod_WithInvalidModinfo(GameIdentity gameIdentity)
    {
        var gameInstallation = GetOrCreateGameInstallation(gameIdentity);
        var game = gameInstallation.Game;

        var ws = GITestUtilities.GetRandomWorkshopFlag(game);
        var modDir = gameInstallation.GetModDirectory("Mod_Name", ws);

        var invalidModinfo = new CustomModInfo(string.Empty); // string.Empty is not valid
        var modData = CreateDetectedModReference(gameInstallation, modDir, ws, invalidModinfo);

        Assert.Throws<ModinfoException>(() => _factory.CreatePhysicalMod(game, modData, CultureInfo.CurrentCulture));
    }

    [Theory]
    [MemberData(nameof(GITestUtilities.RealGameIdentities), MemberType = typeof(GITestUtilities))]
    public void CreatePhysicalMod_InvalidNameResolved_Throws(GameIdentity gameIdentity)
    {
        var gameInstallation = GetOrCreateGameInstallation(gameIdentity);
        var game = gameInstallation.Game;

        var ws = GITestUtilities.GetRandomWorkshopFlag(game);
        var modDir = gameInstallation.GetModDirectory("Mod_Name", ws);
        var modData = CreateDetectedModReference(gameInstallation, modDir, ws, null);

        _nameResolver.ReturnCorrectName = false;
        Assert.Throws<ModException>(() => _factory.CreatePhysicalMod(game, modData, CultureInfo.CurrentCulture));
    }

    [Theory]
    [MemberData(nameof(GITestUtilities.RealGameIdentities), MemberType = typeof(GITestUtilities))]
    public void CreatePhysicalMod_ModNotCompatible_Throws(GameIdentity gameIdentity)
    {
        var game = GetOrCreateGameInstallation(gameIdentity).Game;
        
        var otherGameInstallation = GameInfrastructureTesting
            .Game(new GameIdentity(gameIdentity.Type.Opposite(), Random.Item(GITestUtilities.RealPlatforms)), ServiceProvider);

        var modDir = otherGameInstallation.GetModDirectory("Mod_Name", false);
        var modData = CreateDetectedModReference(otherGameInstallation, modDir, false, null);

        Assert.Throws<ModException>(() => _factory.CreatePhysicalMod(game, modData, CultureInfo.CurrentCulture));
    }

    #endregion

    #region CreateVirtualMod

    [Fact]
    public void CreateVirtualMod_NullArgs_Throws()
    {
        var gameInstallation = GetOrCreateGameInstallation();
        var game = gameInstallation.Game;
        var dep = gameInstallation.InstallMod("dep", false).Mod;

        Assert.Throws<ArgumentNullException>(() => _factory.CreateVirtualMod(null!, new ModinfoData("Name")
        {
            Dependencies = new DependencyList(new List<IModReference> { dep }, DependencyResolveLayout.FullResolved)
        }));
        Assert.Throws<ArgumentNullException>(() => _factory.CreateVirtualMod(game, null!));
    }

    [Theory]
    [MemberData(nameof(GITestUtilities.RealGameIdentities), MemberType = typeof(GITestUtilities))]
    public void CreateVirtualMod_WithInvalidModinfo(GameIdentity gameIdentity)
    {
        var gameInstallation = GetOrCreateGameInstallation(gameIdentity);
        var game = gameInstallation.Game;
        var dep = gameInstallation.InstallMod("dep", false).Mod;

        var invalidModinfo = new CustomModInfo(string.Empty)
        {
            Dependencies = new DependencyList(new List<IModReference> { dep }, DependencyResolveLayout.FullResolved)
        }; // string.Empty is not valid

        Assert.Throws<ModinfoException>(() => _factory.CreateVirtualMod(game, invalidModinfo));
    }

    [Theory]
    [MemberData(nameof(GITestUtilities.RealGameIdentities), MemberType = typeof(GITestUtilities))]
    public void CreateVirtualMod_WithInvalidModinfo_NoDependencies(GameIdentity gameIdentity)
    {
        var game = GetOrCreateGameInstallation(gameIdentity).Game;

        var invalidModinfo = new CustomModInfo("MyVirtualMod")
        {
            Dependencies = new DependencyList([], DependencyResolveLayout.FullResolved)
        };

        Assert.Throws<ModException>(() => _factory.CreateVirtualMod(game, invalidModinfo));
    }

    [Theory]
    [MemberData(nameof(GITestUtilities.RealGameIdentities), MemberType = typeof(GITestUtilities))]
    public void CreateVirtualMod(GameIdentity gameIdentity)
    {
        var gameInstallation = GetOrCreateGameInstallation(gameIdentity);
        var game = gameInstallation.Game;
        var dep = gameInstallation.InstallMod("dep", false).Mod;

        var modinfo = new CustomModInfo("VirtualModName")
        {
            Dependencies = new DependencyList(new List<IModReference> { dep }, DependencyResolveLayout.FullResolved)
        }; // string.Empty is not valid

        var mod = _factory.CreateVirtualMod(game, modinfo);

        Assert.Same(modinfo, mod.ModInfo);
        Assert.Equal("VirtualModName", mod.Name);
        Assert.Equal(ModReferenceBuilder.CreateVirtualModIdentifier(modinfo).Identifier, mod.Identifier);
        Assert.Equal(ModType.Virtual, mod.Type);
        Assert.Single(((IModIdentity)mod).Dependencies);
    }

    #endregion

    protected DetectedModReference CreateDetectedModReference(ITestingGameInstallation gameInstallation, IDirectoryInfo path, bool? isWorkshop, IModinfo? modinfo)
    {
        var workshop = isWorkshop ?? GITestUtilities.GetRandomWorkshopFlag(gameInstallation.Game);
        var mod = gameInstallation.InstallMod(new ModinfoData("MyMod"), path, workshop).Mod;
        return new DetectedModReference(new ModReference(mod), mod.Directory, modinfo);
    }

    private class CustomModNameResolver : IModNameResolver
    {
        public bool ReturnCorrectName { get; set; } = true;

        public string ResolveName(DetectedModReference detectedMod, CultureInfo culture)
        {
            if (!ReturnCorrectName)
            {
                if (new Random().Next() % 2 == 0)
                    return string.Empty;
                return null!;
            }
            return detectedMod.ModReference.Type switch
            {
                ModType.Default => DefaultModName,
                ModType.Workshops => SteamModName,
                _ => "VirtualModName"
            };
        }
    }

    private class CustomModInfo(string name) : IModinfo
    {
        public bool Equals(IModIdentity? other)
        {
            throw new NotImplementedException();
        }

        public string Name { get; } = name;
        public SemVersion? Version => null;
        public IModDependencyList Dependencies { get; init; } = DependencyList.EmptyDependencyList;
        public string ToJson()
        {
            return JsonSerializer.Serialize(this, JsonSerializerOptions.Default);
        }

        public void ToJson(Stream stream)
        {
            JsonSerializer.Serialize(stream, this, JsonSerializerOptions.Default);
        }

        public string? Summary => null;
        public string? Icon => null;
        public IDictionary<string, object> Custom { get; } = new Dictionary<string, object>();
        public ISteamData? SteamData => null;
        public IReadOnlyCollection<ILanguageInfo> Languages { get; } = new List<ILanguageInfo>();
        public bool LanguagesExplicitlySet => false;
    }
}

