namespace PetroGlyph.Games.EawFoc.Clients.Arguments;

/// <summary>
/// Service to build an <see cref="IArgumentCollection"/>
/// </summary>
public interface IArgumentCollectionBuilder
{
    /// <summary>
    /// Adds an argument to the this instance. 
    /// </summary>
    /// <param name="argument">The argument to add.</param>
    /// <returns>This instance.</returns>
    IArgumentCollectionBuilder Add(IGameArgument argument);

    /// <summary>
    /// Removes <paramref name="argument"/> from this instance.
    /// </summary>
    /// <param name="argument">The argument to remove.</param>
    /// <returns>This instance.</returns>
    IArgumentCollectionBuilder Remove(IGameArgument argument);

    /// <summary>
    /// Adds all arguments from <paramref name="argumentCollection"/> to this instance.
    /// </summary>
    /// <param name="argumentCollection">The arguments to add.</param>
    /// <returns>This instance.</returns>
    IArgumentCollectionBuilder AddAll(IArgumentCollection argumentCollection);

    /// <summary>
    /// Creates an <see cref="IArgumentCollection"/> from this instance.
    /// </summary>
    /// <returns>The created <see cref="IArgumentCollection"/>.</returns>
    IArgumentCollection Build();
}