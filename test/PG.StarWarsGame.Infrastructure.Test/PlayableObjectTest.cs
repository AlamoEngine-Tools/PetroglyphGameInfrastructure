using AET.Modinfo.Spec;
using Microsoft.Extensions.DependencyInjection;
using PG.StarWarsGame.Infrastructure.Services.Language;
using PG.StarWarsGame.Infrastructure.Testing;
using PG.StarWarsGame.Infrastructure.Testing.Installations;
using PG.StarWarsGame.Infrastructure.Testing.TestBases;
using System.Collections.Generic;
using Xunit;

namespace PG.StarWarsGame.Infrastructure.Test;

public abstract class PlayableObjectTest : GameInfrastructureTestBase
{
    protected abstract ITestingPlayableObjectInstallation CreatePlayableObjectInstallation(string? iconPath = null, ICollection<ILanguageInfo>? languages = null);

    [Fact]
    public void IconFile_NoIcon()
    {
        var obj = CreatePlayableObjectInstallation().PlayableObject;
        Assert.Null(obj.IconFile);
    }

    [Fact]
    public void IconFile_IconInstalled()
    {
        var obj = CreatePlayableObjectInstallation(iconPath: $"{FileSystem.Path.GetRandomFileName()}.ico")
            .PlayableObject;
        Assert.NotNull(obj.IconFile);
        Assert.NotNull(obj.IconFile);
    }

    [Fact]
    public void InstalledLanguages_NoLanguagesFound()
    {
        var obj = CreatePlayableObjectInstallation().PlayableObject;
        Assert.Empty(obj.InstalledLanguages);
        // Get a second time
        Assert.Empty(obj.InstalledLanguages);
    }

    [Fact]
    public void InstalledLanguages()
    {
        var expected = GITestUtilities.GetRandomLanguages();
        var obj = CreatePlayableObjectInstallation(languages: expected).PlayableObject;
        Assert.Equivalent(expected, obj.InstalledLanguages, true);
        // Get a second time
        Assert.Equivalent(expected, obj.InstalledLanguages, true);
    }

    public class PlayableObectAbstractTest : GameInfrastructureTestBaseWithRandomGame
    { 
        protected override void SetupServices(IServiceCollection serviceCollection)
        {
            base.SetupServices(serviceCollection);
            serviceCollection.AddSingleton<ILanguageFinder>(new NullLanguageFinder());
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