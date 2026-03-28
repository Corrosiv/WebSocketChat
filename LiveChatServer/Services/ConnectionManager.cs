using System.Collections.Concurrent;
using System.Net.WebSockets;
using System.Threading.Tasks;

namespace LiveChatServer.Services
{
    public class ConnectionManager : IConnectionManager
    {
        private readonly ConcurrentDictionary<string, WebSocket> _connections = new();
        private readonly ConcurrentDictionary<string, string> _usernames = new();
        private readonly ConcurrentDictionary<string, bool> _typing = new();

        public Task AddConnectionAsync(string id, WebSocket socket)
        {
            _connections[id] = socket;
            return Task.CompletedTask;
        }

        public Task RemoveConnectionAsync(string id)
        {
            _connections.TryRemove(id, out _);
            // Remove any associated username mapping to avoid stale entries.
            _usernames.TryRemove(id, out _);
            return Task.CompletedTask;
        }

        public int Count => _connections.Count;

        // Expose connection ids for test verification.
        public string[] GetConnectionIds() => _connections.Keys.ToArray();

        public async Task BroadcastAsync(string message)
        {
            var buffer = System.Text.Encoding.UTF8.GetBytes(message);
            var segment = new System.ArraySegment<byte>(buffer);
            foreach (var ws in _connections.Values)
            {
                if (ws.State == WebSocketState.Open)
                {
                    try
                    {
                        await ws.SendAsync(segment, WebSocketMessageType.Text, true, System.Threading.CancellationToken.None);
                    }
                    catch { /* best-effort broadcast for prototype */ }
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
