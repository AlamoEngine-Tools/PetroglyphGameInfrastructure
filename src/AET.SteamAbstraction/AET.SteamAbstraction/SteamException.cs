using System;

namespace AET.SteamAbstraction;

/// <summary>
/// Exception which gets thrown if anything Steam related failed.
/// </summary>
public class SteamException : Exception
{
    /// <inheritdoc/>
    public SteamException()
    {
    }

    /// <inheritdoc/>
    public SteamException(string message) : base(message)
    {
    }

    /// <inheritdoc/>
    public SteamException(string message, Exception exception) : base(message, exception)
    {
    }
}


/// <summary>
/// 
/// </summary>
public sealed class SteamNotFoundException : SteamException
{

}