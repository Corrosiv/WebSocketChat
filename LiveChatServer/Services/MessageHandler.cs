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

        public MessageHandler(IMessageRepository repo, IConnectionManager connections)
        {
            _repo = repo;
            _connections = connections;
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

                            var broadcast = JsonSerializer.Serialize(new { type = "message", username = chat.Username, content = chat.Content, timestamp = chat.Timestamp });
                            await _connections.BroadcastAsync(broadcast);
                        }
                        else if (type == "join")
                        {
                            var username = doc.RootElement.GetProperty("username").GetString() ?? string.Empty;
                            await _connections.SetUsernameAsync(connectionId, username);
                            var evt = JsonSerializer.Serialize(new { type = "join", username, timestamp = DateTime.UtcNow });
                            await _connections.BroadcastAsync(evt);
                        }
                    }
                }
                catch { /* ignore malformed messages for prototype */ }
            }
        }
    }
}
