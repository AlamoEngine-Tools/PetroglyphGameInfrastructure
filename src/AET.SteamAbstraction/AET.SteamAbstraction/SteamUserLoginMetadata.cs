namespace AET.SteamAbstraction;

internal readonly struct SteamUserLoginMetadata(bool mostRecent, bool userWantsOffline)
{
    public bool MostRecent { get; } = mostRecent;

    public bool UserWantsOffline { get; } = userWantsOffline;
}