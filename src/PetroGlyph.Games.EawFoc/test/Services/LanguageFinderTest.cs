using EawModinfo.Spec;
using Moq;
using System.Collections.Generic;
using System;
using Testably.Abstractions.Testing;
using Xunit;

namespace PG.StarWarsGame.Infrastructure.Test.Services;

public class LanguageFinderTest
{
    //[Fact]
    //public void NullArgTest_Throws()
    //{
    //    Assert.ThrowsAny<Exception>(() => new Mock<ModLanguageFinderBase>(null, false).Object);
    //}

    //[Fact]
    //public void TestModinfo()
    //{
    //    var helper = new Mock<ILanguageFinder>();
    //    var sp = new Mock<IServiceProvider>();
    //    sp.Setup(p => p.GetService(typeof(ILanguageFinder))).Returns(helper.Object);

    //    var finder = new Mock<ModLanguageFinderBase>(sp.Object, false) { CallBase = true };

    //    var expected = new LanguageInfo("de", LanguageSupportLevel.Default);

    //    var modinfo = new Mock<IModinfo>();
    //    modinfo.Setup(m => m.Languages)
    //        .Returns(new List<ILanguageInfo> { expected });
    //    var mod = new Mock<IMod>();
    //    mod.Setup(m => m.ModInfo).Returns(modinfo.Object);

    //    finder.Setup(f => f.FindInstalledLanguagesCore(It.IsAny<IMod>())).Throws<Exception>();

    //    var langs = finder.Object.FindInstalledLanguages(mod.Object);
    //    Assert.Single(langs);
    //    Assert.Equal(expected, langs.FirstOrDefault());
    //}

    //[Fact]
    //public void TestModinfoDefault_Throws()
    //{
    //    var helper = new Mock<ILanguageFinder>();
    //    var sp = new Mock<IServiceProvider>();
    //    sp.Setup(p => p.GetService(typeof(ILanguageFinder))).Returns(helper.Object);

    //    var finder = new Mock<ModLanguageFinderBase>(sp.Object, false) { CallBase = true };

    //    var modinfo = new Mock<IModinfo>();
    //    modinfo.Setup(m => m.Languages)
    //        .Returns(new List<ILanguageInfo> { LanguageInfo.Default });
    //    var mod = new Mock<IMod>();
    //    mod.Setup(m => m.ModInfo).Returns(modinfo.Object);

    //    finder.Setup(f => f.FindInstalledLanguagesCore(It.IsAny<IMod>())).Throws<Exception>();

    //    Assert.Throws<Exception>(() => finder.Object.FindInstalledLanguages(mod.Object));
    //}

    //[Fact]
    //public void TestModinfoDefault()
    //{
    //    var helper = new Mock<ILanguageFinder>();
    //    var sp = new Mock<IServiceProvider>();
    //    sp.Setup(p => p.GetService(typeof(ILanguageFinder))).Returns(helper.Object);

    //    var expected = new LanguageInfo("de", LanguageSupportLevel.Default);

    //    var finder = new Mock<ModLanguageFinderBase>(sp.Object, false) { CallBase = true };
    //    finder.Setup(f => f.FindInstalledLanguagesCore(It.IsAny<IMod>())).Returns(new HashSet<ILanguageInfo> { expected });

    //    var mod = new Mock<IMod>();

    //    var langs = finder.Object.FindInstalledLanguages(mod.Object);

    //    Assert.Single(langs);
    //    Assert.Equal(expected, langs.FirstOrDefault());
    //}

    //[Fact]
    //public void TestModinfoDefault_NoDeps()
    //{
    //    var helper = new Mock<ILanguageFinder>();
    //    var sp = new Mock<IServiceProvider>();
    //    sp.Setup(p => p.GetService(typeof(ILanguageFinder))).Returns(helper.Object);

    //    var expected = LanguageInfo.Default;

    //    var finder = new Mock<ModLanguageFinderBase>(sp.Object, false) { CallBase = true };
    //    finder.Setup(f => f.FindInstalledLanguagesCore(It.IsAny<IMod>())).Returns(new HashSet<ILanguageInfo> { expected });

    //    var mod = new Mock<IMod>();

    //    var langs = finder.Object.FindInstalledLanguages(mod.Object);

    //    Assert.Single(langs);
    //    Assert.Equal(expected, langs.FirstOrDefault());
    //}

    //[Fact]
    //public void TestModinfoDefault_DepNotResolved()
    //{
    //    var helper = new Mock<ILanguageFinder>();
    //    var sp = new Mock<IServiceProvider>();
    //    sp.Setup(p => p.GetService(typeof(ILanguageFinder))).Returns(helper.Object);

    //    var expected = LanguageInfo.Default;

    //    var finder = new Mock<ModLanguageFinderBase>(sp.Object, true) { CallBase = true };
    //    finder.Setup(f => f.FindInstalledLanguagesCore(It.IsAny<IMod>())).Returns(new HashSet<ILanguageInfo> { expected });

    //    var mod = new Mock<IMod>();
    //    mod.Setup(m => m.DependencyResolveStatus).Returns(DependencyResolveStatus.None);

    //    var langs = finder.Object.FindInstalledLanguages(mod.Object);

    //    Assert.Single(langs);
    //    Assert.Equal(expected, langs.FirstOrDefault());
    //}

    //[Fact]
    //public void TestModinfoDefault_FirstDep()
    //{
    //    var helper = new Mock<ILanguageFinder>();
    //    var sp = new Mock<IServiceProvider>();
    //    sp.Setup(p => p.GetService(typeof(ILanguageFinder))).Returns(helper.Object);

    //    var expected = new LanguageInfo("de", LanguageSupportLevel.Default);

    //    var finder = new Mock<ModLanguageFinderBase>(sp.Object, true) { CallBase = true };
    //    finder.Setup(f => f.FindInstalledLanguagesCore(It.IsAny<IMod>())).Returns(new HashSet<ILanguageInfo> { LanguageInfo.Default });

    //    var mod = new Mock<IMod>();
    //    mod.Setup(m => m.DependencyResolveStatus).Returns(DependencyResolveStatus.Resolved);

    //    var dep = new Mock<IMod>();
    //    dep.Setup(m => m.InstalledLanguages).Returns(new HashSet<ILanguageInfo> { expected });
    //    mod.Setup(m => m.Dependencies).Returns(new List<ModDependencyEntry> { new(dep.Object) });

    //    var langs = finder.Object.FindInstalledLanguages(mod.Object);

    //    Assert.Single(langs);
    //    Assert.Equal(expected, langs.FirstOrDefault());
    //}

    //[Fact]
    //public void TestModinfoDefault_SecondDep()
    //{
    //    var helper = new Mock<ILanguageFinder>();
    //    var sp = new Mock<IServiceProvider>();
    //    sp.Setup(p => p.GetService(typeof(ILanguageFinder))).Returns(helper.Object);

    //    var expected = new LanguageInfo("de", LanguageSupportLevel.Default);

    //    var finder = new Mock<ModLanguageFinderBase>(sp.Object, true) { CallBase = true };
    //    finder.Setup(f => f.FindInstalledLanguagesCore(It.IsAny<IMod>())).Returns(new HashSet<ILanguageInfo> { LanguageInfo.Default });

    //    var mod = new Mock<IMod>();
    //    mod.Setup(m => m.DependencyResolveStatus).Returns(DependencyResolveStatus.Resolved);

    //    var depA = new Mock<IMod>();
    //    var depB = new Mock<IMod>();
    //    depA.Setup(m => m.InstalledLanguages).Returns(new HashSet<ILanguageInfo> { LanguageInfo.Default });
    //    depB.Setup(m => m.InstalledLanguages).Returns(new HashSet<ILanguageInfo> { expected });
    //    mod.Setup(m => m.Dependencies).Returns(new List<ModDependencyEntry> { new(depA.Object), new(depB.Object) });

    //    var langs = finder.Object.FindInstalledLanguages(mod.Object);

    //    Assert.Single(langs);
    //    Assert.Equal(expected, langs.FirstOrDefault());
    //}

    //[Fact]
    //public void TestModinfoDefault_DepDefault()
    //{
    //    var helper = new Mock<ILanguageFinder>();
    //    var sp = new Mock<IServiceProvider>();
    //    sp.Setup(p => p.GetService(typeof(ILanguageFinder))).Returns(helper.Object);

    //    var expected = LanguageInfo.Default;

    //    var finder = new Mock<ModLanguageFinderBase>(sp.Object, true) { CallBase = true };
    //    finder.Setup(f => f.FindInstalledLanguagesCore(It.IsAny<IMod>())).Returns(new HashSet<ILanguageInfo> { LanguageInfo.Default });

    //    var mod = new Mock<IMod>();
    //    mod.Setup(m => m.DependencyResolveStatus).Returns(DependencyResolveStatus.Resolved);

    //    var dep = new Mock<IMod>();
    //    dep.Setup(m => m.InstalledLanguages).Returns(new HashSet<ILanguageInfo> { expected });
    //    mod.Setup(m => m.Dependencies).Returns(new List<ModDependencyEntry> { new(dep.Object) });

    //    var langs = finder.Object.FindInstalledLanguages(mod.Object);

    //    Assert.Single(langs);
    //    Assert.Equal(expected, langs.FirstOrDefault());
    //}

    //[Fact]
    //public void NullSp_Throws()
    //{
    //    Assert.Throws<ArgumentNullException>(() => new GameLanguageFinder(null));
    //}

    //[Fact]
    //public void TestEmptyResult()
    //{
    //    var fs = new MockFileSystem();
    //    fs.Initialize().WithSubdirectory("Game");

    //    var game = new Mock<IGame>();
    //    game.Setup(g => g.Directory).Returns(fs.DirectoryInfo.New("Game"));

    //    var languageHelper = new Mock<ILanguageFinder>();
    //    languageHelper
    //        .Setup(h => h.Merge(It.IsAny<IEnumerable<ILanguageInfo>[]>()))
    //        .Returns(new HashSet<ILanguageInfo>());

    //    var sp = new Mock<IServiceProvider>();
    //    sp.Setup(p => p.GetService(typeof(ILanguageFinder))).Returns(languageHelper.Object);

    //    var finder = new GameLanguageFinder(sp.Object);
    //    var langs = finder.FindInstalledLanguages(game.Object);
    //    Assert.Empty(langs);
    //}

    //[Fact]
    //public void TestSomeResult()
    //{
    //    var fs = new MockFileSystem();
    //    fs.Initialize().WithSubdirectory("Game");

    //    var game = new Mock<IGame>();
    //    game.Setup(g => g.Directory).Returns(fs.DirectoryInfo.New("Game"));

    //    var langInfo = new Mock<ILanguageInfo>();

    //    var languageHelper = new Mock<ILanguageFinder>();
    //    languageHelper
    //        .Setup(h => h.Merge(It.IsAny<IEnumerable<ILanguageInfo>[]>()))
    //        .Returns(new HashSet<ILanguageInfo> { langInfo.Object });

    //    var sp = new Mock<IServiceProvider>();
    //    sp.Setup(p => p.GetService(typeof(ILanguageFinder))).Returns(languageHelper.Object);

    //    var finder = new GameLanguageFinder(sp.Object);
    //    var langs = finder.FindInstalledLanguages(game.Object);
    //    Assert.Equal(1, langs.Count);
    //}
}