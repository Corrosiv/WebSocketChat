using System.Collections.Concurrent;
using System.Net.WebSockets;
using System.Threading.Tasks;

namespace LiveChatServer.Services
{
    public class ConnectionManager : IConnectionManager
    {
        private readonly ConcurrentDictionary<string, WebSocket> _connections = new();

        public Task AddConnectionAsync(string id, WebSocket socket)
        {
            _connections[id] = socket;
            return Task.CompletedTask;
        }

        public Task RemoveConnectionAsync(string id)
        {
            _connections.TryRemove(id, out _);
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
    }
}
