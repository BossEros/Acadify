# Quick Start Guide

Get the Student Performance Tracker running on your local machine in just a few steps.

## Prerequisites

Before you begin, ensure you have:

### Required Software
- **.NET 8 SDK** - [Download here](https://dotnet.microsoft.com/download/dotnet/8.0)
- **Git** - For cloning the repository
- **Code Editor** - Visual Studio, VS Code, or JetBrains Rider

### Required Access
- **Supabase Connection String** - Get this from your team lead
- **SendGrid API Key** - For email functionality (optional for basic setup)

## Setup Steps

### 1. Clone the Repository
```bash
git clone [repository-url]
cd Student-Performance-Tracker
```

### 2. Configure Database Connection
The project uses .NET User Secrets to keep database credentials secure:

```bash
dotnet user-secrets set "ConnectionStrings:DefaultConnection" "[Your-Supabase-Connection-String]"
```

**Important:** Replace `[Your-Supabase-Connection-String]` with the actual connection string from your team lead.

### 3. Apply Database Migrations
Navigate to the web application directory and update the database:

```bash
cd ASI.Basecode.WebApp
dotnet ef database update
```

This will create all necessary tables in your database.

### 4. Run the Application
Start the development server:

```bash
dotnet run
```

The application will be available at:
- **HTTPS:** `https://localhost:5001`
- **HTTP:** `http://localhost:5000`

## Verify Installation

### Test the Application
1. Open your browser and navigate to `https://localhost:5001`
2. You should see the login page
3. Try registering a new account to test the system

### Check Database Connection
If the application starts without errors and you can access the registration page, your database connection is working correctly.

## Troubleshooting

### Common Issues

#### "Connection string not found"
- Ensure you've set the user secret correctly
- Verify the connection string format with your team lead

#### "Database update failed"
- Check that your connection string is correct
- Ensure you have proper database permissions
- Try running the migration command again

#### "Port already in use"
- Another application might be using port 5001
- Stop other .NET applications or change the port in `launchSettings.json`

#### "Package restore failed"
- Run `dotnet restore` in the solution root directory
- Check your internet connection

### Getting Help

If you encounter issues:
1. Check the console output for specific error messages
2. Verify all prerequisites are installed
3. Confirm your connection string with the team lead
4. Ask for help in the team chat with the specific error message

## Next Steps

Once you have the application running:

1. **Explore the Code** - Review the [System Architecture](04-system-architecture.md) documentation
2. **Understand the Database** - Check out the [Database Schema](05-database-schema.md)
3. **Start Development** - Follow the [Development Guidelines](07-development-guidelines.md)

## Development Workflow

For daily development, you'll typically:

```bash
# Pull latest changes
git pull origin main

# Apply any new migrations
dotnet ef database update --project ASI.Basecode.Data --startup-project ASI.Basecode.WebApp

# Run the application
dotnet run --project ASI.Basecode.WebApp
```

---

*You're now ready to start working with the Student Performance Tracker! Check the other documentation files for detailed information about the system.*