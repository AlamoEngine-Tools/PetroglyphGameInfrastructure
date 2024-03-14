using System;
using System.IO.Abstractions;
using System.Linq;
using System.Text;
using AnakinRaW.CommonUtilities.FileSystem.Normalization;
using EawModinfo.Model;
using EawModinfo.Spec;
using Microsoft.Extensions.DependencyInjection;
using PetroGlyph.Games.EawFoc.Mods;

namespace PetroGlyph.Games.EawFoc.Services.Detection;

internal class ModIdentifierBuilder : IModIdentifierBuilder
{
    private readonly IFileSystem _fileSystem;

    /// <summary>
    /// Creates a new instance.
    /// </summary>
    /// <param name="serviceProvider">The service provider.</param>
    public ModIdentifierBuilder(IServiceProvider serviceProvider)
    {
        _fileSystem = serviceProvider.GetRequiredService<IFileSystem>();
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
            return BuildVirtualModId(mod);

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

    private static string BuildWorkshopsModId(IDirectoryInfo modDir)
    {
        return modDir.Name;
    }

    private static string BuildVirtualModId(IMod mod)
    {
        var sb = new StringBuilder();
        sb.Append(mod.Name);
        if (mod.Dependencies.Any())
            sb.Append("-");
        foreach (var dependency in mod.Dependencies)
        {
            sb.Append(dependency.GetHashCode());
            sb.Append("-");
        }
        return sb.ToString().TrimEnd('-');
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