using System.Net.WebSockets;
using Microsoft.AspNetCore.Http;
using System.Threading.Tasks;

namespace LiveChatServer.WebSockets
{
    // Placeholder middleware to accept WebSocket connections.
    public class WebSocketMiddleware
    {
        private readonly RequestDelegate _next;

        public WebSocketMiddleware(RequestDelegate next) => _next = next;

        public async Task InvokeAsync(HttpContext context)
        {
            if (context.WebSockets.IsWebSocketRequest)
            {
                using var webSocket = await context.WebSockets.AcceptWebSocketAsync();
                // TODO: hand off to connection manager / message handler
                await Task.CompletedTask;
            }
            else
            {
                await _next(context);
            }
        }
    }
}
