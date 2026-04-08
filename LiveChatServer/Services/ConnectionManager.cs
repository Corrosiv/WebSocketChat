using System.Collections.Concurrent;
using System.Net.WebSockets;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using LiveChatServer.Services.Options;

namespace LiveChatServer.Services
{
    public class ConnectionManager : IConnectionManager
    {
        private readonly ConcurrentDictionary<string, WebSocket> _connections = new();
        private readonly ConcurrentDictionary<string, string> _usernames = new();
        private readonly ConcurrentDictionary<string, bool> _typing = new();
        private readonly ConcurrentDictionary<string, int> _failureCounts = new();
        private readonly int _failureThreshold;
        // Template uses doubled braces for JSON outer object so String.Format treats
        // them as literal braces.
        private readonly string _leaveEventTemplate = "{{ \"type\": \"leave\", \"username\": \"{0}\", \"timestamp\": \"{1:O}\" }}";
        private readonly ILogger<ConnectionManager> _logger;

        public ConnectionManager()
            : this(Microsoft.Extensions.Options.Options.Create(new ConnectionManagerOptions()), null)
        {
        }

        public ConnectionManager(IOptions<ConnectionManagerOptions> options, ILogger<ConnectionManager>? logger)
        {
            _logger = logger ?? Microsoft.Extensions.Logging.Abstractions.NullLogger<ConnectionManager>.Instance;
            _failureThreshold = options?.Value?.FailureThreshold > 0 ? options.Value.FailureThreshold : 3;
        }

        public Task AddConnectionAsync(string id, WebSocket socket)
        {
            _connections[id] = socket;
            // reset failure count on new connection
            _failureCounts[id] = 0;
            return Task.CompletedTask;
        }

        public async Task RemoveConnectionAsync(string id)
        {
            if (_connections.TryRemove(id, out var ws))
            {
                try
                {
                    if (ws.State == WebSocketState.Open || ws.State == WebSocketState.CloseReceived)
                    {
                        await ws.CloseAsync(WebSocketCloseStatus.NormalClosure, "Server cleanup", System.Threading.CancellationToken.None);
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogDebug(ex, "Error while closing websocket for {ConnectionId}", id);
                }
                try { ws.Dispose(); } catch { }
            }

            // Remove any associated username mapping to avoid stale entries.
            if (_usernames.TryRemove(id, out var username) && !string.IsNullOrEmpty(username))
            {
                // Emit a leave event so clients see the user left — ConnectionManager owns
                // connection lifecycle events to avoid duplication.
                try
                {
                    var leave = string.Format(_leaveEventTemplate, username, DateTime.UtcNow);
                    // fire-and-forget: best-effort broadcast about the leave
                    _ = BroadcastAsync(leave, System.Threading.CancellationToken.None);
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Failed to broadcast leave event for {ConnectionId}", id);
                }
            }
            // Clear typing state for the disconnected user.
            _typing.TryRemove(id, out _);
            // Clear failure tracking
            _failureCounts.TryRemove(id, out _);
            return;
        }

        public int Count => _connections.Count;

        // Expose connection ids for test verification.
        public string[] GetConnectionIds() => _connections.Keys.ToArray();

        public async Task BroadcastAsync(string message, System.Threading.CancellationToken cancellationToken = default)
        {
            var buffer = System.Text.Encoding.UTF8.GetBytes(message);
            var segment = new System.ArraySegment<byte>(buffer);

            foreach (var kv in _connections)
            {
                var id = kv.Key;
                var ws = kv.Value;
                if (ws.State == WebSocketState.Open)
                {
                    try
                    {
                        await ws.SendAsync(segment, WebSocketMessageType.Text, true, cancellationToken);
                        // reset failure count on success
                        _failureCounts[id] = 0;
                    }
                    catch (Exception ex)
                    {
                        var count = _failureCounts.AddOrUpdate(id, 1, (_, prev) => prev + 1);
                        _logger.LogWarning(ex, "Failed to send message to {ConnectionId} (failure #{Count})", id, count);

                        if (count >= _failureThreshold)
                        {
                            _logger.LogError("Connection {ConnectionId} exceeded failure threshold ({Threshold}) and will be removed", id, _failureThreshold);
                            // attempt removal
                            try
                            {
                                await RemoveConnectionAsync(id);
                            }
                            catch (Exception removeEx)
                            {
                                _logger.LogWarning(removeEx, "Error removing connection {ConnectionId} after repeated send failures", id);
                            }
                        }
                    }
                }
            }
        }

        public Task SetUsernameAsync(string connectionId, string username)
        {
            // Only set the username if the connection is known to avoid mapping unknown ids.
            if (_connections.ContainsKey(connectionId))
            {
                _usernames[connectionId] = username ?? string.Empty;
            }
            return Task.CompletedTask;
        }

        public string? GetUsername(string connectionId)
        {
            return _usernames.TryGetValue(connectionId, out var u) ? u : null;
        }

        public Task SetTypingAsync(string connectionId, bool isTyping)
        {
            if (_connections.ContainsKey(connectionId))
            {
                _typing[connectionId] = isTyping;
            }
            return Task.CompletedTask;
        }

        public bool IsTyping(string connectionId)
        {
            return _typing.TryGetValue(connectionId, out var v) && v;
        }

        public string[] GetTypingUsers()
        {
            var users = new System.Collections.Generic.List<string>();
            foreach (var kv in _typing)
            {
                if (kv.Value && _usernames.TryGetValue(kv.Key, out var name) && !string.IsNullOrEmpty(name))
                {
                    users.Add(name);
                }
            }
            return users.ToArray();
        }
    }
}
