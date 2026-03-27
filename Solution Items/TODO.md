# Sprint Plan & Backlog

This file organizes work into sprints with acceptance criteria and concrete tasks. Use the backlog for ideas that surface during development.

---

## How to use this document
- Each sprint is focused and time-boxed. Move completed tasks to the sprint done section and update status.
- Acceptance criteria (AC) describe verifiable outcomes for reviewers.
- Keep tasks small and actionable (1-2 day effort each where possible).

---

## Sprint 1 — MVP (core realtime + persistence)
Duration: 1-2 weeks (prototype quality)

Acceptance Criteria
- Server accepts WebSocket connections and maintains active sessions. — In progress (middleware accepts sockets; connection registration needs final wiring).
- Clients can join with a username, send chat messages, and receive broadcasts. — In progress (message format and handler implemented; client/middleware handshake needs polish).
- Messages are persisted to SQLite and retrievable via an API. — Done (`SqliteMessageRepository` + `GET /api/messages`).
- A minimal browser client (`Solution Items/client/example.html`) can demonstrate join, send, and receive flows. — In progress (client present; end-to-end broadcast requires middleware->handler connection registration).
- Basic automated tests cover message persistence and handler logic. — Done (unit and integration tests added).

Tasks
- Create `ChatMessage` domain model and persistence schema (SQLite). — Done
- Define `IMessageRepository` and implement `SqliteMessageRepository` (async methods: AddMessageAsync, GetRecentMessagesAsync). — Done
- Implement connection manager that tracks active WebSocket connections and usernames. — In progress (connection tracking implemented; mapping to usernames and lifecycle events need completion)
- Implement message handler: parse incoming JSON, validate, persist, broadcast. — Done (prototype receive loop, persistence and broadcast implemented)
- Add HTTP endpoint `GET /api/messages?limit={n}` to return recent messages. — Done (`MessagesController`)
- Add minimal browser client `Solution Items/client/example.html` with join/send/display logic. — In progress (client exists; adjust for host/port and ensure server binds username query or JSON join)
- Add unit tests for repository and message handler; add one integration test for end-to-end flow (in-memory WebSocket or test client). — Done (unit + integration scaffolds and tests present)
- Update `README.md` to include run/debug steps and point to the client. — Done

Done criteria

- All AC met and tests pass locally.

Notes / next actions to finish Sprint 1
- Ensure `RemoveConnectionAsync` also clears username mapping (prevent stale entries) and consider validating `SetUsernameAsync` against known connections.
- Add small lifecycle end-to-end tests for connect/disconnect and message broadcast (use WebApplicationFactory + real WebSocket client).
- Add minimal structured logging to middleware/handler for easier debugging during manual runs and CI.
- Verify example client host/port configuration and add an instruction in README for opening the client (serve via simple static server or point to the correct port).

---

## Sprint 2 — UX & Reliability
Duration: 1 week

Acceptance Criteria
- Typing indicators are visible to others within the same session.
- Server handles abrupt disconnects gracefully and removes stale connections.
- Logging is structured and useful for debugging.

Tasks
- Add `typing` message type handling (client -> server -> broadcast typing state).
- Harden connection lifecycle (pings/timeouts, graceful close handling).
- Integrate structured logging (Microsoft.Extensions.Logging) and add basic log levels.
- Improve error handling and add user-friendly error messages to client.
- Add more unit tests around connection lifecycle.

Done criteria
- Typing works in the browser client and server logs show lifecycle events.

---

## Sprint 3 — API polish & developer experience
Duration: 1 week

Acceptance Criteria
- HTTP API is documented and returns consistent JSON shapes.
- Repo is easy to build and run for reviewers.

Tasks
- Complete `API-SPEC.md` with request/response examples and error codes.
- Add Postman/HTTP collection or curl examples in `Solution Items/run.md` (optional).
- Add contribution notes to `README.md` and include code style / minimal PR checklist.
- Create simple VS / dotnet debug instructions.

Done criteria
- A new reviewer can clone the repo and run the project following the README.

---

## Sprint 4 — Feature expansion (V1)
Duration: 1-2 weeks

Acceptance Criteria
- Message history is paginated and retrievable.
- File/image sending is supported at a prototype level.

Tasks
- Add pagination support for GET `/api/messages` (offset/limit or cursor-based).
- Prototype file upload flow (HTTP upload + broadcast message with file metadata).
- Consider storing files locally or as base64 in DB for prototype (choose simplest workable approach).
- Add tests and examples for file flow.

---

## Backlog (ideas)
- Chat rooms / channels (rooms identified in messages and connections).
- Private messaging and direct message threads.
- Authentication (JWT/Identity) and persistent users.
- Presence and online users list with heartbeats.
- Message edits and deletions.
- Horizontal scaling: message broker (Redis/SignalR/NGINX PubSub) and sticky sessions.
- Rich text / emojis / markdown support in messages.
- Export of message history (CSV/JSON).
- CI pipeline with tests and build verification.

---

## Labels / status
- To track progress use simple prefixes in this file or GitHub issues: `TODO`, `In Progress`, `Blocked`, `Done`.

---

