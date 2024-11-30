using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using EawModinfo.Model;
using EawModinfo.Spec;
using EawModinfo.Spec.Equality;
using PG.StarWarsGame.Infrastructure.Games;
using PG.StarWarsGame.Infrastructure.Mods;
using PG.StarWarsGame.Infrastructure.Services.Detection;
using PG.StarWarsGame.Infrastructure.Testing;
using PG.StarWarsGame.Infrastructure.Testing.Game.Installation;
using PG.StarWarsGame.Infrastructure.Testing.Mods;
using Semver;
using Xunit;

namespace PG.StarWarsGame.Infrastructure.Test.ModServices;

public class ModIdentifierBuilderTest : CommonTestBaseWithRandomGame
{
    private readonly ModIdentifierBuilder _idBuilder;

    public ModIdentifierBuilderTest()
    {
        _idBuilder = new ModIdentifierBuilder(ServiceProvider);
    }

    [Fact]
    public void Build_DefaultMod()
    { 
        var mod = Game.InstallMod("Mod", false, ServiceProvider);

        var expected = mod.Directory.FullName.TrimEnd(FileSystem.Path.DirectorySeparatorChar, FileSystem.Path.AltDirectorySeparatorChar);
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            expected = expected.ToUpperInvariant();

        Assert.Equal(expected, _idBuilder.Build(mod));
        Assert.Equal(expected, _idBuilder.Build(mod.Directory, false));
    }

    [Fact]
    public void Build_SteamWorkshopMod()
    {
        var steamGame = FileSystem.InstallGame(
            new GameIdentity(TestingUtilities.TestHelpers.GetRandomEnum<GameType>(), GamePlatform.SteamGold),
            ServiceProvider);

        var mod = steamGame.InstallMod("Mod", true, ServiceProvider);

        var expected = mod.Directory.Name;
        _ = uint.Parse(expected); // Check this conversion does not throw

        Assert.Equal(expected, _idBuilder.Build(mod));
        Assert.Equal(expected, _idBuilder.Build(mod.Directory, true));
    }

    [Fact]
    public void Build_SteamWorkshopMod_InvalidPathCannotBeConvertedToSteamWSId_Throws()
    {
        var mod = Game.InstallMod("Mod", false, ServiceProvider);
        Assert.Throws<InvalidOperationException>(() => _idBuilder.Build(mod.Directory, true)); 
    }

    [Fact]
    public void Build_VirtualMod()
    {
        var dep = CreateAndAddMod("Dep");

        var modinfo = new ModinfoData("Name")
        {
            Dependencies = new DependencyList(new List<IModReference>
            {
                dep
            }, DependencyResolveLayout.FullResolved),
            Languages = [new LanguageInfo("de", LanguageSupportLevel.FullLocalized)]
        };
        var mod = new VirtualMod(Game, modinfo, ServiceProvider);

        var id = _idBuilder.Build(mod);
        var parsedModInfo = ModinfoData.Parse(id);

        Assert.Equal<IModIdentity>(modinfo, parsedModInfo);
        Assert.Equal(modinfo.Languages, parsedModInfo.Languages, LanguageInfoEqualityComparer.WithSupportLevel);
    }

    [Fact]
    public void TestNormalizeWorkshops()
    {
        var mRef = new ModReference("123456", ModType.Workshops, SemVersionRange.Parse("1.*"));
        var expected = new ModReference("123456", ModType.Workshops, SemVersionRange.Parse("1.*"));
        var normalizedRef = _idBuilder.Normalize(mRef);

        Assert.Equal(expected, normalizedRef, ModReferenceEqualityComparer.Default);
        Assert.Equal(SemVersionRange.Parse("1.*"), normalizedRef.VersionRange);
    }

    [Fact]
    public void Normalize_VirtualMod()
    {
        var modinfoString = @"{""name"":""Mod""}";
        var mRef = new ModReference(modinfoString, ModType.Virtual, SemVersionRange.Parse("1.*"));
        var expected = new ModReference(modinfoString, ModType.Virtual);
        var normalizedRef = _idBuilder.Normalize(mRef);

        Assert.Equal(expected, normalizedRef);
        Assert.Equal(SemVersionRange.Parse("1.*"), normalizedRef.VersionRange);
    }
}