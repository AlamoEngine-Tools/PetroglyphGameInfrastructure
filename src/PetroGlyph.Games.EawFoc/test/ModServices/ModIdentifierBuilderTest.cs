using System.Collections.Generic;
using System.IO.Abstractions;
using EawModinfo.Model;
using EawModinfo.Spec;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using PetroGlyph.Games.EawFoc.Mods;
using PetroGlyph.Games.EawFoc.Services.Detection;
using PG.TestingUtilities;
using Semver;
using Testably.Abstractions.Testing;
using Xunit;

namespace PG.StarWarsGame.Infrastructure.Test.ModServices;

public class ModIdentifierBuilderTest
{
    private readonly ModIdentifierBuilder _service;
    private readonly Mock<IPhysicalMod> _physicalMod;
    private readonly Mock<IVirtualMod> _virtualMod;
    private readonly MockFileSystem _fileSystem;

    public ModIdentifierBuilderTest()
    {
        _physicalMod = new Mock<IPhysicalMod>();
        _virtualMod = new Mock<IVirtualMod>();
        _fileSystem = new MockFileSystem();
        var sc = new ServiceCollection();
        sc.AddSingleton<IFileSystem>(_fileSystem);
        _service = new ModIdentifierBuilder(sc.BuildServiceProvider());
    }

    [PlatformSpecificFact(TestPlatformIdentifier.Windows)]
    public void TestDefaultMod_Windows()
    {
        _physicalMod.Setup(m => m.Type).Returns(ModType.Default);
        _physicalMod.Setup(m => m.Directory).Returns(_fileSystem.DirectoryInfo.New("ModPath"));

        var identifier = _service.Build(_physicalMod.Object);
        Assert.Equal("C:\\MODPATH", identifier);
    }

    [PlatformSpecificFact(TestPlatformIdentifier.Linux)]
    public void TestDefaultMod_Linux()
    {
        _physicalMod.Setup(m => m.Type).Returns(ModType.Default);
        _physicalMod.Setup(m => m.Directory).Returns(_fileSystem.DirectoryInfo.New("ModPath"));

        var identifier = _service.Build(_physicalMod.Object);
        Assert.Equal("/ModPath", identifier);
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

    [PlatformSpecificFact(TestPlatformIdentifier.Windows)]
    public void TestNormalizeDefault_Windows()
    {
        var mRef = new ModReference("path", ModType.Default, SemVersionRange.Parse("1.*"));
        var expected = new ModReference("C:\\PATH", ModType.Default, SemVersionRange.Parse("1.*"));
        var normalizedRef = _service.Normalize(mRef);
        Assert.Equal(expected, normalizedRef);
    }

    [PlatformSpecificFact(TestPlatformIdentifier.Linux)]
    public void TestNormalizeDefault_Linux()
    {
        var mRef = new ModReference("path", ModType.Default, SemVersionRange.Parse("1.*"));
        var expected = new ModReference("/path", ModType.Default, SemVersionRange.Parse("1.*"));
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