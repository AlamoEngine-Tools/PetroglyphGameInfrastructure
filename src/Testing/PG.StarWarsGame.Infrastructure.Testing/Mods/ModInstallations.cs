using System;
using System.IO.Abstractions;
using EawModinfo.Spec;
using Microsoft.Extensions.DependencyInjection;
using PG.StarWarsGame.Infrastructure.Games;
using PG.StarWarsGame.Infrastructure.Mods;
using PG.StarWarsGame.Infrastructure.Services.Steam;
using PG.StarWarsGame.Infrastructure.Utilities;

namespace PG.StarWarsGame.Infrastructure.Testing.Mods;

public static class ModInstallations
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
            modDir => new Mod(game, modDir, workshop, name, serviceProvider));
    }

    public static Mod InstallMod(this IGame game, bool workshop, IModinfo modinfo, IServiceProvider serviceProvider)
    {
        var name = modinfo.Name;
        return CreateMod(game, name, workshop, serviceProvider,
            modDir => new Mod(game, modDir, workshop, modinfo, serviceProvider));
    }

    private static Mod CreateMod(IGame game, string modName, bool workshop, IServiceProvider serviceProvider, Func<IDirectoryInfo, Mod> modFactory)
    {
        Assert.True(game.ModsLocation.Exists);
        var modDir = game.Directory.FileSystem.DirectoryInfo.New(GetModDirectory(game, modName, workshop, serviceProvider));
        var mod = modFactory(modDir); 
        mod.DataDirectory().Create();
        return mod;
    }

    private static string GetModDirectory(IGame game, string name, bool workshop, IServiceProvider serviceProvider)
    {
        var fs = game.Directory.FileSystem;
        if (!workshop)
            return fs.Path.Combine(game.ModsLocation.FullName, name);
        if (workshop && game.Platform is not GamePlatform.SteamGold)
        {
            Assert.Fail();
            throw new InvalidOperationException("compiler flow");
        }

        var steamHelpers = serviceProvider.GetRequiredService<ISteamGameHelpers>();
        var wsDir = steamHelpers.GetWorkshopsLocation(game);
        Assert.True(wsDir.Exists);

        uint steamId;
        while (true)
        {
            steamId = (uint)new Random().Next(1, int.MaxValue);
            if (!fs.Directory.Exists(fs.Path.Combine(wsDir.FullName, steamId.ToString())))
            {
                steamHelpers.ToSteamWorkshopsId(steamId.ToString(), out var id);
                Assert.Equal(steamId, id);
                break;
            }
        }
        return fs.Path.Combine(wsDir.FullName, steamId.ToString());
    }
}
