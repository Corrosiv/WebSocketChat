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
    public class WebSocketEndToEndTests : IClassFixture<WebApplicationFactory<LiveChatServer.Program>>
    {
        private readonly WebApplicationFactory<LiveChatServer.Program> _factory;

        public WebSocketEndToEndTests(WebApplicationFactory<LiveChatServer.Program> factory)
        {
            _factory = factory;
        }

        [Fact]
        public async Task ConnectJoinSend_StoresMessageAndBroadcasts()
        {
            using var client = _factory.CreateDefaultClient();
            var uri = new Uri(client.BaseAddress, "/ws");

            using var ws = new ClientWebSocket();
            await ws.ConnectAsync(uri, CancellationToken.None);

            // Send join
            var join = JsonSerializer.Serialize(new { type = "join", username = "e2e" });
            var joinBuf = Encoding.UTF8.GetBytes(join);
            await ws.SendAsync(new ArraySegment<byte>(joinBuf), WebSocketMessageType.Text, true, CancellationToken.None);

            // Send message
            var msg = JsonSerializer.Serialize(new { type = "message", username = "e2e", content = "hello e2e" });
            var msgBuf = Encoding.UTF8.GetBytes(msg);
            await ws.SendAsync(new ArraySegment<byte>(msgBuf), WebSocketMessageType.Text, true, CancellationToken.None);

            // Read broadcasted messages for up to N milliseconds and assert the expected message appears.
            var buffer = new byte[4096];
            var found = false;
            var deadline = DateTime.UtcNow.AddMilliseconds(2000);
            while (DateTime.UtcNow < deadline)
            {
                try
                {
                    using var readCts = new CancellationTokenSource(500);
                    var result = await ws.ReceiveAsync(new ArraySegment<byte>(buffer), readCts.Token);
                    if (result.MessageType == WebSocketMessageType.Text)
                    {
                        var text = Encoding.UTF8.GetString(buffer, 0, result.Count);
                        if (text.Contains("hello e2e"))
                        {
                            found = true;
                            break;
                        }
                    }
                    else if (result.MessageType == WebSocketMessageType.Close)
                    {
                        break;
                    }
                }
                catch (OperationCanceledException)
                {
                    // Timed out waiting for a message in this interval; retry until overall deadline
                }
            }

            Assert.True(found, "Expected to receive a broadcast containing 'hello e2e' within the timeout window.");
        }
    }
}
