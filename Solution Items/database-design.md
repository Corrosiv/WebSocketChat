# Database Design

Using SQLite for persistence in MVP. Keep schema minimal and easy to migrate.

## Tables

### `Messages`
- `Id` INTEGER PRIMARY KEY AUTOINCREMENT
- `Username` TEXT NOT NULL
- `Content` TEXT NOT NULL
- `Timestamp` TEXT (ISO 8601) NOT NULL

Indexes:
- `IX_Messages_Timestamp` on `Timestamp` for efficient recent queries

Migration notes:
- Use a simple repository interface `IMessageRepository` with methods:
  - `Task AddMessageAsync(ChatMessage message)`
  - `Task<IEnumerable<ChatMessage>> GetRecentMessagesAsync(int limit)`

Keep the data access layer abstract to allow replacing SQLite later.
