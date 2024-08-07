﻿using System;
using System.Linq;
using EawModinfo.Spec;
using Microsoft.Extensions.DependencyInjection;
using PG.StarWarsGame.Infrastructure.Mods;
using PG.StarWarsGame.Infrastructure.Utilities;

namespace PG.StarWarsGame.Infrastructure.Services.Icon;

/// <summary>
/// Provides a very simple implementation which searches returns the first .ico file in a mod's directory.
/// For virtual mods it returns the icon of the first physical dependency which has a icon.
/// </summary>
public class SimpleModIconFinder(IServiceProvider serviceProvider) : IModIconFinder
{
    private IPlayableObjectFileService _fileService = serviceProvider.GetRequiredService<IPlayableObjectFileService>();

    /// <summary>
    /// Searches for hardcoded icon names.
    /// "eaw.ico" for Empire at War and
    /// "foc.ico" for Forces of Corruption
    /// </summary>
    public string? FindIcon(IMod mod)
    {
        if (mod == null) 
            throw new ArgumentNullException(nameof(mod));

        if (mod is IPhysicalMod physicalMod)
            return _fileService.DataFiles(physicalMod, "*.ico", "..", false, false)
                .FirstOrDefault()?.FullName;
        if (mod.Type == ModType.Virtual)
            throw new NotImplementedException("TODO");
        return null;
    }
}