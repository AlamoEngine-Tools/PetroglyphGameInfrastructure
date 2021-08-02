using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO.Abstractions.TestingHelpers;
using System.Linq;
using EawModinfo.Model;
using EawModinfo.Spec;
using Moq;
using PetroGlyph.Games.EawFoc.Games;
using PetroGlyph.Games.EawFoc.Services;
using PetroGlyph.Games.EawFoc.Services.Detection;
using PetroGlyph.Games.EawFoc.Services.Name;
using Xunit;

namespace PetroGlyph.Games.EawFoc.Test.ModServices
{
    public class ModFactoryTest
    {
        [Fact]
        public void NullCtor_Throws()
        {
            Assert.Throws<ArgumentNullException>(() => new ModFactory(null));
            Assert.Throws<ArgumentNullException>(() => new ModFactory(null, null, null));
            Assert.Throws<ArgumentNullException>(() => new ModFactory(null, null, null, null));
            var locResolver = new Mock<IModReferenceLocationResolver>();
            Assert.Throws<ArgumentNullException>(() => new ModFactory(locResolver.Object, null));
        }

        [Fact]
        public void TestModInfoSpecCreation_NoModinfoFiles()
        {
            var fs = new MockFileSystem();
            fs.AddDirectory("Mods/Name");
            var game = new Mock<IGame>();
            var sp = new Mock<IServiceProvider>();
            var modLoc = fs.DirectoryInfo.FromDirectoryName("Mods/Name");
            var locResolver = new Mock<IModReferenceLocationResolver>();
            locResolver.Setup(r => r.ResolveLocation(It.IsAny<IModReference>(), It.IsAny<IGame>()))
                .Returns(modLoc);

            var nameResolver = new Mock<IModNameResolver>();
            nameResolver.Setup(r => r.ResolveName(It.IsAny<IModReference>())).Returns("Name");

            var modinfoFinder = new Mock<IModinfoFileFinder>();
            modinfoFinder.Setup(f => f.Find(It.IsAny<FindOptions>()))
                .Returns(new ModinfoFinderCollection(modLoc));

            var factory = new ModFactory(locResolver.Object, _ => modinfoFinder.Object, nameResolver.Object,
                CultureInfo.InvariantCulture, sp.Object);

            var modRef = new ModReference("Mods/Name", ModType.Default);

            var mods = factory.FromReference(game.Object, modRef);
            Assert.Single(mods);
        }

        [Fact]
        public void TestModInfoSpecCreation_MainModinfoFiles()
        {
            var fs = new MockFileSystem();
            fs.AddDirectory("Mods/Name");
            var game = new Mock<IGame>();
            var sp = new Mock<IServiceProvider>();
            var modLoc = fs.DirectoryInfo.FromDirectoryName("Mods/Name");
            var locResolver = new Mock<IModReferenceLocationResolver>();
            locResolver.Setup(r => r.ResolveLocation(It.IsAny<IModReference>(), It.IsAny<IGame>()))
                .Returns(modLoc);

            var nameResolver = new Mock<IModNameResolver>();

            var modinfo = new Mock<IModinfo>();
            modinfo.Setup(m => m.Name).Returns("MyName");
            modinfo.Setup(m => m.Dependencies)
                .Returns(new DependencyList(new List<IModReference>(), DependencyResolveLayout.FullResolved));
            modinfo.Setup(m => m.Languages).Returns(new List<ILanguageInfo>());
            var mainFile = new Mock<IModinfoFile>();
            mainFile.Setup(m => m.FileKind).Returns(ModinfoFileKind.MainFile);
            mainFile.Setup(m => m.GetModinfo()).Returns(modinfo.Object);
            var modinfoFinder = new Mock<IModinfoFileFinder>();
            modinfoFinder.Setup(f => f.Find(It.IsAny<FindOptions>()))
                .Returns(new ModinfoFinderCollection(modLoc, mainFile.Object));

            var factory = new ModFactory(locResolver.Object, _ => modinfoFinder.Object, nameResolver.Object,
                CultureInfo.InvariantCulture, sp.Object);

            var modRef = new ModReference("Mods/Name", ModType.Default);

            var mods = factory.FromReference(game.Object, modRef);
            Assert.Single(mods);
        }

        [Fact]
        public void TestModInfoSpecCreation_VariantsOnly()
        {
            var fs = new MockFileSystem();
            fs.AddDirectory("Mods/Name");
            var game = new Mock<IGame>();
            var sp = new Mock<IServiceProvider>();
            var modLoc = fs.DirectoryInfo.FromDirectoryName("Mods/Name");
            var locResolver = new Mock<IModReferenceLocationResolver>();
            locResolver.Setup(r => r.ResolveLocation(It.IsAny<IModReference>(), It.IsAny<IGame>()))
                .Returns(modLoc);

            var nameResolver = new Mock<IModNameResolver>();

            var variantA = new Mock<IModinfo>();
            variantA.Setup(m => m.Name).Returns("MyNameA");
            variantA.Setup(m => m.Dependencies)
                .Returns(new DependencyList(new List<IModReference>(), DependencyResolveLayout.FullResolved));
            variantA.Setup(m => m.Languages).Returns(new List<ILanguageInfo>());
            var variantFileA = new Mock<IModinfoFile>();
            variantFileA.Setup(m => m.FileKind).Returns(ModinfoFileKind.VariantFile);
            variantFileA.Setup(m => m.GetModinfo()).Returns(variantA.Object);

            var variantB = new Mock<IModinfo>();
            variantB.Setup(m => m.Name).Returns("MyNameB");
            variantB.Setup(m => m.Dependencies)
                .Returns(new DependencyList(new List<IModReference>(), DependencyResolveLayout.FullResolved));
            variantB.Setup(m => m.Languages).Returns(new List<ILanguageInfo>());
            var variantFileB = new Mock<IModinfoFile>();
            variantFileB.Setup(m => m.FileKind).Returns(ModinfoFileKind.VariantFile);
            variantFileB.Setup(m => m.GetModinfo()).Returns(variantB.Object);

            var modinfoFinder = new Mock<IModinfoFileFinder>();
            modinfoFinder.Setup(f => f.Find(It.IsAny<FindOptions>()))
                .Returns(new ModinfoFinderCollection(modLoc, new[] { variantFileA.Object, variantFileB.Object }));


            var factory = new ModFactory(locResolver.Object, _ => modinfoFinder.Object, nameResolver.Object,
                CultureInfo.InvariantCulture, sp.Object);

            var modRef = new ModReference("Mods/Name", ModType.Default);

            var mods = factory.FromReference(game.Object, modRef);
            Assert.Equal(2, mods.Count());
        }

        [Fact]
        public void TestModCreation_NoModinfoFiles()
        {
            var fs = new MockFileSystem();
            fs.AddDirectory("Mods/Name");
            var game = new Mock<IGame>();
            var sp = new Mock<IServiceProvider>();
            var modLoc = fs.DirectoryInfo.FromDirectoryName("Mods/Name");
            var locResolver = new Mock<IModReferenceLocationResolver>();
            locResolver.Setup(r => r.ResolveLocation(It.IsAny<IModReference>(), It.IsAny<IGame>()))
                .Returns(modLoc);

            var nameResolver = new Mock<IModNameResolver>();
            nameResolver.Setup(r => r.ResolveName(It.IsAny<IModReference>())).Returns("Name");

            var modinfoFinder = new Mock<IModinfoFileFinder>();
            modinfoFinder.Setup(f => f.Find(It.IsAny<FindOptions>()))
                .Returns(new ModinfoFinderCollection(modLoc));

            var factory = new ModFactory(locResolver.Object, _ => modinfoFinder.Object, nameResolver.Object,
                CultureInfo.InvariantCulture, sp.Object);

            var modRef = new ModReference("Mods/Name", ModType.Default);

            var mod = factory.FromReference(game.Object, modRef, null);
            Assert.NotNull(mod);
            Assert.Equal("Name", mod.Name);
        }

        [Fact]
        public void TestModCreation_WithModinfo()
        {
            var fs = new MockFileSystem();
            fs.AddDirectory("Mods/Name");
            var game = new Mock<IGame>();
            var sp = new Mock<IServiceProvider>();
            var modLoc = fs.DirectoryInfo.FromDirectoryName("Mods/Name");
            var locResolver = new Mock<IModReferenceLocationResolver>();
            locResolver.Setup(r => r.ResolveLocation(It.IsAny<IModReference>(), It.IsAny<IGame>()))
                .Returns(modLoc);

            var modinfo = new Mock<IModinfo>();
            modinfo.Setup(m => m.Name).Returns("MyName");
            modinfo.Setup(m => m.Dependencies)
                .Returns(new DependencyList(new List<IModReference>(), DependencyResolveLayout.FullResolved));
            modinfo.Setup(m => m.Languages).Returns(new List<ILanguageInfo>());

            var nameResolver = new Mock<IModNameResolver>();
            nameResolver.Setup(r => r.ResolveName(It.IsAny<IModReference>())).Returns("Name");

            var modinfoFinder = new Mock<IModinfoFileFinder>();
            modinfoFinder.Setup(f => f.Find(It.IsAny<FindOptions>()))
                .Returns(new ModinfoFinderCollection(modLoc));

            var factory = new ModFactory(locResolver.Object, _ => modinfoFinder.Object, nameResolver.Object,
                CultureInfo.InvariantCulture, sp.Object);

            var modRef = new ModReference("Mods/Name", ModType.Default);

            var mod = factory.FromReference(game.Object, modRef, modinfo.Object);
            Assert.NotNull(mod);
            Assert.Equal("MyName", mod.Name);
        }

        [Fact]
        public void TestModCreation_SearchMainModinfo()
        {
            var fs = new MockFileSystem();
            fs.AddDirectory("Mods/Name");
            var game = new Mock<IGame>();
            var sp = new Mock<IServiceProvider>();
            var modLoc = fs.DirectoryInfo.FromDirectoryName("Mods/Name");
            var locResolver = new Mock<IModReferenceLocationResolver>();
            locResolver.Setup(r => r.ResolveLocation(It.IsAny<IModReference>(), It.IsAny<IGame>()))
                .Returns(modLoc);

            var modinfo = new Mock<IModinfo>();
            modinfo.Setup(m => m.Name).Returns("MyName");
            modinfo.Setup(m => m.Dependencies)
                .Returns(new DependencyList(new List<IModReference>(), DependencyResolveLayout.FullResolved));
            modinfo.Setup(m => m.Languages).Returns(new List<ILanguageInfo>());

            var nameResolver = new Mock<IModNameResolver>();
            nameResolver.Setup(r => r.ResolveName(It.IsAny<IModReference>())).Returns("Name");

            var mainFile = new Mock<IModinfoFile>();
            mainFile.Setup(m => m.FileKind).Returns(ModinfoFileKind.MainFile);
            mainFile.Setup(m => m.GetModinfo()).Returns(modinfo.Object);
            var modinfoFinder = new Mock<IModinfoFileFinder>();
            modinfoFinder.Setup(f => f.Find(It.IsAny<FindOptions>()))
                .Returns(new ModinfoFinderCollection(modLoc, mainFile.Object));

            var factory = new ModFactory(locResolver.Object, _ => modinfoFinder.Object, nameResolver.Object,
                CultureInfo.InvariantCulture, sp.Object);

            var modRef = new ModReference("Mods/Name", ModType.Default);

            var mod = factory.FromReference(game.Object, modRef, true);
            Assert.NotNull(mod);
            Assert.Equal("MyName", mod.Name);
        }

        [Fact]
        public void TestModCreation_NoSearchMainModinfo()
        {
            var fs = new MockFileSystem();
            fs.AddDirectory("Mods/Name");
            var game = new Mock<IGame>();
            var sp = new Mock<IServiceProvider>();
            var modLoc = fs.DirectoryInfo.FromDirectoryName("Mods/Name");
            var locResolver = new Mock<IModReferenceLocationResolver>();
            locResolver.Setup(r => r.ResolveLocation(It.IsAny<IModReference>(), It.IsAny<IGame>()))
                .Returns(modLoc);

            var modinfo = new Mock<IModinfo>();
            modinfo.Setup(m => m.Name).Returns("MyName");
            modinfo.Setup(m => m.Dependencies)
                .Returns(new DependencyList(new List<IModReference>(), DependencyResolveLayout.FullResolved));
            modinfo.Setup(m => m.Languages).Returns(new List<ILanguageInfo>());

            var nameResolver = new Mock<IModNameResolver>();
            nameResolver.Setup(r => r.ResolveName(It.IsAny<IModReference>())).Returns("Name");

            var mainFile = new Mock<IModinfoFile>();
            mainFile.Setup(m => m.FileKind).Returns(ModinfoFileKind.MainFile);
            mainFile.Setup(m => m.GetModinfo()).Returns(modinfo.Object);
            var modinfoFinder = new Mock<IModinfoFileFinder>();
            modinfoFinder.Setup(f => f.Find(It.IsAny<FindOptions>()))
                .Returns(new ModinfoFinderCollection(modLoc, mainFile.Object));


            var factory = new ModFactory(locResolver.Object, _ => modinfoFinder.Object, nameResolver.Object,
                CultureInfo.InvariantCulture, sp.Object);

            var modRef = new ModReference("Mods/Name", ModType.Default);

            var mod = factory.FromReference(game.Object, modRef, false);
            Assert.NotNull(mod);
            Assert.Equal("Name", mod.Name);
        }

        [Fact]
        public void TestVariantModCreation_NoVariants()
        {
            var fs = new MockFileSystem();
            fs.AddDirectory("Mods/Name");
            var game = new Mock<IGame>();
            var sp = new Mock<IServiceProvider>();
            var modLoc = fs.DirectoryInfo.FromDirectoryName("Mods/Name");
            var locResolver = new Mock<IModReferenceLocationResolver>();
            locResolver.Setup(r => r.ResolveLocation(It.IsAny<IModReference>(), It.IsAny<IGame>()))
                .Returns(modLoc);

            var modinfo = new Mock<IModinfo>();
            modinfo.Setup(m => m.Name).Returns("MyName");
            modinfo.Setup(m => m.Dependencies)
                .Returns(new DependencyList(new List<IModReference>(), DependencyResolveLayout.FullResolved));
            modinfo.Setup(m => m.Languages).Returns(new List<ILanguageInfo>());

            var nameResolver = new Mock<IModNameResolver>();
            nameResolver.Setup(r => r.ResolveName(It.IsAny<IModReference>())).Returns("Name");

            var modinfoFinder = new Mock<IModinfoFileFinder>();
            modinfoFinder.Setup(f => f.Find(It.IsAny<FindOptions>()))
                .Returns(new ModinfoFinderCollection(modLoc));


            var factory = new ModFactory(locResolver.Object, _ => modinfoFinder.Object, nameResolver.Object,
                CultureInfo.InvariantCulture, sp.Object);

            var modRef = new ModReference("Mods/Name", ModType.Default);

            var mods = factory.VariantsFromReference(game.Object, modRef);
            Assert.Empty(mods);
        }

        [Fact]
        public void TestVariantModCreation_TwoVariants()
        {
            var fs = new MockFileSystem();
            fs.AddDirectory("Mods/Name");
            var game = new Mock<IGame>();
            var sp = new Mock<IServiceProvider>();
            var modLoc = fs.DirectoryInfo.FromDirectoryName("Mods/Name");
            var locResolver = new Mock<IModReferenceLocationResolver>();
            locResolver.Setup(r => r.ResolveLocation(It.IsAny<IModReference>(), It.IsAny<IGame>()))
                .Returns(modLoc);

            var nameResolver = new Mock<IModNameResolver>();

            var variantA = new Mock<IModinfo>();
            variantA.Setup(m => m.Name).Returns("MyNameA");
            variantA.Setup(m => m.Dependencies)
                .Returns(new DependencyList(new List<IModReference>(), DependencyResolveLayout.FullResolved));
            variantA.Setup(m => m.Languages).Returns(new List<ILanguageInfo>());
            var variantFileA = new Mock<IModinfoFile>();
            variantFileA.Setup(m => m.FileKind).Returns(ModinfoFileKind.VariantFile);
            variantFileA.Setup(m => m.GetModinfo()).Returns(variantA.Object);

            var variantB = new Mock<IModinfo>();
            variantB.Setup(m => m.Name).Returns("MyNameB");
            variantB.Setup(m => m.Dependencies)
                .Returns(new DependencyList(new List<IModReference>(), DependencyResolveLayout.FullResolved));
            variantB.Setup(m => m.Languages).Returns(new List<ILanguageInfo>());
            var variantFileB = new Mock<IModinfoFile>();
            variantFileB.Setup(m => m.FileKind).Returns(ModinfoFileKind.VariantFile);
            variantFileB.Setup(m => m.GetModinfo()).Returns(variantB.Object);

            var modinfoFinder = new Mock<IModinfoFileFinder>();
            modinfoFinder.Setup(f => f.Find(It.IsAny<FindOptions>()))
                .Returns(new ModinfoFinderCollection(modLoc, new[] { variantFileA.Object, variantFileB.Object }));


            var factory = new ModFactory(locResolver.Object, _ => modinfoFinder.Object, nameResolver.Object,
                CultureInfo.InvariantCulture, sp.Object);

            var modRef = new ModReference("Mods/Name", ModType.Default);

            var mods = factory.VariantsFromReference(game.Object, modRef);
            Assert.Equal(2, mods.Count());
        }
    }
}