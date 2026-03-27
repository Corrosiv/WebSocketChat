using System.Net.WebSockets;
using System.Threading.Tasks;
using LiveChatServer.Services;
using Xunit;

namespace LiveChatServer.Tests
{
    public class ConnectionManagerUsernameTests
    {
        [Fact]
        public async Task SetUsername_ForUnknownConnection_DoesNotSetUsername()
        {
            var cm = new ConnectionManager();

            await cm.SetUsernameAsync("unknown", "alice");

            Assert.Null(cm.GetUsername("unknown"));
        }

        [Fact]
        public async Task RemoveConnection_RemovesUsernameMapping()
        {
            var cm = new ConnectionManager();
            var dummy = WebSocket.CreateClientWebSocket();

            await cm.AddConnectionAsync("1", dummy);
            await cm.SetUsernameAsync("1", "alice");
            Assert.Equal("alice", cm.GetUsername("1"));

            await cm.RemoveConnectionAsync("1");
            Assert.Null(cm.GetUsername("1"));
        }
    }
}
