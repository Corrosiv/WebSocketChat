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

        public Task HandleAsync(string connectionId, WebSocket socket, System.Threading.CancellationToken cancellationToken)
        {
            return ReceiveLoopAsync(connectionId, socket, cancellationToken);
        }

        private async Task ReceiveLoopAsync(string connectionId, WebSocket socket, System.Threading.CancellationToken cancellationToken)
        {
            // Buffer handling: assemble fragments until EndOfMessage is true. Start with
            // a small buffer and expand as needed, but enforce a configurable max size
            // to avoid unbounded memory growth.
            var initialBufferSize = 4096;
            var maxMessageSize = 64 * 1024; // 64 KB default (configurable via options later)
            var buffer = new byte[initialBufferSize];

            while (socket.State == WebSocketState.Open && !cancellationToken.IsCancellationRequested)
            {
                var seg = new ArraySegment<byte>(buffer);
                WebSocketReceiveResult? result = null;

                try
                {
                    result = await socket.ReceiveAsync(seg, cancellationToken);
                }
                catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
                {
                    _logger.LogDebug("Receive loop cancelled for {ConnectionId}", connectionId);
                    break;
                }

                if (result.MessageType == WebSocketMessageType.Close)
                {
                    // Don't remove connection here — the middleware's finally block handles
                    // cleanup and leave-event broadcasting (it needs the username still mapped).
                    if (socket.State == WebSocketState.CloseReceived)
                    {
                        try { await socket.CloseOutputAsync(WebSocketCloseStatus.NormalClosure, "Closed by server", cancellationToken); } catch { }
                    }
                    break;
                }

                // If the message fits in the current buffer and is a complete frame, avoid
                // extra allocations. Otherwise, assemble across fragments.
                int totalBytes = result.Count;
                if (result.EndOfMessage && totalBytes <= buffer.Length)
                {
                    var msg = Encoding.UTF8.GetString(buffer, 0, result.Count);
                    await ProcessMessageAsync(connectionId, msg);
                    continue;
                }

                // Need to assemble fragments into a growable buffer.
                using var ms = new System.IO.MemoryStream();
                ms.Write(buffer, 0, result.Count);

                while (!result.EndOfMessage)
                {
                    // enforce max message size
                    if (ms.Length > maxMessageSize)
                    {
                        _logger.LogWarning("Message from {ConnectionId} exceeded max allowed size and will be dropped", connectionId);
                        // drain the remainder of the fragmented message
                        while (!result.EndOfMessage)
                        {
                            try { result = await socket.ReceiveAsync(seg, cancellationToken); } catch { break; }
                        }
                        ms.Dispose();
                        goto ContinueLoop;
                    }

                    // read next fragment
                    try
                    {
                        result = await socket.ReceiveAsync(seg, cancellationToken);
                    }
                    catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
                    {
                        _logger.LogDebug("Receive loop cancelled while reading fragments for {ConnectionId}", connectionId);
                        break;
                    }

                    if (result.MessageType == WebSocketMessageType.Close)
                    {
                        break;
                    }

                    ms.Write(buffer, 0, result.Count);
                }

                var assembledBytes = ms.ToArray();
                var assembled = Encoding.UTF8.GetString(assembledBytes, 0, assembledBytes.Length);
                await ProcessMessageAsync(connectionId, assembled);

            ContinueLoop: ;
            }
        }

        private async Task ProcessMessageAsync(string connectionId, string msg)
        {
            try
            {
                using var doc = JsonDocument.Parse(msg);
                if (doc.RootElement.TryGetProperty("type", out var t))
                {
                    var type = t.GetString();
                    if (type == "message")
                    {
                        var content = doc.RootElement.GetProperty("content").GetString() ?? string.Empty;
                        var username = doc.RootElement.TryGetProperty("username", out var u) 
                            ? (u.GetString() ?? string.Empty) 
                            : (_connections.GetUsername(connectionId) ?? string.Empty);
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
                    else if (type == "typing")
                    {
                        var isTyping = doc.RootElement.TryGetProperty("isTyping", out var tt) ? tt.GetBoolean() : true;
                        // Update connection typing state and broadcast a typing event to others
                        await _connections.SetTypingAsync(connectionId, isTyping);
                        var username = _connections.GetUsername(connectionId) ?? 
                            (doc.RootElement.TryGetProperty("username", out var u2) ? (u2.GetString() ?? string.Empty) : string.Empty);
                        var typingEvt = JsonSerializer.Serialize(new { type = "typing", username, isTyping, timestamp = DateTime.UtcNow });
                        await _connections.BroadcastAsync(typingEvt);
                        _logger.LogDebug("Connection {ConnectionId} typing={IsTyping}", connectionId, isTyping);
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
