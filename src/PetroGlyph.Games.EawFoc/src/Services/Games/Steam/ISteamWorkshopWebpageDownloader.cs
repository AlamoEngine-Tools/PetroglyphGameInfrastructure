using System.Globalization;
using System.Threading.Tasks;
using HtmlAgilityPack;

namespace PetroGlyph.Games.EawFoc.Services.Steam
{
    internal interface ISteamWorkshopWebpageDownloader
    {
        Task<HtmlDocument?> GetSteamWorkshopsPageHtmlAsync(ulong workshopId, CultureInfo? culture);
    }
}
