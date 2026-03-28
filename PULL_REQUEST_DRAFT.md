# Draft PR: Sprint 1 — MVP Complete

## Summary
This PR prepares Sprint 1 for review. It implements a prototype real-time chat server using raw WebSockets, message persistence (SQLite), a minimal browser test client, and automated tests (unit, integration, end-to-end). The code is organized for clarity and reviewability.

Branch: `dev` → `main`

## What I changed
- WebSocket transport and middleware
  - `LiveChatServer/WebSockets/WebSocketMiddleware.cs` — accept sockets, register connections, hand off to handler, structured logs
- Application services
  - `LiveChatServer/Services/ConnectionManager.cs` — track sockets and username mapping, broadcast, cleanup
  - `LiveChatServer/Services/MessageHandler.cs` — receive loop, handle `join` and `message`, persist and broadcast, logging
  - `LiveChatServer/Services/*` interfaces and scaffolding
- Persistence
  - `LiveChatServer/Data/ChatMessage.cs`
  - `LiveChatServer/Data/IMessageRepository.cs`
  - `LiveChatServer/Data/SqliteMessageRepository.cs` (SQLite, creates table if missing)
- HTTP API
  - `LiveChatServer/Controllers/MessagesController.cs` — `GET /api/messages?limit={n}`
- Client
  - `Solution Items/client/example.html` — minimal browser client, now supports overriding WS URL and sends `join` on connect
- Tests
  - `tests/LiveChatServer.Tests/` — unit tests, integration tests, E2E tests (WebSocket -> API persistence)
- Tooling & docs
  - `README.md` — run and client instructions
  - `Solution Items/TODO.md` — sprint status and backlog
  - CI workflow: `.github/workflows/dotnet.yml`
  - `.gitattributes`, `.gitignore`, editorconfig

## How to run and verify locally
1. Start the server (terminal A):
   ```powershell
   dotnet run --project LiveChatServer
   ```
2. Serve the client (terminal B):
   ```powershell
   Set-Location 'Solution Items/client'
   python -m http.server 8000
   ```
   Open `http://localhost:8000/example.html`, enter username, set Server WS URL to the server host (e.g. `ws://localhost:5223/ws`) and Connect.
3. Verify persistence:
   ```powershell
   Invoke-RestMethod 'http://localhost:5223/api/messages?limit=10'
   ```
4. Run tests:
   ```powershell
   dotnet test
   ```

## Files for reviewers to focus on
- `LiveChatServer/WebSockets/WebSocketMiddleware.cs`
- `LiveChatServer/Services/MessageHandler.cs`
- `LiveChatServer/Services/ConnectionManager.cs`
- `LiveChatServer/Data/SqliteMessageRepository.cs`
- `Solution Items/client/example.html`
- `tests/LiveChatServer.Tests/*`

## Known limitations / notes
- Test DB: tests currently use the repository configured in app; consider configuring a temporary SQLite DB per test to ensure isolation in CI.
- No authentication or rooms — single shared chat room (MVP). Private messaging is planned for later sprints.
- Broadcast is best-effort; no delivery guarantees or backpressure handling (prototype quality).

## Suggested review checklist
- [ ] Build succeeds and tests pass in CI
- [ ] Server correctly handles connect/join/message flows (manual test)
- [ ] Messages are persisted and retrievable via HTTP API
- [ ] Code is readable and follows project conventions
- [ ] TODO/sprint notes are accurate

## Next steps after merge
- Improve test isolation (per-test DB)
- Sprint 2: UX (typing indicator), reliability (pings/timeouts), logging polish

---


