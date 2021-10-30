using System;
using System.Collections.Generic;
using EawModinfo.Spec;
using PetroGlyph.Games.EawFoc.Games;
using Xunit;

namespace PetroGlyph.Games.EawFoc.Test
{
    public class PlayableObjectTest
    {
        [Fact]
        public void TestIconFind()
        {
            var obj = new PlayableObjectMock(null);
            Assert.Null(obj.ResetIcon());
            Assert.Equal("1", obj.IconFile);
            Assert.Equal("1", obj.IconFile);
            Assert.Equal("1", obj.ResetIcon());
            Assert.Equal("2", obj.IconFile);
        }

        [Fact]
        public void TestLanguage_Throws()
        {
            var obj = new PlayableObjectMock(() => null!);
            Assert.Throws<PetroglyphException>(() => obj.InstalledLanguages);
        }

        [Fact]
        public void TestLanguages()
        {
            var counter = 0;
            var obj = new PlayableObjectMock(() =>
            {
                counter++;
                return new HashSet<ILanguageInfo>();
            });
            Assert.Null(obj.ResetLanguages());
            var _ = obj.InstalledLanguages;
            Assert.Equal(1, counter);
            var __ = obj.InstalledLanguages;
            Assert.Equal(1, counter);
            obj.ResetLanguages();
            var ___ = obj.InstalledLanguages;
            Assert.Equal(2, counter);
        }

        private class PlayableObjectMock : PlayableObject
        {
            private readonly Func<ISet<ILanguageInfo>> _langSetFunc;
            private int _iconCounter;

            public override string Name => Name;
            public override IGame Game { get; }

            public PlayableObjectMock(Func<ISet<ILanguageInfo>> langSetFunc)
            {
                _langSetFunc = langSetFunc;
            }

            protected override string? ResolveIconFile()
            {
                _iconCounter++;
                return _iconCounter.ToString();
            }

            protected override ISet<ILanguageInfo> ResolveInstalledLanguages()
            {
                return _langSetFunc();
            }
        }
    }
}