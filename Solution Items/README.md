
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

Useful links:
- TODO: `Solution Items/TODO.md`
- API spec: `Solution Items/API-SPEC.md`
- Domain model: `Solution Items/domain-model.md`
- Database design: `Solution Items/database-design.md`
- Minimal browser client: `Solution Items/client/example.html` (created on demand)

Repository layout (high-level):
- `LiveChatServer/` - server project (ASP.NET Core)
- `Solution Items/` - project documentation and small tooling files (system overview, API spec, TODO, database design)

Notes for reviewers:
- This is a portfolio prototype aimed at demonstrating clean architecture and WebSocket fundamentals.
- Message persistence is part of the MVP (SQLite).
