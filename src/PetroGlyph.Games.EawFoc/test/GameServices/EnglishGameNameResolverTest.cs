using System.Globalization;
using PG.StarWarsGame.Infrastructure.Games;
using PG.StarWarsGame.Infrastructure.Services.Name;
using Xunit;

namespace PG.StarWarsGame.Infrastructure.Test.GameServices;

public class EnglishGameNameResolverTest
{
    [Fact]
    public void IgnoreCulture()
    {
        var resolver = new EnglishGameNameResolver();
        var id = new GameIdentity(GameType.EaW, GamePlatform.SteamGold);
        resolver.ResolveName(id, CultureInfo.GetCultureInfo("de"));
        var name = resolver.ResolveName(id);
        Assert.Contains("Steam", name);
        Assert.Contains("Empire at War", name);
    }

    [Fact]
    public void EawSteamName()
    {
        var resolver = new EnglishGameNameResolver();
        var id = new GameIdentity(GameType.EaW, GamePlatform.SteamGold);
        var name = resolver.ResolveName(id);
        Assert.Contains("Steam", name);
        Assert.Contains("Empire at War", name);
    }

    [Fact]
    public void FocSteamName()
    {
        var resolver = new EnglishGameNameResolver();
        var id = new GameIdentity(GameType.Foc, GamePlatform.SteamGold);
        var name = resolver.ResolveName(id);
        Assert.Contains("Steam", name);
        Assert.Contains("Corruption", name);
    }

    [Fact]
    public void DiskSteamName()
    {
        var resolver = new EnglishGameNameResolver();
        var id = new GameIdentity(GameType.Foc, GamePlatform.Disk);
        var name = resolver.ResolveName(id);
        Assert.DoesNotContain("Steam", name);
        Assert.Contains("Corruption", name);
    }
}