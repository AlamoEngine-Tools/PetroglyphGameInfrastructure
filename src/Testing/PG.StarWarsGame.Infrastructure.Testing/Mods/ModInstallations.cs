using System;
using System.IO.Abstractions;
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


    public static Mod InstallMod(this IGame game, string name, bool workshop, IServiceProvider serviceProvider)
    {
        Assert.True(game.ModsLocation.Exists);

        IDirectoryInfo modDir;
        if (!workshop)
            modDir = game.ModsLocation.CreateSubdirectory(name);
        else if (workshop && game.Platform is not GamePlatform.SteamGold)
        {
            Assert.Fail();
            throw new InvalidOperationException("compiler flow");
        }
        else
        {
            var steamHelpers = serviceProvider.GetRequiredService<ISteamGameHelpers>();
            var wsDir = steamHelpers.GetWorkshopsLocation(game);
            Assert.True(wsDir.Exists);
            steamHelpers.ToSteamWorkshopsId(name, out _);
            modDir = wsDir.CreateSubdirectory(name);
        }
        return new Mod(game, modDir, workshop, name, serviceProvider);
    }
}
