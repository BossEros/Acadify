# PostgreSQL Setup Script for Acadify
# This script helps configure PostgreSQL after installation

Write-Host "=== PostgreSQL Setup for Acadify ===" -ForegroundColor Cyan
Write-Host ""

# Check if PostgreSQL is installed
$pgPath = "C:\Program Files\PostgreSQL\16\bin\psql.exe"
if (-not (Test-Path $pgPath)) {
    Write-Host "PostgreSQL not found at expected location. Checking alternative locations..." -ForegroundColor Yellow
    $pgPath = Get-ChildItem -Path "C:\Program Files\PostgreSQL" -Recurse -Filter "psql.exe" -ErrorAction SilentlyContinue | Select-Object -First 1 -ExpandProperty FullName
    if (-not $pgPath) {
        Write-Host "Error: PostgreSQL not found. Please install PostgreSQL first." -ForegroundColor Red
        exit 1
    }
}

Write-Host "Found PostgreSQL at: $pgPath" -ForegroundColor Green
Write-Host ""

# Get PostgreSQL password
Write-Host "During PostgreSQL installation, you should have set a password for the 'postgres' user." -ForegroundColor Yellow
Write-Host "If you don't remember it, you can reset it using pgAdmin or by editing pg_hba.conf" -ForegroundColor Yellow
Write-Host ""
$password = Read-Host "Enter the PostgreSQL password for user 'postgres'" -AsSecureString
$passwordPlain = [Runtime.InteropServices.Marshal]::PtrToStringAuto(
    [Runtime.InteropServices.Marshal]::SecureStringToBSTR($password)
)

# Set environment variable for password (psql uses PGPASSWORD)
$env:PGPASSWORD = $passwordPlain

Write-Host ""
Write-Host "Testing connection..." -ForegroundColor Cyan

# Test connection
$testResult = & $pgPath -U postgres -h localhost -c "SELECT version();" 2>&1
if ($LASTEXITCODE -ne 0) {
    Write-Host "Connection failed. Error:" -ForegroundColor Red
    Write-Host $testResult -ForegroundColor Red
    Write-Host ""
    Write-Host "Possible solutions:" -ForegroundColor Yellow
    Write-Host "1. Check if PostgreSQL service is running: Get-Service postgresql*" -ForegroundColor Gray
    Write-Host "2. Verify the password is correct" -ForegroundColor Gray
    Write-Host "3. Check if PostgreSQL is listening on port 5432" -ForegroundColor Gray
    $env:PGPASSWORD = $null
    exit 1
}

Write-Host "✓ Connection successful!" -ForegroundColor Green
Write-Host ""

# Check if database exists
Write-Host "Checking if database 'acadify' exists..." -ForegroundColor Cyan
$dbExists = & $pgPath -U postgres -h localhost -t -c "SELECT 1 FROM pg_database WHERE datname='acadify';" 2>&1

if ($dbExists -match "1") {
    Write-Host "Database 'acadify' already exists." -ForegroundColor Yellow
    $createDb = Read-Host "Do you want to recreate it? (This will delete all data!) [y/N]"
    if ($createDb -eq "y" -or $createDb -eq "Y") {
        Write-Host "Dropping existing database..." -ForegroundColor Yellow
        & $pgPath -U postgres -h localhost -c "DROP DATABASE acadify;" 2>&1 | Out-Null
        Write-Host "Creating new database..." -ForegroundColor Cyan
        & $pgPath -U postgres -h localhost -c "CREATE DATABASE acadify;" 2>&1 | Out-Null
        Write-Host "✓ Database created successfully!" -ForegroundColor Green
    }
} else {
    Write-Host "Creating database 'acadify'..." -ForegroundColor Cyan
    & $pgPath -U postgres -h localhost -c "CREATE DATABASE acadify;" 2>&1 | Out-Null
    if ($LASTEXITCODE -eq 0) {
        Write-Host "✓ Database created successfully!" -ForegroundColor Green
    } else {
        Write-Host "Error creating database. Please check the output above." -ForegroundColor Red
        $env:PGPASSWORD = $null
        exit 1
    }
}

# Update connection string in appsettings.json
Write-Host ""
Write-Host "Updating connection string in appsettings.json..." -ForegroundColor Cyan
$appsettingsPath = "ASI.Basecode.WebApp\appsettings.json"

if (Test-Path $appsettingsPath) {
    $appsettings = Get-Content $appsettingsPath -Raw | ConvertFrom-Json
    $connectionString = "Host=localhost;Port=5432;Database=acadify;Username=postgres;Password=$passwordPlain"
    $appsettings.ConnectionStrings.DefaultConnection = $connectionString
    $appsettings | ConvertTo-Json -Depth 10 | Set-Content $appsettingsPath
    Write-Host "✓ Connection string updated!" -ForegroundColor Green
} else {
    Write-Host "Warning: appsettings.json not found at $appsettingsPath" -ForegroundColor Yellow
}

# Also set as user secret
Write-Host ""
Write-Host "Setting connection string in user secrets..." -ForegroundColor Cyan
try {
    Set-Location "ASI.Basecode.WebApp"
    dotnet user-secrets set "ConnectionStrings:DefaultConnection" $connectionString 2>&1 | Out-Null
    Write-Host "✓ User secret set!" -ForegroundColor Green
    Set-Location ".."
} catch {
    Write-Host "Warning: Could not set user secret. You may need to install .NET SDK." -ForegroundColor Yellow
}

# Clear password from environment
$env:PGPASSWORD = $null

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

