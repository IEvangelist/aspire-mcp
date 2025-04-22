namespace AspireMcp.Server.Tools;

[McpServerToolType]
public sealed partial class WebContentTool(
    HttpClient client,
    IMemoryCache cache,
    ILogger<WebContentTool> logger) : IDisposable
{
    [
        McpServerTool,
        Description("""
            For a given URL, this tool scrapes the content (HTML) of the page and returns it as a string.
            """)
    ]
    public async Task<string> ScrapeContentAsync(string url)
    {
        var content = await cache.GetOrCreateAsync(url, async entry =>
        {
            entry.AbsoluteExpirationRelativeToNow = TimeSpan.FromHours(1);

            return await ScrapeContentInternalAsync(url);
        });

        return content ?? $"No content found for '{url}'.";
    }

    private async Task<string> ScrapeContentInternalAsync(string url)
    {
        try
        {            
            var html = await client.GetStringAsync(url);

            HtmlDocument doc = new();
            doc.LoadHtml(html);

            // Remove script and style elements
            var scriptsAndStyles = doc.DocumentNode.SelectNodes("//script | //style");
            if (scriptsAndStyles is not null)
            {
                foreach (var node in scriptsAndStyles)
                {
                    node.Remove();
                }
            }

            // Extract content from main elements
            var mainContent = doc.DocumentNode.SelectNodes("//main | //article | //div[@class='content']");
            if (mainContent is { Count: > 0 })
            {
                return CleanText(
                    string.Join("\n", mainContent.Select(n => n.InnerText))
                );
            }

            // Fallback to body
            var body = doc.DocumentNode.SelectSingleNode("//body");

            return body is not null ? CleanText(body.InnerText) : "No content found";
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error scraping {Url}: {Message}", url, ex.Message);

            return $"Error: {ex.Message}";
        }
    }

    private static string CleanText(string text)
    {
        // Remove extra whitespaces, line breaks and tabs
        text = WhitespaceRegex().Replace(text, " ");

        // Remove HTML entities
        text = HtmlRegex().Replace(text, " ");

        // Remove any remaining HTML tags
        text = XmlRegex().Replace(text, "");

        return text.Trim();
    }

    [GeneratedRegex(@"\s+")]
    private static partial Regex WhitespaceRegex();

    [GeneratedRegex(@"&\w+;")]
    private static partial Regex HtmlRegex();

    [GeneratedRegex(@"<[^>]*>")]
    private static partial Regex XmlRegex();

    void IDisposable.Dispose() => client.Dispose();
}
