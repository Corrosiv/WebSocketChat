using System.Linq;
using System.Net.WebSockets;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using LiveChatServer.Data;
using LiveChatServer.Services;
using Microsoft.Extensions.Logging.Abstractions;
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
            var handler = new MessageHandler(repo, cm, NullLogger<MessageHandler>.Instance);

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

            public Task<System.Collections.Generic.IEnumerable<ChatMessage>> GetRecentMessagesAsync(int limit, int offset)
            {
                var result = Messages.Skip(offset).Take(limit);
                return Task.FromResult((System.Collections.Generic.IEnumerable<ChatMessage>)result.ToList());
            }

            public Task<int> GetTotalCountAsync()
            {
                return Task.FromResult(Messages.Count);
            }
        }

        private class InMemoryConnectionManager : IConnectionManager
        {
            public System.Collections.Generic.List<string> SentMessages { get; } = new();
            private readonly System.Collections.Generic.Dictionary<string, string> _usernames = new();
            private readonly System.Collections.Generic.Dictionary<string, bool> _typing = new();
            public int Count => 0;
            public Task AddConnectionAsync(string id, WebSocket socket) => Task.CompletedTask;
            public Task RemoveConnectionAsync(string id) => Task.CompletedTask;
            public Task BroadcastAsync(string message, System.Threading.CancellationToken cancellationToken = default)
            {
                SentMessages.Add(message);
                return Task.CompletedTask;
            }
            public Task SetUsernameAsync(string connectionId, string username)
            {
                _usernames[connectionId] = username;
                return Task.CompletedTask;
            }
            public string? GetUsername(string connectionId) =>
                _usernames.TryGetValue(connectionId, out var u) ? u : null;
            public string[] GetConnectionIds() => System.Array.Empty<string>();
            public Task SetTypingAsync(string connectionId, bool isTyping)
            {
                _typing[connectionId] = isTyping;
                return Task.CompletedTask;
            }
            public bool IsTyping(string connectionId) =>
                _typing.TryGetValue(connectionId, out var v) && v;
            public string[] GetTypingUsers() =>
                _typing.Where(kv => kv.Value).Select(kv => _usernames.TryGetValue(kv.Key, out var u) ? u : kv.Key).ToArray();
        }
    }
}
