# Simple script to configure database connection
# Use this if you know your PostgreSQL password

Write-Host "=== Database Configuration ===" -ForegroundColor Cyan
Write-Host ""
Write-Host "Enter the password you set for the 'postgres' user during PostgreSQL installation." -ForegroundColor Yellow
Write-Host "If you don't remember it, you can:" -ForegroundColor Yellow
Write-Host "  1. Try common passwords you might have used" -ForegroundColor Gray
Write-Host "  2. Run reset-postgres-password.ps1 to reset it (requires admin)" -ForegroundColor Gray
Write-Host ""
$password = Read-Host "Enter PostgreSQL password for user 'postgres'" -AsSecureString
$passwordPlain = [Runtime.InteropServices.Marshal]::PtrToStringAuto(
    [Runtime.InteropServices.Marshal]::SecureStringToBSTR($password)
)

# Test connection
Write-Host ""
Write-Host "Testing connection..." -ForegroundColor Cyan
$psqlPath = "C:\Program Files\PostgreSQL\16\bin\psql.exe"
if (-not (Test-Path $psqlPath)) {
    Write-Host "Error: PostgreSQL not found at $psqlPath" -ForegroundColor Red
    exit 1
}

$env:PGPASSWORD = $passwordPlain
$testResult = & $psqlPath -U postgres -h localhost -c "SELECT version();" 2>&1
if ($LASTEXITCODE -ne 0) {
    Write-Host "Connection failed! The password may be incorrect." -ForegroundColor Red
    Write-Host "Error: $testResult" -ForegroundColor Red
    $env:PGPASSWORD = $null
    exit 1
}
Write-Host "[OK] Connection successful!" -ForegroundColor Green

# Create database
Write-Host ""
Write-Host "Creating database 'acadify'..." -ForegroundColor Cyan
$dbExists = & $psqlPath -U postgres -h localhost -t -c "SELECT 1 FROM pg_database WHERE datname='acadify';" 2>&1
if ($dbExists -match "1") {
    Write-Host "Database 'acadify' already exists." -ForegroundColor Yellow
} else {
    & $psqlPath -U postgres -h localhost -c "CREATE DATABASE acadify;" 2>&1 | Out-Null
    if ($LASTEXITCODE -eq 0) {
        Write-Host "[OK] Database 'acadify' created!" -ForegroundColor Green
    } else {
        Write-Host "Warning: Could not create database. It may already exist." -ForegroundColor Yellow
    }
}

# Update connection string
Write-Host ""
Write-Host "Updating connection string..." -ForegroundColor Cyan
$appsettingsPath = "ASI.Basecode.WebApp\appsettings.json"
if (Test-Path $appsettingsPath) {
    $appsettings = Get-Content $appsettingsPath -Raw | ConvertFrom-Json
    $connectionString = "Host=localhost;Port=5432;Database=acadify;Username=postgres;Password=$passwordPlain"
    $appsettings.ConnectionStrings.DefaultConnection = $connectionString
    $appsettings | ConvertTo-Json -Depth 10 | Set-Content $appsettingsPath
    Write-Host "[OK] Updated appsettings.json" -ForegroundColor Green
} else {
    Write-Host "Error: appsettings.json not found" -ForegroundColor Red
    $env:PGPASSWORD = $null
    exit 1
}

# Set user secret
try {
    Set-Location "ASI.Basecode.WebApp"
    dotnet user-secrets set "ConnectionStrings:DefaultConnection" $connectionString 2>&1 | Out-Null
    Write-Host "[OK] Updated user secrets" -ForegroundColor Green
    Set-Location ".."
} catch {
    Write-Host "Warning: Could not update user secrets" -ForegroundColor Yellow
}

$env:PGPASSWORD = $null

Write-Host ""
Write-Host "=== Configuration Complete! ===" -ForegroundColor Green
Write-Host ""
Write-Host "Next steps:" -ForegroundColor Cyan
Write-Host "1. Apply database migrations:" -ForegroundColor Yellow
Write-Host "   dotnet ef database update --project ASI.Basecode.Data --startup-project ASI.Basecode.WebApp" -ForegroundColor Gray
Write-Host ""
Write-Host "2. Run the application:" -ForegroundColor Yellow
Write-Host "   cd ASI.Basecode.WebApp" -ForegroundColor Gray
Write-Host "   dotnet run" -ForegroundColor Gray
Write-Host ""

