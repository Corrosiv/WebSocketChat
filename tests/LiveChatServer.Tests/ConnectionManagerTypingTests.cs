using System.Net.WebSockets;
using System.Threading.Tasks;
using LiveChatServer.Services;
using Xunit;

namespace LiveChatServer.Tests
{
    public class ConnectionManagerTypingTests
    {
        [Fact]
        public async Task SetTyping_ForKnownConnection_SetsStateAndReturnsUsername()
        {
            var cm = new ConnectionManager();
            var dummy = WebSocket.CreateClientWebSocket();

            await cm.AddConnectionAsync("c1", dummy);
            await cm.SetUsernameAsync("c1", "alice");

            Assert.False(cm.IsTyping("c1"));

            await cm.SetTypingAsync("c1", true);
            Assert.True(cm.IsTyping("c1"));

            var typingUsers = cm.GetTypingUsers();
            Assert.Contains("alice", typingUsers);

            await cm.SetTypingAsync("c1", false);
            Assert.False(cm.IsTyping("c1"));
        }

        [Fact]
        public async Task SetTyping_ForUnknownConnection_DoesNotThrowAndNoState()
        {
            var cm = new ConnectionManager();

            await cm.SetTypingAsync("unknown", true);
            Assert.False(cm.IsTyping("unknown"));
            var typingUsers = cm.GetTypingUsers();
            Assert.Empty(typingUsers);
        }

        [Fact]
        public async Task RemoveConnection_ClearsTypingState()
        {
            var cm = new ConnectionManager();
            var dummy = WebSocket.CreateClientWebSocket();

            await cm.AddConnectionAsync("c2", dummy);
            await cm.SetUsernameAsync("c2", "bob");
            await cm.SetTypingAsync("c2", true);

            Assert.True(cm.IsTyping("c2"));

            await cm.RemoveConnectionAsync("c2");

            Assert.False(cm.IsTyping("c2"));
            var typingUsers = cm.GetTypingUsers();
            Assert.DoesNotContain("bob", typingUsers);
        }
    }
}
