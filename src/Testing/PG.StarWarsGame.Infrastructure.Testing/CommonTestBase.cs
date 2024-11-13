using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO.Abstractions;
using Microsoft.Extensions.DependencyInjection;
using PG.StarWarsGame.Infrastructure.Games;
using PG.StarWarsGame.Infrastructure.Testing.Game.Installation;
using PG.TestingUtilities;
using Testably.Abstractions.Testing;

namespace PG.StarWarsGame.Infrastructure.Testing;

public abstract class CommonTestBase
{
    protected readonly IServiceProvider ServiceProvider;
    protected readonly MockFileSystem FileSystem = new();

    [SuppressMessage("ReSharper", "VirtualMemberCallInConstructor")]
    protected CommonTestBase()
    {
        var sc = new ServiceCollection();
        SetupServiceProvider(sc);
        ServiceProvider = sc.BuildServiceProvider();
    }

    protected virtual void SetupServiceProvider(IServiceCollection sc)
    {
        sc.AddSingleton<IFileSystem>(FileSystem);
        PetroglyphGameInfrastructure.InitializeServices(sc);
    }

    public static IEnumerable<object[]> RealGameIdentities()
    {
        foreach (var platform in GITestUtilities.RealPlatforms)
        {
            yield return [new GameIdentity(GameType.Eaw, platform)];
            yield return [new GameIdentity(GameType.Foc, platform)];
        }
    }

    protected static GameIdentity CreateRandomGameIdentity()
    {
        return new GameIdentity(TestHelpers.GetRandomEnum<GameType>(), TestHelpers.GetRandom(GITestUtilities.RealPlatforms));
    }

    protected PetroglyphStarWarsGame CreateRandomGame()
    {
        return FileSystem.InstallGame(CreateRandomGameIdentity(), ServiceProvider);
    }
}