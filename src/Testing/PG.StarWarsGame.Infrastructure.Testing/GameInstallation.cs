using System;
using PG.StarWarsGame.Infrastructure.Games;
using Testably.Abstractions.Testing;

namespace PG.StarWarsGame.Infrastructure.Testing;

public static class GameInstallation
{
    public static IGame InstallGame(this MockFileSystem fs, string path, GameIdentity gameIdentity, IServiceProvider sp)
    {
        if (gameIdentity.Type == GameType.Foc)
            return InstallFoc(fs, path, gameIdentity.Platform, sp);
        return InstallEaw(fs, path, gameIdentity.Platform, sp);
    }

    private static IGame InstallEaw(this MockFileSystem fs, string path, GamePlatform platform, IServiceProvider sp)
    {
        throw new NotImplementedException();
    }

    public static IGame InstallFoc(this MockFileSystem fs, string path, GamePlatform platform, IServiceProvider sp)
    {
        fs.Initialize().WithSubdirectories(
            path, 
            fs.Path.Combine(path, "Mods"));
        

        var dirInfo = fs.DirectoryInfo.New(path);

        var game = new PetroglyphStarWarsGame(new GameIdentity(GameType.Foc, platform), dirInfo, GameType.Foc.ToString(), sp);

        Assert.True(game.Exists());

        return game;
    }
}