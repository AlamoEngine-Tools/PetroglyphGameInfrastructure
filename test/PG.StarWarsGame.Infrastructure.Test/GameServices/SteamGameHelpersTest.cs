using System;
using AnakinRaW.CommonUtilities.FileSystem.Normalization;
using PG.StarWarsGame.Infrastructure.Games;
using PG.StarWarsGame.Infrastructure.Services.Steam;
using PG.StarWarsGame.Infrastructure.Testing.Game.Installation;
using PG.StarWarsGame.Infrastructure.Testing.TestBases;
using PG.TestingUtilities;
using Xunit;

namespace PG.StarWarsGame.Infrastructure.Test.GameServices;

public class SteamGameHelpersTest : CommonTestBase
{
    private readonly SteamGameHelpers _service;

    public SteamGameHelpersTest()
    {
        _service = new SteamGameHelpers(ServiceProvider);
    }

    [Fact]
    public void GetWorkshopsLocation_NullArgs_Throws()
    {
        Assert.Throws<ArgumentNullException>(() => _service.GetWorkshopsLocation(null!));
        Assert.Throws<ArgumentNullException>(() => _service.TryGetWorkshopsLocation(null!, out _));
    }

    [Fact]
    public void GetWorkshopsLocation_Success()
    {
        var game = FileSystem.InstallGame(new GameIdentity(TestHelpers.GetRandomEnum<GameType>(), GamePlatform.SteamGold),
            ServiceProvider);

        var wsDir = _service.GetWorkshopsLocation(game);

        var expectedEnd = PathNormalizer.Normalize("/workshop/content/32470", PathNormalizeOptions.UnifySeparators);
        Assert.EndsWith(expectedEnd, wsDir.FullName);

        Assert.True(_service.TryGetWorkshopsLocation(game, out var directoryInfo));
        Assert.EndsWith(expectedEnd, directoryInfo.FullName);
    }

    [Fact]
    public void GetWorkshopsLocation_CannotFindWorkshopPath()
    {
        var gameDir = FileSystem.DirectoryInfo.New("SteamFakeFame");
        gameDir.Create();
        var game = new PetroglyphStarWarsGame(
            new GameIdentity(TestHelpers.GetRandomEnum<GameType>(), GamePlatform.SteamGold), gameDir, "Game",
            ServiceProvider);

        Assert.Throws<GameException>(() => _service.GetWorkshopsLocation(game));
        Assert.False(_service.TryGetWorkshopsLocation(game, out _));
    }

    [Theory]
    [InlineData(GamePlatform.Disk)]
    [InlineData(GamePlatform.DiskGold)]
    [InlineData(GamePlatform.GoG)]
    [InlineData(GamePlatform.Origin)]
    public void GetWorkshopsLocation_FailNoSteam(GamePlatform platform)
    {
        var game = FileSystem.InstallGame(new GameIdentity(TestHelpers.GetRandomEnum<GameType>(), platform),
            ServiceProvider);
        Assert.Throws<GameException>(() => _service.GetWorkshopsLocation(game));
        Assert.False(_service.TryGetWorkshopsLocation(game, out _));
    }

    [Fact]
    public void ToSteamWorkshopsId()
    {
        var buf = new byte[8];
        new Random().NextBytes(buf);
        var steamId = (ulong)BitConverter.ToInt64(buf, 0);

        Assert.True(_service.ToSteamWorkshopsId(steamId.ToString(), out var result));
        Assert.Equal(steamId, result);

        Assert.True(SteamGameHelpers.IstValidSteamWorkshopsDir(steamId.ToString(), out result));
        Assert.Equal(steamId, result);

        Assert.True(SteamGameHelpers.IstValidSteamWorkshopsDir(steamId.ToString()));
    }

    [Theory]
    [InlineData("-1")]
    [InlineData("abc")]
    [InlineData("1.0")]
    [InlineData("1,0")]
    [InlineData("1f")]
    [InlineData("1d")]
    [InlineData("1_0")]
    [InlineData("1-1")]
    [InlineData("+1")]
    [InlineData("  1")]
    [InlineData("1  ")]
    public void ToSteamWorkshopsId_InvalidFormats(string input)
    {
        Assert.False(_service.ToSteamWorkshopsId(input, out _));
        Assert.False(SteamGameHelpers.IstValidSteamWorkshopsDir(input, out _));
        Assert.False(SteamGameHelpers.IstValidSteamWorkshopsDir(input));
    }

    [Fact]
    public void ToSteamWorkshopsId_InvalidArgs_Throws()
    {
        Assert.Throws<ArgumentNullException>(() => _service.ToSteamWorkshopsId(null!, out _));
        Assert.Throws<ArgumentException>(() => _service.ToSteamWorkshopsId(string.Empty, out _));

        Assert.Throws<ArgumentNullException>(() => SteamGameHelpers.IstValidSteamWorkshopsDir(null!, out _));
        Assert.Throws<ArgumentException>(() => SteamGameHelpers.IstValidSteamWorkshopsDir(string.Empty, out _));

        Assert.Throws<ArgumentNullException>(() => SteamGameHelpers.IstValidSteamWorkshopsDir(null!));
        Assert.Throws<ArgumentException>(() => SteamGameHelpers.IstValidSteamWorkshopsDir(string.Empty));
    }
}