namespace PetroGlyph.Games.EawFoc
{
    /// <summary>
    /// Action for <see cref="ModCollectionChangedEventArgs"/>.
    /// </summary>
    public enum ModCollectionChangedAction
    {
        /// <summary>
        /// Used when a mod was added to an <see cref="IModContainer"/>.
        /// </summary>
        Add,
        /// <summary>
        /// Used when a mod was removed from an <see cref="IModContainer"/>.
        /// </summary>
        Remove
    }
}