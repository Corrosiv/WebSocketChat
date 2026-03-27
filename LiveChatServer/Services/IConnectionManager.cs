using System.Net.WebSockets;
using System.Threading.Tasks;

namespace LiveChatServer.Services
{
    public interface IConnectionManager
    {
        Task AddConnectionAsync(string id, WebSocket socket);
        Task RemoveConnectionAsync(string id);
        // Number of active connections (read-only) for monitoring/testing.
        int Count { get; }
        // Broadcast a text message to all connected clients.
        Task BroadcastAsync(string message);
    }
}
