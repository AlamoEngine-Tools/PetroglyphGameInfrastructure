using System;
using System.Collections.Generic;
using System.Linq;
using EawModinfo.Spec;
using Microsoft.Extensions.DependencyInjection;
using PetroGlyph.Games.EawFoc.Mods;
using PetroGlyph.Games.EawFoc.Services.Dependencies;
using PetroGlyph.Games.EawFoc.Services.Steam;
using Sklavenwalker.CommonUtilities.FileSystem;

namespace PetroGlyph.Games.EawFoc.Clients.Arguments;

public class ModArgumentListFactory : IModArgumentListFactory
{
    private readonly IServiceProvider _serviceProvider;

    public ModArgumentListFactory(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    public IGameArgument<IReadOnlyList<IGameArgument<string>>> BuildArgumentList(IList<IMod> traversedModChain)
    {
        var dependencyArgs = new List<ModArgument>();
        foreach (var dependency in traversedModChain)
        {
            // Virtual and non physical mods are just "placeholders" we cannot add them to the argument list.
            if (dependency.Type == ModType.Virtual || dependency is not IPhysicalMod physicalMod)
                continue;
            dependencyArgs.Add(BuildSingleModArgument(physicalMod));
        }

        return !dependencyArgs.Any() ? ModArgumentList.Empty : new ModArgumentList(dependencyArgs);
    }

    public IGameArgument<IReadOnlyList<IGameArgument<string>>> BuildArgumentList(IMod modInstance)
    {
        if (modInstance.DependencyResolveStatus == DependencyResolveStatus.Resolved)
        {
            var traverser = _serviceProvider.GetRequiredService<IModDependencyTraverser>();
            var dependencies = traverser.Traverse(modInstance).Select(d => d.Mod).ToList();
            return BuildArgumentList(dependencies);
        }

        // Virtual mods must have dependencies and are resolved at the time they are created.
        // Also Mods with no dependencies must be physical. 
        // This simply results in an empty list.
        if (modInstance.Type == ModType.Virtual || modInstance is not IPhysicalMod physicalMod)
            return ModArgumentList.Empty;

        var arg = BuildSingleModArgument(physicalMod);
        return new ModArgumentList(new[] { arg });
    }

    private ModArgument BuildSingleModArgument(IPhysicalMod mod)
    {
        var isWorkshop = mod.Type == ModType.Workshops;
        var argumentValue = mod.Identifier;

        if (!isWorkshop)
        {
            // The path MUST NOT contain any spacing. Even escaping it with " or ' does not help.
            // It will not be launched by the game. To allow the game part of the full path is allowed to have spaces,
            // we try to relativize to path if it's not absolute.
            var relativeOrAbsoluteModPath = GetAbsoluteOrRelativeModPath(mod);
            if (relativeOrAbsoluteModPath.Any(char.IsWhiteSpace))
                throw new ModException(mod,
                    $"MODPATH value '{relativeOrAbsoluteModPath}' must not contain white space characters");
            argumentValue = relativeOrAbsoluteModPath;
        }
        else
        {
            var steamHelper = _serviceProvider.GetRequiredService<ISteamGameHelpers>();
            if (!steamHelper.ToSteamWorkshopsId(argumentValue, out _))
                throw new InvalidOperationException("Identifier is not a valid SteamID");
        }

        return new ModArgument(argumentValue, isWorkshop);
    }


    private string GetAbsoluteOrRelativeModPath(IPhysicalMod mod)
    {
        var pathHelper = _serviceProvider.GetService<IPathHelperService>() ?? new PathHelperService(mod.FileSystem);
        
        var gamePath = pathHelper.EnsureTrailingSeparator(
            pathHelper.NormalizePath(mod.Game.Directory.FullName, PathNormalizeOptions.Full));
        var modPath = pathHelper.EnsureTrailingSeparator(
            pathHelper.NormalizePath(mod.Directory.FullName, PathNormalizeOptions.Full));

        // It's important not to resolve, as that would give us an absolute path again.
        return pathHelper.NormalizePath(pathHelper.GetRelativePath(gamePath, modPath), PathNormalizeOptions.FullNoResolve);
    }
}