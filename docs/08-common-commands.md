# Common Commands

## Quick Reference

This page contains frequently used commands for daily development tasks. Copy and paste these commands as needed.

## Project Setup

### Initial Setup
```bash
# Clone repository
git clone [repository-url]
cd Student-Performance-Tracker

# Set up database connection (get connection string from team lead)
dotnet user-secrets set "ConnectionStrings:DefaultConnection" "[Your-Supabase-Connection-String]"

# Apply database migrations
cd ASI.Basecode.WebApp
dotnet ef database update

# Run the application
dotnet run
```

### Daily Development Setup
```bash
# Pull latest changes
git pull origin main

# Apply any new migrations
dotnet ef database update --project ASI.Basecode.Data --startup-project ASI.Basecode.WebApp

# Run the application
dotnet run --project ASI.Basecode.WebApp
```

## Database Operations

### Entity Framework Migrations

#### Create New Migration
```bash
# Navigate to solution root first
cd [solution-root]

# Create migration
dotnet ef migrations add [MigrationName] --project ASI.Basecode.Data --startup-project ASI.Basecode.WebApp

# Examples:
dotnet ef migrations add AddAssignmentEntity --project ASI.Basecode.Data --startup-project ASI.Basecode.WebApp
dotnet ef migrations add UpdateUserProfileFields --project ASI.Basecode.Data --startup-project ASI.Basecode.WebApp
```

#### Apply Migrations
```bash
# Apply all pending migrations
dotnet ef database update --project ASI.Basecode.Data --startup-project ASI.Basecode.WebApp

# Apply to specific migration
dotnet ef database update [MigrationName] --project ASI.Basecode.Data --startup-project ASI.Basecode.WebApp
```

#### View Migration Status
```bash
# List all migrations and their status
dotnet ef migrations list --project ASI.Basecode.Data --startup-project ASI.Basecode.WebApp

# View migration history
dotnet ef database update --project ASI.Basecode.Data --startup-project ASI.Basecode.WebApp --verbose
```

#### Remove Last Migration (if not applied)
```bash
# Remove the last migration (only if not applied to database)
dotnet ef migrations remove --project ASI.Basecode.Data --startup-project ASI.Basecode.WebApp
```

#### Generate SQL Script
```bash
# Generate SQL script for all migrations
dotnet ef migrations script --project ASI.Basecode.Data --startup-project ASI.Basecode.WebApp

# Generate SQL script from specific migration to latest
dotnet ef migrations script [FromMigration] --project ASI.Basecode.Data --startup-project ASI.Basecode.WebApp
```

### Database Troubleshooting

#### Reset Database (Development Only)
```bash
# Drop database and recreate
dotnet ef database drop --project ASI.Basecode.Data --startup-project ASI.Basecode.WebApp
dotnet ef database update --project ASI.Basecode.Data --startup-project ASI.Basecode.WebApp
```

#### View Database Connection
```bash
# Test database connection
dotnet ef dbcontext info --project ASI.Basecode.Data --startup-project ASI.Basecode.WebApp
```

## Build and Run

### Build Commands
```bash
# Build entire solution
dotnet build

# Build specific project
dotnet build ASI.Basecode.WebApp

# Build in Release mode
dotnet build --configuration Release

# Clean build artifacts
dotnet clean
```

### Run Commands
```bash
# Run web application
dotnet run --project ASI.Basecode.WebApp

# Run with specific environment
dotnet run --project ASI.Basecode.WebApp --environment Development

# Run and watch for changes (auto-restart)
dotnet watch run --project ASI.Basecode.WebApp
```

### Package Management
```bash
# Restore NuGet packages
dotnet restore

# Add package to specific project
dotnet add ASI.Basecode.WebApp package [PackageName]

# Remove package from project
dotnet remove ASI.Basecode.WebApp package [PackageName]

# List packages in project
dotnet list ASI.Basecode.WebApp package

# Update packages
dotnet add ASI.Basecode.WebApp package [PackageName] --version [Version]
```

## Testing

### Run Tests
```bash
# Run all tests
dotnet test

# Run tests with verbose output
dotnet test --verbosity normal

# Run tests in specific project
dotnet test ASI.Basecode.Tests

# Run tests with coverage
dotnet test --collect:"XPlat Code Coverage"
```

### Create Test Project
```bash
# Create new test project
dotnet new mstest -n ASI.Basecode.Tests

# Add test project to solution
dotnet sln add ASI.Basecode.Tests

# Add reference to project being tested
dotnet add ASI.Basecode.Tests reference ASI.Basecode.Services
```

## User Secrets Management

### Set User Secrets
```bash
# Set connection string
dotnet user-secrets set "ConnectionStrings:DefaultConnection" "[connection-string]"

# Set SendGrid API key
dotnet user-secrets set "SendGrid:ApiKey" "[api-key]"

# Set multiple related settings
dotnet user-secrets set "SendGrid:FromEmail" "noreply@yourapp.com"
dotnet user-secrets set "SendGrid:FromName" "Student Performance Tracker"
```

### View User Secrets
```bash
# List all user secrets
dotnet user-secrets list --project ASI.Basecode.WebApp

# Clear all user secrets
dotnet user-secrets clear --project ASI.Basecode.WebApp

# Remove specific secret
dotnet user-secrets remove "SendGrid:ApiKey" --project ASI.Basecode.WebApp
```

## Git Commands

### Daily Git Workflow
```bash
# Check status
git status

# Pull latest changes
git pull origin main

# Create new feature branch
git checkout -b feature/assignment-management

# Stage changes
git add .

# Commit changes
git commit -m "Add assignment creation functionality"

# Push branch
git push origin feature/assignment-management

# Switch back to main
git checkout main

# Delete feature branch (after merge)
git branch -d feature/assignment-management
```

### Git Troubleshooting
```bash
# Undo last commit (keep changes)
git reset --soft HEAD~1

# Undo last commit (discard changes)
git reset --hard HEAD~1

# Stash changes temporarily
git stash
git stash pop

# View commit history
git log --oneline

# View changes in specific file
git diff [filename]
```

## Development Tools

### Code Formatting
```bash
# Format code in solution
dotnet format

# Format specific project
dotnet format ASI.Basecode.WebApp

# Check formatting without applying changes
dotnet format --verify-no-changes
```

### Project Templates
```bash
# Create new controller
dotnet new controller -n AssignmentController -o Controllers

# Create new class
dotnet new class -n AssignmentService -o Services/Implementation

# Create new interface
dotnet new interface -n IAssignmentService -o Services/Interfaces
```

## Debugging

### Debug Information
```bash
# Run with debug information
dotnet run --project ASI.Basecode.WebApp --configuration Debug

# Enable detailed errors
export ASPNETCORE_ENVIRONMENT=Development
dotnet run --project ASI.Basecode.WebApp
```

### Logging
```bash
# Run with specific log level
dotnet run --project ASI.Basecode.WebApp -- --Logging:LogLevel:Default=Debug

# View application logs
tail -f logs/app.log
```

## Performance

### Performance Analysis
```bash
# Run performance tests
dotnet run --project ASI.Basecode.WebApp --configuration Release

# Profile application
dotnet-trace collect --process-id [pid] --providers Microsoft-AspNetCore-Server-Kestrel
```

## Deployment

### Publish Application
```bash
# Publish for production
dotnet publish ASI.Basecode.WebApp --configuration Release --output ./publish

# Publish for specific runtime
dotnet publish ASI.Basecode.WebApp --configuration Release --runtime win-x64 --self-contained

# Create deployment package
dotnet publish ASI.Basecode.WebApp --configuration Release --output ./publish --no-restore
```

## Troubleshooting Common Issues

### Port Already in Use
```bash
# Find process using port 5001
netstat -ano | findstr :5001

# Kill process (Windows)
taskkill /PID [process-id] /F

# Kill process (macOS/Linux)
kill -9 [process-id]
```

### Package Restore Issues
```bash
# Clear NuGet cache
dotnet nuget locals all --clear

# Restore packages with verbose output
dotnet restore --verbosity detailed

# Force package restore
dotnet restore --force
```

### Database Connection Issues
```bash
# Test connection string
dotnet ef dbcontext info --project ASI.Basecode.Data --startup-project ASI.Basecode.WebApp

# Verify user secrets
dotnet user-secrets list --project ASI.Basecode.WebApp

# Check migration status
dotnet ef migrations list --project ASI.Basecode.Data --startup-project ASI.Basecode.WebApp
```

## Environment Variables

### Set Environment Variables (Windows)
```cmd
set ASPNETCORE_ENVIRONMENT=Development
set ConnectionStrings__DefaultConnection=[connection-string]
```

### Set Environment Variables (macOS/Linux)
```bash
export ASPNETCORE_ENVIRONMENT=Development
export ConnectionStrings__DefaultConnection="[connection-string]"
```

### PowerShell
```powershell
$env:ASPNETCORE_ENVIRONMENT="Development"
$env:ConnectionStrings__DefaultConnection="[connection-string]"
```

---

*Bookmark this page for quick access to commonly used commands. Most commands should be run from the solution root directory unless otherwise specified.*