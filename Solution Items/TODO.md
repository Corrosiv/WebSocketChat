# Sprint Plan & Backlog

This file organizes work into sprints with acceptance criteria and concrete tasks. Use the backlog for ideas that surface during development.

---

## How to use this document
- Each sprint is focused and time-boxed. Move completed tasks to the sprint done section and update status.
- Acceptance criteria (AC) describe verifiable outcomes for reviewers.
- Keep tasks small and actionable (1-2 day effort each where possible).

---

## Sprint 1 — MVP (core realtime + persistence)
Duration: Completed

Acceptance Criteria
- Server accepts WebSocket connections and maintains active sessions. — Done (middleware accepts sockets, registers connections and cleans up on close).
- Clients can join with a username, send chat messages, and receive broadcasts. — Done (client and handler support `join` and `message`, broadcasts labelled messages).
- Messages are persisted to SQLite and retrievable via an API. — Done (`SqliteMessageRepository` + `GET /api/messages`).
- A minimal browser client (`Solution Items/client/example.html`) can demonstrate join, send, and receive flows. — Done (client updated and manual verification completed).
- Basic automated tests cover message persistence and handler logic. — Done (unit, integration and E2E tests present).

Tasks
- Create `ChatMessage` domain model and persistence schema (SQLite). — Done
- Define `IMessageRepository` and implement `SqliteMessageRepository` (async methods: AddMessageAsync, GetRecentMessagesAsync). — Done
- Implement connection manager that tracks active WebSocket connections and usernames. — Done
- Implement message handler: parse incoming JSON, validate, persist, broadcast. — Done
- Add HTTP endpoint `GET /api/messages?limit={n}` to return recent messages. — Done
- Add minimal browser client `Solution Items/client/example.html` with join/send/display logic. — Done
- Add unit tests for repository and message handler; add one integration test for end-to-end flow (in-memory WebSocket or test client). — Done
- Update `README.md` to include run/debug steps and point to the client. — Done

Done criteria

- Sprint 1 is complete: all AC met and tests pass locally.

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

Status (current)
- `typing` message handling: Done (server + client + unit & E2E tests added).
- Connection lifecycle (pings/timeouts, background cleanup): In Progress (remove-on-close implemented; ping/timeout and background cleanup TODO).
- Structured logging: Partial (basic logs added; expand for structured properties and levels).
- Error handling / client messages: Partial (improvements present; more UX polish pending).
- Additional lifecycle tests: Partial (connection manager tests exist; more lifecycle/timeout tests planned).

Sprint 2 — Initial UX tasks (checklist)
Priorities: Completed (C), High (H), Medium (M), Low (L)

Priority legend: use `[C]` to mark completed tasks so they are easy to scan.

- [C] Client: show connection status prominently (connecting / connected / disconnected) and retry guidance. — Completed
- [C] Client: display join/system events in message area (e.g., "Pedro joined"). — Completed
- [C] Client & Server: typing indicator support (client sends `typing` events; server broadcasts typing state; client shows "user is typing..."). — Completed
- [M] Client: load recent message history on connect via `GET /api/messages` and render as initial chat state.
- [M] Client: keep scroll pinned to bottom when user is at the bottom; do not auto-scroll when the user is reading history.
- [M] Client: show timestamps in local timezone and make format configurable.
- [M] Client: small UI polish (input focus, enter-to-send, disabled send when offline).
- [L] Client: add visual distinction for system messages vs user messages.
- [L] Tests: add UI-level integration tests (headless browser or simulated DOM) for key UX flows: join, send, typing indicator.

Notes:
- Start with High priorities for Sprint 2; Medium can follow in the sprint scope depending on velocity.
- Use feature branches off `dev` (e.g., `feat/typing-indicator`, `feat/client-ux`) and small commits describing changes.

Done criteria
- Typing works in the browser client and server logs show lifecycle events.

Notes / Next steps
- Add per-test SQLite isolation for tests (high priority) to make E2E hermetic for CI.
- Implement ping/timeout and background cleanup for stale connections; add unit + E2E tests.
- Add server-side typing timeout to auto-clear stale typing state.
- Expand structured logging with connectionId/username and correlation IDs.

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

