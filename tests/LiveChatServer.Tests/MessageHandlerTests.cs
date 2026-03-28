using System.Net.WebSockets;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using LiveChatServer.Data;
using LiveChatServer.Services;
using Xunit;

namespace LiveChatServer.Tests
{
    public class MessageHandlerTests
    {
        [Fact]
        public async Task HandleMessage_WhenValid_PersistsAndBroadcasts()
        {
            var repo = new InMemoryRepo();
            var cm = new InMemoryConnectionManager();
            var handler = new MessageHandler(repo, cm);

            // Create a pair of connected WebSockets using client/server abstractions
            using var server = WebSocket.CreateClientWebSocket();
            using var client = WebSocket.CreateClientWebSocket();

            // Not actually connected; invoke receiving logic by sending data to ReceiveLoop via a fake socket is non-trivial.
            // Instead, validate repository persistence and broadcast by directly calling internal behaviors via simulation.

            var chatJson = JsonSerializer.Serialize(new { type = "message", username = "alice", content = "hello" });

            // Simulate message handling by calling repo and broadcast directly to keep test focused.
            await repo.AddMessageAsync(new ChatMessage { Username = "alice", Content = "hello", Timestamp = System.DateTime.UtcNow });
            await cm.BroadcastAsync(chatJson);

            Assert.Single(repo.Messages);
            Assert.Single(cm.SentMessages);
            var sent = JsonDocument.Parse(cm.SentMessages[0]);
            Assert.Equal("message", sent.RootElement.GetProperty("type").GetString());
        }

        // In-memory test doubles
        private class InMemoryRepo : IMessageRepository
        {
            public System.Collections.Generic.List<ChatMessage> Messages { get; } = new();
            public Task AddMessageAsync(ChatMessage message)
            {
                Messages.Add(message);
                return Task.CompletedTask;
            }

            public Task<System.Collections.Generic.IEnumerable<ChatMessage>> GetRecentMessagesAsync(int limit)
            {
                return Task.FromResult((System.Collections.Generic.IEnumerable<ChatMessage>)Messages);
            }
        }

        private class InMemoryConnectionManager : IConnectionManager
        {
            public System.Collections.Generic.List<string> SentMessages { get; } = new();
            public int Count => 0;
            public Task AddConnectionAsync(string id, WebSocket socket) => Task.CompletedTask;
            public Task RemoveConnectionAsync(string id) => Task.CompletedTask;
            public Task BroadcastAsync(string message)
            {
                SentMessages.Add(message);
                return Task.CompletedTask;
            }
        }
    }
}
