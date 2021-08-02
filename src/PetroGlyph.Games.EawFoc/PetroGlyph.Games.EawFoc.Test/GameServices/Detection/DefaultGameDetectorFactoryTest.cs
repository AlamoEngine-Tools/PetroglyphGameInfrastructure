using System;
using System.IO.Abstractions;
using System.IO.Abstractions.TestingHelpers;
using Moq;
using PetroGlyph.Games.EawFoc.Games.Registry;
using PetroGlyph.Games.EawFoc.Services.Detection;
using Xunit;

namespace PetroGlyph.Games.EawFoc.Test.GameServices.Detection
{
    public class DefaultGameDetectorFactoryTest
    {
        [Fact]
        public void Test()
        {
            var sp = new Mock<IServiceProvider>();
            sp.Setup(p => p.GetService(typeof(IFileSystem))).Returns(new MockFileSystem());
            var reg = new Mock<IGameRegistry>();
            var detector = DefaultGameDetectorFactory.CreateDefaultDetector(reg.Object, sp.Object);
            var composite = Assert.IsType<CompositeGameDetector>(detector);
            Assert.Equal(2, composite.SortedDetectors.Count);
            Assert.IsType<DirectoryGameDetector>(composite.SortedDetectors[0]);
            Assert.IsType<RegistryGameDetector>(composite.SortedDetectors[1]);
        }
    }
}