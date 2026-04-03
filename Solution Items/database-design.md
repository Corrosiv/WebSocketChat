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

## Repository interface — `IMessageRepository`

- `Task AddMessageAsync(ChatMessage message)`
- `Task<IEnumerable<ChatMessage>> GetRecentMessagesAsync(int limit)` — returns the `limit` most recent messages in chronological order
- `Task<IEnumerable<ChatMessage>> GetRecentMessagesAsync(int limit, int offset)` — paginated: skips `offset` newest rows, takes `limit`, returns in chronological order
- `Task<int> GetTotalCountAsync()` — total row count (used for `PagedResponse.Total`)

Keep the data access layer abstract to allow replacing SQLite later.
