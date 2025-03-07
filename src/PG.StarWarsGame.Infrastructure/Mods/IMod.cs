﻿using System;
using System.Collections.Generic;
using AET.Modinfo.Spec;
using PG.StarWarsGame.Infrastructure.Services.Dependencies;

namespace PG.StarWarsGame.Infrastructure.Mods;

/// <summary>
/// Represents a mod for a Petroglyph Star Wars game.
/// </summary>
public interface IMod : IModIdentity, IModReference, IPlayableObject, IModContainer, IEquatable<IMod>
{
    /// <summary>
    /// The event that is raised when the mod's dependencies have been resolved.
    /// </summary>
    event EventHandler DependenciesResolved;

    /// <summary>
    /// Gets the name of the mod.
    /// </summary>
    new string Name { get; }

    /// <summary>
    /// Gets the modinfo data of the mod, or <see langword="null"/> if no modinfo data was specified.
    /// </summary>
    IModinfo? ModInfo { get; }

    /// <summary>
    /// Gets an ordered List of the mod's direct dependencies.
    /// </summary>
    /// <remarks>
    /// Initially this list will be empty. You need to call <see cref="ResolveDependencies"/> first, in order to fill it.
    /// <br/>
    /// To get a dependency list including, transitive dependencies, use <see cref="IModDependencyTraverser"/>.
    /// </remarks>
    new IReadOnlyList<IMod> Dependencies { get; }

    /// <summary>
    /// Gets the status about the mod's dependency resolve status.
    /// </summary>
    DependencyResolveStatus DependencyResolveStatus { get; }

    /// <summary>
    /// Gets the resolve layout of the <see cref="Dependencies"/> property.
    /// Correct value only available if dependencies have been resolved.
    /// Default value is <see cref="AET.Modinfo.Spec.DependencyResolveLayout.ResolveRecursive"/>.
    /// </summary>
    DependencyResolveLayout DependencyResolveLayout { get; }

    /// <summary>
    /// Resolves the mod's dependencies by filling the <see cref="Dependencies"/> list
    /// and sets the <see cref="DependencyResolveStatus"/> accordingly.
    /// </summary>
    /// <remarks>This operation does <b>not</b> update the <see cref="Mods"/> collection of dependencies.</remarks>
    /// <exception cref="ModDependencyCycleException">A dependency cycle was found.</exception>
    /// <exception cref="ModDependencyCycleException">This method gets called while already resolving this instance.</exception>
    /// <exception cref="ModNotFoundException">A dependency could not be found.</exception>
    /// <exception cref="ModDependencyException">The dependencies could not be resolved.</exception>
    void ResolveDependencies();
}