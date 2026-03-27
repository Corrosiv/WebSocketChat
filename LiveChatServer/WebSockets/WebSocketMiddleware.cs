using System;
using System.Net.WebSockets;
using System.Threading.Tasks;
using LiveChatServer.Services;
using Microsoft.AspNetCore.Http;

namespace LiveChatServer.WebSockets
{
    // Middleware that accepts WebSocket requests, registers the connection and hands
    // the socket off to the IMessageHandler for receive processing.
    public class WebSocketMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly IConnectionManager _connections;
        private readonly IMessageHandler _handler;

        public WebSocketMiddleware(RequestDelegate next, IConnectionManager connections, IMessageHandler handler)
        {
            _next = next;
            _connections = connections;
            _handler = handler;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            if (!context.WebSockets.IsWebSocketRequest)
            {
                await _next(context);
                return;
            }

            var webSocket = await context.WebSockets.AcceptWebSocketAsync();
            var connectionId = Guid.NewGuid().ToString();

            // Register connection so it is discoverable/broadcastable
            await _connections.AddConnectionAsync(connectionId, webSocket);

            try
            {
                // Hand off to the message handler which will process incoming messages
                // and perform persistence/broadcasting. The handler is expected to remove
                // the connection on close, but we defensively remove it here as well.
                await _handler.HandleAsync(connectionId, webSocket);
            }
            catch (Exception)
            {
                // Best-effort: ensure connection is removed on error.
                await _connections.RemoveConnectionAsync(connectionId);
                try { await webSocket.CloseAsync(WebSocketCloseStatus.InternalServerError, "Server error", System.Threading.CancellationToken.None); } catch { }
                throw;
            }
            finally
            {
                // Ensure cleanup if the handler did not already remove it
                await _connections.RemoveConnectionAsync(connectionId);
                try { webSocket.Dispose(); } catch { }
            }
        }
    }
}
