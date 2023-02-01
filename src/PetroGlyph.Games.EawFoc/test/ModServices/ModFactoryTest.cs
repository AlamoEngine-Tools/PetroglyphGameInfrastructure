using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO.Abstractions;
using System.IO.Abstractions.TestingHelpers;
using System.Linq;
using EawModinfo.Model;
using EawModinfo.Spec;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using PetroGlyph.Games.EawFoc.Games;
using PetroGlyph.Games.EawFoc.Services;
using PetroGlyph.Games.EawFoc.Services.Detection;
using PetroGlyph.Games.EawFoc.Services.Name;
using Xunit;

namespace PetroGlyph.Games.EawFoc.Test.ModServices;

public class ModFactoryTest
{
    private readonly ModFactory _service;
    private readonly Mock<IModNameResolver> _nameResolver;
    private readonly Mock<IModinfoFileFinder> _modInfoFinder;
    private readonly Mock<IModReferenceLocationResolver> _locationResolver;
    private readonly MockFileSystem _fileSystem;

    public ModFactoryTest()
    {
        var sc = new ServiceCollection();
        _nameResolver = new Mock<IModNameResolver>();
        _modInfoFinder = new Mock<IModinfoFileFinder>();
        _locationResolver = new Mock<IModReferenceLocationResolver>();
        _fileSystem = new MockFileSystem();

        sc.AddTransient(_ => _nameResolver.Object);
        sc.AddTransient(_ => _locationResolver.Object);
        sc.AddTransient<IFileSystem>(_ => _fileSystem);

        _service = new ModFactory(_ => _modInfoFinder.Object, CultureInfo.InvariantCulture, sc.BuildServiceProvider());
    }


    [Fact]
    public void NullCtor_Throws()
    {
        Assert.Throws<ArgumentNullException>(() => new ModFactory(null, null, null));
    }

    [Fact]
    public void TestModInfoSpecCreation_NoModinfoFiles()
    {
        _fileSystem.AddDirectory("Mods/Name");
        var game = new Mock<IGame>();
        var modLoc = _fileSystem.DirectoryInfo.New("Mods/Name");
        _locationResolver.Setup(r => r.ResolveLocation(It.IsAny<IModReference>(), It.IsAny<IGame>()))
            .Returns(modLoc);

        _nameResolver.Setup(r => r.ResolveName(It.IsAny<IModReference>())).Returns("Name");

        _modInfoFinder.Setup(f => f.Find(It.IsAny<FindOptions>()))
            .Returns(new ModinfoFinderCollection(modLoc));

        var modRef = new ModReference("Mods/Name", ModType.Default);

        var mods = _service.FromReference(game.Object, modRef);
        Assert.Single(mods);
    }

    [Fact]
    public void TestModInfoSpecCreation_MainModinfoFiles()
    {
        _fileSystem.AddDirectory("Mods/Name");
        var game = new Mock<IGame>();
        var modLoc = _fileSystem.DirectoryInfo.New("Mods/Name");
        _locationResolver.Setup(r => r.ResolveLocation(It.IsAny<IModReference>(), It.IsAny<IGame>()))
            .Returns(modLoc);
        var modinfo = new Mock<IModinfo>();
        modinfo.Setup(m => m.Name).Returns("MyName");
        modinfo.Setup(m => m.Dependencies)
            .Returns(new DependencyList(new List<IModReference>(), DependencyResolveLayout.FullResolved));
        modinfo.Setup(m => m.Languages).Returns(new List<ILanguageInfo>());
        var mainFile = new Mock<IModinfoFile>();
        mainFile.Setup(m => m.FileKind).Returns(ModinfoFileKind.MainFile);
        mainFile.Setup(m => m.GetModinfo()).Returns(modinfo.Object);
        _modInfoFinder.Setup(f => f.Find(It.IsAny<FindOptions>()))
            .Returns(new ModinfoFinderCollection(modLoc, mainFile.Object));

        var modRef = new ModReference("Mods/Name", ModType.Default);

        var mods = _service.FromReference(game.Object, modRef);
        Assert.Single(mods);
    }

    [Fact]
    public void TestModInfoSpecCreation_VariantsOnly()
    {
        _fileSystem.AddDirectory("Mods/Name");
        var game = new Mock<IGame>();
        var modLoc = _fileSystem.DirectoryInfo.New("Mods/Name");
        _locationResolver.Setup(r => r.ResolveLocation(It.IsAny<IModReference>(), It.IsAny<IGame>()))
            .Returns(modLoc);

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

        _modInfoFinder.Setup(f => f.Find(It.IsAny<FindOptions>()))
            .Returns(new ModinfoFinderCollection(modLoc, new[] { variantFileA.Object, variantFileB.Object }));

            
        var modRef = new ModReference("Mods/Name", ModType.Default);

        var mods = _service.FromReference(game.Object, modRef);
        Assert.Equal(2, mods.Count());
    }

    [Fact]
    public void TestModCreation_NoModinfoFiles()
    {
        _fileSystem.AddDirectory("Mods/Name");
        var game = new Mock<IGame>();
        var modLoc = _fileSystem.DirectoryInfo.New("Mods/Name");
        _locationResolver.Setup(r => r.ResolveLocation(It.IsAny<IModReference>(), It.IsAny<IGame>()))
            .Returns(modLoc);

        _nameResolver.Setup(r => r.ResolveName(It.IsAny<IModReference>())).Returns("Name");

        _modInfoFinder.Setup(f => f.Find(It.IsAny<FindOptions>()))
            .Returns(new ModinfoFinderCollection(modLoc));

        var modRef = new ModReference("Mods/Name", ModType.Default);

        var mod = _service.FromReference(game.Object, modRef, null);
        Assert.NotNull(mod);
        Assert.Equal("Name", mod.Name);
    }

    [Fact]
    public void TestModCreation_WithModinfo()
    {
        _fileSystem.AddDirectory("Mods/Name");
        var game = new Mock<IGame>();
        var modLoc = _fileSystem.DirectoryInfo.New("Mods/Name");
            
        _locationResolver.Setup(r => r.ResolveLocation(It.IsAny<IModReference>(), It.IsAny<IGame>()))
            .Returns(modLoc);
        _nameResolver.Setup(r => r.ResolveName(It.IsAny<IModReference>())).Returns("Name");

        var modinfo = new Mock<IModinfo>();
        modinfo.Setup(m => m.Name).Returns("MyName");
        modinfo.Setup(m => m.Dependencies)
            .Returns(new DependencyList(new List<IModReference>(), DependencyResolveLayout.FullResolved));
        modinfo.Setup(m => m.Languages).Returns(new List<ILanguageInfo>());

        _modInfoFinder.Setup(f => f.Find(It.IsAny<FindOptions>()))
            .Returns(new ModinfoFinderCollection(modLoc));

        var modRef = new ModReference("Mods/Name", ModType.Default);

        var mod = _service.FromReference(game.Object, modRef, modinfo.Object);
        Assert.NotNull(mod);
        Assert.Equal("MyName", mod.Name);
    }

    [Fact]
    public void TestModCreation_SearchMainModinfo()
    {
        _fileSystem.AddDirectory("Mods/Name");
        var game = new Mock<IGame>();
        var modLoc = _fileSystem.DirectoryInfo.New("Mods/Name");
        _locationResolver.Setup(r => r.ResolveLocation(It.IsAny<IModReference>(), It.IsAny<IGame>()))
            .Returns(modLoc);

        var modinfo = new Mock<IModinfo>();
        modinfo.Setup(m => m.Name).Returns("MyName");
        modinfo.Setup(m => m.Dependencies)
            .Returns(new DependencyList(new List<IModReference>(), DependencyResolveLayout.FullResolved));
        modinfo.Setup(m => m.Languages).Returns(new List<ILanguageInfo>());

        _nameResolver.Setup(r => r.ResolveName(It.IsAny<IModReference>())).Returns("Name");

        var mainFile = new Mock<IModinfoFile>();
        mainFile.Setup(m => m.FileKind).Returns(ModinfoFileKind.MainFile);
        mainFile.Setup(m => m.GetModinfo()).Returns(modinfo.Object);
        _modInfoFinder.Setup(f => f.Find(It.IsAny<FindOptions>()))
            .Returns(new ModinfoFinderCollection(modLoc, mainFile.Object));

            
        var modRef = new ModReference("Mods/Name", ModType.Default);

        var mod = _service.FromReference(game.Object, modRef, true);
        Assert.NotNull(mod);
        Assert.Equal("MyName", mod.Name);
    }

    [Fact]
    public void TestModCreation_NoSearchMainModinfo()
    {
        _fileSystem.AddDirectory("Mods/Name");
        var game = new Mock<IGame>();
        var modLoc = _fileSystem.DirectoryInfo.New("Mods/Name");
        _locationResolver.Setup(r => r.ResolveLocation(It.IsAny<IModReference>(), It.IsAny<IGame>()))
            .Returns(modLoc);

        var modinfo = new Mock<IModinfo>();
        modinfo.Setup(m => m.Name).Returns("MyName");
        modinfo.Setup(m => m.Dependencies)
            .Returns(new DependencyList(new List<IModReference>(), DependencyResolveLayout.FullResolved));
        modinfo.Setup(m => m.Languages).Returns(new List<ILanguageInfo>());

        _nameResolver.Setup(r => r.ResolveName(It.IsAny<IModReference>())).Returns("Name");

        var mainFile = new Mock<IModinfoFile>();
        mainFile.Setup(m => m.FileKind).Returns(ModinfoFileKind.MainFile);
        mainFile.Setup(m => m.GetModinfo()).Returns(modinfo.Object);
        _modInfoFinder.Setup(f => f.Find(It.IsAny<FindOptions>()))
            .Returns(new ModinfoFinderCollection(modLoc, mainFile.Object));

            
        var modRef = new ModReference("Mods/Name", ModType.Default);

        var mod = _service.FromReference(game.Object, modRef, false);
        Assert.NotNull(mod);
        Assert.Equal("Name", mod.Name);
    }

    [Fact]
    public void TestVariantModCreation_NoVariants()
    {
        _fileSystem.AddDirectory("Mods/Name");
        var game = new Mock<IGame>();
        var modLoc = _fileSystem.DirectoryInfo.New("Mods/Name");
        _locationResolver.Setup(r => r.ResolveLocation(It.IsAny<IModReference>(), It.IsAny<IGame>()))
            .Returns(modLoc);

        var modinfo = new Mock<IModinfo>();
        modinfo.Setup(m => m.Name).Returns("MyName");
        modinfo.Setup(m => m.Dependencies)
            .Returns(new DependencyList(new List<IModReference>(), DependencyResolveLayout.FullResolved));
        modinfo.Setup(m => m.Languages).Returns(new List<ILanguageInfo>());

        _nameResolver.Setup(r => r.ResolveName(It.IsAny<IModReference>())).Returns("Name");

        _modInfoFinder.Setup(f => f.Find(It.IsAny<FindOptions>()))
            .Returns(new ModinfoFinderCollection(modLoc));
            
        var modRef = new ModReference("Mods/Name", ModType.Default);

        var mods = _service.VariantsFromReference(game.Object, modRef);
        Assert.Empty(mods);
    }

    [Fact]
    public void TestVariantModCreation_TwoVariants()
    {
        _fileSystem.AddDirectory("Mods/Name");
        var game = new Mock<IGame>();
        var modLoc = _fileSystem.DirectoryInfo.New("Mods/Name");
        _locationResolver.Setup(r => r.ResolveLocation(It.IsAny<IModReference>(), It.IsAny<IGame>()))
            .Returns(modLoc);

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

        _modInfoFinder.Setup(f => f.Find(It.IsAny<FindOptions>()))
            .Returns(new ModinfoFinderCollection(modLoc, new[] { variantFileA.Object, variantFileB.Object }));

            
        var modRef = new ModReference("Mods/Name", ModType.Default);

        var mods = _service.VariantsFromReference(game.Object, modRef);
        Assert.Equal(2, mods.Count());
    }
}