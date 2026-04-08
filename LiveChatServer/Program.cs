using LiveChatServer.Data;
using LiveChatServer.Middleware;
using LiveChatServer.Services;
using LiveChatServer.Services.Options;
using LiveChatServer.WebSockets;
using Microsoft.Extensions.Logging;
using System.Threading;
using Microsoft.Extensions.Options;

var builder = WebApplication.CreateBuilder(args);

// Configure logging for local development and CI visibility
builder.Logging.ClearProviders();
builder.Logging.AddConsole();
builder.Logging.SetMinimumLevel(LogLevel.Debug);

// Register message repository (SQLite) for persistence. The repository is a simple scaffold
// and can be replaced or extended later.
builder.Services.AddSingleton<IMessageRepository>(sp =>
    new SqliteMessageRepository("Data Source=chat.db"));

// Application services
builder.Services.AddSingleton<IConnectionManager, ConnectionManager>();
builder.Services.AddSingleton<IMessageHandler, MessageHandler>();

builder.Services.AddControllers();

// Bind ConnectionManager options from configuration (appsettings.json)
builder.Services.Configure<ConnectionManagerOptions>(builder.Configuration.GetSection("ConnectionManager"));
// Replay/backpressure options
builder.Services.Configure<LiveChatServer.Services.Options.ReplayOptions>(builder.Configuration.GetSection("Replay"));

// Semaphore shared across requests to limit concurrent replay requests.
builder.Services.AddSingleton<SemaphoreSlim>(sp =>
{
    var opts = sp.GetService<IOptions<LiveChatServer.Services.Options.ReplayOptions>>()?.Value;
    var max = opts?.ConcurrentReplayLimit > 0 ? opts.ConcurrentReplayLimit : 5;
    return new SemaphoreSlim(max, max);
});

var app = builder.Build();

// Return JSON errors for /api routes instead of default HTML error pages
app.UseMiddleware<ApiExceptionMiddleware>();

app.UseWebSockets();
app.MapControllers();

// Map websocket endpoint (simple middleware currently).
app.Map("/ws", builder => builder.UseMiddleware<WebSocketMiddleware>());

app.MapGet("/", () => "Hello World!");

app.Run();
