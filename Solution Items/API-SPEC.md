# API Specification — Live Chat

Base URL: `http://localhost:{port}` (port shown in console on `dotnet run`).

---

## HTTP API

### `GET /api/messages`

Returns paginated message history, newest first within each page, ordered chronologically (oldest → newest) within the items array.

**Query parameters**

| Param    | Type | Default | Description                        |
|----------|------|---------|------------------------------------|
| `limit`  | int  | 50      | Maximum number of messages to return |
| `offset` | int  | 0       | Number of messages to skip (from newest) |

**Example request**

```
GET /api/messages?limit=2&offset=0
```

**Example response** (`200 OK`)

```json
{
  "items": [
    {
      "type": "message",
      "timestamp": "2026-04-01T12:00:00Z",
      "username": "alice",
      "content": "Hello!",
      "isTyping": false
    },
    {
      "type": "message",
      "timestamp": "2026-04-01T12:00:05Z",
      "username": "bob",
      "content": "Hi Alice!",
      "isTyping": false
    }
  ],
  "total": 42,
  "limit": 2,
  "offset": 0
}
```

**Response shape — `PagedResponse<MessageDto>`**

| Field   | Type            | Description                          |
|---------|-----------------|--------------------------------------|
| `items` | `MessageDto[]`  | Messages for the requested page      |
| `total` | `int`           | Total message count in the database  |
| `limit` | `int`           | Echoed limit parameter               |
| `offset`| `int`           | Echoed offset parameter              |

**`MessageDto` fields**

| Field      | Type       | Description                         |
|------------|------------|-------------------------------------|
| `type`     | `string`   | Always `"message"`                  |
| `timestamp`| `DateTime` | UTC timestamp of the message        |
| `username` | `string`   | Author username                     |
| `content`  | `string`   | Message body                        |
| `isTyping` | `bool`     | Always `false` for persisted messages |

**Status codes**

| Code | Meaning              |
|------|----------------------|
| 200  | Success              |
| 500  | Unexpected server error |

---

## WebSocket Protocol

Endpoint: `ws://localhost:{port}/ws`

All messages are JSON-encoded UTF-8 text frames. Every message must include a `type` field.

### Client → Server

#### `join`

Must be the first message after connecting. Registers a username for the connection.

```json
{ "type": "join", "username": "alice" }
```

| Field      | Required | Description          |
|------------|----------|----------------------|
| `type`     | yes      | `"join"`             |
| `username` | yes      | Display name to use  |

#### `message`

Sends a chat message. The server persists it and broadcasts to all connections.

```json
{ "type": "message", "content": "Hello everyone!" }
```

| Field     | Required | Description                                        |
|-----------|----------|----------------------------------------------------|
| `type`    | yes      | `"message"`                                        |
| `content` | yes      | Message text                                       |
| `username`| no       | Overrides the joined username (falls back to joined name) |

#### `typing`

Indicates typing state. The server broadcasts to all connections.

```json
{ "type": "typing", "isTyping": true }
```

| Field      | Required | Default | Description                |
|------------|----------|---------|----------------------------|
| `type`     | yes      |         | `"typing"`                 |
| `isTyping` | no       | `true`  | `true` = started, `false` = stopped |

#### `leave`

Explicit leave (the server also auto-broadcasts a leave event on disconnect).

```json
{ "type": "leave", "username": "alice" }
```

| Field      | Required | Description          |
|------------|----------|----------------------|
| `type`     | yes      | `"leave"`            |
| `username` | yes      | Username leaving     |

### Server → Client (broadcasts)

The server broadcasts the following events to **all** connected clients.

#### `join` (broadcast)

```json
{ "type": "join", "username": "alice", "timestamp": "2026-04-01T12:00:00Z" }
```

#### `message` (broadcast)

```json
{
  "type": "message",
  "username": "alice",
  "content": "Hello everyone!",
  "timestamp": "2026-04-01T12:00:01Z"
}
```

#### `typing` (broadcast)

```json
{ "type": "typing", "username": "alice", "isTyping": true, "timestamp": "2026-04-01T12:00:02Z" }
```

#### `leave` (broadcast)

Sent when a client disconnects (automatic) or sends a `leave` message.

```json
{ "type": "leave", "username": "alice", "timestamp": "2026-04-01T12:00:03Z" }
```

---

## Error handling

### WebSocket errors

- **Malformed JSON or missing fields**: the server logs a warning and silently ignores the frame. The connection stays open.
- **Unknown `type` value**: silently ignored (no broadcast, no error sent back).
- **Missing `type` field**: silently ignored.

The server does not currently send error frames back to the client over WebSocket. Malformed input is a no-op.

### HTTP errors

Unhandled exceptions return the default ASP.NET Core error response. A structured error DTO is available for future use:

**`ApiErrorDto` shape**

```json
{
  "code": "error",
  "message": "A human-readable description.",
  "details": "Optional additional context or null."
}
```

| Field     | Type      | Description                          |
|-----------|-----------|--------------------------------------|
| `code`    | `string`  | Machine-readable error code          |
| `message` | `string`  | Human-readable description           |
| `details` | `string?` | Optional extra context (nullable)    |

