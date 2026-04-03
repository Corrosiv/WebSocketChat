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
    public class WebSocketPersistenceIntegrationTests : IClassFixture<IsolatedChatAppFactory>
    {
        private readonly IsolatedChatAppFactory _factory;

        public WebSocketPersistenceIntegrationTests(IsolatedChatAppFactory factory)
        {
            _factory = factory;
        }

        [Fact]
        public async Task WebSocketSend_WhenMessageSent_IsPersistedAndReturnedByApi()
        {
            var wsClient = _factory.Server.CreateWebSocketClient();
            using var ws = await wsClient.ConnectAsync(new Uri("ws://localhost/ws"), CancellationToken.None);
            using var client = _factory.CreateClient();

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
                var resp = await client.GetAsync("/api/messages?limit=10&offset=0");
                resp.EnsureSuccessStatusCode();
                var json = await resp.Content.ReadAsStringAsync();
                try
                {
                    using var doc = JsonDocument.Parse(json);
                    if (doc.RootElement.TryGetProperty("items", out var items))
                    {
                        foreach (var m in items.EnumerateArray())
                        {
                            var u = m.TryGetProperty("username", out var uv) ? uv.GetString() : null;
                            var c = m.TryGetProperty("content", out var cv) ? cv.GetString() : null;
                            if (u == "e2e-persist" && c == content)
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
