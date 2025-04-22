namespace AspireMcp.Server.Services;

internal sealed class WebCrawlerService(
    HttpClient client,
    WebContentReader reader,
    ILogger<WebCrawlerService> logger)
{
    public async Task ScrapeWebsiteAsync(string rootUrl = "https://learn.microsoft.com/dotnet/aspire")
    {
        var robotsTxtUrl = $"{rootUrl.TrimEnd('/')}/robots.txt";

        var robotsContent = await GetUrlContentAsync(robotsTxtUrl);

        if (string.IsNullOrEmpty(robotsContent))
        {
            logger.LogInformation("Could not fetch robots.txt.");

            return;
        }

        var sitemapUrls = ParseRobotsTxtForSitemaps(robotsContent);

        foreach (var sitemapUrl in sitemapUrls)
        {
            logger.LogInformation("Found sitemap: {SitemapUrl}", sitemapUrl);

            await ScrapeSitemapAsync(sitemapUrl);
        }
    }

    // Step 2: Fetch robots.txt content
    public async Task<string> GetUrlContentAsync(string url)
    {
        try
        {
            return await client.GetStringAsync(url);
        }
        catch (Exception ex)
        {
            logger.LogError("Error fetching {Url}: {Message}", url, ex.Message);

            return string.Empty;
        }
    }

    // Step 3: Parse robots.txt for Sitemap URLs
    public static HashSet<string> ParseRobotsTxtForSitemaps(string robotsContent)
    {
        var sitemapUrls = new HashSet<string>();

        var lines = robotsContent.Split(['\r', '\n'], StringSplitOptions.RemoveEmptyEntries);

        foreach (var line in lines)
        {
            if (line.StartsWith("Sitemap:", StringComparison.OrdinalIgnoreCase))
            {
                var sitemapUrl = line["Sitemap:".Length..].Trim();
                sitemapUrls.Add(sitemapUrl);
            }
        }

        return sitemapUrls;
    }

    // Step 4: Scrape and parse the sitemap(s)
    public async Task ScrapeSitemapAsync(string sitemapUrl)
    {
        var sitemapContent = await GetUrlContentAsync(sitemapUrl);

        if (string.IsNullOrEmpty(sitemapContent))
        {
            logger.LogInformation("Could not fetch sitemap at {SitemapUrl}", sitemapUrl);

            return;
        }

        var urls = ParseSitemap(sitemapContent);

        foreach (var url in urls)
        {
            logger.LogInformation("Scraping URL: {Url}", url);

            await ScrapePageAsync(url);
        }
    }

    // Step 5: Parse the sitemap XML to extract URLs
    public HashSet<string> ParseSitemap(string sitemapContent)
    {
        var urls = new HashSet<string>();

        try
        {
            var doc = XDocument.Parse(sitemapContent);

            foreach (var urlElement in doc.Descendants("url"))
            {
                var locElement = urlElement.Element("loc");

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

    public async Task<string> ScrapePageAsync(string url)
    {
        return await reader.ScrapeContentAsync(url);
    }
}
