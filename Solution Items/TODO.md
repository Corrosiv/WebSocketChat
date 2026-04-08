# Sprint Plan & Backlog

This file organizes work into sprints with acceptance criteria and concrete tasks. Use the backlog for ideas that surface during development.

---

## Feature Overview

| Feature | Status | Sprint | Notes |
|---------|--------|--------|-------|
| **Core** | | | |
| WebSocket connections & session management | ✅ Done | 1 | Middleware accepts sockets, registers/cleans up connections |
| Join with username | ✅ Done | 1 | `join` message type, username tracked per connection |
| Send & broadcast chat messages | ✅ Done | 1 | `message` type, persisted and broadcast to all clients |
| Message persistence (SQLite) | ✅ Done | 1 | `SqliteMessageRepository`, async read/write |
| HTTP message history API | ✅ Done | 1 | `GET /api/messages?limit=&offset=` with `PagedResponse<T>` |
| Browser client | ✅ Done | 1 | `example.html` — join, send, display |
| **UX & Reliability** | | | |
| Typing indicators | ✅ Done | 2 | Client sends `typing` events; server broadcasts; UI shows "user is typing..." |
| Graceful disconnect handling | ✅ Done | 2 | Leave-event broadcast, connection cleanup, error recovery |
| Structured logging | ✅ Done | 2 | `ILogger<T>` with `{ConnectionId}`, `{Username}`, `{RemoteIp}` |
| Client connection status | ✅ Done | 2 | Connecting / connected / disconnected states with retry guidance |
| Message history on connect | ✅ Done | 2 | Client loads recent messages via API on WebSocket open |
| Client UX polish | ✅ Done | 2 | Auto-scroll, timestamps, enter-to-send, disabled send when offline |
| **API & DevEx** | | | |
| API input validation | ✅ Done | 3 | `MessagesController` returns `ApiErrorDto` for invalid params |
| Global exception middleware | ✅ Done | 3 | `ApiExceptionMiddleware` returns JSON errors on `/api` routes |
| API documentation (`API-SPEC.md`) | ✅ Done | 3 | HTTP endpoints, WS message types, error shapes |
| CI pipeline (GitHub Actions) | ✅ Done | 3 | Build + test on push/PR to `main` and `dev` |
| README with run/debug instructions | ✅ Done | 3 | Visual Studio and CLI debug sections, contributing link |
| **Planned** | | | |
| File/image sending (prototype) | 📋 Planned | 4 | HTTP upload + broadcast with file metadata |
| **Backlog** | | | |
| Ping/timeout for stale connections | 💡 Idea | — | Background cleanup of idle WebSocket connections |
| Server-side typing timeout | 💡 Idea | — | Auto-clear stale typing state |
| Correlation IDs in logging | 💡 Idea | — | Expand structured logging |
| System message visual distinction | 💡 Idea | — | Client-side styling for join/leave vs chat messages |
| UI-level integration tests | 💡 Idea | — | Headless browser or simulated DOM tests |
| Chat rooms / channels | 💡 Idea | — | Rooms identified in messages and connections |

---

## How to use this document
- Each sprint is focused and time-boxed.
- Acceptance criteria (AC) describe verifiable outcomes for reviewers.
- Keep tasks small and actionable (1-2 day effort each where possible).
- Completed sprints are summarized below; see `git log` for full history.

---

## Completed Sprints

### Sprint 1 — MVP (core realtime + persistence) ✅
WebSocket server with join/message/broadcast, SQLite persistence via `SqliteMessageRepository`, paginated HTTP API (`GET /api/messages`), browser client (`example.html`), unit + integration + E2E tests.

### Sprint 2 — UX & Reliability ✅
Typing indicators, graceful disconnect handling with leave-event broadcasts, structured logging (`ILogger<T>`), client UX polish (connection status, message history on connect, auto-scroll, timestamps, enter-to-send), per-test SQLite isolation (`IsolatedChatAppFactory`). 15 tests passing.

### Sprint 3 — API polish & developer experience ✅
CI pipeline (GitHub Actions on `main`/`dev`), `.editorconfig`, MIT license, `API-SPEC.md` rewrite (HTTP + WS + error shapes), README with Visual Studio and CLI debug sections, `CONTRIBUTING.md` cleanup, docs refresh (`system-overview`, `domain-model`, `database-design`), API input validation (`ApiErrorDto` for bad params), global exception middleware (`ApiExceptionMiddleware`). 15 tests passing.

---

## Compatibility & Reuse — Immediate technical tasks

These are prioritized items to make the WebSocket implementation reusable by other projects. Each item is small and actionable so it can be scheduled into a sprint.

- [ ] Fix framing: properly handle fragmented messages and respect `WebSocketReceiveResult.EndOfMessage`. Acceptance criteria: `MessageHandler.ReceiveLoopAsync` assembles frames until `EndOfMessage == true` and supports messages larger than current 4KB buffer.
- [ ] Add cancellation support: accept and propagate `CancellationToken` into receive/send loops. Acceptance criteria: `HandleAsync` accepts a `CancellationToken` (or uses one from the middleware) and passes it to `ReceiveAsync`/`SendAsync` so tests and hosting shutdown can cancel loops.
- [ ] Improve broadcast error handling: replace silent swallow with structured logging and optional removal of stale sockets. Acceptance criteria: `ConnectionManager.BroadcastAsync` logs send failures and removes/cleans connections that consistently fail to send (configurable threshold).
- [ ] Replay/backpressure safeguards: add configurable replay window limits and basic backpressure behavior for `GET /api/messages` and replay endpoints. Acceptance criteria: API returns `429 Too Many Requests` or `400` for excessively large replay requests and server prevents simultaneous large replays from overwhelming memory/CPU.
- [ ] Add integration tests for large/fragmented messages and cancellation. Acceptance criteria: new tests demonstrate correct reassembly and graceful cancellation.

Notes:
- These tasks are required before treating this WebSocket layer as a reusable component for other projects (for example, integrating into `RealTimeDashboard`).
- I recommend tackling framing and cancellation first (high impact, small scope), then broadcast error handling, then replay/backpressure and tests.

---

## Sprint 4 — Feature expansion (V1)
Duration: 1-2 weeks

Acceptance Criteria
- File/image sending is supported at a prototype level.
- (Pagination was completed in Sprint 2.)

Tasks
- Prototype file upload flow (HTTP upload + broadcast message with file metadata).
- Consider storing files locally or as base64 in DB for prototype (choose simplest workable approach).
- Add tests and examples for file flow.

---

## Backlog (ideas)
- Ping/timeout and background cleanup for stale WebSocket connections.
- Server-side typing timeout to auto-clear stale typing state.
- Expand structured logging with correlation IDs.
- Client: add visual distinction for system messages vs user messages.
- UI-level integration tests (headless browser or simulated DOM) for key UX flows.
- Chat rooms / channels (rooms identified in messages and connections).

---

## Labels / status
- To track progress use simple prefixes in this file or GitHub issues: `TODO`, `In Progress`, `Blocked`, `Done`.

---

