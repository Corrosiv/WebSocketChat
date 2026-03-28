using System.Net.WebSockets;
using System.Threading.Tasks;
using LiveChatServer.Data;
using System.Text.Json;
using System.Text;
using System;

namespace LiveChatServer.Services
{
    public class MessageHandler : IMessageHandler
    {
        private readonly IMessageRepository _repo;
        private readonly IConnectionManager _connections;
        private readonly Microsoft.Extensions.Logging.ILogger<MessageHandler> _logger;

        public MessageHandler(IMessageRepository repo, IConnectionManager connections, Microsoft.Extensions.Logging.ILogger<MessageHandler> logger)
        {
            _repo = repo;
            _connections = connections;
            _logger = logger;
        }
                        else if (type == "typing")
                        {
                            var isTyping = doc.RootElement.TryGetProperty("isTyping", out var tt) ? tt.GetBoolean() : true;
                            // Update connection typing state and broadcast a typing event to others
                            await _connections.SetTypingAsync(connectionId, isTyping);
                            var username = _connections.GetUsername(connectionId) ?? (doc.RootElement.TryGetProperty("username", out var u2) ? u2.GetString() ?? string.Empty : string.Empty);
                            var typingEvt = JsonSerializer.Serialize(new { type = "typing", username, isTyping, timestamp = DateTime.UtcNow });
                            await _connections.BroadcastAsync(typingEvt);
                            _logger.LogDebug("Connection {ConnectionId} typing={IsTyping}", connectionId, isTyping);
                        }
        public Task HandleAsync(string connectionId, WebSocket socket)
        {
            return ReceiveLoopAsync(connectionId, socket);
        }

        private async Task ReceiveLoopAsync(string connectionId, WebSocket socket)
        {
            var buffer = new byte[4096];
            var seg = new ArraySegment<byte>(buffer);
            while (socket.State == WebSocketState.Open)
            {
                var result = await socket.ReceiveAsync(seg, System.Threading.CancellationToken.None);
                if (result.MessageType == WebSocketMessageType.Close)
                {
                    await _connections.RemoveConnectionAsync(connectionId);
                    await socket.CloseAsync(WebSocketCloseStatus.NormalClosure, "Closed by server", System.Threading.CancellationToken.None);
                    break;
                }

                var msg = Encoding.UTF8.GetString(buffer, 0, result.Count);
                try
                {
                    using var doc = JsonDocument.Parse(msg);
                    if (doc.RootElement.TryGetProperty("type", out var t))
                    {
                        var type = t.GetString();
                        if (type == "message")
                        {
                            var content = doc.RootElement.GetProperty("content").GetString() ?? string.Empty;
                            var username = doc.RootElement.TryGetProperty("username", out var u) ? u.GetString() ?? string.Empty : _connections.GetUsername(connectionId) ?? string.Empty;
                            var chat = new ChatMessage { Username = username, Content = content, Timestamp = DateTime.UtcNow };
                            await _repo.AddMessageAsync(chat);
                            _logger.LogInformation("Persisted message from {Username}: {Content}", chat.Username, chat.Content);

                            var broadcast = JsonSerializer.Serialize(new { type = "message", username = chat.Username, content = chat.Content, timestamp = chat.Timestamp });
                            await _connections.BroadcastAsync(broadcast);
                            _logger.LogDebug("Broadcasted message event for {ConnectionId}", connectionId);
                        }
                        else if (type == "join")
                        {
                            var username = doc.RootElement.GetProperty("username").GetString() ?? string.Empty;
                            await _connections.SetUsernameAsync(connectionId, username);
                            var evt = JsonSerializer.Serialize(new { type = "join", username, timestamp = DateTime.UtcNow });
                            await _connections.BroadcastAsync(evt);
                            _logger.LogInformation("Connection {ConnectionId} joined as {Username}", connectionId, username);
                        }
                        else if (type == "leave")
                        {
                            var username = doc.RootElement.GetProperty("username").GetString() ?? string.Empty;
                            var evt = JsonSerializer.Serialize(new { type = "leave", username, timestamp = DateTime.UtcNow });
                            await _connections.BroadcastAsync(evt);
                            _logger.LogInformation("Processed leave event for {ConnectionId} (user: {Username})", connectionId, username);
                        }
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Failed to parse or handle message from {ConnectionId}", connectionId);
                    // ignore malformed messages for prototype
                }
            }
        }
    }
}
