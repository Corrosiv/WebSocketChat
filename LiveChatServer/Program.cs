using LiveChatServer.Data;
using LiveChatServer.Services;
using LiveChatServer.WebSockets;
using Microsoft.Extensions.Logging;

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

var app = builder.Build();

app.UseWebSockets();
app.MapControllers();

// Map websocket endpoint (simple middleware currently).
app.Map("/ws", builder => builder.UseMiddleware<WebSocketMiddleware>());

app.MapGet("/", () => "Hello World!");

app.Run();
