namespace AspireMcp.Server.Services;

internal sealed record class WebCrawlerOptions(string RootUrl)
{
    internal string BaseAddress { get; } = new Uri(RootUrl).GetLeftPart(UriPartial.Authority);
}

internal sealed class WebCrawlerService(
    HttpClient client,
    WebContentReader reader,
    ILogger<WebCrawlerService> logger)
{
    private const string RobotsText = "robots.txt";
    private const string Sitemap = "Sitemap:";
    private const string Url = "url";
    private const string Loc = "loc";

    internal async Task StartCrawlingWebsiteAsync(WebCrawlerOptions options, CancellationToken token)
    {
        var robotsTxtUrl = $"{options.BaseAddress}/{RobotsText}";

        var robotsContent = await GetUrlContentAsync(robotsTxtUrl, token);

        if (string.IsNullOrEmpty(robotsContent))
        {
            logger.LogInformation("Could not fetch {File}.", robotsTxtUrl);

            return;
        }

        var sitemapUrls = ParseRobotsTxtForSitemaps(robotsContent);

        await Parallel.ForEachAsync(sitemapUrls, async (sitemapUrl, token) =>
        {
            logger.LogInformation("Found sitemap: {SitemapUrl}", sitemapUrl);

            await ScrapeSitemapAsync(sitemapUrl, token);
        });
    }

    private async Task<string> GetUrlContentAsync(string url, CancellationToken token)
    {
        try
        {
            return await client.GetStringAsync(url, token);
        }
        catch (Exception ex)
        {
            logger.LogError("Error fetching {Url}: {Message}", url, ex.Message);

            return "Unable to get content from URL";
        }
    }

    private static HashSet<string> ParseRobotsTxtForSitemaps(string robotsContent)
    {
        var sitemapUrls = new HashSet<string>();

        var lines = robotsContent.Split(['\r', '\n'], StringSplitOptions.RemoveEmptyEntries);

        foreach (var line in lines)
        {
            if (line.StartsWith(Sitemap, StringComparison.OrdinalIgnoreCase))
            {
                var sitemapUrl = line[Sitemap.Length..].Trim();

                sitemapUrls.Add(sitemapUrl);
            }
        }

        return sitemapUrls;
    }

    private async Task ScrapeSitemapAsync(string sitemapUrl, CancellationToken token)
    {
        var sitemapContent = await GetUrlContentAsync(sitemapUrl, token);

        if (string.IsNullOrEmpty(sitemapContent))
        {
            logger.LogInformation("Could not fetch sitemap at {SitemapUrl}", sitemapUrl);

            return;
        }

        var urls = ParseSitemap(sitemapContent);

        await Parallel.ForEachAsync(urls, async (url, token) =>
        {
            logger.LogInformation("Scraping URL: {Url}", url);

            await reader.ScrapeContentAsync(url);
        });
    }

    private HashSet<string> ParseSitemap(string sitemapContent)
    {
        var urls = new HashSet<string>();

        try
        {
            var doc = XDocument.Parse(sitemapContent);

            foreach (var urlElement in doc.Descendants(Url))
            {
                var locElement = urlElement.Element(Loc);

                if (locElement is not null)
                {
                    urls.Add(locElement.Value);
                }
            }
        }
        catch (Exception ex)
        {
            logger.LogInformation("Error parsing sitemap XML: {Message}", ex.Message);
        }

        return urls;
    }
}
