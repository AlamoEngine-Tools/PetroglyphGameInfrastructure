using System;
using System.IO.Abstractions;
using AET.Modinfo.Spec;
using Microsoft.Extensions.DependencyInjection;
using PG.StarWarsGame.Infrastructure.Games;
using PG.StarWarsGame.Infrastructure.Mods;
using PG.StarWarsGame.Infrastructure.Services.Steam;
using PG.StarWarsGame.Infrastructure.Utilities;

namespace PG.StarWarsGame.Infrastructure.Testing.Mods;

public static partial class ModInstallations
{
    public static Mod InstallAndAddMod(this IGame game, string name, bool workshop, IServiceProvider serviceProvider)
    {
        var mod = game.InstallMod(name, workshop, serviceProvider);
        game.AddMod(mod);
        return mod;
    }

    public static Mod InstallAndAddMod(this IGame game, bool workshop, IModinfo modinfo, IServiceProvider serviceProvider)
    {
        var mod = game.InstallMod(workshop, modinfo, serviceProvider);
        game.AddMod(mod);
        return mod;
    }

    public static Mod InstallMod(this IGame game, string name, bool workshop, IServiceProvider serviceProvider)
    {
        return CreateMod(game, name, workshop, serviceProvider,
            modDir => new Mod(game, name, modDir, workshop, name, serviceProvider));
    }

    public static Mod InstallMod(this IGame game, bool workshop, IModinfo modinfo, IServiceProvider serviceProvider)
    {
        var name = modinfo.Name;
        return CreateMod(game, name, workshop, serviceProvider,
            modDir => new Mod(game, name, modDir, workshop, modinfo, serviceProvider));
    }

    public static Mod InstallMod(this IGame game, IDirectoryInfo directory, bool isWorkshop, IModinfo modinfo, IServiceProvider serviceProvider)
    {
        return CreateMod(game, directory, dir => new Mod(game, modinfo.Name, dir, isWorkshop, modinfo, serviceProvider));
    }

    private static Mod CreateMod(IGame game, string modName, bool workshop, IServiceProvider serviceProvider, Func<IDirectoryInfo, Mod> modFactory)
    {
        var modDir = game.Directory.FileSystem.DirectoryInfo.New(game.GetModDirectory(modName, workshop, serviceProvider));
        return CreateMod(game, modDir, modFactory);
    }

    private static Mod CreateMod(IGame game, IDirectoryInfo directory, Func<IDirectoryInfo, Mod> modFactory)
    {
        Assert.True(game.ModsLocation.Exists);
        var mod = modFactory(directory); 
        mod.Directory.Create();
        mod.DataDirectory().Create();
        return mod;
    }

    public static string GetModDirectory(this IGame game, string name, bool workshop, IServiceProvider serviceProvider)
    {
        var fs = game.Directory.FileSystem;
        if (!workshop)
            return fs.Path.Combine(game.ModsLocation.FullName, name);
        if (workshop && game.Platform is not GamePlatform.SteamGold)
        {
            Assert.Fail($"Game: {game}, Mod: {name}, {workshop}");
            throw new InvalidOperationException("compiler flow");
        }

        var steamHelpers = serviceProvider.GetRequiredService<ISteamGameHelpers>();
        var wsDir = steamHelpers.GetWorkshopsLocation(game);
        Assert.True(wsDir.Exists);

        var nameHash = name.GetHashCode();
        var steamId = (ulong)nameHash;
        if (!fs.Directory.Exists(fs.Path.Combine(wsDir.FullName, steamId.ToString())))
        {
            steamHelpers.ToSteamWorkshopsId(steamId.ToString(), out var id);
            Assert.Equal(steamId, id);
        }
        return fs.Path.Combine(wsDir.FullName, steamId.ToString());
    }
}
