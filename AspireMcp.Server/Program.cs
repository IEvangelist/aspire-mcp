var builder = Host.CreateApplicationBuilder(args);

builder.Logging.AddConsole(consoleLogOptions =>
{
    // Configure all logs to go to stderr
    consoleLogOptions.LogToStandardErrorThreshold = LogLevel.Trace;
});

builder.Services.AddMemoryCache();
builder.Services
       .AddMcpServer()
       .WithStdioServerTransport()
       .WithTools<WebContentTool>();

using var app = builder.Build();

await app.RunAsync();