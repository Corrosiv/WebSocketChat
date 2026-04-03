using System;

namespace LiveChatServer.Dtos
{
    public class PresenceDto
    {
        public string Username { get; set; } = string.Empty;
        public DateTime ConnectedAt { get; set; }
        public DateTime? LastSeen { get; set; }
    }
}
