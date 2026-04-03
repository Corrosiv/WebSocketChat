# Draft PR: Sprint 2 — UX & Reliability Complete

## Summary
Sprint 2 delivers typing indicators, connection lifecycle hardening, structured logging, client UX polish, and per-test database isolation. All acceptance criteria are met and tests pass (15/15).

Branch: `dev` → `main`

## What I changed
- Typing indicator support
  - `LiveChatServer/Services/MessageHandler.cs` — handle `typing` message type, broadcast typing state
  - `LiveChatServer/Services/ConnectionManager.cs` — track per-connection typing state (`SetTypingAsync`, `IsTyping`, `GetTypingUsers`)
  - `Solution Items/client/example.html` — send typing events on input, display "user is typing..." indicator
- Connection lifecycle hardening
  - `LiveChatServer/WebSockets/WebSocketMiddleware.cs` — finally block ensures cleanup + leave broadcast on any disconnect; catch block handles mid-session errors
  - `LiveChatServer/Services/ConnectionManager.cs` — `RemoveConnectionAsync` clears username and typing state
- Structured logging
  - `ILogger<T>` injected in `WebSocketMiddleware` and `MessageHandler` with structured properties (`{ConnectionId}`, `{Username}`, `{RemoteIp}`) and appropriate levels
  - `Program.cs` — console logging configured with Debug minimum level
- Client UX
  - Connection status display (connecting / connected / disconnected)
  - Join/leave system events in message area
  - Message history loaded on connect via `GET /api/messages`
  - Smart scroll (pinned to bottom unless user is reading history)
  - Local timestamps, input focus, enter-to-send, disabled send when offline
- Test infrastructure
  - `IsolatedChatAppFactory` — per-test-class in-memory SQLite database for hermetic tests
  - Test project added to solution (`WebSocketChat.slnx`) so `dotnet test` works from repo root
  - Fixed `GetMessagesIntegrationTests` deserialization cast bug
- Tests added
  - `ConnectionManagerTypingTests` — typing state unit tests
  - `WebSocketTypingIntegrationTests` — E2E typing broadcast
  - `WebSocketTypingStopIntegrationTests` — E2E typing stop broadcast
- Documentation
  - `CONTRIBUTING.md` — added Conventional Commits section
  - `README.md` — added test instructions and repo layout
  - `Solution Items/TODO.md` — Sprint 2 marked complete

## How to run and verify locally
1. Start the server:
   ```powershell
   dotnet run --project LiveChatServer
   ```
2. Run tests:
   ```powershell
   dotnet test
   ```
3. Manual client verification (optional):
   ```powershell
   Set-Location 'Solution Items/client'
   python -m http.server 8000
   ```
   Open `http://localhost:8000/example.html` — test join, messaging, typing indicator, disconnect/reconnect.

## Checklist
- [x] Project builds and runs locally
- [x] All 15 tests pass (`dotnet test`)
- [x] Typing indicators work in browser client
- [x] Disconnect cleanup broadcasts leave events and removes state
- [x] Structured logging visible in console output
- [x] Documentation updated (README, CONTRIBUTING, TODO)
- [x] Commit history follows Conventional Commits format

## Notes for reviewers
- Ping/timeout background service and server-side typing timeout are deferred to the backlog — the core AC (graceful disconnect handling) is met without them.
- Low-priority items (system message styling, headless UI tests) moved to backlog.

## Next steps after merge
- Sprint 3: API polish and developer experience
- Sprint 4: Pagination and file upload prototype
