# System Overview - Live Chat (WebSockets Learning Project)

## 1. Purpose

This project is a learning-focused real-time chat application built using raw WebSockets.

The primary goal is to deepen understanding of:
- Real-time communication systems
- WebSocket protocol and connection lifecycle
- Clean backend architecture
- Scalable system design principles

This is not intended to be production-ready, but it is designed with extensibility and maintainability in mind.

---

## 2. Scope

- Single developer (local usage)
- No authentication (initial version)
- Lightweight environment for experimentation and iteration

---

## 3. Getting Started

### Prerequisites

- [.NET 10 SDK](https://dotnet.microsoft.com/download)
- Visual Studio 2026 (recommended) or `dotnet` CLI

### Run the server

From the repo root:

```powershell
dotnet run --project LiveChatServer
```

The console will print the URL (e.g. `http://localhost:5223`). The WebSocket endpoint is at `/ws`.

### Run with hot-reload

```powershell
dotnet watch --project LiveChatServer
```

### Open the browser client

1. Start the server (see above) and note the port.
2. Serve the client folder with any static file server:

```powershell
cd "Solution Items/client"
python -m http.server 8000
```

3. Open `http://localhost:8000/example.html` in a browser.

Alternatively, open `example.html` directly and edit the WebSocket URL in the file to match the server port.

### Run tests

```powershell
dotnet test
```

### Debug in Visual Studio

1. Open `WebSocketChat.slnx`.
2. Set **LiveChatServer** as the startup project.
3. Press **F5**. The `http` launch profile starts on `http://localhost:5223`.

> For full details see the [README](../README.md).

---

## 4. Implemented Features

### Core Functionality
- Establish WebSocket connections
- Send messages from client to server
- Broadcast messages to all connected clients
- Display messages with username distinction
- Message persistence (SQLite)
- Paginated message history (`GET /api/messages?limit=&offset=`)

### UX & Reliability
- Typing indicators (client sends `typing` events; server broadcasts; client displays "user is typing...")
- Connection lifecycle: graceful close handling, leave-event broadcast on disconnect, error cleanup
- Structured logging (`ILogger<T>` with `{ConnectionId}`, `{Username}`, `{RemoteIp}`)

---

## 5. Planned Features (V1)

- File/image sending (prototype)

---

## 6. Future Features (V2+)

- Chat rooms
- Private messaging
- Online users list
- Authentication system

---

## 7. High-Level Architecture

    [ Browser Test Client ]
               |
               | WebSocket (ws://)
               v
    [ WebSocket Server (ASP.NET Core) ]
               |
               v
    [ Application Layer ]
       - Message Handler
       - Connection Manager
       - Domain Models
               |
               v
    [ Data Layer (SQLite) ]

---

## 8. Technology Stack

### Backend
- ASP.NET Core
- Raw WebSockets via System.Net.WebSockets

### Client
- Simple HTML + JavaScript test page

### Database
- SQLite (via provider abstraction)

---

## 9. Core Components

### 9.1 WebSocket Server Layer
Handles:
- Accepting connections
- Managing connection lifecycle
- Receiving and sending raw messages

### 9.2 Connection Manager
Responsible for:
- Tracking active connections
- Mapping connections to usernames
- Tracking per-connection typing state
- Broadcasting messages to all connected clients

### 9.3 Message Handler
Handles:
- Parsing incoming messages (JSON)
- Routing messages (broadcast, future: private)
- Triggering events (typing, etc.)

### 9.4 Domain Models

Example:

n    class ChatMessage
    {
        public string Username { get; set; }
        public string Content { get; set; }
        public DateTime Timestamp { get; set; }
    }

### 9.5 Data Layer
- SQLite for persistence
- Repository pattern to remain database-agnostic

---

## 10. Data Flow

### Sending a Message

    Client -> WebSocket -> Server -> Message Handler -> Connection Manager -> All Clients

### With Persistence (V1)

    Client -> Server -> Message Handler -> Database
                                         -> Broadcast to Clients

---

## 11. Testing Strategy

- Automated tests (xUnit): unit, integration, and WebSocket E2E tests
- Per-test SQLite isolation via `IsolatedChatAppFactory`
- CI pipeline runs tests on every push and PR
- Browser-based test client (HTML + JavaScript) for manual verification

---

## 12. Design Principles

### Separation of Concerns
Each layer has a clear responsibility:
- Transport (WebSockets)
- Application logic
- Data access

### Extensibility
The system should allow adding:
- New message types
- Features like rooms or private messaging
- Different database providers

Without major refactoring.

### Simplicity First
Avoid premature complexity:
- No authentication (yet)
- No distributed systems
- Focus on correctness and clarity

### Database Agnostic Design
- Use interfaces for repositories
- Allow switching from SQLite to other databases later

---

## 13. Constraints and Trade-offs

- Raw WebSockets increase complexity but improve learning
- No authentication simplifies early development but limits realism
- Single-instance server (no horizontal scaling)

---

## 14. Future Improvements

- Introduce authentication (JWT or Identity)
- Add horizontal scaling (multiple instances and message broker)
- Improve protocol (message types, validation)
- Add structured logging and monitoring
