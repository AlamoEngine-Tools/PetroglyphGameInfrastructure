using System;
using System.Collections.Generic;
using System.Globalization;
using PG.StarWarsGame.Infrastructure.Games;
using PG.StarWarsGame.Infrastructure.Services.Name;
using PG.StarWarsGame.Infrastructure.Testing.TestBases;
using Xunit;

namespace PG.StarWarsGame.Infrastructure.Test.GameServices;

public class EnglishGameNameResolverTest : CommonTestBase
{
    public static IEnumerable<object[]> GetCultures()
    {
        yield return [CultureInfo.InvariantCulture];
        yield return [CultureInfo.CurrentUICulture];
        yield return [new CultureInfo("en")];
        yield return [new CultureInfo("de")];
        yield return [new CultureInfo("es")];
        yield return [new CultureInfo("fr")];
    }

    [Theory]
    [MemberData(nameof(GetCultures))]
    public void ResolveName_IgnoreCulture(CultureInfo culture)
    {
        var resolver = new EnglishGameNameResolver();
        var id = CreateRandomGameIdentity();
        resolver.ResolveName(id, culture);
        var name = resolver.ResolveName(id, CultureInfo.CurrentCulture);

        Assert.Contains(id.Platform.ToString(), name);
        Assert.Contains(id.Type == GameType.Eaw ? "Empire at War" : "Forces of Corruption", name);
    }

    [Fact]
    public void ResolveName_NullArgThrows()
    {
        var resolver = new EnglishGameNameResolver();
        Assert.Throws<ArgumentNullException>(() => resolver.ResolveName(null!, CultureInfo.CurrentCulture));
    }
}