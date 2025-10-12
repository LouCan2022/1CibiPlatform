# 1CibiPlatform

[![.NET](https://img.shields.io/badge/.NET-10.0-purple)](https://dotnet.microsoft.com/)
[![Docker](https://img.shields.io/badge/Docker-Containerized-blue)](https://www.docker.com/)
[![Blazor](https://img.shields.io/badge/Blazor-WebAssembly-green)](https://blazor.net/)
[![YARP](https://img.shields.io/badge/YARP-API_Gateway-orange)](https://microsoft.github.io/reverse-proxy/)
[![Carter](https://img.shields.io/badge/Carter-Minimal_APIs-lightblue)](https://github.com/CarterCommunity/Carter)
[![MudBlazor](https://img.shields.io/badge/MudBlazor-UI_Components-darkblue)](https://mudblazor.com/)

**1CibiPlatform** is a hybrid single platform designed for both client-facing and internal applications, built as a modular monolith using .NET 10.0.

## 🏗️ Architecture Overview

This platform follows a **Vertical Slice Architecture** approach with **Domain-Driven Design (DDD)** principles, implemented as a modular monolith with the following key components:

### 🎯 Core Components

- **🌐 Frontend (UI)** - Blazor WebAssembly application with MudBlazor UI components for rich user interfaces
- **🚪 API Gateway** - YARP-based reverse proxy for routing and load balancing
- **🔧 Backend (APIs & Modules)** - Modular REST APIs with CQRS pattern, organized as feature modules within a single deployable application
- **🧱 Building Blocks** - Shared libraries and cross-cutting concerns
- **🐳 Docker Compose** - Container orchestration for development and deployment

## 📁 Project Structure

```
1CibiPlatform/
├── UI/
│   └── FrontendWebassembly/         # Blazor WebAssembly UI with MudBlazor
│       ├── Component/
│       ├── Layout/
│       ├── Pages/
│       ├── Services/
│       ├── SharedService/
│       └── wwwroot/
├── ApiGateways/
│   └── YarpApiGateway/             # YARP Reverse Proxy
├── BackendAPI/
│   ├── API/
│   │   └── APIs/                   # Main API Host Project
│   ├── Modules/                    # Independent Feature Modules
│   │   ├── Auth/                   # Authentication Module
│   │   │   ├── Features/           # Auth vertical slices
│   │   │   ├── Services/           # Auth-specific services
│   │   │   └── Data/               # Auth data access
│   │   ├── CNX/                    # CNX Business Module
│   │   │   ├── Features/           # CNX vertical slices
│   │   │   └── Services/           # CNX-specific services
│   │   ├── Philsys/                # Philsys Integration Module
│   │   │   ├── Features/           # Philsys vertical slices
│   │   │   └── Services/           # Philsys-specific services
│   │   └── SSO/                    # Single Sign-On Module
│   │       ├── Features/           # SSO vertical slices
│   │       └── Services/           # SSO-specific services
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

- **.NET 10.0** - Latest .NET framework
- **Carter** - Minimal API framework for building HTTP APIs
- **MediatR** - CQRS and Mediator pattern implementation
- **FluentValidation** - Validation library
- **Mapster** - Object mapping
- **Microsoft Feature Management** - Feature flags

### Frontend

- **Blazor WebAssembly** - Client-side interactive web UI framework
- **MudBlazor** - Material Design component library for Blazor
- **Razor Components** - Reusable UI components

### Infrastructure

- **YARP (Yet Another Reverse Proxy)** - Microsoft's reverse proxy
- **Docker & Docker Compose** - Containerization
- **Linux Containers** - Production deployment target

### Architecture Patterns

- **Vertical Slice Architecture** - Features are organized as independent slices, each containing all layers (UI, logic, data) for a specific feature or use case
- **Domain-Driven Design (DDD)** - Domain modeling
- **CQRS** - Command Query Responsibility Segregation
- **Modular Monolith** - Organized code structure with feature modules inside a single deployable application

## 🚀 Getting Started

### Prerequisites

- [.NET 10.0 SDK](https://dotnet.microsoft.com/download/dotnet/10.0)
- [Docker Desktop](https://www.docker.com/products/docker-desktop)
- [Visual Studio 2025 Insiders](https://visualstudio.microsoft.com/vs/preview/) or [VS Code](https://code.visualstudio.com/)

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
   cd UI/FrontendWebassembly
   dotnet run
   ```

## 🏛️ Architectural Principles

### Vertical Slice Principles

- Each feature or use case is implemented as a "slice" containing its own request, handler, validation, and data access logic
- Slices are independent and encapsulate all concerns for that feature
- Encourages high cohesion and low coupling between features

### Key Features

- **🔄 CQRS Pattern** - Separate read and write operations
- **📋 Modular Monolith Design** - Organized by business domains (Auth, CNX, Philsys, SSO) as feature modules within a single application
- **🛡️ Validation** - FluentValidation for input validation
- **🗺️ Object Mapping** - Mapster for efficient object mapping
- **⚡ Feature Flags** - Microsoft Feature Management
- **🔀 API Gateway** - YARP for routing and load balancing
- **🐳 Containerization** - Docker support for all services

## 📦 Modules

The platform is organized into independent feature modules, each implementing vertical slice architecture:

### Auth Module

Handles authentication and authorization functionality including user management, login/logout, token validation, and user session management.

### CNX Module

Core business logic and functionality related to CNX operations and processes specific to the platform's primary business domain.

### Philsys Module

Integration and functionality related to the Philippine System for Civil Registration and Vital Statistics (PhilSys), handling national ID and civil registry operations.

### SSO Module

Single Sign-On implementation providing seamless authentication across multiple applications and services, enabling users to access various systems with one set of credentials.

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

---

**Built with ❤️ using .NET 10.0 and modern architectural patterns**
