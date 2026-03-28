using System.Net.WebSockets;
using LiveChatServer.Services;
using Xunit;

namespace LiveChatServer.Tests
{
    public class ConnectionManagerTests
    {
        [Fact]
        public async void AddRemoveConnections_UpdatesCount()
        {
            var cm = new ConnectionManager();
            var dummy = WebSocket.CreateClientWebSocket();

            await cm.AddConnectionAsync("1", dummy);
            Assert.Equal(1, cm.Count);

            await cm.AddConnectionAsync("2", dummy);
            Assert.Equal(2, cm.Count);

            await cm.RemoveConnectionAsync("1");
            Assert.Equal(1, cm.Count);
        }
    }
}
