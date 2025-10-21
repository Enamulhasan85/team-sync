# Team Sync - Real-Time Team Collaboration Board

[![.NET](https://img.shields.io/badge/.NET-8.0-512BD4?logo=dotnet)](https://dotnet.microsoft.com/)
[![MongoDB](https://img.shields.io/badge/MongoDB-4EA94B?logo=mongodb&logoColor=white)](https://www.mongodb.com/)
[![RabbitMQ](https://img.shields.io/badge/RabbitMQ-FF6600?logo=rabbitmq&logoColor=white)](https://www.rabbitmq.com/)
[![Redis](https://img.shields.io/badge/Redis-DC382D?logo=redis&logoColor=white)](https://redis.io/)
[![SignalR](https://img.shields.io/badge/SignalR-512BD4?logo=dotnet)](https://dotnet.microsoft.com/apps/aspnet/signalr)
[![Angular](https://img.shields.io/badge/Angular-18-DD0031?logo=angular)](https://angular.io/)

A modern, real-time collaborative project and task management system built with .NET 8, MongoDB, RabbitMQ, and SignalR. This application enables teams to manage projects, tasks, and chat messages with real-time updates across all connected clients.

## 🚀 Features

- ✅ **User Management**: JWT-based registration, login, and session caching
- ✅ **Project Management**: Create and manage projects with team members
- ✅ **Task Management**: Full CRUD operations with status tracking and assignments
- ✅ **Real-time Notifications**: Instant updates via SignalR WebSockets
- ✅ **Team Chat**: Project-specific chat rooms with message history
- ✅ **Event-Driven Architecture**: RabbitMQ for asynchronous task notifications
- ✅ **Performance Caching**: Redis for user sessions, tasks, and chat messages
- ✅ **Background Worker**: Dedicated service for event processing

## 🏗️ Architecture Overview

This project follows **Clean Architecture** principles with **CQRS** (Command Query Responsibility Segregation) pattern using MediatR.

### System Architecture

The system consists of four main layers with clear separation of concerns:

```
┌─────────────────────────────────────────────────────────────┐
│                    Presentation Layer                         │
│                     (Template.API)                            │
│           REST API Controllers, SignalR Hubs                  │
└───────────────────────────┬───────────────────────────────────┘
                            │
┌───────────────────────────▼───────────────────────────────────┐
│                    Application Layer                          │
│                  (Template.Application)                       │
│     CQRS Commands/Queries, Handlers, Validators              │
└───────────────────────────┬───────────────────────────────────┘
                            │
┌───────────────────────────▼───────────────────────────────────┐
│                      Domain Layer                             │
│                   (Template.Domain)                           │
│              Entities, Enums, Business Rules                  │
└───────────────────────────┬───────────────────────────────────┘
                            │
┌───────────────────────────▼───────────────────────────────────┐
│                  Infrastructure Layer                         │
│                (Template.Infrastructure)                      │
│    MongoDB, RabbitMQ, Redis, SignalR, Repositories           │
└─────────────────────────────────────────────────────────────────┘
```

### Real-Time Event Flow

```
User Action → API Controller → MediatR Command Handler
                                         ↓
                        ┌────────────────┴────────────────┐
                        ↓                                 ↓
                    MongoDB                         RabbitMQ
                  (Persistence)                   (Event Queue)
                                                        ↓
                                              Background Consumer
                                                        ↓
                                                  SignalR Hub
                                                        ↓
                                              Connected Clients
```

### Key Technologies

- **Clean Architecture**: Separation of concerns and dependency inversion
- **CQRS with MediatR**: Separated read/write operations
- **MongoDB**: NoSQL database for flexible schema
- **Redis**: Distributed caching for performance
- **RabbitMQ**: Message broker for event-driven architecture
- **SignalR**: WebSocket-based real-time communication
- **JWT**: Token-based authentication

## 🛠️ Technology Stack

### Backend

- **.NET 8.0**: Latest .NET framework
- **MongoDB**: NoSQL database for flexible schema
- **RabbitMQ**: Message broker for event-driven architecture
- **SignalR**: Real-time WebSocket communication
- **Redis**: Distributed caching
- **ASP.NET Core Identity**: User authentication and authorization
- **MediatR**: CQRS and mediator pattern implementation
- **FluentValidation**: Request validation
- **AutoMapper**: Object-to-object mapping
- **JWT**: Token-based authentication
- **RateLimiter**: Throttling and rate limiting

### Testing

- **xUnit**: Unit testing framework
- **Moq**: Mocking framework
- **FluentAssertions**: Fluent assertion library

### Frontend

- **Angular 18**: Frontend framework
- **SignalR Client**: Real-time communication with backend

## 📋 Prerequisites

Before running the application, ensure you have the following installed:

- [.NET 8.0 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)
- [MongoDB](https://www.mongodb.com/try/download/community) (v4.0 or higher)
- [RabbitMQ](https://www.rabbitmq.com/download.html) (v3.0 or higher)
- [Redis](https://redis.io/download) (Optional, for caching)
- An IDE such as [Visual Studio 2022](https://visualstudio.microsoft.com/), [JetBrains Rider](https://www.jetbrains.com/rider/), or [VS Code](https://code.visualstudio.com/)

## ⚙️ Setup Instructions

### 1. Clone the Repository

```powershell
git clone https://github.com/Enamulhasan85/team-sync.git
cd team-sync
```

### 2. Install MongoDB

**Windows:**

```powershell
# Download and install from: https://www.mongodb.com/try/download/community
# Or use Chocolatey:
choco install mongodb

# Start MongoDB service
net start MongoDB
```

**Docker (Alternative):**

```powershell
docker run -d --name mongodb -p 27017:27017 mongo:latest
```

### 3. Install RabbitMQ

**Windows:**

```powershell
# Download and install from: https://www.rabbitmq.com/download.html
# Or use Chocolatey:
choco install rabbitmq

# Start RabbitMQ service
rabbitmq-server start

# Access Management UI: http://localhost:15672 (guest/guest)
```

**Docker (Alternative):**

```powershell
docker run -d --name rabbitmq -p 5672:5672 -p 15672:15672 rabbitmq:3-management
```

### 4. Install Redis

**Windows:**

```powershell
# Download from: https://github.com/microsoftarchive/redis/releases
# Or use Docker:
docker run -d --name redis -p 6379:6379 redis:latest
```

### 5. Configure Application Settings

Navigate to `src/Template.API/appsettings.json` and update the configuration:

```json
{
  "MongoDbSettings": {
    "ConnectionString": "mongodb://localhost:27017",
    "DatabaseName": "TeamSyncDB"
  },
  "RabbitMq": {
    "Host": "localhost",
    "Port": 5672,
    "Username": "guest",
    "Password": "guest",
    "ExchangeName": "tasks_exchange"
  },
  "ConnectionStrings": {
    "Redis": "localhost:6379"
  },
  "JwtSettings": {
    "Secret": "YourSuperSecureSecretKey1234567890",
    "Issuer": "Template.API",
    "Audience": "Template.API.Users",
    "ExpiryMinutes": 60
  }
}
```

**⚠️ Security Note**: For production, use environment variables or Azure Key Vault for sensitive data.

### 6. Restore Dependencies

```powershell
dotnet restore
```

### 7. Build the Solution

```powershell
dotnet build
```

### 8. Run the Application

```powershell
cd src/Template.API
dotnet run
```

Or with hot reload during development:

```powershell
dotnet watch run
```

### 9. Access the Application

- **API Base URL**: https://localhost:4030
- **Swagger UI**: https://localhost:4030/swagger
- **SignalR Hub**: http://localhost:4030/hubs/notifications

### 10. Testing SignalR Hub

You can test the SignalR hub using the web-based SignalR client:

**SignalR Web Client**: https://gourav-d.github.io/SignalR-Web-Client/dist/

1. Enter the hub URL: `http://localhost:4030/hubs/notifications`
2. Add your JWT token in the connection options
3. Connect and test real-time events

### 11. Default Credentials

The application seeds a default admin user:

- **Email**: admin@template.com
- **Password**: Admin@123456
- **Role**: Admin

## 🧪 Running Tests

```powershell
# Run all tests
dotnet test

# Run with coverage
dotnet test --collect:"XPlat Code Coverage"

# Run specific test project
dotnet test tests/Application.UnitTests/Application.UnitTests.csproj
```

## 📁 Project Structure

```
team-sync/
├── src/
│   ├── Template.API/              # Presentation layer (REST API)
│   │   ├── Controllers/           # API endpoints (versioned)
│   │   ├── Extensions/            # Service configurations
│   │   ├── Filters/               # Global filters
│   │   └── Program.cs             # Application entry point
│   │
│   ├── Template.Application/      # Application layer (Business logic)
│   │   ├── Common/
│   │   │   ├── Behaviors/         # MediatR pipeline behaviors
│   │   │   ├── Events/            # Domain events (RabbitMQ)
│   │   │   └── Interfaces/        # Service contracts
│   │   └── Features/              # CQRS Commands & Queries
│   │       ├── Authentication/    # Login, Register, Token refresh
│   │       ├── Projects/          # Project management
│   │       ├── Tasks/             # Task management
│   │       └── ChatMessages/      # Chat functionality
│   │
│   ├── Template.Domain/           # Domain layer (Entities)
│   │   ├── Entities/              # Domain models
│   │   ├── Enums/                 # Enumerations
│   │   └── Identity/              # Identity entities
│   │
│   └── Template.Infrastructure/   # Infrastructure layer
│       ├── Data/                  # Database context & seeding
│       ├── Repositories/          # Data access
│       ├── Services/              # External services
│       │   ├── RabbitMqPublisher.cs
│       │   └── RabbitMqConsumer.cs
│       └── Hubs/                  # SignalR hubs
│           └── NotificationHub.cs
│
├── tests/
│   └── Application.UnitTests/     # Unit tests
│
├── docs/                          # Documentation
└── teamsync-frontend/             # Frontend application (Angular)
```

## 🔌 API Endpoints

### Authentication

- `POST /api/v1/auth/register` - Register new user
- `POST /api/v1/auth/login` - User login
- `POST /api/v1/auth/refresh-token` - Refresh access token

### Projects

- `GET /api/v1/projects` - Get all projects
- `GET /api/v1/projects/{id}` - Get project by ID
- `POST /api/v1/projects` - Create new project
- `PUT /api/v1/projects/{id}` - Update project
- `DELETE /api/v1/projects/{id}` - Delete project

### Tasks

- `GET /api/v1/tasks` - Get all tasks
- `GET /api/v1/tasks/{id}` - Get task by ID
- `POST /api/v1/tasks` - Create new task
- `PUT /api/v1/tasks/{id}` - Update task
- `DELETE /api/v1/tasks/{id}` - Delete task

### Chat Messages

- `GET /api/v1/chat/projects/{projectId}/messages` - Get project messages
- `POST /api/v1/chat/messages` - Send message

For complete API documentation, visit the Swagger UI after starting the application.

## 🔔 SignalR Real-time Integration

### Connect to SignalR Hub

```typescript
import * as signalR from "@microsoft/signalr";

const connection = new signalR.HubConnectionBuilder()
  .withUrl("https://localhost:4030/hubs/notifications", {
    accessTokenFactory: () => yourJwtToken,
  })
  .withAutomaticReconnect()
  .build();

await connection.start();

// Join project-specific group
await connection.invoke("JoinProjectGroup", projectId);

// Listen for events
connection.on("TaskCreated", (event) => {
  console.log("New task:", event);
});

connection.on("TaskUpdated", (event) => {
  console.log("Task updated:", event);
});

connection.on("TaskDeleted", (event) => {
  console.log("Task deleted:", event);
});
```

You can test the SignalR hub using the web-based SignalR client:

**SignalR Web Client**: https://gourav-d.github.io/SignalR-Web-Client/dist/

1. Enter the hub URL: `http://localhost:4030/hubs/notifications`
2. Add your JWT token in the connection options
3. Connect and test real-time events

## 📚 Additional Documentation

- **[Redis, RabbitMQ & SignalR Integration Strategy](docs/INTEGRATION-STRATEGY.md)** - Detailed explanation of caching, messaging, and real-time architecture
- [MediatR & CQRS Setup Guide](docs/MEDIATR-CQRS-GUIDE.md)
- [RabbitMQ & SignalR Real-time Setup](docs/RABBITMQ-SIGNALR-SETUP.md)
- [Background Consumer Implementation](docs/BACKGROUND-CONSUMER-IMPLEMENTATION.md)
- [Unit Testing Checklist](docs/UNIT-TEST-CHECKLIST.md)

## 📄 License

This project is licensed under the MIT License.

## 👤 Author

**Enamul Hasan**

- GitHub: [@Enamulhasan85](https://github.com/Enamulhasan85)

---

**Happy Coding! 🚀**
