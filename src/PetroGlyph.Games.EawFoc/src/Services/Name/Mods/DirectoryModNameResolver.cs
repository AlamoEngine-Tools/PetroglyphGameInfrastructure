﻿using System;
using System.Globalization;
using System.IO.Abstractions;
using EawModinfo.Spec;
using Microsoft.Extensions.DependencyInjection;
using PG.StarWarsGame.Infrastructure.Mods;

namespace PG.StarWarsGame.Infrastructure.Services.Name;

/// <summary>
/// Resolves a mod's name by its directory name. The name will be beautified by removing separator characters like '_'.
/// This instance is always culture invariant.
/// This instance does not work with virtual mods. 
/// </summary>
public sealed class DirectoryModNameResolver : ModNameResolverBase
{
    /// <inheritdoc/>
    public DirectoryModNameResolver(IServiceProvider serviceProvider) : base(serviceProvider)
    {
    }

    /// <inheritdoc/>
    protected internal override string ResolveCore(IModReference modReference, CultureInfo culture)
    {
        if (modReference.Type == ModType.Virtual)
            throw new NotSupportedException("Cannot resolve name for virtual mods.");
        if (modReference is IPhysicalMod mod)
            return BeautifyDirectoryName(mod.Directory.Name);
        var fs = ServiceProvider.GetRequiredService<IFileSystem>();
        var directoryName = fs.DirectoryInfo.New(modReference.Identifier).Name;
        var beautifiedName = BeautifyDirectoryName(directoryName);
        return string.IsNullOrWhiteSpace(beautifiedName) ? directoryName : beautifiedName;
    }

    private static string BeautifyDirectoryName(string directoryName)
    {
        var removedUnderscore = directoryName.Replace('_', ' ');
        return removedUnderscore;
    }
}