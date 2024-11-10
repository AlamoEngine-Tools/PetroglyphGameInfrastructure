using PG.StarWarsGame.Infrastructure.Testing;
using Xunit;

namespace PG.StarWarsGame.Infrastructure.Test;

public abstract class PlayableObjectTest : CommonTestBase
{
    protected abstract IPlayableObject CreatePlayableObject();

    [Fact]
    public void IconFile_NoIcon()
    {
        var obj = CreatePlayableObject();
        Assert.Null(obj.IconFile);
    }

    [Fact]
    public void InstalledLanguages_NoLanguagesFound()
    {
        var obj = CreatePlayableObject();
        Assert.Empty(obj.InstalledLanguages);
    }
}