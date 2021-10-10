using System;
using System.Collections.Generic;
using System.IO.Abstractions.TestingHelpers;
using EawModinfo.Spec;
using Moq;
using PetroGlyph.Games.EawFoc.Games;
using PetroGlyph.Games.EawFoc.Services.Language;
using Xunit;

namespace PetroGlyph.Games.EawFoc.Test.GameServices
{
    public class GameLanguageFinderTest
    {
        [Fact]
        public void NullSp_Throws()
        {
            Assert.Throws<ArgumentNullException>(() => new GameLanguageFinder(null));
        }

        [Fact]
        public void TestEmptyResult()
        {
            var fs = new MockFileSystem();
            fs.AddDirectory("Game");

            var game = new Mock<IGame>();
            game.Setup(g => g.Directory).Returns(fs.DirectoryInfo.FromDirectoryName("Game"));

            var languageHelper = new Mock<ILanguageFinder>();
            languageHelper
                .Setup(h => h.Merge(It.IsAny<IEnumerable<ILanguageInfo>[]>()))
                .Returns(new HashSet<ILanguageInfo>());

            var sp = new Mock<IServiceProvider>();
            sp.Setup(p => p.GetService(typeof(ILanguageFinder))).Returns(languageHelper.Object);

            var finder = new GameLanguageFinder(sp.Object);
            var langs = finder.FindInstalledLanguages(game.Object);
            Assert.Empty(langs);
        }

        [Fact]
        public void TestSomeResult()
        {
            var fs = new MockFileSystem();
            fs.AddDirectory("Game");

            var game = new Mock<IGame>();
            game.Setup(g => g.Directory).Returns(fs.DirectoryInfo.FromDirectoryName("Game"));

            var langInfo = new Mock<ILanguageInfo>();

            var languageHelper = new Mock<ILanguageFinder>();
            languageHelper
                .Setup(h => h.Merge(It.IsAny<IEnumerable<ILanguageInfo>[]>()))
                .Returns(new HashSet<ILanguageInfo> { langInfo.Object });

            var sp = new Mock<IServiceProvider>();
            sp.Setup(p => p.GetService(typeof(ILanguageFinder))).Returns(languageHelper.Object);

            var finder = new GameLanguageFinder(sp.Object);
            var langs = finder.FindInstalledLanguages(game.Object);
            Assert.Equal(1, langs.Count);
        }
    }
}