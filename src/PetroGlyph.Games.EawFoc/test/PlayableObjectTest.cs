using System.Collections.Generic;
using EawModinfo.Spec;
using Microsoft.Extensions.DependencyInjection;
using PG.StarWarsGame.Infrastructure.Services.Language;
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
        var obj = CreatePlayableObject(iconPath: $"{FileSystem.Path.GetRandomFileName()}.ico");
        Assert.NotNull(obj.IconFile);
        Assert.NotNull(obj.IconFile);
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

    public class PlayableObjectAbstractTest : CommonTestBaseWithRandomGame
    { 
        protected override void SetupServiceProvider(IServiceCollection sc)
        {
            base.SetupServiceProvider(sc);
            sc.AddSingleton<ILanguageFinder>(new NullLanguageFinder());
        }

        [Fact]
        public void InstalledLanguages_FinderReturnsNull_Throws()
        {
            Assert.Throws<PetroglyphException>(() => Game.InstalledLanguages);
        }

        private class NullLanguageFinder : ILanguageFinder
        {
            public IReadOnlyCollection<ILanguageInfo> FindLanguages(IPlayableObject playableObject) => null!;
        }
    }
}