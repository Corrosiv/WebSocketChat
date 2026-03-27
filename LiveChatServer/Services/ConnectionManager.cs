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
    }
}
