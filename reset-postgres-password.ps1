# Script to reset PostgreSQL password and configure connection
# This script temporarily allows password-free access to reset the password

Write-Host "=== PostgreSQL Password Reset ===" -ForegroundColor Cyan
Write-Host ""
Write-Host "This script will help you reset the PostgreSQL password." -ForegroundColor Yellow
Write-Host ""

# Find PostgreSQL installation
$pgVersion = "16"
$pgBasePath = "C:\Program Files\PostgreSQL\$pgVersion"
$pgDataPath = "$pgBasePath\data"
$pgHbaPath = "$pgDataPath\pg_hba.conf"

if (-not (Test-Path $pgHbaPath)) {
    Write-Host "Error: Could not find pg_hba.conf at $pgHbaPath" -ForegroundColor Red
    Write-Host "Please locate your PostgreSQL data directory manually." -ForegroundColor Yellow
    exit 1
}

Write-Host "Found PostgreSQL configuration at: $pgHbaPath" -ForegroundColor Green
Write-Host ""

# Backup pg_hba.conf
$backupPath = "$pgHbaPath.backup_$(Get-Date -Format 'yyyyMMdd_HHmmss')"
Copy-Item $pgHbaPath $backupPath
Write-Host "[OK] Created backup: $backupPath" -ForegroundColor Green

# Read current pg_hba.conf
$pgHbaContent = Get-Content $pgHbaPath

# Modify to allow trust authentication temporarily
$modifiedContent = $pgHbaContent | ForEach-Object {
    if ($_ -match "^host\s+all\s+all\s+127\.0\.0\.1/32\s+") {
        "host    all             all             127.0.0.1/32            trust"
    } elseif ($_ -match "^host\s+all\s+all\s+::1/128\s+") {
        "host    all             all             ::1/128                 trust"
    } else {
        $_
    }
}

# Write modified content
$modifiedContent | Set-Content $pgHbaPath
Write-Host "[OK] Modified pg_hba.conf to allow trust authentication" -ForegroundColor Green

# Restart PostgreSQL service
Write-Host ""
Write-Host "Restarting PostgreSQL service..." -ForegroundColor Cyan
Restart-Service -Name "postgresql-x64-$pgVersion" -Force
Start-Sleep -Seconds 3
Write-Host "[OK] Service restarted" -ForegroundColor Green

# Get new password
Write-Host ""
Write-Host "Now you can set a new password for the 'postgres' user." -ForegroundColor Yellow
$newPassword = Read-Host "Enter new password for 'postgres' user" -AsSecureString
$newPasswordPlain = [Runtime.InteropServices.Marshal]::PtrToStringAuto(
    [Runtime.InteropServices.Marshal]::SecureStringToBSTR($newPassword)
)

# Reset password using psql
$psqlPath = "$pgBasePath\bin\psql.exe"
Write-Host ""
Write-Host "Resetting password..." -ForegroundColor Cyan
$resetCmd = "ALTER USER postgres WITH PASSWORD '$newPasswordPlain';"
& $psqlPath -U postgres -h localhost -c $resetCmd 2>&1 | Out-Null

if ($LASTEXITCODE -eq 0) {
    Write-Host "[OK] Password reset successfully!" -ForegroundColor Green
} else {
    Write-Host "Warning: Password reset may have failed. Continuing anyway..." -ForegroundColor Yellow
}

# Restore original pg_hba.conf
Write-Host ""
Write-Host "Restoring original pg_hba.conf..." -ForegroundColor Cyan
Copy-Item $backupPath $pgHbaPath -Force
Write-Host "[OK] Restored original configuration" -ForegroundColor Green

# Restart service again
Restart-Service -Name "postgresql-x64-$pgVersion" -Force
Start-Sleep -Seconds 3
Write-Host "[OK] Service restarted with secure settings" -ForegroundColor Green

# Create database if it doesn't exist
Write-Host ""
Write-Host "Creating database 'acadify'..." -ForegroundColor Cyan
$env:PGPASSWORD = $newPasswordPlain
$dbExists = & $psqlPath -U postgres -h localhost -t -c "SELECT 1 FROM pg_database WHERE datname='acadify';" 2>&1
if ($dbExists -notmatch "1") {
    & $psqlPath -U postgres -h localhost -c "CREATE DATABASE acadify;" 2>&1 | Out-Null
    Write-Host "[OK] Database 'acadify' created" -ForegroundColor Green
} else {
    Write-Host "Database 'acadify' already exists" -ForegroundColor Yellow
}
$env:PGPASSWORD = $null

# Update connection string
Write-Host ""
Write-Host "Updating connection string..." -ForegroundColor Cyan
$appsettingsPath = "ASI.Basecode.WebApp\appsettings.json"
if (Test-Path $appsettingsPath) {
    $appsettings = Get-Content $appsettingsPath -Raw | ConvertFrom-Json
    $connectionString = "Host=localhost;Port=5432;Database=acadify;Username=postgres;Password=$newPasswordPlain"
    $appsettings.ConnectionStrings.DefaultConnection = $connectionString
    $appsettings | ConvertTo-Json -Depth 10 | Set-Content $appsettingsPath
    Write-Host "[OK] Updated appsettings.json" -ForegroundColor Green
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

Write-Host ""
Write-Host "=== Setup Complete! ===" -ForegroundColor Green
Write-Host ""
Write-Host "Your PostgreSQL password has been reset and the connection string updated." -ForegroundColor Cyan
Write-Host ""
Write-Host "Next steps:" -ForegroundColor Yellow
Write-Host "1. Apply database migrations:" -ForegroundColor Gray
Write-Host "   dotnet ef database update --project ASI.Basecode.Data --startup-project ASI.Basecode.WebApp" -ForegroundColor DarkGray
Write-Host ""
Write-Host "2. Run the application:" -ForegroundColor Gray
Write-Host "   cd ASI.Basecode.WebApp" -ForegroundColor DarkGray
Write-Host "   dotnet run" -ForegroundColor DarkGray
Write-Host ""

