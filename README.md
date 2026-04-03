
# Live Chat (WebSockets Learning Project)

Learning project implementing a simple real-time chat server using raw WebSockets (ASP.NET Core).

- System overview: `Solution Items/system-overview.md`

Prerequisites:
- .NET 10 SDK
- Visual Studio 2026 (recommended) or `dotnet` CLI

Quick run (from repo root):

```powershell
dotnet run --project LiveChatServer
```

Run tests (from repo root):

```powershell
dotnet test
```

Each test class gets its own in-memory SQLite database via `IsolatedChatAppFactory`, so tests are hermetic and can run in any order.

Useful links:
- TODO: `Solution Items/TODO.md`
- API spec: `Solution Items/API-SPEC.md`
- Domain model: `Solution Items/domain-model.md`
- Database design: `Solution Items/database-design.md`
- Minimal browser client: `Solution Items/client/example.html` (created on demand)

Open the example client
-----------------------
There are two simple ways to open the minimal browser client to talk to the running server.

1) Serve the `Solution Items/client` folder over HTTP (recommended)

- From the `Solution Items/client` directory run a small static server, for example with Python:
  - `python -m http.server 8000` (then open `http://localhost:8000/example.html`)

  - Or use `npx http-server` or any static file host.

  The client uses `location.host` to connect with WebSockets so it will automatically connect to the same origin (`ws://.../ws`).

2) Open the file directly (file://) — manual URL edit
- If you open `example.html` directly from the file system the `location.host` will be empty. Edit the WebSocket creation line near the top of the file and replace the URL with the server address shown when you run the app, for example:
  - `new WebSocket('ws://localhost:5223/ws')` (replace `5223` with the port displayed by `dotnet run`).

Notes:
- Start the server first with `dotnet run --project LiveChatServer` and note the URL / port printed to the console (e.g. `http://localhost:5223`).
- The client sends a `join` message on connect; use the username input before connecting.

Repository layout (high-level):
- `LiveChatServer/` - server project (ASP.NET Core)
- `tests/LiveChatServer.Tests/` - unit and integration tests (xUnit)
- `Solution Items/` - project documentation and small tooling files (system overview, API spec, TODO, database design)

Debugging
---------

### Visual Studio

1. Open `WebSocketChat.slnx` in Visual Studio.
2. Set **LiveChatServer** as the startup project (right-click → Set as Startup Project).
3. Press **F5** (or Debug → Start Debugging). The `http` launch profile starts the server on `http://localhost:5223`.
4. Breakpoints work in `WebSocketMiddleware`, `MessageHandler`, `ConnectionManager`, etc.

### CLI

Run with hot-reload (restarts on file changes):

```powershell
dotnet watch --project LiveChatServer
```

Or run without hot-reload:

```powershell
dotnet run --project LiveChatServer
```

Contributing
------------
See [CONTRIBUTING.md](CONTRIBUTING.md) for branching, commit message conventions, and PR guidelines.

Notes for reviewers:
- This is a portfolio prototype aimed at demonstrating clean architecture and WebSocket fundamentals.
- Message persistence is part of the MVP (SQLite).
