using System.Net.WebSockets;
using System.Threading.Tasks;

namespace LiveChatServer.Services
{
    public interface IMessageHandler
    {
        // Handle incoming messages for a connection. Accepts a CancellationToken so the
        // receive loop can be cancelled when the HttpContext is aborted or the host is
        // shutting down.
        Task HandleAsync(string connectionId, WebSocket socket, System.Threading.CancellationToken cancellationToken);
    }
}
