using System;

namespace LiveChatServer.Dtos
{
    public class MessageDto
    {
        public string Type { get; set; } = string.Empty;
        public DateTime Timestamp { get; set; }
        public string Username { get; set; } = string.Empty;
        public string Content { get; set; } = string.Empty;
        public bool IsTyping { get; set; }
    }
}
