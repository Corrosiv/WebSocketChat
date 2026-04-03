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
Duration: Completed

Acceptance Criteria
- Typing indicators are visible to others within the same session. — Done (server handles `typing` messages, broadcasts state, client displays "user is typing..."; unit + E2E tests cover the flow).
- Server handles abrupt disconnects gracefully and removes stale connections. — Done (middleware finally block cleans up connections, broadcasts leave events, disposes sockets; error handler catches mid-session failures).
- Logging is structured and useful for debugging. — Done (`ILogger<T>` injected in middleware and handler with structured properties: `{ConnectionId}`, `{Username}`, `{RemoteIp}`; levels: Information, Debug, Warning, Error).

Tasks
- Add `typing` message type handling (client -> server -> broadcast typing state). — Done
- Harden connection lifecycle (pings/timeouts, graceful close handling). — Done (graceful close and error cleanup implemented; ping/timeout deferred to backlog).
- Integrate structured logging (Microsoft.Extensions.Logging) and add basic log levels. — Done
- Improve error handling and add user-friendly error messages to client. — Done
- Add more unit tests around connection lifecycle. — Done (ConnectionManagerTests, ConnectionManagerTypingTests, ConnectionManagerUsernameTests).

Sprint 2 — UX tasks (checklist)

- [x] Client: show connection status prominently (connecting / connected / disconnected) and retry guidance.
- [x] Client: display join/system events in message area (e.g., "Pedro joined").
- [x] Client & Server: typing indicator support (client sends `typing` events; server broadcasts typing state; client shows "user is typing...").
- [x] Client: load recent message history on connect via `GET /api/messages` and render as initial chat state.
- [x] Client: keep scroll pinned to bottom when user is at the bottom; do not auto-scroll when the user is reading history.
- [x] Client: show timestamps in local timezone and make format configurable.
- [x] Client: small UI polish (input focus, enter-to-send, disabled send when offline).
- [x] Per-test SQLite isolation (`IsolatedChatAppFactory` gives each test class its own in-memory DB).

Done criteria
- Sprint 2 is complete: all AC met and tests pass locally (15/15).

---

## Sprint 3 — API polish & developer experience
Duration: 1 week

Acceptance Criteria
- HTTP API is documented with request/response examples, error shapes, and all current endpoints/WS message types.
- CI pipeline builds and runs tests on every push and PR; test failures break the build.
- Repo is easy to clone, build, run, and debug for a new reviewer following just the README.
- Project documentation is accurate and reflects the current state of the codebase.

### Task group A — CI & repo hygiene

- [x] A1. Fix CI workflow: add `dev` to push/PR triggers; remove `|| true` so test failures break the build; update `actions/setup-dotnet` to v4.
- [x] A2. Add root `.editorconfig` with basic C# formatting rules (indentation, namespace style, etc.) so contributors get consistent formatting without relying on generated files.
- [x] A3. Add a `LICENSE` file (MIT) to the repo root.
- [x] A4. Remove stale `PULL_REQUEST_DRAFT.md` from repo root (sprint-specific; no longer needed after merge).
- [x] A5. Fix `CONTRIBUTING.md`: remove reference to non-existent `SECURITY.md`; fix editorconfig reference to point to the new root `.editorconfig` instead of the generated one in `obj/`.

### Task group B — API-SPEC.md rewrite

- [x] B1. Document current HTTP endpoints: `GET /api/messages?limit={n}&offset={n}` with full request/response examples, status codes, and the `PagedResponse<T>` shape.
- [x] B2. Document all WebSocket message types (client → server and server → client): `join`, `message`, `typing`, `leave` — with JSON payload examples for each.
- [x] B3. Document error handling: what happens on malformed JSON, missing fields, unknown message types. Include the `ApiErrorDto` shape.
- [x] B4. Remove stale/planned items that were never implemented (`history_request`, `POST /api/messages`).

### Task group C — README & docs refresh

- [x] C1. Add "Debug in Visual Studio" section to README: how to set the startup project, launch profile, and attach to the running server.
- [x] C2. Add "Debug with CLI" section: `dotnet run` + `dotnet watch` instructions.
- [x] C3. Add link to `CONTRIBUTING.md` in README.
- [x] C4. Update `Solution Items/system-overview.md`: move typing indicators from "Planned" to "Implemented"; add connection lifecycle and structured logging to the feature list.
- [x] C5. Update `Solution Items/domain-model.md`: mention the pagination overload on `IMessageRepository` and the typing state tracked in `ConnectionManager`.
- [x] C6. Update `Solution Items/database-design.md`: add the `GetRecentMessagesAsync(limit, offset)` overload and `GetTotalCountAsync` to the repository interface section.

### Task group D — API consistency (code)

- [ ] D1. Review `MessagesController` and ensure error responses use `ApiErrorDto` for invalid query parameters (e.g., negative limit/offset).
- [ ] D2. Add a simple global exception handler or middleware that returns `ApiErrorDto` JSON instead of the default HTML error page for API routes.

Done criteria
- CI passes on a push to `dev` and a PR to `main`; test failures break the build.
- A new reviewer can clone the repo, follow the README, and have the project running and debuggable within minutes.
- `API-SPEC.md` accurately describes every endpoint and WebSocket message type with examples.

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

