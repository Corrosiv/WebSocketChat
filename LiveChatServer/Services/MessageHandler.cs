using System.Net.WebSockets;
using System.Threading.Tasks;

namespace LiveChatServer.Services
{
    public class MessageHandler : IMessageHandler
    {
        public Task HandleAsync(string connectionId, WebSocket socket)
        {
            // TODO: implement receiving loop, parsing JSON messages, persistence and broadcast
            return Task.CompletedTask;
        }
    }
}
