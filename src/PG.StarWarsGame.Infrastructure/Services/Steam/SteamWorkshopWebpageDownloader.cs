using System.Globalization;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web;
using HtmlAgilityPack;

namespace PG.StarWarsGame.Infrastructure.Services.Steam;

internal class SteamWorkshopWebpageDownloader : ISteamWorkshopWebpageDownloader
{
    private const string SteamWorkshopsBaseUrl = "https://steamcommunity.com/sharedfiles/filedetails/?";

    public async Task<HtmlDocument?> GetSteamWorkshopsPageHtmlAsync(ulong workshopId, CultureInfo? culture)
    {
        var queryString = HttpUtility.ParseQueryString(string.Empty);
        queryString.Add("id", workshopId.ToString());

        if (culture != null && !Equals(culture, CultureInfo.InvariantCulture))
            queryString.Add("l", culture.EnglishName.ToLower());

        try
        {
            var address = $"{SteamWorkshopsBaseUrl}{queryString}";
            using var client = new HttpClient();
            var reply = await client.GetStringAsync(address);
            var htmlDocument = new HtmlDocument();
            htmlDocument.LoadHtml(reply);
            return htmlDocument;
        }
        catch
        {
            return null;
        }
    }
}