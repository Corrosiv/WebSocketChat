// This file is intentionally left as a lightweight forwarding type for compatibility.
// The actual DTO has been moved to LiveChatServer.Dtos namespace (LiveChatServer/Dtos/MessageDto.cs).
using LiveChatServer.Dtos;

// keep the original type name in this namespace for any existing references that may import it
// (tests and other files should prefer LiveChatServer.Dtos.MessageDto going forward).
public class MessageDto : LiveChatServer.Dtos.MessageDto { }
