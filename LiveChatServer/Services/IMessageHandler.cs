using System.Net.WebSockets;
using System.Threading.Tasks;

namespace LiveChatServer.Services
{
    public interface IMessageHandler
    {
        Task HandleAsync(string connectionId, WebSocket socket);
    }
}
