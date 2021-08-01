using System.Collections.Specialized;
using System.Globalization;
using System.Net;
using System.Threading.Tasks;
using System.Web;
using HtmlAgilityPack;

namespace PetroGlyph.Games.EawFoc.Services.Steam
{
    internal class SteamWorkshopWebpageDownloader : ISteamWorkshopWebpageDownloader
    {
        private const string SteamWorkshopsBaseUrl = "https://steamcommunity.com/sharedfiles/filedetails/?";

        public async Task<HtmlDocument?> GetSteamWorkshopsPageHtmlAsync(ulong workshopId, CultureInfo? culture)
        {
            NameValueCollection queryString = HttpUtility.ParseQueryString(string.Empty);
            queryString.Add("id", workshopId.ToString());

            if (culture != null && !Equals(culture, CultureInfo.InvariantCulture))
                queryString.Add("l", culture.EnglishName.ToLower());

            try
            {
                var address = $"{SteamWorkshopsBaseUrl}{queryString}";
                var client = new WebClient();
                var reply = await client.DownloadStringTaskAsync(address);

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
}