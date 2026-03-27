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

## 3. MVP Features

### Core Functionality
- Establish WebSocket connections
- Send messages from client to server
- Broadcast messages to all connected clients
- Display messages with username distinction
- Message persistence (MVP)

---

## 4. Planned Features (V1)

These are not required for MVP, but part of the first full version:

- Typing indicators
- File/image sending

---

## 5. Future Features (V2+)

- Chat rooms
- Private messaging
- Online users list
- Authentication system

---

## 6. High-Level Architecture

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

## 7. Technology Stack

### Backend
- ASP.NET Core
- Raw WebSockets via System.Net.WebSockets

### Client
- Simple HTML + JavaScript test page

### Database
- SQLite (via provider abstraction)

---

## 8. Core Components

### 8.1 WebSocket Server Layer
Handles:
- Accepting connections
- Managing connection lifecycle
- Receiving and sending raw messages

### 8.2 Connection Manager
Responsible for:
- Tracking active connections
- Mapping connections to usernames
- Broadcasting messages

### 8.3 Message Handler
Handles:
- Parsing incoming messages (JSON)
- Routing messages (broadcast, future: private)
- Triggering events (typing, etc.)

### 8.4 Domain Models

Example:
n    class ChatMessage
    {
        public string Username { get; set; }
        public string Content { get; set; }
        public DateTime Timestamp { get; set; }
    }

### 8.5 Data Layer
- SQLite for persistence
- Repository pattern to remain database-agnostic

---

## 9. Data Flow

### Sending a Message

    Client -> WebSocket -> Server -> Message Handler -> Connection Manager -> All Clients

### With Persistence (V1)

    Client -> Server -> Message Handler -> Database
                                         -> Broadcast to Clients

---

## 10. Testing Strategy

- Browser-based test client (HTML + JavaScript)
- Console logging for debugging
- Optional CLI tools for WebSocket testing

---

## 11. Design Principles

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

## 12. Constraints and Trade-offs

- Raw WebSockets increase complexity but improve learning
- No authentication simplifies early development but limits realism
- Single-instance server (no horizontal scaling)

---

## 13. Future Improvements

- Introduce authentication (JWT or Identity)
- Add horizontal scaling (multiple instances and message broker)
- Improve protocol (message types, validation)
- Add structured logging and monitoring
