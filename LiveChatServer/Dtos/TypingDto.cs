using System;

namespace LiveChatServer.Dtos
{
    public class TypingDto
    {
        public string Username { get; set; } = string.Empty;
        public bool IsTyping { get; set; }
        public DateTime Timestamp { get; set; }
    }
}
