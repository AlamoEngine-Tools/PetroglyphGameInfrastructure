// Copyright (c) Alamo Engine Tools and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System;
using PG.StarWarsGame.Infrastructure.Mods;
using PG.StarWarsGame.Infrastructure.Testing.Installations.Game;

namespace PG.StarWarsGame.Infrastructure.Testing.Installations.Mods;

internal sealed class TestingVirtualModInstallationImpl(ITestingGameInstallation gameInstallation, IVirtualMod mod, IServiceProvider serviceProvider)
    : TestingModInstallationImpl(gameInstallation, mod, serviceProvider), ITestingVirtualModInstallation
{
    public new ITestingGameInstallation GameInstallation { get; } = gameInstallation;

    public new IVirtualMod Mod { get; } = mod;
}