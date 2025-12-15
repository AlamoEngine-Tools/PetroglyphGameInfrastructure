namespace AET.SteamAbstraction.Testing;

public sealed class TestingSteamUserLoginMetadata
{
    public ulong UserId { get; init; }
    public bool UserWantsOffline { get; init; }
    public bool MostRecent { get; init; }

    public TestingSteamUserLoginMetadata()
    {
    }

    public TestingSteamUserLoginMetadata(ulong userId, bool mostRecent = false, bool userWantsOffline = false)
    {
        UserId = userId;
        MostRecent = mostRecent;
        UserWantsOffline = userWantsOffline;
    }
}