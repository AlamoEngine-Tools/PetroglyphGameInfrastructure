namespace AET.SteamAbstraction;

internal readonly struct SteamUserLoginMetadata(ulong userId, bool mostRecent, bool userWantsOffline)
{
    public ulong UserId { get; } = userId;

    public bool MostRecent { get; } = mostRecent;

    public bool UserWantsOffline { get; } = userWantsOffline;
}