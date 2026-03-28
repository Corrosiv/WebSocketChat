using System;
using System.Net.WebSockets;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using LiveChatServer.Data;
using Microsoft.AspNetCore.Mvc.Testing;
using Xunit;

namespace LiveChatServer.Tests
{
    public class WebSocketPersistenceIntegrationTests : IClassFixture<WebApplicationFactory<LiveChatServer.Program>>
    {
        private readonly WebApplicationFactory<LiveChatServer.Program> _factory;

        public WebSocketPersistenceIntegrationTests(WebApplicationFactory<LiveChatServer.Program> factory)
        {
            _factory = factory;
        }

        [Fact]
        public async Task WebSocketSend_WhenMessageSent_IsPersistedAndReturnedByApi()
        {
            using var client = _factory.CreateDefaultClient();
            var wsUri = new Uri(client.BaseAddress, "/ws");

            using var ws = new ClientWebSocket();
            await ws.ConnectAsync(wsUri, CancellationToken.None);

            // Send join and message
            var join = JsonSerializer.Serialize(new { type = "join", username = "e2e-persist" });
            var joinBuf = Encoding.UTF8.GetBytes(join);
            await ws.SendAsync(new ArraySegment<byte>(joinBuf), WebSocketMessageType.Text, true, CancellationToken.None);

            var content = "persisted via ws";
            var msg = JsonSerializer.Serialize(new { type = "message", username = "e2e-persist", content });
            var msgBuf = Encoding.UTF8.GetBytes(msg);
            await ws.SendAsync(new ArraySegment<byte>(msgBuf), WebSocketMessageType.Text, true, CancellationToken.None);

            // Allow short time for handler to persist
            var deadline = DateTime.UtcNow.AddSeconds(3);
            var found = false;
            while (DateTime.UtcNow < deadline)
            {
                var resp = await client.GetAsync("/api/messages?limit=10");
                resp.EnsureSuccessStatusCode();
                var json = await resp.Content.ReadAsStringAsync();
                var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
                try
                {
                    var list = JsonSerializer.Deserialize<ChatMessage[]>(json, options);
                    if (list != null)
                    {
                        foreach (var m in list)
                        {
                            if (m.Username == "e2e-persist" && m.Content == content)
                            {
                                found = true;
                                break;
                            }
                        }
                    }
                }
                catch { }

                if (found) break;
                await Task.Delay(200);
            }

            await ws.CloseAsync(WebSocketCloseStatus.NormalClosure, "test done", CancellationToken.None);

            Assert.True(found, "Expected persisted message to be returned by GET /api/messages within timeout");
        }
    }
}
