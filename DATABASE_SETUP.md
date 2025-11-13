# Database Connection Setup Guide

## Option 1: Get Supabase Connection String (Recommended)

If you have access to the Supabase project, follow these steps:

1. **Go to Supabase Dashboard**
   - Visit https://app.supabase.com
   - Log in to your account
   - Select your project

2. **Get Connection String**
   - Go to **Settings** → **Database**
   - Scroll down to **Connection string** section
   - Select **URI** or **Connection pooling** tab
   - Copy the connection string (it will look like):
     ```
     postgresql://postgres:[YOUR-PASSWORD]@db.[PROJECT-REF].supabase.co:5432/postgres
     ```
   - Or use the **Connection parameters** format:
     ```
     Host=db.[PROJECT-REF].supabase.co;Port=5432;Database=postgres;Username=postgres;Password=[YOUR-PASSWORD];SSL Mode=Require;
     ```

3. **Set the Connection String**
   
   **Using User Secrets (Recommended):**
   ```powershell
   cd ASI.Basecode.WebApp
   dotnet user-secrets set "ConnectionStrings:DefaultConnection" "YOUR_CONNECTION_STRING_HERE"
   ```
   
   **Or update appsettings.json directly:**
   - Open `ASI.Basecode.WebApp/appsettings.json`
   - Replace the placeholder connection string with your Supabase connection string

4. **Apply Migrations**
   ```powershell
   cd ..
   dotnet ef database update --project ASI.Basecode.Data --startup-project ASI.Basecode.WebApp
   ```

## Option 2: Local PostgreSQL Database

If you want to set up a local database instead:

### Install PostgreSQL on Windows:
1. Download from: https://www.postgresql.org/download/windows/
2. Install PostgreSQL (remember the password you set for the `postgres` user)
3. Create a database:
   ```sql
   CREATE DATABASE acadify;
   ```

### Update Connection String:
Use this format in `appsettings.json`:
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Host=localhost;Port=5432;Database=acadify;Username=postgres;Password=YOUR_POSTGRES_PASSWORD"
  }
}
```

## Option 3: Use Supabase Connection Pooling (For Production/Development)

Supabase provides connection pooling which is better for production:
- Go to Supabase Dashboard → Settings → Database
- Use the **Connection pooling** connection string
- Format: `postgresql://postgres.[PROJECT-REF]:[PASSWORD]@aws-0-[REGION].pooler.supabase.com:6543/postgres`

## Verify Connection

After setting up, test the connection:
```powershell
cd ASI.Basecode.WebApp
dotnet run
```

If you see no database connection errors, you're all set!

## Need Help?

- **Don't have Supabase access?** Contact your team lead to get the connection string
- **Connection issues?** Make sure:
  - Your connection string is correct
  - Your IP is allowed in Supabase (if using IP restrictions)
  - SSL mode is set to "Require" for Supabase connections

