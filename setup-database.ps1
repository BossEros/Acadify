# Database Connection Setup Script
# This script helps you set up your database connection string

Write-Host "=== Database Connection Setup ===" -ForegroundColor Cyan
Write-Host ""

# Check if we're in the right directory
if (-not (Test-Path "ASI.Basecode.WebApp\ASI.Basecode.WebApp.csproj")) {
    Write-Host "Error: Please run this script from the project root directory" -ForegroundColor Red
    exit 1
}

Write-Host "Choose your database setup option:" -ForegroundColor Yellow
Write-Host "1. Use Supabase (Cloud PostgreSQL)"
Write-Host "2. Use Local PostgreSQL"
Write-Host "3. I already have a connection string"
Write-Host ""
$choice = Read-Host "Enter your choice (1-3)"

$connectionString = ""

switch ($choice) {
    "1" {
        Write-Host ""
        Write-Host "To get your Supabase connection string:" -ForegroundColor Cyan
        Write-Host "1. Go to https://app.supabase.com"
        Write-Host "2. Select your project"
        Write-Host "3. Go to Settings → Database"
        Write-Host "4. Copy the connection string from the 'Connection string' section"
        Write-Host ""
        Write-Host "Connection string format should be:" -ForegroundColor Yellow
        Write-Host "Host=db.xxxxx.supabase.co;Port=5432;Database=postgres;Username=postgres;Password=xxxxx;SSL Mode=Require;" -ForegroundColor Gray
        Write-Host ""
        $connectionString = Read-Host "Paste your Supabase connection string here"
    }
    "2" {
        Write-Host ""
        Write-Host "Local PostgreSQL Setup" -ForegroundColor Cyan
        $dbName = Read-Host "Database name (default: acadify)" 
        if ([string]::IsNullOrWhiteSpace($dbName)) { $dbName = "acadify" }
        
        $dbUser = Read-Host "PostgreSQL username (default: postgres)"
        if ([string]::IsNullOrWhiteSpace($dbUser)) { $dbUser = "postgres" }
        
        $dbPassword = Read-Host "PostgreSQL password" -AsSecureString
        $dbPasswordPlain = [Runtime.InteropServices.Marshal]::PtrToStringAuto(
            [Runtime.InteropServices.Marshal]::SecureStringToBSTR($dbPassword)
        )
        
        $connectionString = "Host=localhost;Port=5432;Database=$dbName;Username=$dbUser;Password=$dbPasswordPlain"
        
        Write-Host ""
        Write-Host "Note: Make sure PostgreSQL is installed and the database '$dbName' exists." -ForegroundColor Yellow
        Write-Host "You can create it with: CREATE DATABASE $dbName;" -ForegroundColor Gray
    }
    "3" {
        Write-Host ""
        $connectionString = Read-Host "Enter your connection string"
    }
    default {
        Write-Host "Invalid choice. Exiting." -ForegroundColor Red
        exit 1
    }
}

if ([string]::IsNullOrWhiteSpace($connectionString)) {
    Write-Host "Error: Connection string cannot be empty" -ForegroundColor Red
    exit 1
}

Write-Host ""
Write-Host "Setting up connection string..." -ForegroundColor Green

# Set user secret (recommended)
try {
    Set-Location "ASI.Basecode.WebApp"
    dotnet user-secrets set "ConnectionStrings:DefaultConnection" $connectionString
    Write-Host "✓ Connection string set in user secrets" -ForegroundColor Green
} catch {
    Write-Host "Warning: Could not set user secret. You may need to install .NET SDK." -ForegroundColor Yellow
    Write-Host "You can manually update appsettings.json instead." -ForegroundColor Yellow
}

# Also update appsettings.json as backup
try {
    Set-Location ".."
    $appsettingsPath = "ASI.Basecode.WebApp\appsettings.json"
    $appsettings = Get-Content $appsettingsPath -Raw | ConvertFrom-Json
    $appsettings.ConnectionStrings.DefaultConnection = $connectionString
    $appsettings | ConvertTo-Json -Depth 10 | Set-Content $appsettingsPath
    Write-Host "✓ Connection string also updated in appsettings.json" -ForegroundColor Green
} catch {
    Write-Host "Warning: Could not update appsettings.json" -ForegroundColor Yellow
}

Write-Host ""
Write-Host "=== Setup Complete! ===" -ForegroundColor Green
Write-Host ""
Write-Host "Next steps:" -ForegroundColor Cyan
Write-Host "1. Apply database migrations:" -ForegroundColor Yellow
Write-Host "   dotnet ef database update --project ASI.Basecode.Data --startup-project ASI.Basecode.WebApp" -ForegroundColor Gray
Write-Host ""
Write-Host "2. Run the application:" -ForegroundColor Yellow
Write-Host "   cd ASI.Basecode.WebApp" -ForegroundColor Gray
Write-Host "   dotnet run" -ForegroundColor Gray
Write-Host ""

