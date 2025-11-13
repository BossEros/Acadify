# Quick script to fix PostgreSQL password in connection string
# Run this after you know your PostgreSQL postgres user password

param(
    [Parameter(Mandatory=$true)]
    [string]$Password
)

Write-Host "=== Updating Database Connection String ===" -ForegroundColor Cyan
Write-Host ""

# Update appsettings.json
$appsettingsPath = "ASI.Basecode.WebApp\appsettings.json"
if (Test-Path $appsettingsPath) {
    $appsettings = Get-Content $appsettingsPath -Raw | ConvertFrom-Json
    $connectionString = "Host=localhost;Port=5432;Database=acadify;Username=postgres;Password=$Password"
    $appsettings.ConnectionStrings.DefaultConnection = $connectionString
    $appsettings | ConvertTo-Json -Depth 10 | Set-Content $appsettingsPath
    Write-Host "✓ Updated appsettings.json" -ForegroundColor Green
} else {
    Write-Host "Error: appsettings.json not found" -ForegroundColor Red
    exit 1
}

# Set user secret
try {
    Set-Location "ASI.Basecode.WebApp"
    dotnet user-secrets set "ConnectionStrings:DefaultConnection" $connectionString 2>&1 | Out-Null
    Write-Host "✓ Updated user secrets" -ForegroundColor Green
    Set-Location ".."
} catch {
    Write-Host "Warning: Could not update user secrets" -ForegroundColor Yellow
}

# Test connection
Write-Host ""
Write-Host "Testing database connection..." -ForegroundColor Cyan
$pgPath = "C:\Program Files\PostgreSQL\16\bin\psql.exe"
if (-not (Test-Path $pgPath)) {
    $pgPath = Get-ChildItem -Path "C:\Program Files\PostgreSQL" -Recurse -Filter "psql.exe" -ErrorAction SilentlyContinue | Select-Object -First 1 -ExpandProperty FullName
}

if ($pgPath) {
    $env:PGPASSWORD = $Password
    $testResult = & $pgPath -U postgres -h localhost -d acadify -c "SELECT 1;" 2>&1
    if ($LASTEXITCODE -eq 0) {
        Write-Host "✓ Database connection successful!" -ForegroundColor Green
    } else {
        Write-Host "⚠ Connection test failed. The database 'acadify' may not exist yet." -ForegroundColor Yellow
        Write-Host "  Run migrations to create it: dotnet ef database update --project ASI.Basecode.Data --startup-project ASI.Basecode.WebApp" -ForegroundColor Gray
    }
    $env:PGPASSWORD = $null
} else {
    Write-Host "⚠ Could not find psql.exe to test connection" -ForegroundColor Yellow
}

Write-Host ""
Write-Host "=== Done ===" -ForegroundColor Green

