
# Student Performance Tracker Documentation

## Overview

Welcome to the Student Performance Tracker project documentation! This ASP.NET Core MVC application manages academic performance tracking with support for multiple user roles and comprehensive course management.

## 📚 Documentation Index

The documentation has been organized into focused sections for easy navigation:

### Getting Started
- **[Project Overview](docs/01-project-overview.md)** - What the project is and its purpose
- **[Quick Start Guide](docs/02-quick-start.md)** - Get up and running in minutes
- **[Tech Stack](docs/03-tech-stack.md)** - Technologies and frameworks used

### Architecture & Design
- **[System Architecture](docs/04-system-architecture.md)** - How components work together
- **[Database Schema](docs/05-database-schema.md)** - Data models and relationships
- **[Project Structure](docs/06-project-structure.md)** - Code organization and layers

### Development
- **[Development Guidelines](docs/07-development-guidelines.md)** - How to add features and make changes
- **[Common Commands](docs/08-common-commands.md)** - Frequently used CLI commands
- **[Implementation Status](docs/09-implementation-status.md)** - What's done and what's next

## 🚀 Quick Links

### For New Team Members
1. Read the [Project Overview](docs/01-project-overview.md) to understand the system
2. Follow the [Quick Start Guide](docs/02-quick-start.md) to set up your development environment
3. Study the [System Architecture](docs/04-system-architecture.md) to understand how components interact

### For Developers
1. Review [Development Guidelines](docs/07-development-guidelines.md) for coding standards
2. Check [Implementation Status](docs/09-implementation-status.md) to see what needs to be built
3. Use [Common Commands](docs/08-common-commands.md) for daily development tasks

### For Database Work
1. Study the [Database Schema](docs/05-database-schema.md) for entity relationships
2. Follow database procedures in [Development Guidelines](docs/07-development-guidelines.md)
3. Use migration commands from [Common Commands](docs/08-common-commands.md)

## 🎯 Key Information

### Tech Stack Summary
- **Framework:** ASP.NET Core 8 MVC
- **Database:** PostgreSQL (Supabase)
- **ORM:** Entity Framework Core
- **Authentication:** ASP.NET Core Identity
- **Email:** SendGrid

### Architecture Pattern
The application follows a **layered architecture**:
```
Presentation Layer (Controllers/Views) 
    ↓
Business Logic Layer (Services)
    ↓  
Data Access Layer (Repositories)
    ↓
Database (PostgreSQL)
```

### Current Status
- ✅ **Authentication & User Management** - Complete
- ✅ **Database Schema** - Complete  
- ✅ **Project Structure** - Complete
- 🚧 **Course/Class Management** - In Progress
- ❌ **Grade Management** - Not Started
- ❌ **Dashboards & Reports** - Not Started

## 📋 Next Steps

### Immediate Priorities
1. Implement Course and Class management controllers
2. Create user interfaces for academic management
3. Add role-based authorization to controllers
4. Build basic dashboards for each user role

### Getting Help

- **Setup Issues:** Check the [Quick Start Guide](docs/02-quick-start.md)
- **Architecture Questions:** Read [System Architecture](docs/04-system-architecture.md)
- **Development Help:** Follow [Development Guidelines](docs/07-development-guidelines.md)
- **Command Reference:** Use [Common Commands](docs/08-common-commands.md)

---

*This documentation is designed to help you understand and contribute to the Student Performance Tracker project. Each section focuses on specific aspects of the system for easy reference.*

