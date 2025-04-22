namespace AspireMcp.Server.Tools;

[McpServerToolType]
internal sealed partial class WebContentTool(
    IMemoryCache cache,
    WebContentReader reader) : IDisposable
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

            return await reader.ScrapeContentAsync(url);
        });

        return content ?? $"No content found for '{url}'.";
    }

    void IDisposable.Dispose() => ((IDisposable)reader).Dispose();
}
