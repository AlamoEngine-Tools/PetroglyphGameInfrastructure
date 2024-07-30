using System;
using System.Collections.Generic;
using System.Globalization;
using EawModinfo.Model;
using EawModinfo.Spec;
using Moq;
using PG.StarWarsGame.Infrastructure.Mods;
using PG.StarWarsGame.Infrastructure.Services.Name;
using Xunit;

namespace PG.StarWarsGame.Infrastructure.Test.ModServices;

public class CompositeModNameResolverTest
{
    [Fact]
    public void NullArgTest_Throws()
    {
        Assert.Throws<ArgumentNullException>(() => new CompositeModNameResolver(null, null));
        Assert.Throws<ArgumentException>(() => new CompositeModNameResolver(new List<IModNameResolver>(), new Mock<IServiceProvider>().Object));
        Assert.Throws<ArgumentNullException>(() => new CompositeModNameResolver(new List<IModNameResolver> { null }, null));
        Assert.Throws<ArgumentNullException>(() => CompositeModNameResolver.CreateDefaultModNameResolver(null));
        var sp = new Mock<IServiceProvider>();
        Assert.Throws<ArgumentNullException>(() => new CompositeModNameResolver(new List<IModNameResolver> { null }, sp.Object).ResolveName(null, CultureInfo.CurrentCulture));
        Assert.Throws<ArgumentNullException>(() => new CompositeModNameResolver(new List<IModNameResolver> { null }, sp.Object).ResolveName(new ModReference(), null));
    }


    [Fact]
    public void SingleInternalResolver()
    {
        var sp = new Mock<IServiceProvider>();

        var internalResolver = new Mock<IModNameResolver>();
        internalResolver.Setup(r => r.ResolveName(It.IsAny<IModReference>(), It.IsAny<CultureInfo>()))
            .Returns("Name");

        var modRef = new ModReference("Id", ModType.Default);

        var resolver = new CompositeModNameResolver(new List<IModNameResolver> { internalResolver.Object }, sp.Object);

        var name1 = resolver.ResolveName(modRef, CultureInfo.InvariantCulture);

        Assert.Equal("Name", name1);
    }

    [Fact]
    public void RequiresInvariant_Throws()
    {
        var sp = new Mock<IServiceProvider>();

        var internalResolver = new Mock<IModNameResolver>();
        internalResolver.Setup(r => r.ResolveName(It.IsAny<IModReference>(), It.IsAny<CultureInfo>()))
            .Returns((string)null!);

        var modRef = new ModReference("Id", ModType.Default);

        var resolver = new CompositeModNameResolver(new List<IModNameResolver> { internalResolver.Object }, sp.Object);

        Assert.Throws<ModException>(() => resolver.ResolveName(modRef, CultureInfo.InvariantCulture));
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

        var resolver = new CompositeModNameResolver(
            new List<IModNameResolver> { internalResolverA.Object, internalResolverB.Object }, sp.Object);

        var name1 = resolver.ResolveName(modRef, CultureInfo.InvariantCulture);

        Assert.Equal("Name", name1);
    }

    [Fact]
    public void InternalThrows_Throws()
    {
        var sp = new Mock<IServiceProvider>();

        var internalResolver = new Mock<IModNameResolver>();
        internalResolver.Setup(r => r.ResolveName(It.IsAny<IModReference>(), It.IsAny<CultureInfo>()))
            .Throws<Exception>();

        var modRef = new ModReference("Id", ModType.Default);

        var resolver = new CompositeModNameResolver(new List<IModNameResolver> { internalResolver.Object }, sp.Object);

        Assert.Throws<ModException>(() => resolver.ResolveName(modRef, CultureInfo.InvariantCulture));
    }
}