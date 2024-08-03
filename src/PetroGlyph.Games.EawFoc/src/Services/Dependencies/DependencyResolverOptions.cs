using EawModinfo.Spec;

namespace PG.StarWarsGame.Infrastructure.Services.Dependencies;

/// <summary>
/// Options used by an <see cref="IDependencyResolver"/>.
/// </summary>
public sealed record DependencyResolverOptions
{
    /// <summary>
    /// When set to <see langword="true"/> the complete mod dependency chain gets resolved
    /// by honoring the individual <see cref="DependencyResolveLayout"/> property.
    /// </summary>
    public bool ResolveCompleteChain { get; init; }

    /// <summary>
    /// When dependencies are resolved, a cycle check will be performed.
    /// When set to <see langword="true"/> this may cause an <see cref="IDependencyResolver"/> to throw an exception.
    /// </summary>
    public bool CheckForCycle { get; init; }
}