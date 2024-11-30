using System;
using System.IO.Abstractions;
using AnakinRaW.CommonUtilities.FileSystem.Normalization;
using EawModinfo.Model;
using EawModinfo.Spec;
using Microsoft.Extensions.DependencyInjection;
using PG.StarWarsGame.Infrastructure.Mods;
using PG.StarWarsGame.Infrastructure.Services.Steam;

namespace PG.StarWarsGame.Infrastructure.Services.Detection;

internal class ModIdentifierBuilder : IModIdentifierBuilder
{
    private readonly IFileSystem _fileSystem;
    private readonly ISteamGameHelpers _steamGameHelper;

    /// <summary>
    /// Creates a new instance.
    /// </summary>
    /// <param name="serviceProvider">The service provider.</param>
    public ModIdentifierBuilder(IServiceProvider serviceProvider)
    {
        _fileSystem = serviceProvider.GetRequiredService<IFileSystem>();
        _steamGameHelper = serviceProvider.GetRequiredService<ISteamGameHelpers>();
    }

    public string Build(IDirectoryInfo modDirectory, bool isWorkshop)
    {
        return isWorkshop ? BuildWorkshopsModId(modDirectory) : BuildDefaultModId(modDirectory);
    }

    /// <inheritdoc/>
    public string Build(IMod mod)
    {
        if (mod.Type == ModType.Default)
        {
            if (mod is not IPhysicalMod physicalMod)
                throw new InvalidOperationException();
            return BuildDefaultModId(physicalMod.Directory);
        }

        if (mod.Type == ModType.Workshops)
        {
            if (mod is not IPhysicalMod physicalMod)
                throw new InvalidOperationException();
            return BuildWorkshopsModId(physicalMod.Directory);
        }

        if (mod.Type == ModType.Virtual)
        {
            if (mod is not IVirtualMod virtualMod)
                throw new InvalidOperationException();
            return BuildVirtualModId(virtualMod);
        }

        throw new NotSupportedException($"Cannot create identifier for unsupported mod type {mod.Type}.");
    }

    private string BuildDefaultModId(IDirectoryInfo modDir)
    {
        return BuildDefaultModId(modDir.FullName);
    }

    private string BuildDefaultModId(string modDirPath)
    {
        return PathNormalizer.Normalize(_fileSystem.Path.GetFullPath(modDirPath), new PathNormalizeOptions
        {
            UnifyCase = UnifyCasingKind.UpperCase,
            TrailingDirectorySeparatorBehavior = TrailingDirectorySeparatorBehavior.Trim
        });
    }

    private string BuildWorkshopsModId(IDirectoryInfo modDir)
    {
        if (!_steamGameHelper.ToSteamWorkshopsId(modDir.Name, out _))
            throw new InvalidOperationException($"{modDir} is not a valid Steam Workshop directory.");
        return modDir.Name;
    }

    private static string BuildVirtualModId(IVirtualMod mod)
    {
        if (mod.ModInfo is null)
            throw new NotSupportedException("Virtual mods without modinfo data are not supported.");
        return mod.ModInfo.ToJson();
    }

    public ModReference Normalize(IModReference modReference)
    {
        var id = modReference.Type switch
        {
            ModType.Default => BuildDefaultModId(modReference.Identifier),
            ModType.Workshops => modReference.Identifier,
            ModType.Virtual => modReference.Identifier,
            _ => throw new ArgumentOutOfRangeException()
        };
        return new ModReference(id, modReference.Type, modReference.VersionRange);
    }
}