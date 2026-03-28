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
            var list = JsonSerializer.Deserialize<ChatMessage[]>(json, options);

            Assert.NotNull(list);
            Assert.Single(list);
            Assert.Equal("testuser", list[0].Username);
            Assert.Equal("persisted", list[0].Content);
        }
    }
}
