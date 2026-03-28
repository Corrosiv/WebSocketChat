using System;
using System.Net.WebSockets;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.Testing;
using Xunit;

namespace LiveChatServer.Tests
{
    public class WebSocketTypingIntegrationTests : IClassFixture<WebApplicationFactory<LiveChatServer.Program>>
    {
        private readonly WebApplicationFactory<LiveChatServer.Program> _factory;

        public WebSocketTypingIntegrationTests(WebApplicationFactory<LiveChatServer.Program> factory)
        {
            _factory = factory;
        }

        [Fact]
        public async Task TypingEvent_WhenSent_IsBroadcastToOtherClients()
        {
            using var client = _factory.CreateDefaultClient();
            var wsUri = new Uri(client.BaseAddress, "/ws");

            using var ws1 = new ClientWebSocket();
            using var ws2 = new ClientWebSocket();

            await ws1.ConnectAsync(wsUri, CancellationToken.None);
            await ws2.ConnectAsync(wsUri, CancellationToken.None);

            // Send join for both so usernames are known
            var join1 = JsonSerializer.Serialize(new { type = "join", username = "alice" });
            await ws1.SendAsync(new ArraySegment<byte>(Encoding.UTF8.GetBytes(join1)), WebSocketMessageType.Text, true, CancellationToken.None);

            var join2 = JsonSerializer.Serialize(new { type = "join", username = "bob" });
            await ws2.SendAsync(new ArraySegment<byte>(Encoding.UTF8.GetBytes(join2)), WebSocketMessageType.Text, true, CancellationToken.None);

            // Send typing event from ws1
            var typing = JsonSerializer.Serialize(new { type = "typing", username = "alice", isTyping = true });
            await ws1.SendAsync(new ArraySegment<byte>(Encoding.UTF8.GetBytes(typing)), WebSocketMessageType.Text, true, CancellationToken.None);

            // ws2 should receive a typing event
            var buffer = new byte[4096];
            var found = false;
            var deadline = DateTime.UtcNow.AddSeconds(3);
            while (DateTime.UtcNow < deadline)
            {
                try
                {
                    using var cts = new CancellationTokenSource(500);
                    var result = await ws2.ReceiveAsync(new ArraySegment<byte>(buffer), cts.Token);
                    if (result.MessageType == WebSocketMessageType.Text)
                    {
                        var text = Encoding.UTF8.GetString(buffer, 0, result.Count);
                        try
                        {
                            using var doc = JsonDocument.Parse(text);
                            if (doc.RootElement.TryGetProperty("type", out var t) && t.GetString() == "typing")
                            {
                                var user = doc.RootElement.GetProperty("username").GetString();
                                var isTyping = doc.RootElement.GetProperty("isTyping").GetBoolean();
                                if (user == "alice" && isTyping)
                                {
                                    found = true;
                                    break;
                                }
                            }
                        }
                        catch { }
                    }
                    else if (result.MessageType == WebSocketMessageType.Close)
                    {
                        break;
                    }
                }
                catch (OperationCanceledException) { }
            }

            // Cleanup
            await ws1.CloseAsync(WebSocketCloseStatus.NormalClosure, "done", CancellationToken.None);
            await ws2.CloseAsync(WebSocketCloseStatus.NormalClosure, "done", CancellationToken.None);

            Assert.True(found, "Expected ws2 to receive typing event from ws1");
        }
    }
}
