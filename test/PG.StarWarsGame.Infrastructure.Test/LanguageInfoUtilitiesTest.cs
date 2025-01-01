using System;
using EawModinfo.Model;
using EawModinfo.Spec;
using PG.StarWarsGame.Infrastructure.Services.Language;
using Xunit;

namespace PG.StarWarsGame.Infrastructure.Test;

public class LanguageInfoUtilitiesTest
{
    [Fact]
    public void TestCreateInfo()
    {
        Assert.Throws<ArgumentNullException>(() => LanguageInfoUtilities.FromEnglishName(null!, LanguageSupportLevel.FullLocalized));
        Assert.Equal(new LanguageInfo("de", LanguageSupportLevel.FullLocalized),
            LanguageInfoUtilities.FromEnglishName("german", LanguageSupportLevel.FullLocalized));
        Assert.Equal(new LanguageInfo("en", LanguageSupportLevel.FullLocalized),
            LanguageInfoUtilities.FromEnglishName("english", LanguageSupportLevel.FullLocalized));
        Assert.Equal(new LanguageInfo("en", LanguageSupportLevel.FullLocalized),
            LanguageInfoUtilities.FromEnglishName("ENGLISH", LanguageSupportLevel.FullLocalized));
        Assert.Null(LanguageInfoUtilities.FromEnglishName("BLAH", LanguageSupportLevel.Default));
    }

    [Fact]
    public void TestGetEnglishName()
    {
        var de = new LanguageInfo("de", LanguageSupportLevel.FullLocalized);
        var en = new LanguageInfo("en", LanguageSupportLevel.FullLocalized);
        var unknown = new LanguageInfo("zz", LanguageSupportLevel.FullLocalized);

        Assert.Equal("German", LanguageInfoUtilities.GetEnglishName(de));
        Assert.Equal("English", LanguageInfoUtilities.GetEnglishName(en));
        Assert.Null(LanguageInfoUtilities.GetEnglishName(unknown));
        Assert.Throws<ArgumentNullException>(() => LanguageInfoUtilities.GetEnglishName(null!));
    }
}