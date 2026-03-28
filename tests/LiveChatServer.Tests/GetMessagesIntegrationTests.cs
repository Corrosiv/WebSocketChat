using System;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using LiveChatServer.Data;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace LiveChatServer.Tests
{
    public class GetMessagesIntegrationTests : IClassFixture<WebApplicationFactory<LiveChatServer.Program>>
    {
        private readonly WebApplicationFactory<LiveChatServer.Program> _factory;

        public GetMessagesIntegrationTests(WebApplicationFactory<LiveChatServer.Program> factory)
        {
            _factory = factory;
        }

        [Fact]
        public async Task GetMessages_WhenMessagesExist_ReturnsPersistedMessage()
        {
            using var client = _factory.CreateDefaultClient();

            // Insert a message directly via the repository in the test server's DI container
            using (var scope = _factory.Services.CreateScope())
            {
                var repo = scope.ServiceProvider.GetRequiredService<IMessageRepository>();
                await repo.AddMessageAsync(new ChatMessage
                {
                    Username = "testuser",
                    Content = "persisted",
                    Timestamp = DateTime.UtcNow
                });
            }

            var resp = await client.GetAsync("/api/messages?limit=1");
            resp.EnsureSuccessStatusCode();

            var json = await resp.Content.ReadAsStringAsync();
            var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
            var list = JsonSerializer.Deserialize<MessageDto[]>(json, options);

            Assert.NotNull(list);
            Assert.Single(list);
            Assert.Equal("message", list[0].Type);
            Assert.Equal("testuser", list[0].Username);
            Assert.Equal("persisted", list[0].Content);
            Assert.False(list[0].IsTyping);
        }
    }

    // simple DTO used to deserialize the controller response for assertions
    private class MessageDto
    {
        public string Type { get; set; } = string.Empty;
        public DateTime Timestamp { get; set; }
        public string Username { get; set; } = string.Empty;
        public string Content { get; set; } = string.Empty;
        public bool IsTyping { get; set; }
    }
}
