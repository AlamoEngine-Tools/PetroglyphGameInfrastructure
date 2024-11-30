using System.IO.Abstractions;
using System.Runtime.InteropServices;
using EawModinfo.Spec;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using PG.StarWarsGame.Infrastructure.Mods;
using PG.StarWarsGame.Infrastructure.Services.Detection;
using PG.StarWarsGame.Infrastructure.Services.Steam;
using PG.StarWarsGame.Infrastructure.Testing;
using PG.StarWarsGame.Infrastructure.Testing.Mods;
using PG.TestingUtilities;
using Testably.Abstractions.Testing;
using Xunit;

namespace PG.StarWarsGame.Infrastructure.Test.ModServices;

public class ModIdentifierBuilderTest : CommonTestBaseWithRandomGame
{
    private readonly ModIdentifierBuilder _service;

    public ModIdentifierBuilderTest()
    {
        _service = new ModIdentifierBuilder(ServiceProvider);
    }

    [Fact]
    public void Build_DefaultMod()
    { 
        var mod = Game.InstallMod("Mod", false, ServiceProvider);

        var expected = mod.Directory.FullName.TrimEnd(FileSystem.Path.DirectorySeparatorChar, FileSystem.Path.AltDirectorySeparatorChar);
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            expected = expected.ToUpperInvariant();
        Assert.Equal(expected, _service.Build(mod));
    }

    //[Fact]
    //public void TestWorkshopMod()
    //{
    //    _physicalMod.Setup(m => m.Type).Returns(ModType.Workshops);
    //    _physicalMod.Setup(m => m.Directory).Returns(_fileSystem.DirectoryInfo.New("12345"));

    //    var identifier = _service.Build(_physicalMod.Object);
    //    Assert.Equal("12345", identifier);
    //}

    //[Fact]
    //public void TestVirtualMod()
    //{
    //    var dep1 = new ModDependencyEntry(_physicalMod.Object);
    //    _physicalMod.Setup(m => m.GetHashCode()).Returns(1138);
    //    var dep2 = new ModDependencyEntry(_physicalMod.Object);
    //    _virtualMod.Setup(m => m.Type).Returns(ModType.Virtual);
    //    _virtualMod.Setup(m => m.Name).Returns("VirtualMod");
    //    _virtualMod.Setup(m => m.Dependencies).Returns(new List<ModDependencyEntry>{dep1, dep2});
    //    var identifier = _service.Build(_virtualMod.Object);
    //    Assert.Equal("VirtualMod-1138-1138", identifier);
    //}

    //[PlatformSpecificFact(TestPlatformIdentifier.Windows)]
    //public void TestNormalizeDefault_Windows()
    //{
    //    var mRef = new ModReference("path", ModType.Default, SemVersionRange.Parse("1.*"));
    //    var expected = new ModReference("C:\\PATH", ModType.Default, SemVersionRange.Parse("1.*"));
    //    var normalizedRef = _service.Normalize(mRef);
    //    Assert.Equal(expected, normalizedRef);
    //}

    //[PlatformSpecificFact(TestPlatformIdentifier.Linux)]
    //public void TestNormalizeDefault_Linux()
    //{
    //    var mRef = new ModReference("path", ModType.Default, SemVersionRange.Parse("1.*"));
    //    var expected = new ModReference("/path", ModType.Default, SemVersionRange.Parse("1.*"));
    //    var normalizedRef = _service.Normalize(mRef);
    //    Assert.Equal(expected, normalizedRef);
    //}

    //[Fact]
    //public void TestNormalizeWorkshops()
    //{
    //    var mRef = new ModReference("123456", ModType.Workshops, SemVersionRange.Parse("1.*"));
    //    var expected = new ModReference("123456", ModType.Workshops, SemVersionRange.Parse("1.*"));
    //    var normalizedRef = _service.Normalize(mRef);
    //    Assert.Equal(expected, normalizedRef);
    //}

    //[Fact]
    //public void TestNormalizeVirtual()
    //{
    //    var mRef = new ModReference("123456", ModType.Virtual, SemVersionRange.Parse("1.*"));
    //    var expected = new ModReference("123456", ModType.Virtual, SemVersionRange.Parse("1.*"));
    //    var normalizedRef = _service.Normalize(mRef);
    //    Assert.Equal(expected, normalizedRef);
    //}
}