namespace AET.SteamAbstraction;

/// <summary>
/// Factory to create a platform dependent <see cref="ISteamWrapper"/>.
/// </summary>
public interface ISteamWrapperFactory
{
    /// <summary>
    /// Creates a new <see cref="ISteamWrapper"/> for the current platform.
    /// </summary>
    /// <returns>The <see cref="ISteamWrapper"/>for the current platform.</returns>
    ISteamWrapper CreateWrapper();
}