var builder = Host.CreateApplicationBuilder(args);

builder.Logging.AddConsole(consoleLogOptions =>
{
    // Configure all logs to go to stderr
    consoleLogOptions.LogToStandardErrorThreshold = LogLevel.Trace;
});

builder.Services.AddMemoryCache();
builder.Services.AddSingleton<WebContentReader>();
builder.Services.AddSingleton<WebCrawlerService>();
builder.Services.AddMcpServer()
                .WithStdioServerTransport()
                .WithTools<WebContentTool>();

using var app = builder.Build();

await app.RunAsync();