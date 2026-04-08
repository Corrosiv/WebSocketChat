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
    public class WebSocketFragmentationTests : IClassFixture<IsolatedChatAppFactory>
    {
        private readonly IsolatedChatAppFactory _factory;

        public WebSocketFragmentationTests(IsolatedChatAppFactory factory)
        {
            _factory = factory;
        }

        [Fact]
        public async Task FragmentedLargeMessage_IsReassembledAndPersisted()
        {
            var wsClient = _factory.Server.CreateWebSocketClient();
            using var ws = await wsClient.ConnectAsync(new Uri("ws://localhost/ws"), CancellationToken.None);

            // Send join
            var join = JsonSerializer.Serialize(new { type = "join", username = "frag" });
            var joinBuf = Encoding.UTF8.GetBytes(join);
            await ws.SendAsync(new ArraySegment<byte>(joinBuf), WebSocketMessageType.Text, true, CancellationToken.None);

            // Build a large message (> 8KB) to force fragmentation
            var largeContent = new string('x', 10000);
            var msg = JsonSerializer.Serialize(new { type = "message", username = "frag", content = largeContent });
            var msgBuf = Encoding.UTF8.GetBytes(msg);

            // Send in chunks of 3000 bytes
            var chunkSize = 3000;
            var offset = 0;
            while (offset < msgBuf.Length)
            {
                var remaining = msgBuf.Length - offset;
                var sendBytes = Math.Min(chunkSize, remaining);
                var end = (offset + sendBytes) >= msgBuf.Length;
                var segment = new ArraySegment<byte>(msgBuf, offset, sendBytes);
                await ws.SendAsync(segment, WebSocketMessageType.Text, end, CancellationToken.None);
                offset += sendBytes;
            }

            // Wait and then call API to verify the message was persisted
            var client = _factory.CreateClient();
            var deadline = DateTime.UtcNow.AddSeconds(3);
            string body = null;
            while (DateTime.UtcNow < deadline)
            {
                var res = await client.GetAsync("/api/messages?limit=1");
                if (res.IsSuccessStatusCode)
                {
                    body = await res.Content.ReadAsStringAsync();
                    if (!string.IsNullOrEmpty(body) && body.Contains(largeContent.Substring(0, 50)))
                    {
                        break;
                    }
                }
                await Task.Delay(100);
            }

            Assert.NotNull(body);
            Assert.Contains(largeContent.Substring(0, 50), body);
        }

        [Fact]
        public async Task AbruptClientAbort_CleansUpAndServerRemainsResponsive()
        {
            var wsClient = _factory.Server.CreateWebSocketClient();
            var ws1 = await wsClient.ConnectAsync(new Uri("ws://localhost/ws"), CancellationToken.None);

            // Send join then abort abruptly
            var join = JsonSerializer.Serialize(new { type = "join", username = "abrt" });
            var joinBuf = Encoding.UTF8.GetBytes(join);
            await ws1.SendAsync(new ArraySegment<byte>(joinBuf), WebSocketMessageType.Text, true, CancellationToken.None);

            // Abort without close handshake
            ws1.Abort();
            ws1.Dispose();

            // Server should still accept new connections after abort
            using var ws2 = await wsClient.ConnectAsync(new Uri("ws://localhost/ws"), CancellationToken.None);
            var join2 = JsonSerializer.Serialize(new { type = "join", username = "after" });
            var join2Buf = Encoding.UTF8.GetBytes(join2);
            await ws2.SendAsync(new ArraySegment<byte>(join2Buf), WebSocketMessageType.Text, true, CancellationToken.None);

            // If no exception was thrown, test is successful. Optionally check API is responsive
            var client = _factory.CreateClient();
            var res = await client.GetAsync("/api/messages?limit=1");
            Assert.True(res.IsSuccessStatusCode);
        }
    }
}
