using PG.StarWarsGame.Infrastructure.Games;
using PG.TestingUtilities;
using Xunit;

namespace PG.StarWarsGame.Infrastructure.Test;

public class GameIdentityTest
{
    [Fact]
    public void Ctor()
    {
        var type = TestHelpers.GetRandomEnum<GameType>();
        var platform = TestHelpers.GetRandomEnum<GamePlatform>();
        var id = new GameIdentity(type, platform);
        
        Assert.Equal(type, id.Type);
        Assert.Equal(platform, id.Platform);
    }

    [Fact]
    public void Equals_GetHashCode()
    {
        var id = new GameIdentity(GameType.Eaw, GamePlatform.SteamGold);
        var idSameish = new GameIdentity(GameType.Eaw, GamePlatform.SteamGold);
        var idOtherPlatform = new GameIdentity(GameType.Eaw, GamePlatform.Disk);
        var idOtherType = new GameIdentity(GameType.Foc, GamePlatform.SteamGold);
        var idOtherAll = new GameIdentity(GameType.Foc, GamePlatform.Disk);

        Assert.False(id.Equals(null));
        Assert.False(id.Equals((object)null!));

        Assert.True(id.Equals(id));
        Assert.True(id.Equals((object)id));
        Assert.True(((IGameIdentity)id).Equals(id));
        Assert.Equal(id.GetHashCode(), id.GetHashCode());

        Assert.True(id.Equals(idSameish));
        Assert.True(id.Equals((object)idSameish));
        Assert.True(((IGameIdentity)id).Equals(idSameish));
        Assert.Equal(id.GetHashCode(), idSameish.GetHashCode());

        Assert.False(id.Equals(idOtherPlatform));
        Assert.False(id.Equals((object)idOtherPlatform));
        Assert.False(((IGameIdentity)id).Equals(idOtherPlatform));
        Assert.NotEqual(id.GetHashCode(), idOtherPlatform.GetHashCode());

        Assert.False(id.Equals(idOtherType));
        Assert.False(id.Equals((object)idOtherType));
        Assert.False(((IGameIdentity)id).Equals(idOtherType));
        Assert.NotEqual(id.GetHashCode(), idOtherType.GetHashCode());

        Assert.False(id.Equals(idOtherAll));
        Assert.False(id.Equals((object)idOtherAll));
        Assert.False(((IGameIdentity)id).Equals(idOtherAll));
        Assert.NotEqual(id.GetHashCode(), idOtherAll.GetHashCode());
    }
}