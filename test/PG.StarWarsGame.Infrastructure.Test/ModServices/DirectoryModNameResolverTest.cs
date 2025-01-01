using System;
using System.Globalization;
using EawModinfo.Model;
using EawModinfo.Spec;
using EawModinfo.Utilities;
using PG.StarWarsGame.Infrastructure.Services.Name;
using PG.StarWarsGame.Infrastructure.Testing.TestBases;
using Xunit;

namespace PG.StarWarsGame.Infrastructure.Test.ModServices;

public class OfflineModNameResolverTest : ModNameResolverTestBase
{
    public override ModNameResolverBase CreateResolver()
    {
        return new OfflineModNameResolver(ServiceProvider);
    }

    [Fact]
    public void Ctor_NullArgs_Throws()
    {
        Assert.Throws<ArgumentNullException>(() => new OfflineModNameResolver(null!));
    }
}

public class OnlineModNameResolverTest : ModNameResolverTestBase
{
    public override ModNameResolverBase CreateResolver()
    {
        return new OnlineModNameResolver(ServiceProvider);
    }

    [Fact]
    public void Ctor_NullArgs_Throws()
    {
        Assert.Throws<ArgumentNullException>(() => new OnlineModNameResolver(null!));
    }

    [Theory]
    [InlineData("path/1125718579", "z3r0x's mod version 3.5")]
    [InlineData("path/2508288191", "Empire at War Expanded: Deep Core 3.1")]
    public void ResolveName_Steam_WithoutModinfo_FindNameOnline(string path, string containsExpected)
    {
        var modRef = CreateDetectedModReference(path, ModType.Workshops, null);

        var resolver = CreateResolver();

        var nameInv = resolver.ResolveName(modRef, CultureInfo.InvariantCulture);
        var nameDe = resolver.ResolveName(modRef, new CultureInfo("de"));

        Assert.Equal(nameInv, nameDe);
        Assert.Contains(containsExpected, nameInv);
    }
}

public abstract class ModNameResolverTestBase : CommonTestBase
{
    public abstract ModNameResolverBase CreateResolver();

    protected DetectedModReference CreateDetectedModReference(string path, ModType type, IModinfo? modinfo)
    {
        return new DetectedModReference(new ModReference("SOME_ID", type), FileSystem.DirectoryInfo.New(path), modinfo);
    }

    [Fact]
    public void ResolveName_NullArg_Throws()
    {
        Assert.Throws<ArgumentNullException>(() => CreateResolver().ResolveName(null!, CultureInfo.CurrentCulture));
        Assert.Throws<ArgumentNullException>(() => CreateResolver()
            .ResolveName(CreateDetectedModReference("path", ModType.Default, null), null!));
    }

    [Fact]
    public void ResolveName_VirtualModsNotSupported()
    {
        Assert.Throws<NotSupportedException>(() => CreateResolver()
            .ResolveName(CreateDetectedModReference("path", ModType.Virtual, null), CultureInfo.CurrentCulture));
    }

    [Theory]
    [InlineData("MyMod", "MyMod")]
    [InlineData("MyMod/", "MyMod")]
    [InlineData("My Mod", "My Mod")]
    [InlineData("My-Mod", "My Mod")]
    [InlineData("My_Mod", "My Mod")]
    [InlineData("  MyMod  ", "MyMod")]
    [InlineData("  My Mod  ", "My Mod")]
    [InlineData("path/MyMod", "MyMod")]
    [InlineData("path/sub/MyMod", "MyMod")]
    [InlineData("path/MyMod/", "MyMod")]
    [InlineData("my_path/My_Mod/", "My Mod")]
    [InlineData("my-path/My-Mod/", "My Mod")]
    [InlineData("my-path/123456789/", "123456789")]
    [InlineData("___---___", "___---___")]
    public void ResolveName_NotSteam_WithoutModinfo_IsCultureIsIgnored(string path, string expectedName)
    {
        var modRef = CreateDetectedModReference(path, ModType.Default, null);

        var resolver = CreateResolver();

        var nameInv = resolver.ResolveName(modRef, CultureInfo.InvariantCulture);
        var nameDe = resolver.ResolveName(modRef, new CultureInfo("de"));

        Assert.Equal(nameInv, nameDe);
        Assert.Equal(expectedName, nameInv);
    }

    [Theory]
    [InlineData(ModType.Default, "path/MyMod")]
    [InlineData(ModType.Default, "path/123456789")]
    [InlineData(ModType.Workshops, "path/1234")] // Does not exist
    [InlineData(ModType.Workshops, "path/1129810972")] // Republic at War (in cache)
    [InlineData(ModType.Workshops, "path/1125718579")] // z3r0x's mod (online)
    public void ResolveName_AnyType_WithModinfo_AlwaysUseModinfoName(ModType modType, string path)
    {
        var modRef = CreateDetectedModReference(path, modType, new ModinfoData("ModinfoName"));

        var resolver = CreateResolver();

        var nameInv = resolver.ResolveName(modRef, CultureInfo.InvariantCulture);
        var nameDe = resolver.ResolveName(modRef, new CultureInfo("de"));

        Assert.Equal(nameInv, nameDe);
        Assert.Equal("ModinfoName", nameInv);
    }

    [Theory]
    [InlineData("path/1234", "1234")] // Does not exist
    [InlineData("path/1129810972", "Republic at War")] // Republic at War (in cache)
    public void ResolveName_Steam_WithoutModinfo_OfflineFallback(string path, string expected)
    {
        var modRef = CreateDetectedModReference(path, ModType.Workshops, null);

        var resolver = CreateResolver();

        var nameInv = resolver.ResolveName(modRef, CultureInfo.InvariantCulture);
        var nameDe = resolver.ResolveName(modRef, new CultureInfo("de"));

        Assert.Equal(nameInv, nameDe);
        Assert.Equal(expected, nameInv);
    }
}