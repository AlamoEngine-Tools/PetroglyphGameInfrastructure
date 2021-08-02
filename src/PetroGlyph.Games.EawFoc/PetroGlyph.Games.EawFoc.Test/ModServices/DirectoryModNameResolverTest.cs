using System;
using System.Globalization;
using EawModinfo.Model;
using EawModinfo.Spec;
using Moq;
using PetroGlyph.Games.EawFoc.Services.Name;
using Xunit;

namespace PetroGlyph.Games.EawFoc.Test.ModServices
{
    public class DirectoryModNameResolverTest
    {
        [Fact]
        public void TestWithModsPath()
        {
            var sp = new Mock<IServiceProvider>();
            var resolver = new DirectoryModNameResolver(sp.Object);

            var modRef = new ModReference("Mods/Name", ModType.Default);
            var nameInv = resolver.ResolveCore(modRef, CultureInfo.InvariantCulture);
            var nameDe = resolver.ResolveCore(modRef, new CultureInfo("de"));

            Assert.Equal(nameInv, nameDe);
            Assert.Equal("Name", nameInv);
        }

        [Fact]
        public void TestWithoutPath()
        {
            var sp = new Mock<IServiceProvider>();
            var resolver = new DirectoryModNameResolver(sp.Object);

            var modRef = new ModReference("Name", ModType.Default);
            var nameInv = resolver.ResolveCore(modRef, CultureInfo.InvariantCulture);
            var nameDe = resolver.ResolveCore(modRef, new CultureInfo("de"));

            Assert.Equal(nameInv, nameDe);
            Assert.Equal("Name", nameInv);
        }

        [Fact]
        public void TestBeautify()
        {
            var sp = new Mock<IServiceProvider>();
            var resolver = new DirectoryModNameResolver(sp.Object);

            var modRef = new ModReference("Name_Name", ModType.Default);
            var nameInv = resolver.ResolveCore(modRef, CultureInfo.InvariantCulture);
            var nameDe = resolver.ResolveCore(modRef, new CultureInfo("de"));

            Assert.Equal(nameInv, nameDe);
            Assert.Equal("Name Name", nameInv);
        }

        [Fact]
        public void TestBeautify_EdgeCase()
        {
            var sp = new Mock<IServiceProvider>();
            var resolver = new DirectoryModNameResolver(sp.Object);

            var modRef = new ModReference("___", ModType.Default);
            var nameInv = resolver.ResolveCore(modRef, CultureInfo.InvariantCulture);
            var nameDe = resolver.ResolveCore(modRef, new CultureInfo("de"));

            Assert.Equal(nameInv, nameDe);
            Assert.Equal("___", nameInv);
        }

        [Fact]
        public void TestVirtualMod_Throws()
        {
            var sp = new Mock<IServiceProvider>();
            var resolver = new DirectoryModNameResolver(sp.Object);

            var modRef = new ModReference("___", ModType.Virtual);
            Assert.Throws<NotSupportedException>(() => resolver.ResolveCore(modRef, CultureInfo.InvariantCulture));
        }
    }
}