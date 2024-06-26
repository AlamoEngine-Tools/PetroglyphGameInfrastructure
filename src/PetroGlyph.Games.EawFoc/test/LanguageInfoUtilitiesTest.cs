﻿using EawModinfo.Model;
using EawModinfo.Spec;
using PG.StarWarsGame.Infrastructure.Services.Language;
using Xunit;

namespace PG.StarWarsGame.Infrastructure.Test;

public class LanguageInfoUtilitiesTest
{
    [Fact]
    public void TestCreateInfo()
    {
        Assert.Equal(new LanguageInfo("de", LanguageSupportLevel.FullLocalized),
            LanguageInfoUtilities.FromEnglishName("german", LanguageSupportLevel.FullLocalized));
        Assert.Equal(new LanguageInfo("en", LanguageSupportLevel.FullLocalized),
            LanguageInfoUtilities.FromEnglishName("english", LanguageSupportLevel.FullLocalized));
        Assert.Equal(new LanguageInfo("en", LanguageSupportLevel.FullLocalized),
            LanguageInfoUtilities.FromEnglishName("ENGLISH", LanguageSupportLevel.FullLocalized));
    }

    [Fact]
    public void TestGetEnglishName()
    {
        var de = new LanguageInfo("de", LanguageSupportLevel.FullLocalized);
        var en = new LanguageInfo("en", LanguageSupportLevel.FullLocalized);

        Assert.Equal("German", LanguageInfoUtilities.GetEnglishName(de));
        Assert.Equal("English", LanguageInfoUtilities.GetEnglishName(en));
    }
}