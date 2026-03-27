# Domain Models

Primary domain model for the chat application.

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

Other models to consider:
- `ConnectionInfo` - mapping between WebSocket connection id and username
- `User` - for future authentication and profile information
