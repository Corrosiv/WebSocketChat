using System;

namespace LiveChatServer.Data
{
    public class ChatMessage
    {
        public int Id { get; set; }
        // Provide defaults to satisfy nullable reference type checks for the prototype.
        public string Username { get; set; } = string.Empty;
        public string Content { get; set; } = string.Empty;
        public DateTime Timestamp { get; set; }
    }
}
