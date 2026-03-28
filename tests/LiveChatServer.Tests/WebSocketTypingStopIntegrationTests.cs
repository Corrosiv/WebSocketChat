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
    public class WebSocketTypingStopIntegrationTests : IClassFixture<WebApplicationFactory<LiveChatServer.Program>>
    {
        private readonly WebApplicationFactory<LiveChatServer.Program> _factory;

        public WebSocketTypingStopIntegrationTests(WebApplicationFactory<LiveChatServer.Program> factory)
        {
            _factory = factory;
        }

        [Fact]
        public async Task TypingStopEvent_WhenSent_IsBroadcastToOtherClients()
        {
            using var client = _factory.CreateDefaultClient();
            var wsUri = new Uri(client.BaseAddress, "/ws");

            using var ws1 = new ClientWebSocket();
            using var ws2 = new ClientWebSocket();

            await ws1.ConnectAsync(wsUri, CancellationToken.None);
            await ws2.ConnectAsync(wsUri, CancellationToken.None);

            // Send join for both so usernames are known
            var join1 = JsonSerializer.Serialize(new { type = "join", username = "carol" });
            await ws1.SendAsync(new ArraySegment<byte>(Encoding.UTF8.GetBytes(join1)), WebSocketMessageType.Text, true, CancellationToken.None);

            var join2 = JsonSerializer.Serialize(new { type = "join", username = "dave" });
            await ws2.SendAsync(new ArraySegment<byte>(Encoding.UTF8.GetBytes(join2)), WebSocketMessageType.Text, true, CancellationToken.None);

            // Send typing=true then typing=false from ws1
            var typingStart = JsonSerializer.Serialize(new { type = "typing", username = "carol", isTyping = true });
            await ws1.SendAsync(new ArraySegment<byte>(Encoding.UTF8.GetBytes(typingStart)), WebSocketMessageType.Text, true, CancellationToken.None);

            var typingStop = JsonSerializer.Serialize(new { type = "typing", username = "carol", isTyping = false });
            await ws1.SendAsync(new ArraySegment<byte>(Encoding.UTF8.GetBytes(typingStop)), WebSocketMessageType.Text, true, CancellationToken.None);

            // ws2 should receive a typing=false event for carol
            var buffer = new byte[4096];
            var foundStop = false;
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
                                if (user == "carol" && isTyping == false)
                                {
                                    foundStop = true;
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

            Assert.True(foundStop, "Expected ws2 to receive a typing=false event from ws1 for carol");
        }
    }
}
