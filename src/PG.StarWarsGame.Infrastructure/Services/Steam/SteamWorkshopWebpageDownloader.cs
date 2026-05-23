using System;
using System.Globalization;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web;
using HtmlAgilityPack;

namespace PG.StarWarsGame.Infrastructure.Services.Steam;

internal class SteamWorkshopWebpageDownloader : ISteamWorkshopWebpageDownloader
{
    private const string SteamWorkshopsBaseUrl = "https://steamcommunity.com/sharedfiles/filedetails/?";
    private const int MaxRetries = 4;
    private static readonly TimeSpan InitialBackoff = TimeSpan.FromSeconds(1);
    private static readonly TimeSpan MaxBackoff = TimeSpan.FromSeconds(30);

    public async Task<HtmlDocument> GetSteamWorkshopsPageHtmlAsync(ulong workshopId, CultureInfo? culture)
    {
        var queryString = HttpUtility.ParseQueryString(string.Empty);
        queryString.Add("id", workshopId.ToString());

        if (culture != null && !Equals(culture, CultureInfo.InvariantCulture))
            queryString.Add("l", culture.EnglishName.ToLower());

        var address = $"{SteamWorkshopsBaseUrl}{queryString}";
        using var client = new HttpClient();

        var backoff = InitialBackoff;
        for (var attempt = 0; ; attempt++)
        {
            using var response = await client.GetAsync(address).ConfigureAwait(false);

            if ((int)response.StatusCode != 429)
            {
                response.EnsureSuccessStatusCode();
                var reply = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
                var htmlDocument = new HtmlDocument();
                htmlDocument.LoadHtml(reply);
                return htmlDocument;
            }

            if (attempt >= MaxRetries)
                response.EnsureSuccessStatusCode();

            var wait = GetRetryAfter(response) ?? backoff;
            if (wait > MaxBackoff)
                wait = MaxBackoff;
            await Task.Delay(wait).ConfigureAwait(false);

            backoff = TimeSpan.FromTicks(Math.Min(backoff.Ticks * 2, MaxBackoff.Ticks));
        }
    }

    private static TimeSpan? GetRetryAfter(HttpResponseMessage response)
    {
        var header = response.Headers.RetryAfter;
        if (header is null)
            return null;
        if (header.Delta is { } delta && delta > TimeSpan.Zero)
            return delta;
        if (header.Date is { } when)
        {
            var delay = when - DateTimeOffset.UtcNow;
            return delay > TimeSpan.Zero ? delay : null;
        }
        return null;
    }
}