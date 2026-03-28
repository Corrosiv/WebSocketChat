using System;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using LiveChatServer.Data;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace LiveChatServer.Tests
{
    public class GetMessagesPagingTests : IClassFixture<WebApplicationFactory<LiveChatServer.Program>>
    {
        private readonly WebApplicationFactory<LiveChatServer.Program> _factory;

        public GetMessagesPagingTests(WebApplicationFactory<LiveChatServer.Program> factory)
        {
            _factory = factory;
        }

        [Fact]
        public async Task Paging_Works_WithLimitAndOffset()
        {
            using var client = _factory.CreateDefaultClient();

            // Insert 5 messages
            using (var scope = _factory.Services.CreateScope())
            {
                var repo = scope.ServiceProvider.GetRequiredService<IMessageRepository>();
                for (int i = 1; i <= 5; i++)
                {
                    await repo.AddMessageAsync(new ChatMessage
                    {
                        Username = "user",
                        Content = $"msg-{i}",
                        Timestamp = DateTime.UtcNow.AddSeconds(i)
                    });
                }
            }

            var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };

            // Page 1: limit=2 offset=0 -> should return newest two (ids 5 and 4) in ascending order [4,5]
            var resp1 = await client.GetAsync("/api/messages?limit=2&offset=0");
            resp1.EnsureSuccessStatusCode();
            var page1Json = await resp1.Content.ReadAsStringAsync();
            var page1 = JsonSerializer.Deserialize<PagedResponse<MessageDto>>(page1Json, options);
            Assert.NotNull(page1);
            Assert.Equal(5, page1.Total);
            var items1 = page1.Items.ToArray();
            Assert.Equal(2, items1.Length);
            Assert.Equal("msg-4", items1[0].Content);
            Assert.Equal("msg-5", items1[1].Content);

            // Page 2: limit=2 offset=2 -> next older two -> ids 3 and 2 => returned as [2,3]
            var resp2 = await client.GetAsync("/api/messages?limit=2&offset=2");
            resp2.EnsureSuccessStatusCode();
            var page2 = JsonSerializer.Deserialize<PagedResponse<MessageDto>>(await resp2.Content.ReadAsStringAsync(), options);
            Assert.NotNull(page2);
            var items2 = page2.Items.ToArray();
            Assert.Equal(2, items2.Length);
            Assert.Equal("msg-2", items2[0].Content);
            Assert.Equal("msg-3", items2[1].Content);

            // Page 3: limit=2 offset=4 -> remaining oldest -> id 1
            var resp3 = await client.GetAsync("/api/messages?limit=2&offset=4");
            resp3.EnsureSuccessStatusCode();
            var page3 = JsonSerializer.Deserialize<PagedResponse<MessageDto>>(await resp3.Content.ReadAsStringAsync(), options);
            Assert.NotNull(page3);
            var items3 = page3.Items.ToArray();
            Assert.Single(items3);
            Assert.Equal("msg-1", items3[0].Content);
        }

        private class MessageDto
        {
            public string Type { get; set; } = string.Empty;
            public DateTime Timestamp { get; set; }
            public string Username { get; set; } = string.Empty;
            public string Content { get; set; } = string.Empty;
            public bool IsTyping { get; set; }
        }
    }
}
