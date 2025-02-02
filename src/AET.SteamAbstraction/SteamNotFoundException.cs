using System.Diagnostics.CodeAnalysis;

namespace AET.SteamAbstraction;

/// <summary>
/// The exception that is thrown if an installation of Steam is not found.
/// </summary>
public sealed class SteamNotFoundException : SteamException
{
    /// <inheritdoc />
    [ExcludeFromCodeCoverage]
    public override string Message => "No Steam installation could be found on the current system.";
}