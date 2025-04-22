internal sealed class WebCrawlerServic(HttpClient client)
{
    // Step 1: Scrape the robots.txt to get the sitemap URL
    public async Task ScrapeWebsite(string rootUrl)
    {
        var robotsTxtUrl = rootUrl.TrimEnd('/') + "/robots.txt";
        var robotsContent = await GetUrlContent(robotsTxtUrl);
        if (string.IsNullOrEmpty(robotsContent))
        {
            Console.WriteLine("Could not fetch robots.txt.");
            return;
        }

        var sitemapUrls = ParseRobotsTxtForSitemaps(robotsContent);
        foreach (var sitemapUrl in sitemapUrls)
        {
            Console.WriteLine($"Found sitemap: {sitemapUrl}");
            await ScrapeSitemap(sitemapUrl);
        }
    }

    // Step 2: Fetch robots.txt content
    public async Task<string> GetUrlContent(string url)
    {
        try
        {
            return await client.GetStringAsync(url);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error fetching {url}: {ex.Message}");
            return string.Empty;
        }
    }

    // Step 3: Parse robots.txt for Sitemap URLs
    public static List<string> ParseRobotsTxtForSitemaps(string robotsContent)
    {
        var sitemapUrls = new List<string>();
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
    public async Task ScrapeSitemap(string sitemapUrl)
    {
        var sitemapContent = await GetUrlContent(sitemapUrl);
        if (string.IsNullOrEmpty(sitemapContent))
        {
            Console.WriteLine($"Could not fetch sitemap at {sitemapUrl}");
            return;
        }

        var urls = ParseSitemap(sitemapContent);
        foreach (var url in urls)
        {
            Console.WriteLine($"Scraping URL: {url}");
            await ScrapePage(url);
        }
    }

    // Step 5: Parse the sitemap XML to extract URLs
    public static List<string> ParseSitemap(string sitemapContent)
    {
        var urls = new List<string>();
        try
        {
            var doc = XDocument.Parse(sitemapContent);
            foreach (var urlElement in doc.Descendants("url"))
            {
                var locElement = urlElement.Element("loc");
                if (locElement != null)
                {
                    urls.Add(locElement.Value);
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error parsing sitemap XML: {ex.Message}");
        }
        return urls;
    }

    // Step 6: Scrape individual pages
    public async Task ScrapePage(string url)
    {
        var pageContent = await GetUrlContent(url);
        if (!string.IsNullOrEmpty(pageContent))
        {
            var htmlDoc = new HtmlDocument();
            htmlDoc.LoadHtml(pageContent);

            // Example: Print all links on the page
            var links = htmlDoc.DocumentNode.SelectNodes("//a[@href]");
            if (links != null)
            {
                foreach (var link in links)
                {
                    var href = link.GetAttributeValue("href", string.Empty);
                    Console.WriteLine($"Link: {href}");
                }
            }
        }
    }
}
