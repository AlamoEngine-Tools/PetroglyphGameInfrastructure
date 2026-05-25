using System;
using System.Globalization;
using System.Threading.Tasks;
using HtmlAgilityPack;
using PG.StarWarsGame.Infrastructure.Services.Steam;

namespace PG.StarWarsGame.Infrastructure.Test.ModServices;

internal sealed class FakeSteamWorkshopWebpageDownloader : ISteamWorkshopWebpageDownloader
{
    private const string FixtureNamespace = "PG.StarWarsGame.Infrastructure.Test.ModServices.Fixtures";

    public Task<HtmlDocument> GetSteamWorkshopsPageHtmlAsync(ulong workshopId, CultureInfo? culture)
    {
        var doc = new HtmlDocument();
        var resourceName = $"{FixtureNamespace}.workshop_{workshopId}.html";
        var assembly = typeof(FakeSteamWorkshopWebpageDownloader).Assembly;
        using var stream = assembly.GetManifestResourceStream(resourceName);
        if (stream is null)
            return Task.FromResult(doc);
        doc.Load(stream);
        return Task.FromResult(doc);
    }
}
