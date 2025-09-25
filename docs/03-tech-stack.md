# Technology Stack

## Overview

The Student Performance Tracker is built using modern .NET technologies with a focus on maintainability, security, and scalability.

## Backend Technologies

### Core Framework
- **ASP.NET Core 8 MVC** - Web application framework
  - Model-View-Controller pattern
  - Built-in dependency injection
  - Cross-platform compatibility
  - High performance and scalability

### Database & ORM
- **PostgreSQL** - Primary database (hosted on Supabase)
  - ACID compliance
  - Advanced data types
  - Excellent performance
  - Open source

- **Entity Framework Core** - Object-Relational Mapping
  - Code-first migrations
  - LINQ query support
  - Change tracking
  - Database provider abstraction

### Authentication & Security
- **ASP.NET Core Identity** - User management system
  - Built-in user registration/login
  - Password hashing and validation
  - Role-based authorization
  - Account lockout protection
  - Two-factor authentication support

### External Services
- **SendGrid** - Email service provider
  - Reliable email delivery
  - Template management
  - Analytics and tracking
  - Scalable infrastructure

## Architecture Patterns

### Clean Architecture
The application follows clean architecture principles:
- **Separation of Concerns** - Each layer has specific responsibilities
- **Dependency Inversion** - Higher layers depend on abstractions
- **Testability** - Easy to unit test individual components
- **Maintainability** - Changes in one layer don't affect others

### Repository Pattern
- **Data Abstraction** - Repositories abstract database operations
- **Testability** - Easy to mock for unit testing
- **Flexibility** - Can switch data providers without changing business logic

### Dependency Injection
- **Loose Coupling** - Components depend on interfaces, not implementations
- **Lifecycle Management** - Framework manages object creation and disposal
- **Configuration** - Services configured in `Program.cs`

## Project Structure

### ASI.Basecode.WebApp
- **Framework:** ASP.NET Core MVC
- **Purpose:** Presentation layer
- **Contains:** Controllers, Views, ViewModels, Static files

### ASI.Basecode.Services
- **Pattern:** Service layer
- **Purpose:** Business logic
- **Contains:** Service interfaces and implementations, DTOs

### ASI.Basecode.Data
- **Framework:** Entity Framework Core
- **Purpose:** Data access layer
- **Contains:** Models, DbContext, Repositories, Migrations

### ASI.Basecode.Resources
- **Purpose:** Resource management
- **Contains:** Localization files, Message constants, Email templates

## Development Tools

### Package Management
- **NuGet** - .NET package manager
- **Package References** - Modern package management approach

### Database Migrations
- **Entity Framework Migrations** - Version control for database schema
- **Code-First Approach** - Database schema defined in C# models

### Configuration Management
- **User Secrets** - Local development configuration
- **appsettings.json** - Application configuration
- **Environment Variables** - Production configuration

## Key NuGet Packages

### Core Packages
```xml
<PackageReference Include="Microsoft.AspNetCore.App" />
<PackageReference Include="Microsoft.EntityFrameworkCore" />
<PackageReference Include="Microsoft.AspNetCore.Identity.EntityFrameworkCore" />
```

### Database
```xml
<PackageReference Include="Npgsql.EntityFrameworkCore.PostgreSQL" />
<PackageReference Include="Microsoft.EntityFrameworkCore.Tools" />
```

### External Services
```xml
<PackageReference Include="SendGrid" />
```

## Security Features

### Authentication
- **Password Requirements** - Complex password policies
- **Account Lockout** - Protection against brute force attacks
- **Secure Cookies** - HttpOnly and Secure flags
- **CSRF Protection** - Anti-forgery tokens

### Authorization
- **Role-Based Access** - Admin, Teacher, Student roles
- **Claims-Based Authorization** - Fine-grained permissions
- **Resource-Based Authorization** - Context-aware access control

### Data Protection
- **Connection String Security** - User Secrets for local development
- **SQL Injection Prevention** - Parameterized queries via EF Core
- **XSS Protection** - Razor view engine automatic encoding

## Performance Considerations

### Database
- **Connection Pooling** - Efficient database connection management
- **Async Operations** - Non-blocking database calls
- **Query Optimization** - LINQ to SQL translation

### Caching
- **Built-in Caching** - ASP.NET Core memory caching
- **Response Caching** - HTTP response caching middleware

### Scalability
- **Stateless Design** - No server-side session state
- **Dependency Injection** - Proper service lifetimes
- **Async/Await** - Scalable request handling

## Development Environment

### Supported Platforms
- **Windows** - Primary development platform
- **macOS** - Cross-platform .NET support
- **Linux** - Production deployment option

### IDE Support
- **Visual Studio** - Full-featured IDE with debugging
- **Visual Studio Code** - Lightweight editor with extensions
- **JetBrains Rider** - Cross-platform .NET IDE

---

*This technology stack provides a solid foundation for building scalable, maintainable web applications. Each technology was chosen for its specific strengths and how it contributes to the overall architecture.*