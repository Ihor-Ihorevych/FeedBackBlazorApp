# FeedBack Blazor App

A full-stack movie feedback application built with **.NET 10**, **Blazor WebAssembly**, and **Clean Architecture** principles. Users can browse movies, leave comments, and administrators can moderate feedback in real-time.

## 🏗️ Architecture

This solution follows **Clean Architecture** with clear separation of concerns:

```
FB_App/
├── src/
│   ├── Domain/           # Entities, Value Objects, Domain Events
│   ├── Application/      # Use Cases, CQRS Commands/Queries, Interfaces
│   ├── Infrastructure/   # Data Access, Identity, External Services
│   ├── Web/              # ASP.NET Core Web API
│   ├── AppHost/          # .NET Aspire Orchestration
│   └── ServiceDefaults/  # Shared Service Configuration
├── tests/
│   ├── Domain.UnitTests/
│   ├── Application.UnitTests/
│   ├── Application.FunctionalTests/
│   └── Infrastructure.IntegrationTests/
└── FBUI/                 # Blazor WebAssembly Client
```

## ✨ Features

### Core Functionality
- **Movie Management** - Create, update, delete, and browse movies
- **Comment System** - Users can leave feedback on movies
- **Comment Moderation** - Admins can approve or reject comments
- **Real-time Notifications** - SignalR-powered admin notifications for new comments

### Technical Features
- **CQRS Pattern** with MediatR
- **JWT Authentication** with refresh tokens
- **Role-based Authorization** (Admin, User)
- **Fluent Validation** for request validation
- **AutoMapper** for object mapping
- **Entity Framework Core** with SQLite
- **OpenAPI/Swagger** documentation with NSwag client generation
- **.NET Aspire** for distributed application orchestration

## 🛠️ Tech Stack

### Backend
- .NET 10
- ASP.NET Core Web API
- Entity Framework Core (SQLite)
- MediatR
- FluentValidation
- AutoMapper
- ASP.NET Core Identity
- SignalR

### Frontend
- Blazor WebAssembly
- Microsoft Fluent UI Components
- Blazored LocalStorage
- NSwag API Client Generation

### Infrastructure
- .NET Aspire (AppHost)
- Azure Identity Support

## 🚀 Getting Started

### Prerequisites
- [.NET 10 SDK](https://dotnet.microsoft.com/download/dotnet/10.0)
- [Visual Studio 2022/2026](https://visualstudio.microsoft.com/) or [VS Code](https://code.visualstudio.com/)

### Running the Application

1. **Clone the repository**
   ```bash
   git clone https://github.com/Ihor-Ihorevych/FeedBackBlazorApp.git
   cd FeedBackBlazorApp
   ```

2. **Run with .NET Aspire** (Recommended)
   ```bash
   cd FB_App/src/AppHost
   dotnet run
   ```
   This will start both the Web API and Blazor WASM client with proper orchestration.

3. **Or run projects individually**
   ```bash
   # Terminal 1 - Start the API
   cd FB_App/src/Web
   dotnet run

   # Terminal 2 - Start the Blazor client
   cd FBUI
   dotnet run
   ```

### Running Tests

```bash
# Run all tests
dotnet test

# Run specific test projects
dotnet test FB_App/tests/Application.UnitTests
dotnet test FB_App/tests/Domain.UnitTests
dotnet test FB_App/tests/Infrastructure.IntegrationTests
```

## 📁 Project Structure

### Domain Layer
Contains enterprise business rules:
- **Entities**: `Movie`, `Comment`
- **Value Objects**: `MovieId`, `CommentId`
- **Domain Events**: `MovieCreatedEvent`, `CommentCreatedEvent`, `CommentApprovedEvent`, etc.
- **Enums**: `CommentStatus`

### Application Layer
Contains application business rules:
- **Commands**: `CreateMovie`, `DeleteMovie`, `UpdateMovie`, `CreateComment`, `ApproveComment`, `RejectComment`
- **Queries**: `GetMovies`, `GetMovieById`, `GetCommentsByMovie`, `GetCurrentUser`
- **Behaviors**: Validation, Authorization, Logging, Performance tracking, Atomic operations
- **Interfaces**: `IApplicationDbContext`, `IIdentityService`, `IAdminNotificationService`

### Infrastructure Layer
Contains external concerns implementation:
- Entity Framework Core DbContext
- Identity Services (JWT authentication)
- SQLite database configuration

### Web Layer
Contains API endpoints and configuration:
- REST API Controllers
- OpenAPI/Swagger documentation
- Middleware configuration
- Dependency injection setup

### FBUI (Blazor Client)
Contains the frontend application:
- Razor Components
- Authentication State Provider
- API Client Services
- Admin Notification Panel

## 🔐 Authentication

The application uses JWT Bearer authentication with:
- Access tokens for API authorization
- Refresh tokens for seamless token renewal
- Role-based access control (Admin, User roles)

## 📝 API Documentation

When running the Web API, Swagger UI is available at:
```
https://localhost:{port}/swagger
```

## 🤝 Contributing

1. Fork the repository
2. Create a feature branch (`git checkout -b feature/amazing-feature`)
3. Commit your changes (`git commit -m 'Add amazing feature'`)
4. Push to the branch (`git push origin feature/amazing-feature`)
5. Open a Pull Request

## 📄 License

This project is open source and available under the [MIT License](LICENSE).

## 👤 Author

**Ihor Voloshyn**
- GitHub: [@Ihor-Ihorevych](https://github.com/Ihor-Ihorevych)
