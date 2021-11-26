﻿using System;
using System.Collections.Generic;
using System.Linq;
using EawModinfo.Spec;
using Microsoft.Extensions.DependencyInjection;
using PetroGlyph.Games.EawFoc.Clients.Arguments.GameArguments;
using PetroGlyph.Games.EawFoc.Mods;
using PetroGlyph.Games.EawFoc.Services.Dependencies;
using PetroGlyph.Games.EawFoc.Services.Steam;
using Validation;

namespace PetroGlyph.Games.EawFoc.Clients.Arguments;

/// <summary>
///  Create a <see cref="ModArgumentList"/> from a given mod instance
/// by using a registered service instance of <see cref="IModDependencyTraverser"/>
/// </summary>
public class ModArgumentListFactory : IModArgumentListFactory
{
    private readonly IServiceProvider _serviceProvider;
    private IArgumentValidator? _validator;

    /// <summary>
    /// Initializes a new builder instance.
    /// </summary>
    /// <param name="serviceProvider">The service provider.</param>
    public ModArgumentListFactory(IServiceProvider serviceProvider)
    {
        Requires.NotNull(serviceProvider, nameof(serviceProvider));
        _serviceProvider = serviceProvider;
    }

    /// <inheritdoc/>
    public ModArgumentList BuildArgumentList(IList<IMod> traversedModChain, bool validateArgumentOnCreation)
    {
        var dependencyArgs = new List<ModArgument>();
        foreach (var dependency in traversedModChain)
        {
            // Virtual and non physical mods are just "placeholders" we cannot add them to the argument list.
            if (dependency.Type == ModType.Virtual || dependency is not IPhysicalMod physicalMod)
                continue;
            dependencyArgs.Add(BuildSingleModArgument(physicalMod, validateArgumentOnCreation));
        }

        return !dependencyArgs.Any() ? ModArgumentList.Empty : new ModArgumentList(dependencyArgs);
    }

    /// <inheritdoc/>
    public ModArgumentList BuildArgumentList(IMod modInstance, bool validateArgumentOnCreation)
    {
        if (modInstance.DependencyResolveStatus == DependencyResolveStatus.Resolved)
        {
            var traverser = _serviceProvider.GetRequiredService<IModDependencyTraverser>();
            var dependencies = traverser.Traverse(modInstance).Select(d => d.Mod).ToList();
            return BuildArgumentList(dependencies, validateArgumentOnCreation);
        }

        // Virtual mods must have dependencies and are resolved at the time they are created.
        // Also Mnoods with no dependencies must be physical. 
        // This simply results in an empty list.
        if (modInstance.Type == ModType.Virtual || modInstance is not IPhysicalMod physicalMod)
            return ModArgumentList.Empty;

        var arg = BuildSingleModArgument(physicalMod, validateArgumentOnCreation);
        return new ModArgumentList(new[] { arg });
    }

    private ModArgument BuildSingleModArgument(IPhysicalMod mod, bool validate)
    {
        var isWorkshop = mod.Type == ModType.Workshops;
        var argumentValue = mod.Identifier;

        if (!isWorkshop)
        {
            // Shortening to the relative game's path allows the game to exit on a path which has space characters.
            var relativeOrAbsoluteModPath = new ArgumentValueSerializer().ShortenPath(mod.Directory, mod.Game.Directory);
            argumentValue = relativeOrAbsoluteModPath;
        }
        else
        {
            var steamHelper = _serviceProvider.GetRequiredService<ISteamGameHelpers>();
            // Throwing here is OK because this mod clearly violates its contract. 
            if (!steamHelper.ToSteamWorkshopsId(argumentValue, out _))
                throw new SteamException("Identifier is not a valid SteamID");
        }

        var argument = new ModArgument(argumentValue, isWorkshop);
        if (validate)
        {
            _validator ??= _serviceProvider.GetRequiredService<IArgumentValidator>();
            var validity = _validator.CheckArgument(argument, out _, out _);
            if (validity != ArgumentValidityStatus.Valid)
                throw new ModException(mod, $"Create mod argument for {mod} is not valid: {validity}");
        }

        return argument;
    }
}