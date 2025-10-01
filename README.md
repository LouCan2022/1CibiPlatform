# 1CibiPlatform

[![.NET](https://img.shields.io/badge/.NET-9.0-purple)](https://dotnet.microsoft.com/)
[![Docker](https://img.shields.io/badge/Docker-Containerized-blue)](https://www.docker.com/)
[![Blazor](https://img.shields.io/badge/Blazor-Frontend-green)](https://blazor.net/)
[![YARP](https://img.shields.io/badge/YARP-API_Gateway-orange)](https://microsoft.github.io/reverse-proxy/)

**1CibiPlatform** is a hybrid single platform designed for both client-facing and internal applications, built as a modular monolith using .NET 9.0.

## 🏗️ Architecture Overview

This platform follows a **Clean Architecture** approach with **Domain-Driven Design (DDD)** principles, implemented as a modular monolith with the following key components:

### 🎯 Core Components

- **🌐 Frontend (UI)** - Blazor Server application for user interfaces
- **🚪 API Gateway** - YARP-based reverse proxy for routing and load balancing
- **🔧 Backend (APIs & Modules)** - Modular REST APIs with CQRS pattern, organized as feature modules within a single deployable application
- **🧱 Building Blocks** - Shared libraries and cross-cutting concerns
- **🐳 Docker Compose** - Container orchestration for development and deployment

## 📁 Project Structure

```
1CibiPlatform/
├── UI/
│   └── Frontend/                    # Blazor Server UI
│       ├── Components/
│       ├── Pages/
│       └── wwwroot/
├── ApiGateways/
│   └── YarpApiGateway/             # YARP Reverse Proxy
├── BackendAPI/
│   ├── API/
│   │   └── APIs/                   # Main API Project
│   │       └── Modules/            # Modular Feature Design
│   │           ├── CNX/            # CNX Feature Module
│   │           └── Philsys/        # Philsys Feature Module
│   └── BuildingBlocks/
│       └── BuildingBlocks/         # Shared Libraries
│           ├── CQRS/              # Command Query Responsibility Segregation
│           ├── Behaviors/         # MediatR Pipeline Behaviors
│           ├── Exceptions/        # Custom Exception Handling
│           └── Pagination/        # Pagination Utilities
└── Docker/                        # Docker configuration files
```

## 🛠️ Technology Stack

### Backend

- **.NET 9.0** - Latest .NET framework
- **MediatR** - CQRS and Mediator pattern implementation
- **FluentValidation** - Validation library
- **Mapster** - Object mapping
- **Microsoft Feature Management** - Feature flags

### Frontend

- **Blazor Server** - Interactive web UI framework
- **Razor Components** - Reusable UI components

### Infrastructure

- **YARP (Yet Another Reverse Proxy)** - Microsoft's reverse proxy
- **Docker & Docker Compose** - Containerization
- **Linux Containers** - Production deployment target

### Architecture Patterns

- **Clean Architecture** - Separation of concerns
- **Domain-Driven Design (DDD)** - Domain modeling
- **CQRS** - Command Query Responsibility Segregation
- **Modular Monolith** - Organized code structure with feature modules inside a single deployable application

## 🚀 Getting Started

### Prerequisites

- [.NET 9.0 SDK](https://dotnet.microsoft.com/download/dotnet/9.0)
- [Docker Desktop](https://www.docker.com/products/docker-desktop)
- [Visual Studio 2022](https://visualstudio.microsoft.com/) or [VS Code](https://code.visualstudio.com/)

### Running with Docker Compose

1. **Clone the repository**

   ```bash
   git clone https://github.com/RusselG21/1CibiPlatform.git
   cd 1CibiPlatform
   ```

2. **Build and run with Docker Compose**

   ```bash
   docker-compose up --build
   ```

3. **Access the applications**
   - Frontend UI: `http://localhost:[port]`
   - API Gateway: `http://localhost:[port]`
   - APIs: `http://localhost:[port]`

### Running Locally for Development

1. **Restore NuGet packages**

   ```bash
   dotnet restore
   ```

2. **Build the solution**

   ```bash
   dotnet build
   ```

3. **Run individual projects**

   ```bash
   # Run API Gateway
   cd ApiGateways/YarpApiGateway
   dotnet run

   # Run Backend APIs
   cd BackendAPI/API/APIs
   dotnet run

   # Run Frontend
   cd UI/Frontend
   dotnet run
   ```

## 🏛️ Architectural Principles

### Clean Architecture Layers

- **Presentation Layer** - UI components and API controllers
- **Application Layer** - Use cases, CQRS handlers, and application logic
- **Domain Layer** - Business logic and domain entities
- **Infrastructure Layer** - External concerns (databases, external services)

### Key Features

- **🔄 CQRS Pattern** - Separate read and write operations
- **📋 Modular Monolith Design** - Organized by business domains (CNX, Philsys) as feature modules within a single application
- **🛡️ Validation** - FluentValidation for input validation
- **🗺️ Object Mapping** - Mapster for efficient object mapping
- **⚡ Feature Flags** - Microsoft Feature Management
- **🔀 API Gateway** - YARP for routing and load balancing
- **🐳 Containerization** - Docker support for all services

## 🤝 Contributing

1. Fork the repository
2. Create a feature branch (`git checkout -b feature/amazing-feature`)
3. Commit your changes (`git commit -m 'Add some amazing feature'`)
4. Push to the branch (`git push origin feature/amazing-feature`)
5. Open a Pull Request

## 📄 License

This project is licensed under the terms specified in the [LICENSE](LICENSE) file.

## 📞 Support

For support and questions, please contact the development team or create an issue in the repository.
