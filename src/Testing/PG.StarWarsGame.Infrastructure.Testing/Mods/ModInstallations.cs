using System;
using EawModinfo.Spec;
using Microsoft.Extensions.DependencyInjection;
using PG.StarWarsGame.Infrastructure.Games;
using PG.StarWarsGame.Infrastructure.Mods;
using PG.StarWarsGame.Infrastructure.Services.Steam;

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
        Assert.True(game.ModsLocation.Exists);
        var modDir = game.Directory.FileSystem.DirectoryInfo.New(GetModDirectory(game, name, workshop, serviceProvider));
        return new Mod(game, modDir, workshop, name, serviceProvider);
    }

    public static Mod InstallMod(this IGame game, bool workshop, IModinfo modinfo, IServiceProvider serviceProvider)
    {
        var name = modinfo.Name;
        var modDir = game.Directory.FileSystem.DirectoryInfo.New(GetModDirectory(game, name, workshop, serviceProvider));
        return new Mod(game, modDir, workshop, modinfo, serviceProvider);
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
