using System;
using System.Collections.Generic;
using System.Globalization;
using Microsoft.Extensions.DependencyInjection;
using PG.StarWarsGame.Infrastructure.Games;
using PG.StarWarsGame.Infrastructure.Services;
using PG.StarWarsGame.Infrastructure.Services.Detection;
using PG.StarWarsGame.Infrastructure.Services.Name;
using PG.StarWarsGame.Infrastructure.Testing;
using PG.StarWarsGame.Infrastructure.Testing.Game.Installation;
using PG.StarWarsGame.Infrastructure.Testing.TestBases;
using PG.TestingUtilities;
using Xunit;

namespace PG.StarWarsGame.Infrastructure.Test.GameServices;

public class GameFactoryTest : CommonTestBase
{
    private readonly GameFactory _factory;
    private readonly IGameNameResolver _nameResolver;

    public GameFactoryTest()
    {
        _factory = new GameFactory(ServiceProvider);
        _nameResolver = ServiceProvider.GetRequiredService<IGameNameResolver>();
    }

    [Fact]
    public void Ctor_NullArg_Throws()
    {
        Assert.Throws<ArgumentNullException>(() => new GameFactory(null!));
    }

    [Fact]
    public void CreateGame_NullArg_Throws()
    {
        Assert.Throws<ArgumentNullException>(() => _factory.CreateGame(null!, CultureInfo.CurrentCulture));
        Assert.Throws<ArgumentNullException>(() => _factory.CreateGame(
            GameDetectionResult.NotInstalled(TestHelpers.GetRandomEnum<GameType>()),
            null!));

        Assert.Throws<ArgumentNullException>(() => _factory.CreateGame(
            null!,
            FileSystem.DirectoryInfo.New("path"),
            TestHelpers.RandomBool(),
            CultureInfo.CurrentCulture));
        Assert.Throws<ArgumentNullException>(() => _factory.CreateGame(
            CreateRandomGameIdentity(),
            null!,
            TestHelpers.RandomBool(),
            CultureInfo.CurrentCulture));
        Assert.Throws<ArgumentNullException>(() => _factory.CreateGame(
            CreateRandomGameIdentity(),
            FileSystem.DirectoryInfo.New("path"),
            TestHelpers.RandomBool(),
            null!));
    }

    [Fact]
    public void TryCreateGame_NullArg_Throws()
    {
        Assert.Throws<ArgumentNullException>(() => _factory.TryCreateGame(null!, CultureInfo.CurrentCulture, out _));
        Assert.Throws<ArgumentNullException>(() => _factory.TryCreateGame(
            GameDetectionResult.NotInstalled(TestHelpers.GetRandomEnum<GameType>()),
            null!, out _));

        Assert.Throws<ArgumentNullException>(() => _factory.TryCreateGame(
            null!,
            FileSystem.DirectoryInfo.New("path"),
            TestHelpers.RandomBool(),
            CultureInfo.CurrentCulture, out _));
        Assert.Throws<ArgumentNullException>(() => _factory.TryCreateGame(
            CreateRandomGameIdentity(),
            null!,
            TestHelpers.RandomBool(),
            CultureInfo.CurrentCulture, out _));
        Assert.Throws<ArgumentNullException>(() => _factory.TryCreateGame(
            CreateRandomGameIdentity(),
            FileSystem.DirectoryInfo.New("path"),
            TestHelpers.RandomBool(),
            null!, out _));
    }

    [Fact]
    public void CreateGame_NoGameInstalled_Throws()
    {
        Assert.Throws<GameException>(() =>
            _factory.CreateGame(GameDetectionResult.NotInstalled(TestHelpers.GetRandomEnum<GameType>()),
                CultureInfo.CurrentCulture));
        Assert.Throws<GameException>(() =>
            _factory.CreateGame(GameDetectionResult.RequiresInitialization(TestHelpers.GetRandomEnum<GameType>()),
                CultureInfo.CurrentCulture));
    }

    [Fact]
    public void TryCreateGame_NoGameInstalled_Throws()
    {
        Assert.False(_factory.TryCreateGame(GameDetectionResult.NotInstalled(TestHelpers.GetRandomEnum<GameType>()),
            CultureInfo.CurrentCulture, out _));
        Assert.False(_factory.TryCreateGame(
            GameDetectionResult.RequiresInitialization(TestHelpers.GetRandomEnum<GameType>()),
            CultureInfo.CurrentCulture, out _));
    }

    [Fact]
    public void CreateGame_GameNotExists_Throws()
    {
        Assert.Throws<GameException>(() =>
            _factory.CreateGame(
                CreateRandomGameIdentity(),
                FileSystem.DirectoryInfo.New("gamePath"),
                true, CultureInfo.CurrentCulture));

        Assert.False(_factory.TryCreateGame(
            CreateRandomGameIdentity(),
            FileSystem.DirectoryInfo.New("gamePath"),
            true,
            CultureInfo.CurrentCulture,
            out _));

        // Try again with existing dir
        FileSystem.Directory.CreateDirectory("gamePath");

        Assert.Throws<GameException>(() =>
            _factory.CreateGame(
                CreateRandomGameIdentity(),
                FileSystem.DirectoryInfo.New("gamePath"),
                true, CultureInfo.CurrentCulture));

        Assert.False(_factory.TryCreateGame(
            CreateRandomGameIdentity(),
            FileSystem.DirectoryInfo.New("gamePath"),
            true,
            CultureInfo.CurrentCulture,
            out _));
    }

    [Theory]
    [InlineData(GamePlatform.DiskGold, true)]
    [InlineData(GamePlatform.GoG, true)]
    [InlineData(GamePlatform.Origin, true)]
    [InlineData(GamePlatform.SteamGold, true)]
    [InlineData(GamePlatform.DiskGold, false)]
    [InlineData(GamePlatform.GoG, false)]
    [InlineData(GamePlatform.Origin, false)]
    [InlineData(GamePlatform.SteamGold, false)]
    public void CreateGame_WrongGameIdentityInstalled_WrongPlatform_Throws(GamePlatform platform, bool checkExists)
    {
        var actualGameType = TestHelpers.GetRandomEnum<GameType>();

        // Use Disk so that we can check against more specific platforms
        var installedGame =
            FileSystem.InstallGame(new GameIdentity(actualGameType, GamePlatform.Disk), ServiceProvider);

        var createIdentity = new GameIdentity(actualGameType, platform);

        TestCreateGameWithPredefinedExistCheck(checkExists, installedGame, createIdentity);
    }

    public static IEnumerable<object[]> GetWrongGameTypeTestData()
    {
        foreach (var platform in GITestUtilities.RealPlatforms)
        {
            yield return [platform, true];
            yield return [platform, false];
        }
    }

    [Theory]
    [MemberData(nameof(GetWrongGameTypeTestData))]
    public void CreateGame_WrongGameIdentityInstalled_WrongType_Throws(GamePlatform platform, bool checkExists)
    {
        var actualGameType = TestHelpers.GetRandomEnum<GameType>();

        // Use Disk so that we can check against more specific platforms
        var installedGame = FileSystem.InstallGame(
            new GameIdentity(actualGameType, platform), ServiceProvider);

        var createGameType = actualGameType == GameType.Eaw ? GameType.Foc : GameType.Eaw;
        var createIdentity = new GameIdentity(createGameType, platform);

        TestCreateGameWithPredefinedExistCheck(checkExists, installedGame, createIdentity);
    }

    [Fact]
    public void CreateGame_UnidentifiedPlatform_Throws()
    {
        var installedGame = CreateRandomGame();
        Assert.Throws<GameException>(() => _factory.CreateGame(
            new GameIdentity(TestHelpers.GetRandomEnum<GameType>(), GamePlatform.Undefined),
            installedGame.Directory,
            TestHelpers.RandomBool(),
            CultureInfo.CurrentCulture));

        Assert.False(_factory.TryCreateGame(
            new GameIdentity(TestHelpers.GetRandomEnum<GameType>(), GamePlatform.Undefined),
            installedGame.Directory,
            TestHelpers.RandomBool(),
            CultureInfo.CurrentCulture, out _));
    }

    private void TestCreateGameWithPredefinedExistCheck(bool checkExists, IGame installedGame,
        IGameIdentity createIdentity)
    {
        if (checkExists)
        {
            Assert.Throws<GameException>(() => _factory.CreateGame(
                createIdentity,
                installedGame.Directory,
                true,
                CultureInfo.CurrentCulture));

            Assert.False(_factory.TryCreateGame(
                createIdentity,
                installedGame.Directory,
                true,
                CultureInfo.CurrentCulture, out _));
        }
        else
        {
            var game = _factory.CreateGame(createIdentity, installedGame.Directory, false, CultureInfo.CurrentCulture);
            var expectedName = _nameResolver.ResolveName(game, CultureInfo.CurrentCulture);

            AssertGame(game, createIdentity, installedGame.Directory.FullName, expectedName!);

            _factory.TryCreateGame(createIdentity, installedGame.Directory, false, CultureInfo.CurrentCulture,
                out var tryGame);

            Assert.NotNull(tryGame);
            AssertGame(tryGame, createIdentity, installedGame.Directory.FullName, expectedName!);
        }
    }

    private void AssertGame(IGame game, IGameIdentity expectedIdentity, string expectedPath, string expectedName)
    {
        Assert.Equal(expectedIdentity, game);
        Assert.Equal(FileSystem.Path.GetFullPath(expectedPath), game.Directory.FullName);
        Assert.Equal(expectedName, game.Name);
    }

    [Theory]
    [MemberData(nameof(RealGameIdentities))]
    public void CreateGame_FromIdentity_GameCreated(GameIdentity gameIdentity)
    {
        var installedGame = FileSystem.InstallGame(gameIdentity, ServiceProvider);

        var expectedName = _nameResolver.ResolveName(gameIdentity, CultureInfo.CurrentCulture);

        var createdGame = _factory.CreateGame(gameIdentity, installedGame.Directory, true, CultureInfo.CurrentCulture);

        AssertGame(createdGame, gameIdentity, installedGame.Directory.FullName, expectedName!);

        Assert.True(_factory.TryCreateGame(gameIdentity, installedGame.Directory, true, CultureInfo.CurrentCulture,
            out var tryGame));

        AssertGame(tryGame, gameIdentity, installedGame.Directory.FullName, expectedName!);
    }

    [Theory]
    [MemberData(nameof(RealGameIdentities))]
    public void CreateGame_FromDetectionResult_GameCreated(GameIdentity gameIdentity)
    {
        var installedGame = FileSystem.InstallGame(gameIdentity, ServiceProvider);

        var detector = new DirectoryGameDetector(installedGame.Directory, ServiceProvider);
        var detectionResult = detector.Detect(gameIdentity.Type, gameIdentity.Platform);

        var expectedName = _nameResolver.ResolveName(gameIdentity, CultureInfo.CurrentCulture);

        var createdGame = _factory.CreateGame(detectionResult, CultureInfo.CurrentCulture);

        AssertGame(createdGame, gameIdentity, installedGame.Directory.FullName, expectedName!);

        Assert.True(_factory.TryCreateGame(detectionResult, CultureInfo.CurrentCulture, out var tryGame));

        AssertGame(tryGame, gameIdentity, installedGame.Directory.FullName, expectedName!);
    }

    public class GameFactoryWithNullNameResolver : CommonTestBaseWithRandomGame
    {
        private readonly GameFactory _factory;

        public GameFactoryWithNullNameResolver()
        {
            _factory = new GameFactory(ServiceProvider);
        }

        protected override void SetupServiceProvider(IServiceCollection sc)
        {
            base.SetupServiceProvider(sc);
            sc.AddSingleton<IGameNameResolver>(new NullGameNameResolver());
        }

        [Fact]
        public void CreateGame_NameResolverReturnNull_Throws()
        {
            Assert.Throws<GameException>(() =>
                _factory.CreateGame(Game, Game.Directory, false, CultureInfo.CurrentCulture));
            Assert.Throws<GameException>(() =>
                _factory.CreateGame(GameDetectionResult.FromInstalled(Game, Game.Directory),
                    CultureInfo.CurrentCulture));

            Assert.False(_factory.TryCreateGame(Game, Game.Directory, false, CultureInfo.CurrentCulture, out _));
            Assert.False(_factory.TryCreateGame(GameDetectionResult.FromInstalled(Game, Game.Directory), CultureInfo.CurrentCulture,
                out _));
        }
    }

    public class GameFactoryWithCustomNameResolver : CommonTestBaseWithRandomGame
    {
        private readonly GameFactory _factory;

        public GameFactoryWithCustomNameResolver()
        {
            _factory = new GameFactory(ServiceProvider);
        }

        protected override void SetupServiceProvider(IServiceCollection sc)
        {
            base.SetupServiceProvider(sc);
            sc.AddSingleton<IGameNameResolver>(new CultureAwareGameNameResolver());
        }

        public static IEnumerable<object[]> GetCultures()
        {
            yield return [CultureInfo.InvariantCulture];
            yield return [CultureInfo.CurrentUICulture];
        }
 
        [Theory]
        [MemberData(nameof(GetCultures))]
        public void CreateGame_NameResolverReceivesCulture(CultureInfo culture)
        {
            var game = _factory.CreateGame(Game, Game.Directory, false, culture);
            Assert.Equal(culture.EnglishName, game.Name);

            game = _factory.CreateGame(GameDetectionResult.FromInstalled(Game, Game.Directory), culture);
            Assert.Equal(culture.EnglishName, game.Name);

            _factory.TryCreateGame(Game, Game.Directory, false, culture, out game);
            Assert.Equal(culture.EnglishName, game!.Name);

            _factory.TryCreateGame(GameDetectionResult.FromInstalled(Game, Game.Directory), culture, out game);
            Assert.Equal(culture.EnglishName, game!.Name);
        }
    }

    private class NullGameNameResolver : IGameNameResolver
    {
        public string ResolveName(IGameIdentity game, CultureInfo culture)
        {
            return null!;
        }
    }

    private class CultureAwareGameNameResolver : IGameNameResolver
    {
        public string ResolveName(IGameIdentity game, CultureInfo culture)
        {
            return culture.EnglishName;
        }
    }
}