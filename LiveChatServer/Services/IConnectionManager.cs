using System.Net.WebSockets;
using System.Threading.Tasks;

namespace LiveChatServer.Services
{
    public interface IConnectionManager
    {
        Task AddConnectionAsync(string id, WebSocket socket);
        Task RemoveConnectionAsync(string id);
    }
}
