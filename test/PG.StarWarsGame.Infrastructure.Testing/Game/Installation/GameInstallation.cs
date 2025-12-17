using Microsoft.Extensions.DependencyInjection;
using PG.StarWarsGame.Infrastructure.Games;
using System;
using System.Diagnostics.CodeAnalysis;
using System.IO.Abstractions;
using Testably.Abstractions.Testing;
using Xunit;

namespace PG.StarWarsGame.Infrastructure.Testing.Game.Installation;

public static class GameTesting
{
    public static ITestingGameInstallation Game(IServiceProvider serviceProvider)
    {
        return new TestingGameImpl(serviceProvider);
    }
}

public interface ITestingGameInstallation
{
    IGame? Game { get; }

    [MemberNotNull(nameof(Game))]
    void Install(GameIdentity gameIdentity);

    void InstallDebug();
}

internal class TestingGameImpl(IServiceProvider serviceProvider) : ITestingGameInstallation
{
    private readonly IFileSystem _fileSystem = serviceProvider.GetRequiredService<IFileSystem>();

    public IGame? Game { get; private set; }

    [MemberNotNull(nameof(Game))]
    public void Install(GameIdentity gameIdentity)
    {
        var gameDir = gameIdentity.Type == GameType.Foc
            ? GameInstallation.InstallFoc(_fileSystem, gameIdentity.Platform)
            : GameInstallation.InstallEaw(_fileSystem, gameIdentity.Platform);

        _fileSystem.InstallModsLocations(gameDir);

        var game = new PetroglyphStarWarsGame(gameIdentity, gameDir, gameIdentity.ToString(), serviceProvider);
        Assert.True(game.Exists());
        Game = game;
    }

    public void InstallDebug()
    {
        ThrowIfNotInstalled();
        Game.InstallDebug();
    }


    [MemberNotNull(nameof(Game))]
    private void ThrowIfNotInstalled()
    {
        if (Game is null)
            throw new InvalidOperationException("Game not installed");
    }
}


public static partial class GameInstallation
{
    // Ensure starts on path 'steam'. See SteamInstallation.cs
    private const string SteamBasePath = "steam/steamapps/common/Star Wars Empire at War";
    private const string GogBasePath = "games/gog";
    private const string OriginBasePath = "games/origin";

    public static IGame InstallGame(this IFileSystem fileSystem, GameIdentity gameIdentity, IServiceProvider serviceProvider)
    {
        var gameDir = gameIdentity.Type == GameType.Foc
            ? InstallFoc(fileSystem, gameIdentity.Platform)
            : InstallEaw(fileSystem, gameIdentity.Platform);

        fileSystem.InstallModsLocations(gameDir);

        var game = new PetroglyphStarWarsGame(gameIdentity, gameDir, gameIdentity.ToString(), serviceProvider);
        Assert.True(game.Exists());
        return game;
    }

    public static void InstallDebug(this IGame game)
    {
        if (game.Platform is not GamePlatform.SteamGold)
            Assert.Fail($"Cannot install Debug files for non-Steam game '{game}'");

        var fs = game.Directory.FileSystem;
        CreateFile(fs, fs.Path.Combine(game.Directory.FullName, "StarWarsI.exe"));
    }

    private static void InstallDataAndMegaFilesXml(this IFileSystem fs, IDirectoryInfo directory)
    {
        fs.Initialize();
        CreateFile(fs, fs.Path.Combine(directory.FullName, "Data", "megafiles.xml"));
    }

    private static void InstallSteamFiles(this IFileSystem fs, Action initAction)
    {
        fs.Initialize();
        CreateFile(fs, fs.Path.Combine(SteamBasePath, "32470_install.vdf"));
        CreateFile(fs, fs.Path.Combine(SteamBasePath, "32472_install.vdf"));
        CreateFile(fs, fs.Path.Combine(SteamBasePath, "runme.dat"));
        CreateFile(fs, fs.Path.Combine(SteamBasePath, "runm2.dat"));
        CreateFile(fs, fs.Path.Combine(SteamBasePath, "runme.exe"));
        CreateFile(fs, fs.Path.Combine(SteamBasePath, "runme2.exe"));

        fs.Directory.CreateDirectory(fs.Path.Combine(SteamBasePath, "..", "..", "workshop", "content", "32470"));
        initAction();
    }

    private static void InstallGoGFiles(this IFileSystem fs, Action initAction)
    {
        fs.Initialize();
        CreateFile(fs, fs.Path.Combine(GogBasePath, "goggame.sdb"));
        CreateFile(fs, fs.Path.Combine(GogBasePath, "goggame-1421404887.hashdb"));
        CreateFile(fs, fs.Path.Combine(GogBasePath, "goggame-1421404887.info"));
        CreateFile(fs, fs.Path.Combine(GogBasePath, "Language.exe"));
        initAction();
    }

    private static void InstallOriginFiles(this IFileSystem fs, Action initAction)
    {
        fs.Initialize();
        fs.Directory.CreateDirectory(fs.Path.Combine(OriginBasePath, "Manuals"));
        fs.Directory.CreateDirectory(fs.Path.Combine(OriginBasePath, "__Installer"));
        initAction();
    }

    internal static void InstallModsLocations(this IFileSystem fileSystem, IDirectoryInfo directory)
    {
        fileSystem.Directory.CreateDirectory(fileSystem.Path.Combine(directory.FullName, "Mods"));
    }
    
    private static void CreateFile(IFileSystem fs, string path)
    {
        var dir = fs.Path.GetDirectoryName(path)!;
        fs.Directory.CreateDirectory(dir);
        using var file = fs.File.Create(path);
    }
}