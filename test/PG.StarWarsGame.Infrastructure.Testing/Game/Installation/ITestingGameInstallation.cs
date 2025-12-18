using System.Diagnostics.CodeAnalysis;
using System.IO.Abstractions;
using PG.StarWarsGame.Infrastructure.Games;

namespace PG.StarWarsGame.Infrastructure.Testing.Game.Installation;

public interface ITestingGameInstallation
{
    IGame? Game { get; }

    [MemberNotNull(nameof(Game))]
    IGame Install(IGameIdentity gameIdentity);

    [MemberNotNull(nameof(Game))]
    IGame InstallRandom();

    void InstallDebug();

    IDirectoryInfo GetWrongOriginFocRegistryLocation();
}