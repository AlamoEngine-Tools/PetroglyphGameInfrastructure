using System;
using System.IO.Abstractions.TestingHelpers;
using EawModinfo;
using EawModinfo.Model;
using EawModinfo.Spec;
using Moq;
using PetroGlyph.Games.EawFoc.Games;
using PetroGlyph.Games.EawFoc.Mods;
using Xunit;

namespace PetroGlyph.Games.EawFoc.Test
{
    public class ModTest
    {
        [Fact]
        public void InvalidCtor_Throws()
        {
            Assert.Throws<ArgumentNullException>(() => new Mod(null, null, false, (IModinfo)null, null));
            Assert.Throws<ArgumentNullException>(() => new Mod(null, null, false, (IModinfoFile)null, null));
            Assert.Throws<ArgumentNullException>(() => new Mod(null, null, false, (string)null, null));
            var game = new Mock<IGame>();
            Assert.Throws<ArgumentNullException>(() => new Mod(game.Object, null, false, (IModinfo)null, null));
            Assert.Throws<ArgumentNullException>(() => new Mod(game.Object, null, false, (IModinfoFile)null, null));
            Assert.Throws<ArgumentNullException>(() => new Mod(game.Object, null, false, (string)null, null));
            var fs = new MockFileSystem();
            var modDir = fs.DirectoryInfo.FromDirectoryName("Game/Mods/A");
            Assert.Throws<ArgumentNullException>(() => new Mod(game.Object, modDir, false, (IModinfo)null, null));
            Assert.Throws<ArgumentNullException>(() => new Mod(game.Object, modDir, false, (IModinfoFile)null, null));
            Assert.Throws<ArgumentNullException>(() => new Mod(game.Object, modDir, false, (string)null, null));
            Assert.Throws<ArgumentException>(() => new Mod(game.Object, modDir, false, string.Empty, null));
            var sp = new Mock<IServiceProvider>();
            Assert.Throws<ArgumentNullException>(() => new Mod(game.Object, modDir, false, (IModinfo)null, sp.Object));
            Assert.Throws<ArgumentNullException>(() => new Mod(game.Object, modDir, false, (IModinfoFile)null, sp.Object));
            Assert.Throws<ArgumentNullException>(() => new Mod(game.Object, modDir, false, (string)null, sp.Object));

            Assert.Throws<ArgumentNullException>(() => new Mod(game.Object, modDir, false, new Mock<IModinfo>().Object, null));
            Assert.Throws<ArgumentNullException>(() => new Mod(game.Object, modDir, false, new Mock<IModinfoFile>().Object, null));
            Assert.Throws<ArgumentNullException>(() => new Mod(game.Object, modDir, false, "name", null));

            Assert.Throws<ModinfoException>(() => new Mod(game.Object, modDir, false, new Mock<IModinfo>().Object, sp.Object));
        }

        [Fact]
        public void ValidCtors_Properties()
        {
            var game = new Mock<IGame>();
            var fs = new MockFileSystem();
            var modDir = fs.DirectoryInfo.FromDirectoryName("Game/Mods/A");
            var sp = new Mock<IServiceProvider>();
            var mod = new Mod(game.Object, modDir, false, "Name", sp.Object);
            Assert.Equal("Name", mod.Name);
            Assert.Equal(ModType.Default, mod.Type);
            Assert.Equal("C:\\Game\\Mods\\A", mod.Identifier);
            Assert.NotNull(mod.FileService);
            Assert.NotNull(mod.FileSystem);
            Assert.Null(mod.ModinfoFile);
            Assert.Null(mod.ModInfo);

            var file = new Mock<IModinfoFile>();
            file.Setup(f => f.GetModinfo()).Returns(new ModinfoData("Name"));
            var modA = new Mod(game.Object, modDir, false, file.Object, sp.Object);
            Assert.NotNull(modA.ModinfoFile);

            var modB = new Mod(game.Object, modDir, true, "Name", sp.Object);
            Assert.Equal("A", modB.Identifier);
        }
    }
}