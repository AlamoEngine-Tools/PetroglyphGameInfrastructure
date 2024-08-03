using System;
using System.Globalization;
using EawModinfo.Model;
using EawModinfo.Spec;
using Moq;
using PG.StarWarsGame.Infrastructure.Services.Name;
using Xunit;

namespace PG.StarWarsGame.Infrastructure.Test.ModServices;

public class CompositeModNameResolverTest
{
    [Fact]
    public void NullArgTest_Throws()
    {
        Assert.Throws<ArgumentNullException>(() => new CompositeModNameResolver(null, null));
        Assert.Throws<ArgumentNullException>(() => new CompositeModNameResolver(new Mock<IServiceProvider>().Object, _ => null));
        Assert.Throws<ArgumentException>(() => new CompositeModNameResolver(new Mock<IServiceProvider>().Object, _ => []));
        Assert.Throws<ArgumentNullException>(() => new CompositeModNameResolver(null, _ => [null]));
        var sp = new Mock<IServiceProvider>();
        Assert.Throws<ArgumentNullException>(() => new CompositeModNameResolver(sp.Object, _ => [null]).ResolveName(null, CultureInfo.CurrentCulture));
        Assert.Throws<ArgumentNullException>(() => new CompositeModNameResolver(sp.Object, _ => [null]).ResolveName(new ModReference(), null));
    }


    [Fact]
    public void SingleInternalResolver()
    {
        var sp = new Mock<IServiceProvider>();

        var internalResolver = new Mock<IModNameResolver>();
        internalResolver.Setup(r => r.ResolveName(It.IsAny<IModReference>(), It.IsAny<CultureInfo>()))
            .Returns("Name");

        var modRef = new ModReference("Id", ModType.Default);

        var resolver = new CompositeModNameResolver(sp.Object, _ => [internalResolver.Object]);

        var name1 = resolver.ResolveName(modRef, CultureInfo.InvariantCulture);

        Assert.Equal("Name", name1);
    }

    [Fact]
    public void ReturnsNull()
    {
        var sp = new Mock<IServiceProvider>();

        var internalResolver = new Mock<IModNameResolver>();
        internalResolver.Setup(r => r.ResolveName(It.IsAny<IModReference>(), It.IsAny<CultureInfo>()))
            .Returns((string?)null);

        var modRef = new ModReference("Id", ModType.Default);

        var resolver = new CompositeModNameResolver(sp.Object, _ => [internalResolver.Object]);

        Assert.Null(resolver.ResolveName(modRef, CultureInfo.InvariantCulture));
    }


    [Fact]
    public void SecondResolverReturns()
    {
        var sp = new Mock<IServiceProvider>();

        var internalResolverA = new Mock<IModNameResolver>();
        internalResolverA.Setup(r => r.ResolveName(It.IsAny<IModReference>(), It.IsAny<CultureInfo>()))
            .Returns((string)null!);

        var internalResolverB = new Mock<IModNameResolver>();
        internalResolverB.Setup(r => r.ResolveName(It.IsAny<IModReference>(), It.IsAny<CultureInfo>()))
            .Returns("Name");

        var modRef = new ModReference("Id", ModType.Default);

        var resolver = new CompositeModNameResolver(sp.Object, _ => [internalResolverA.Object, internalResolverB.Object]);

        var name1 = resolver.ResolveName(modRef, CultureInfo.InvariantCulture);

        Assert.Equal("Name", name1);
    }

    [Fact]
    public void InternalThrows_Throws()
    {
        var sp = new Mock<IServiceProvider>();

        var internalResolver = new Mock<IModNameResolver>();
        internalResolver.Setup(r => r.ResolveName(It.IsAny<IModReference>(), It.IsAny<CultureInfo>()))
            .Throws<InvalidOperationException>();

        var modRef = new ModReference("Id", ModType.Default);

        var resolver = new CompositeModNameResolver(sp.Object, _ => [internalResolver.Object]);

        Assert.Throws<InvalidOperationException>(() => resolver.ResolveName(modRef, CultureInfo.InvariantCulture));
    }
}