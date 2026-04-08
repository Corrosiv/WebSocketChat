using System;
using System.Collections.Generic;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using LiveChatServer.Services;
using Xunit;

namespace LiveChatServer.Tests
{
    public class ConnectionManagerFailureTests
    {
        [Fact]
        public async Task RepeatedSendFailures_RemoveConnectionAndBroadcastLeave()
        {
            var cm = new ConnectionManager();

            var goodSocket = new TestWebSocketCollector();
            var badSocket = new TestWebSocketThrower();

            await cm.AddConnectionAsync("good", goodSocket);
            await cm.AddConnectionAsync("bad", badSocket);

            // Associate usernames so leave event contains a username
            await cm.SetUsernameAsync("good", "alice");
            await cm.SetUsernameAsync("bad", "bob");

            // Trigger broadcast enough times to exceed default threshold (3)
            for (var i = 0; i < 3; i++)
            {
                await cm.BroadcastAsync("{\"type\":\"message\",\"content\":\"ping\"}");
            }

            // Wait briefly for the fire-and-forget leave broadcast to be delivered
            var sw = System.Diagnostics.Stopwatch.StartNew();
            while (sw.ElapsedMilliseconds < 1000 && goodSocket.SentMessages.Count < 1)
            {
                await Task.Delay(50);
            }

            // Bad connection should be removed
            var ids = cm.GetConnectionIds();
            Assert.DoesNotContain("bad", ids);

            // Good socket should have received at least one message (the leave event)
            Assert.True(goodSocket.SentMessages.Count >= 1, "Expected at least one message delivered to good socket");

            var foundLeave = false;
            foreach (var m in goodSocket.SentMessages)
            {
                if (m.Contains("\"type\": \"leave\"") && m.Contains("\"username\": \"bob\""))
                {
                    foundLeave = true;
                    break;
                }
            }

            Assert.True(foundLeave, "Expected a leave event for 'bob' to be broadcast to remaining clients");
        }

        // Test doubles
        private class TestWebSocketCollector : WebSocket
        {
            public List<string> SentMessages { get; } = new List<string>();
            private WebSocketState _state = WebSocketState.Open;

            public override WebSocketCloseStatus? CloseStatus => null;
            public override string? CloseStatusDescription => null;
            public override WebSocketState State => _state;
            public override string SubProtocol => null!;

            public override Task CloseAsync(WebSocketCloseStatus closeStatus, string? statusDescription, CancellationToken cancellationToken)
            {
                _state = WebSocketState.Closed;
                return Task.CompletedTask;
            }

            public override Task CloseOutputAsync(WebSocketCloseStatus closeStatus, string? statusDescription, CancellationToken cancellationToken)
            {
                _state = WebSocketState.Closed;
                return Task.CompletedTask;
            }

            public override void Abort() => _state = WebSocketState.Aborted;

            public override Task<WebSocketReceiveResult> ReceiveAsync(ArraySegment<byte> buffer, CancellationToken cancellationToken)
                => throw new NotSupportedException();

            public override Task SendAsync(ArraySegment<byte> buffer, WebSocketMessageType messageType, bool endOfMessage, CancellationToken cancellationToken)
            {
                var msg = Encoding.UTF8.GetString(buffer.Array!, buffer.Offset, buffer.Count);
                SentMessages.Add(msg);
                return Task.CompletedTask;
            }

            public override void Dispose() { }
        }

        private class TestWebSocketThrower : WebSocket
        {
            private WebSocketState _state = WebSocketState.Open;
            public override WebSocketCloseStatus? CloseStatus => null;
            public override string? CloseStatusDescription => null;
            public override WebSocketState State => _state;
            public override string SubProtocol => null!;

            public override Task CloseAsync(WebSocketCloseStatus closeStatus, string? statusDescription, CancellationToken cancellationToken)
            {
                _state = WebSocketState.Closed;
                return Task.CompletedTask;
            }

            public override Task CloseOutputAsync(WebSocketCloseStatus closeStatus, string? statusDescription, CancellationToken cancellationToken)
            {
                _state = WebSocketState.Closed;
                return Task.CompletedTask;
            }

            public override void Abort() => _state = WebSocketState.Aborted;

            public override Task<WebSocketReceiveResult> ReceiveAsync(ArraySegment<byte> buffer, CancellationToken cancellationToken)
                => throw new NotSupportedException();

            public override Task SendAsync(ArraySegment<byte> buffer, WebSocketMessageType messageType, bool endOfMessage, CancellationToken cancellationToken)
            {
                throw new InvalidOperationException("Simulated send failure");
            }

            public override void Dispose() { }
        }
    }
}
