using System.Collections.Generic;
using EawModinfo.Spec;
using PG.StarWarsGame.Infrastructure.Testing;
using Xunit;

namespace PG.StarWarsGame.Infrastructure.Test;

public abstract class PlayableObjectTest : CommonTestBase
{
    protected abstract IPlayableObject CreatePlayableObject(
        string? iconPath = null, 
        ICollection<ILanguageInfo>? languages = null);

    [Fact]
    public void IconFile_NoIcon()
    {
        var obj = CreatePlayableObject();
        Assert.Null(obj.IconFile);
    }

    [Fact]
    public void IconFile_IconInstalled()
    {
        var obj = CreatePlayableObject();
        Assert.Null(obj.IconFile);
    }

    [Fact]
    public void InstalledLanguages_NoLanguagesFound()
    {
        var obj = CreatePlayableObject();
        Assert.Empty(obj.InstalledLanguages);
        // Get a second time
        Assert.Empty(obj.InstalledLanguages);
    }

    [Fact]
    public void InstalledLanguages()
    {
        var expected = GetRandomLanguages();
        var obj = CreatePlayableObject(languages: expected);
        Assert.Equivalent(expected, obj.InstalledLanguages, true);
        // Get a second time
        Assert.Equivalent(expected, obj.InstalledLanguages, true);
    }
}