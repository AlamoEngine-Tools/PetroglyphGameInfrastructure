using System.Globalization;
using System.Threading.Tasks;
using PG.StarWarsGame.Infrastructure.Services.Steam;
using Xunit;

namespace PG.StarWarsGame.Infrastructure.Test.GameServices;

public class SteamWorkshopWebpageDownloaderTest
{
    [Fact]
    public async Task GetSteamWorkshopsPageHtmlAsync()
    {
        var downloader = new SteamWorkshopWebpageDownloader();
        var html = await downloader.GetSteamWorkshopsPageHtmlAsync(1129810972, CultureInfo.InvariantCulture);
        var htmlDe = await downloader.GetSteamWorkshopsPageHtmlAsync(1129810972, new CultureInfo("de"));

        var lang = html!.GetElementbyId("language_pulldown").InnerText;
        var langDe = htmlDe!.GetElementbyId("language_pulldown").InnerText;

        Assert.Equal("language", lang);
        Assert.Equal("Sprache", langDe);
    }
}