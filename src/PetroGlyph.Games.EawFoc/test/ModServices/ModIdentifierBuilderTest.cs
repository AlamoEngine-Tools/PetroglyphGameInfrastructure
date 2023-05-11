using System.Collections.Generic;
using System.IO.Abstractions.TestingHelpers;
using AnakinRaW.CommonUtilities.FileSystem;
using EawModinfo.Model;
using EawModinfo.Spec;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using PetroGlyph.Games.EawFoc.Mods;
using PetroGlyph.Games.EawFoc.Services.Detection;
using Semver;
using Xunit;

namespace PetroGlyph.Games.EawFoc.Test.ModServices;

public class ModIdentifierBuilderTest
{
    private readonly ModIdentifierBuilder _service;
    private readonly Mock<IPathHelperService> _pathHelper;
    private readonly Mock<IPhysicalMod> _physicalMod;
    private readonly Mock<IVirtualMod> _virtualMod;
    private readonly MockFileSystem _fileSystem;

    public ModIdentifierBuilderTest()
    {
        var sc = new ServiceCollection();
        _pathHelper = new Mock<IPathHelperService>();
        sc.AddTransient(sp => _pathHelper.Object);
        _service = new ModIdentifierBuilder(sc.BuildServiceProvider());
        _physicalMod = new Mock<IPhysicalMod>();
        _virtualMod = new Mock<IVirtualMod>();
        _fileSystem = new MockFileSystem();
    }

    [Fact]
    public void TestDefaultMod()
    {
        _physicalMod.Setup(m => m.Type).Returns(ModType.Default);
        _physicalMod.Setup(m => m.Directory).Returns(_fileSystem.DirectoryInfo.New("ModPath"));

        _pathHelper.Setup(p => p.NormalizePath(It.IsAny<string>(), PathNormalizeOptions.Full)).Returns("c:\\modpath");

        var identifier = _service.Build(_physicalMod.Object);
        Assert.Equal("c:\\modpath", identifier);
    }

    [Fact]
    public void TestWorkshopMod()
    {
        _physicalMod.Setup(m => m.Type).Returns(ModType.Workshops);
        _physicalMod.Setup(m => m.Directory).Returns(_fileSystem.DirectoryInfo.New("12345"));

        var identifier = _service.Build(_physicalMod.Object);
        Assert.Equal("12345", identifier);
    }

    [Fact]
    public void TestVirtualMod()
    {
        var dep1 = new ModDependencyEntry(_physicalMod.Object);
        _physicalMod.Setup(m => m.GetHashCode()).Returns(1138);
        var dep2 = new ModDependencyEntry(_physicalMod.Object);
        _virtualMod.Setup(m => m.Type).Returns(ModType.Virtual);
        _virtualMod.Setup(m => m.Name).Returns("VirtualMod");
        _virtualMod.Setup(m => m.Dependencies).Returns(new List<ModDependencyEntry>{dep1, dep2});
        var identifier = _service.Build(_virtualMod.Object);
        Assert.Equal("VirtualMod-1138-1138", identifier);
    }

    [Fact]
    public void TestNormalizeDefault()
    {
        var mRef = new ModReference("path", ModType.Default, SemVersionRange.Parse("1.*"));
        var expected = new ModReference("c:\\path", ModType.Default, SemVersionRange.Parse("1.*"));
        _pathHelper.Setup(p => p.NormalizePath(It.IsAny<string>(), PathNormalizeOptions.Full)).Returns("c:\\path");
        var normalizedRef = _service.Normalize(mRef);
        Assert.Equal(expected, normalizedRef);
    }

    [Fact]
    public void TestNormalizeWorkshops()
    {
        var mRef = new ModReference("123456", ModType.Workshops, SemVersionRange.Parse("1.*"));
        var expected = new ModReference("123456", ModType.Workshops, SemVersionRange.Parse("1.*"));
        var normalizedRef = _service.Normalize(mRef);
        Assert.Equal(expected, normalizedRef);
    }

    [Fact]
    public void TestNormalizeVirtual()
    {
        var mRef = new ModReference("123456", ModType.Virtual, SemVersionRange.Parse("1.*"));
        var expected = new ModReference("123456", ModType.Virtual, SemVersionRange.Parse("1.*"));
        var normalizedRef = _service.Normalize(mRef);
        Assert.Equal(expected, normalizedRef);
    }
}