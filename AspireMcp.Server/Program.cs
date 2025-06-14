﻿var builder = Host.CreateApplicationBuilder(args);

builder.Logging.AddConsole(consoleLogOptions =>
{
    // Configure all logs to go to stderr
    consoleLogOptions.LogToStandardErrorThreshold = LogLevel.Trace;
});

builder.Services.AddHttpClient();
builder.Services.AddMemoryCache();
builder.Services.AddSingleton<WebContentReader>();
builder.Services.AddSingleton<WebCrawlerService>();
builder.Services.AddMcpServer()
                .WithStdioServerTransport()
                .WithTools<WebContentTool>();

using var app = builder.Build();

var crawler = app.Services.GetRequiredService<WebCrawlerService>();
var lifetime = app.Services.GetRequiredService<IHostApplicationLifetime>();

var crawlTask = crawler.StartCrawlingWebsiteAsync(
    new("https://learn.microsoft.com/dotnet/aspire"),
    lifetime.ApplicationStopping);

await Task.WhenAll(crawlTask, app.RunAsync());