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
        // Associate a username with a connection id (join event).
        Task SetUsernameAsync(string connectionId, string username);
        // Get the username associated with a connection id, or null if none.
        string? GetUsername(string connectionId);
        // Get all active connection ids (test / monitoring helper).
        string[] GetConnectionIds();
        // Track typing state for a connection (true = typing, false = not typing)
        Task SetTypingAsync(string connectionId, bool isTyping);
        // Query typing state for a connection
        bool IsTyping(string connectionId);
        // Get list of usernames currently typing
        string[] GetTypingUsers();
    }
}
