# Domain Models

Primary domain models and service state for the chat application.

## ChatMessage

Properties:
- `Id` (int) - primary key
- `Username` (string)
- `Content` (string)
- `Timestamp` (DateTime)

Example C# model:

```csharp
public class ChatMessage
{
    public int Id { get; set; }
    public string Username { get; set; }
    public string Content { get; set; }
    public DateTime Timestamp { get; set; }
}
```

## IMessageRepository

Persistence interface for chat messages. Implemented by `SqliteMessageRepository`.

Methods:
- `Task AddMessageAsync(ChatMessage message)`
- `Task<IEnumerable<ChatMessage>> GetRecentMessagesAsync(int limit)` — returns the most recent messages
- `Task<IEnumerable<ChatMessage>> GetRecentMessagesAsync(int limit, int offset)` — paginated overload (newest-first skip/take, returned in chronological order)
- `Task<int> GetTotalCountAsync()` — total message count for pagination metadata

## ConnectionManager (in-memory state)

Tracks active WebSocket connections and associated state. Not persisted.

- **Connections**: `ConcurrentDictionary<string, WebSocket>` — connectionId → socket
- **Usernames**: `ConcurrentDictionary<string, string>` — connectionId → username
- **Typing state**: `ConcurrentDictionary<string, bool>` — connectionId → isTyping

On disconnect, all three dictionaries are cleaned up for the connection.

## Other models to consider
- `User` — for future authentication and profile information
- `Room` / `Channel` — for future chat rooms
