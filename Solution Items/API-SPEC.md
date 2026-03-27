# API Specification - Live Chat

## WebSocket Protocol

The real-time messaging is performed over a WebSocket connection. Messages use JSON.

### Message Types

- `join` - client announces username
  - payload: `{ "type": "join", "username": "alice" }`
- `message` - chat message to broadcast
  - payload: `{ "type": "message", "content": "Hello" }`
- `history_request` - request recent message history
  - payload: `{ "type": "history_request", "limit": 50 }`

### Server Messages

- `message` - broadcasted chat message
  - payload: `{ "type": "message", "username": "alice", "content": "Hello", "timestamp": "..." }`
- `history` - message history response
  - payload: `{ "type": "history", "messages": [ ... ] }`

## HTTP Endpoints (planned)

- `GET /api/messages?limit={n}` - returns the most recent `n` messages (JSON array)
- `POST /api/messages` - create/persist a message (used by admin or testing clients)

