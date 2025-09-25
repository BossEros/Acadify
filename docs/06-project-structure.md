# Project Structure

## Solution Overview

The Student Performance Tracker solution is organized into four main projects, each with specific responsibilities and clear boundaries.

```
ASI.Basecode.sln
├── ASI.Basecode.WebApp/          # Presentation Layer
├── ASI.Basecode.Services/        # Business Logic Layer
├── ASI.Basecode.Data/           # Data Access Layer
└── ASI.Basecode.Resources/      # Resource Management
```

## ASI.Basecode.WebApp (Presentation Layer)

**Purpose:** Handles HTTP requests, user interface, and web-specific concerns.

### Directory Structure
```
ASI.Basecode.WebApp/
├── Controllers/                 # MVC Controllers
│   ├── AccountController.cs     # Authentication & user management
│   ├── AccountManagementController.cs  # Admin user management
│   └── HomeController.cs        # Landing pages
├── Views/                       # Razor Views
│   ├── Account/                 # Login, Register, etc.
│   ├── Home/                    # Home pages
│   └── Shared/                  # Layout and shared views
├── ViewModels/                  # Data transfer objects for views
├── wwwroot/                     # Static files
│   ├── css/                     # Stylesheets
│   ├── js/                      # JavaScript files
│   └── images/                  # Images and assets
├── Program.cs                   # Application startup and configuration
├── appsettings.json            # Configuration settings
└── ASI.Basecode.WebApp.csproj  # Project file
```

### Key Components

#### Controllers
Controllers act as bridges between HTTP requests and business logic:

```csharp
[AllowAnonymous]
public class AccountController : Controller
{
    private readonly IAccountService _accountService;
    
    public AccountController(IAccountService accountService)
    {
        _accountService = accountService;
    }
    
    // Controllers delegate business logic to services
    [HttpPost]
    public async Task<IActionResult> Login(LoginViewModel model)
    {
        var result = await _accountService.LoginAsync(new LoginRequest
        {
            Email = model.Email,
            Password = model.Password,
            RememberMe = model.RememberMe
        });
        
        // Handle response and return appropriate view
        return result.Succeeded ? RedirectToAction("Dashboard") : View(model);
    }
}
```

#### ViewModels
Data transfer objects specifically designed for UI binding:

```csharp
public class LoginViewModel
{
    [Required]
    [EmailAddress]
    public string Email { get; set; } = null!;
    
    [Required]
    [DataType(DataType.Password)]
    public string Password { get; set; } = null!;
    
    public bool RememberMe { get; set; }
}
```

#### Program.cs
Application configuration and dependency injection setup:

```csharp
var builder = WebApplication.CreateBuilder(args);

// Add services to the container
builder.Services.AddControllersWithViews();
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

// Register custom services
builder.Services.AddScoped<IAccountService, AccountService>();
builder.Services.AddScoped<IUserRepository, UserRepository>();

var app = builder.Build();

// Configure the HTTP request pipeline
app.UseAuthentication();
app.UseAuthorization();
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Account}/{action=Login}/{id?}");

app.Run();
```

## ASI.Basecode.Services (Business Logic Layer)

**Purpose:** Contains all business rules, application logic, and workflow orchestration.

### Directory Structure
```
ASI.Basecode.Services/
├── Interfaces/                  # Service contracts
│   ├── IAccountService.cs       # Account management interface
│   └── IEmailService.cs         # Email service interface
├── Implementation/              # Service implementations
│   ├── AccountService.cs        # Account business logic
│   └── SendGridEmailService.cs  # Email service implementation
├── DTOs/                        # Data Transfer Objects
│   ├── LoginRequest.cs          # Login data
│   ├── RegisterRequest.cs       # Registration data
│   └── ...
├── Results/                     # Response objects
│   ├── AuthResult.cs            # Authentication results
│   ├── SignInAuthResult.cs      # Sign-in results
│   └── ...
└── ASI.Basecode.Services.csproj # Project file
```

### Key Components

#### Service Interfaces
Define contracts for business operations:

```csharp
public interface IAccountService
{
    Task<AuthResult> RegisterAsync(RegisterRequest request);
    Task<SignInAuthResult> LoginAsync(LoginRequest request);
    Task SignOutAsync();
    Task<IList<string>> GetUserRolesAsync(string email);
    Task<ForgotPasswordResult> SendPasswordResetTokenAsync(string email);
    Task<AuthResult> ResetPasswordAsync(string email, string token, string newPassword);
    Task<string> GetRedirectPathBasedOnRoleAsync(string email);
}
```

#### Service Implementations
Execute business logic and coordinate between repositories:

```csharp
public class AccountService : IAccountService
{
    private readonly IUserRepository _userRepository;
    private readonly IAuthRepository _authRepository;
    private readonly IEmailService _emailService;
    
    public AccountService(IUserRepository userRepository, 
                         IAuthRepository authRepository, 
                         IEmailService emailService)
    {
        _userRepository = userRepository;
        _authRepository = authRepository;
        _emailService = emailService;
    }
    
    public async Task<AuthResult> RegisterAsync(RegisterRequest request)
    {
        // Business rule: Check if email already exists
        bool emailExists = await _userRepository.FindByEmailAsync(request.Email) != null;
        if (emailExists)
            return AuthResult.Failure(new[] { "Email is already in use." });
        
        // Business rule: Auto-approve students
        var user = new User
        {
            UserName = request.Email,
            FirstName = request.FirstName,
            LastName = request.LastName,
            Email = request.Email,
            IsApproved = request.Role == "Student"
        };
        
        // Delegate data operations to repository
        var (succeeded, errors) = await _userRepository.CreateUserAsync(user, request.Password);
        if (!succeeded)
            return AuthResult.Failure(errors);
        
        await _userRepository.AddToRoleAsync(user, request.Role);
        return AuthResult.Success();
    }
}
```

#### DTOs (Data Transfer Objects)
Carry data between layers without exposing internal models:

```csharp
public class RegisterRequest
{
    public string FirstName { get; set; } = null!;
    public string LastName { get; set; } = null!;
    public string Email { get; set; } = null!;
    public string Password { get; set; } = null!;
    public string Role { get; set; } = null!;
}
```

## ASI.Basecode.Data (Data Access Layer)

**Purpose:** Manages database operations, entity models, and data persistence.

### Directory Structure
```
ASI.Basecode.Data/
├── Models/                      # Entity Framework models
│   ├── User.cs                  # User entity
│   ├── Course.cs                # Course entity
│   ├── Class.cs                 # Class entity
│   ├── Enrollment.cs            # Enrollment entity
│   └── Grade.cs                 # Grade entity
├── Data/                        # Database context
│   └── AppDbContext.cs          # EF Core DbContext
├── Repositories/                # Data access repositories
│   ├── IUserRepository.cs       # User repository interface
│   ├── UserRepository.cs        # User repository implementation
│   ├── IAuthRepository.cs       # Auth repository interface
│   └── AuthRepository.cs        # Auth repository implementation
├── Migrations/                  # EF Core migrations
│   ├── 20241201000000_InitialCreate.cs
│   └── ...
└── ASI.Basecode.Data.csproj    # Project file
```

### Key Components

#### Entity Models
Define the database schema and relationships:

```csharp
public class User : IdentityUser<int>
{
    public string FirstName { get; set; } = null!;
    public string LastName { get; set; } = null!;
    public string? ProfilePicture { get; set; }
    public bool IsApproved { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    
    // Navigation properties
    public virtual ICollection<Class> ClassesTeaching { get; set; } = new List<Class>();
    public virtual ICollection<Enrollment> Enrollments { get; set; } = new List<Enrollment>();
}
```

#### DbContext
Configures Entity Framework and database relationships:

```csharp
public class AppDbContext : IdentityDbContext<User, IdentityRole<int>, int>
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }
    
    public DbSet<Course> Courses { get; set; }
    public DbSet<Class> Classes { get; set; }
    public DbSet<Enrollment> Enrollments { get; set; }
    public DbSet<Grade> Grades { get; set; }
    
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        
        // Configure relationships and constraints
        modelBuilder.Entity<Class>()
            .HasOne(c => c.Course)
            .WithMany(c => c.Classes)
            .HasForeignKey(c => c.CourseId);
    }
}
```

#### Repositories
Abstract database operations and provide clean interfaces:

```csharp
public class UserRepository : IUserRepository
{
    private readonly UserManager<User> _userManager;
    
    public UserRepository(UserManager<User> userManager)
    {
        _userManager = userManager;
    }
    
    public async Task<(bool Succeeded, IEnumerable<string> Errors)> CreateUserAsync(User user, string password)
    {
        var result = await _userManager.CreateAsync(user, password);
        return (result.Succeeded, result.Errors.Select(e => e.Description));
    }
    
    public Task<User?> FindByEmailAsync(string email) => _userManager.FindByEmailAsync(email);
}
```

## ASI.Basecode.Resources (Resource Management)

**Purpose:** Manages application resources, localization, and shared constants.

### Directory Structure
```
ASI.Basecode.Resources/
├── Messages/                    # Message constants
│   ├── AccountMessages.cs       # Account-related messages
│   └── ValidationMessages.cs    # Validation error messages
├── Templates/                   # Email templates
│   ├── PasswordResetTemplate.html
│   └── WelcomeTemplate.html
├── Localization/               # Multi-language support
│   ├── Resources.en.resx       # English resources
│   └── Resources.es.resx       # Spanish resources
└── ASI.Basecode.Resources.csproj # Project file
```

### Key Components

#### Message Constants
Centralized message management:

```csharp
public static class AccountMessages
{
    public const string InvalidLoginAttempt = "Invalid email or password.";
    public const string AccountLockedOut = "Account is locked due to multiple failed login attempts.";
    public const string PasswordResetEmailSent = "Password reset instructions have been sent to your email.";
    public const string PasswordResetSuccessful = "Your password has been reset successfully.";
    public const string PasswordResetFailed = "Failed to send password reset email. Please try again.";
    public const string InvalidPasswordResetRequest = "Invalid password reset request.";
}
```

## Project Dependencies

### Dependency Flow
```
WebApp ──→ Services ──→ Data
   │          │          │
   │          └──→ Resources
   │
   └──→ Resources (for localization)
```

### NuGet Package Dependencies

#### WebApp Project
- Microsoft.AspNetCore.App
- Microsoft.AspNetCore.Identity.EntityFrameworkCore
- Microsoft.EntityFrameworkCore.Tools

#### Services Project
- Microsoft.AspNetCore.Identity
- SendGrid (for email services)

#### Data Project
- Microsoft.AspNetCore.Identity.EntityFrameworkCore
- Npgsql.EntityFrameworkCore.PostgreSQL
- Microsoft.EntityFrameworkCore.Tools

#### Resources Project
- No external dependencies (pure .NET)

## Configuration Files

### appsettings.json
```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning"
    }
  },
  "AllowedHosts": "*"
}
```

### User Secrets (Development)
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Host=localhost;Database=StudentTracker;Username=user;Password=pass"
  },
  "SendGrid": {
    "ApiKey": "your-sendgrid-api-key"
  }
}
```

## Build and Deployment

### Project Build Order
1. ASI.Basecode.Resources (no dependencies)
2. ASI.Basecode.Data (references Resources)
3. ASI.Basecode.Services (references Data and Resources)
4. ASI.Basecode.WebApp (references Services)

### Output Structure
```
bin/Debug/net8.0/
├── ASI.Basecode.WebApp.dll
├── ASI.Basecode.Services.dll
├── ASI.Basecode.Data.dll
├── ASI.Basecode.Resources.dll
└── [NuGet package dependencies]
```

---

*This project structure promotes separation of concerns, maintainability, and testability. Each project has clear responsibilities and well-defined boundaries.*