using System.Globalization;
using System.Threading.Tasks;
using HtmlAgilityPack;

namespace PG.StarWarsGame.Infrastructure.Services.Steam;

internal interface ISteamWorkshopWebpageDownloader
{
    Task<HtmlDocument?> GetSteamWorkshopsPageHtmlAsync(ulong workshopId, CultureInfo? culture);
}