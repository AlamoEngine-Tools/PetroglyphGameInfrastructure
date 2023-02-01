using System;
using System.Collections.Generic;
using System.ComponentModel;
using EawModinfo.Spec;
using PetroGlyph.Games.EawFoc.Services.Dependencies;

namespace PetroGlyph.Games.EawFoc.Mods;

/// <summary>
/// This instance represents a mod for a Petroglyph Star Wars game.
/// </summary>
public interface IMod : IModIdentity, IModReference, IPlayableObject, IModContainer, IEquatable<IMod>
{
    /// <summary>
    /// Gets fired when this instance is about to lazy-load <see cref="ModInfo"/>.
    /// <br></br>
    /// This operation can be cancelled by settings <see cref="CancelEventArgs.Cancel"/> to <see langword="true"/>. 
    /// In this case <see cref="ModInfo"/> will be set to <see langword="null"/>
    /// </summary>
    event EventHandler<ResolvingModinfoEventArgs> ResolvingModinfo;

    /// <summary>
    /// Gets fired after the lazy-load of <see cref="ModInfo"/> was completed.
    /// </summary>
    event EventHandler<ModinfoResolvedEventArgs> ModinfoResolved;

    /// <summary>
    /// Gets fired when the <see cref="IModIdentity.Dependencies"/> list was altered.
    /// </summary>
    event EventHandler<ModDependenciesChangedEventArgs> DependenciesChanged;

    /// <inheritdoc cref="IModIdentity.Name" />
    new string Name { get; }

    /// <summary>
    /// If a modinfo.json file is available its data gets stored here; otherwise this returns <see langword="null"/>
    /// </summary>
    IModinfo? ModInfo { get; }

    /// <summary>
    /// Ordered List of <see cref="IMod"/>s this instance depends on. Initially this list will be empty. To fill it,
    /// once resolve the dependencies by calling <see cref="ResolveDependencies"/>.
    /// </summary>
    new IReadOnlyList<ModDependencyEntry> Dependencies { get; }


    /// <summary>
    /// DependencyResolveStatus flag if and at which result dependencies have been resolved for this instance.
    /// </summary>
    DependencyResolveStatus DependencyResolveStatus { get; }

    /// <summary>
    /// Resolve layout of the <see cref="Dependencies"/> property.
    /// Correct value only available if dependencies have been resolved.
    /// Default value is <see cref="EawModinfo.Spec.DependencyResolveLayout.ResolveRecursive"/>.
    /// </summary>
    DependencyResolveLayout DependencyResolveLayout { get; }

    /// <summary>
    /// Searches for direct <see cref="IMod"/> dependencies.
    /// Updates <see cref="IModIdentity.Dependencies"/> property.
    /// This operation ignores whether dependencies have already been resolved or not.
    /// This operation sets the <see cref="DependencyResolveStatus"/> accordingly to the current state of the operation.
    /// </summary>
    /// <param name="resolver">Resolver service to use</param>
    /// <param name="options"></param>
    /// <remarks>This operation does NOT update the <see cref="IModContainer.Mods"/>collection of dependencies.</remarks>
    /// <exception cref="ModDependencyCycleException">If a dependency cycle was found.</exception>
    /// <exception cref="ModDependencyCycleException">if the method get's called again while already resolving this instance.</exception>
    /// <exception cref="ModNotFoundException">if a dependency could not be found.</exception>
    /// <exception cref="PetroglyphException">if some internal constrained failed.</exception>
    void ResolveDependencies(IDependencyResolver resolver, DependencyResolverOptions options);
}