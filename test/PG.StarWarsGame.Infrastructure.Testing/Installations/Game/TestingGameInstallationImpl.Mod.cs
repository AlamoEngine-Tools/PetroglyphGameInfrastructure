// Copyright (c) Alamo Engine Tools and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using AET.Modinfo.Model;
using AET.Modinfo.Spec;
using Microsoft.Extensions.DependencyInjection;
using PG.StarWarsGame.Infrastructure.Games;
using PG.StarWarsGame.Infrastructure.Mods;
using PG.StarWarsGame.Infrastructure.Services.Steam;
using PG.StarWarsGame.Infrastructure.Testing.Installations.Mods;
using System;
using System.IO.Abstractions;
using PG.StarWarsGame.Infrastructure.Utilities;
using Xunit;

namespace PG.StarWarsGame.Infrastructure.Testing.Installations.Game;

internal partial class TestingGameInstallationImpl
{
    public ITestingPhysicalModInstallation InstallMod(string name, bool workshop)
    {
        var mod = CreateMod(name, workshop, modDir => new Mod(Game, name, modDir, workshop, name, ServiceProvider));
        return new TestingPhysicalModInstallationImpl(this, mod, ServiceProvider);
    }

    public ITestingPhysicalModInstallation InstallMod(IModinfo modinfo)
    {
        var workshop = GITestUtilities.GetRandomWorkshopFlag(Game);
        return InstallMod(modinfo, workshop);
    }

    public ITestingPhysicalModInstallation InstallMod(IModinfo modinfo, bool workshop)
    {
        var name = modinfo.Name;
        var mod = CreateMod(name, workshop, modDir => new Mod(Game, name, modDir, workshop, modinfo, ServiceProvider));
        return new TestingPhysicalModInstallationImpl(this, mod, ServiceProvider);
    }

    public ITestingPhysicalModInstallation InstallMod(IModinfo modinfo, IDirectoryInfo directory)
    {
        var workshop = GITestUtilities.GetRandomWorkshopFlag(Game);
        return InstallMod(modinfo, directory, workshop);
    }

    public ITestingPhysicalModInstallation InstallMod(IModinfo modinfo, IDirectoryInfo directory, bool workshop)
    {
        var mod = CreateMod(directory, dir => new Mod(Game, modinfo.Name, dir, workshop, modinfo, ServiceProvider));
        return new TestingPhysicalModInstallationImpl(this, mod, ServiceProvider);
    }

    public ITestingPhysicalModInstallation InstallMod(string name)
    {
        var workshop = GITestUtilities.GetRandomWorkshopFlag(Game);
        return InstallMod(name, workshop);
    }

    public ITestingPhysicalModInstallation InstallAndAddMod(string name)
    {
        var isWorkshop = GITestUtilities.GetRandomWorkshopFlag(Game);
        return InstallAndAddMod(name, isWorkshop);
    }

    public ITestingPhysicalModInstallation InstallAndAddMod(string name, bool workshop)
    {
        var modInstallation = InstallMod(name, workshop);
        Game.AddMod(modInstallation.Mod);
        return modInstallation;
    }

    public ITestingPhysicalModInstallation InstallAndAddMod(IModinfo modinfo)
    {
        return InstallAndAddMod(modinfo, GITestUtilities.GetRandomWorkshopFlag(Game));
    }

    public ITestingPhysicalModInstallation InstallAndAddMod(IModinfo modinfo, bool workshop)
    {
        var modInstallation = InstallMod(modinfo, workshop);
        Game.AddMod(modInstallation.Mod);
        return modInstallation;
    }

    public ITestingPhysicalModInstallation InstallAndAddMod(IModinfo modinfo, IDirectoryInfo directory, bool workshops)
    {
        var modInstallation = InstallMod(modinfo, directory, workshops);
        Game.AddMod(modInstallation.Mod);
        return modInstallation;
    }

    public ITestingPhysicalModInstallation InstallAndAddMod(string name, IModDependencyList dependencies)
    {
        var workshop = GITestUtilities.GetRandomWorkshopFlag(Game);
        return InstallAndAddMod(name, workshop, dependencies);
    }

    public ITestingVirtualModInstallation AddVirtualMod(string name, ModinfoData modinfo)
    {
        var mod = new VirtualMod(Game, "VirtualModId", modinfo, ServiceProvider);
        Game.AddMod(mod);
        return new TestingVirtualModInstallationImpl(this, mod, ServiceProvider);
    }

    public ITestingPhysicalModInstallation InstallAndAddMod(string name, bool isWorkshop, IModDependencyList dependencies)
    {
        if (dependencies.Count == 0)
            return InstallAndAddMod(name, isWorkshop);

        var modinfo = new ModinfoData(name)
        {
            Dependencies = dependencies
        };
        return InstallAndAddMod(modinfo, isWorkshop);
    }

    public IDirectoryInfo GetModDirectory(string name, bool workshop)
    {
        if (!workshop)
            return FileSystem.DirectoryInfo.New(FileSystem.Path.Combine(Game.ModsLocation.FullName, name));
        if (workshop && Game.Platform is not GamePlatform.SteamGold)
        {
            Assert.Fail($"Game: {Game}, Mod: {name}, {workshop}");
            throw new InvalidOperationException("compiler flow");
        }

        var steamHelpers = ServiceProvider.GetRequiredService<ISteamGameHelpers>();
        var wsDir = steamHelpers.GetWorkshopsLocation(Game);
        Assert.True(wsDir.Exists);

        var nameHash = name.GetHashCode();
        var steamId = (ulong)nameHash;
        if (!FileSystem.Directory.Exists(FileSystem.Path.Combine(wsDir.FullName, steamId.ToString())))
        {
            steamHelpers.ToSteamWorkshopsId(steamId.ToString(), out var id);
            Assert.Equal(steamId, id);
        }
        return FileSystem.DirectoryInfo.New(FileSystem.Path.Combine(wsDir.FullName, steamId.ToString()));
    }

    private Mod CreateMod(string modName, bool workshop, Func<IDirectoryInfo, Mod> modFactory)
    {
        var modDir = GetModDirectory(modName, workshop);
        return CreateMod(modDir, modFactory);
    }

    private Mod CreateMod(IDirectoryInfo directory, Func<IDirectoryInfo, Mod> modFactory)
    {
        Assert.True(Game.ModsLocation.Exists);
        var mod = modFactory(directory);
        mod.Directory.Create();
        mod.DataDirectory().Create();
        return mod;
    }
}