# System Architecture

## Architecture Overview

The Student Performance Tracker follows a **layered architecture** pattern that promotes separation of concerns, maintainability, and testability.

### Request Flow Diagram
```
User Request → Controller → Service → Repository → Database
     ↓            ↓          ↓          ↓          ↓
   Browser → AccountController → AccountService → UserRepository → PostgreSQL
```

## Layer Responsibilities

### 1. Presentation Layer (ASI.Basecode.WebApp)
**Role:** Handles HTTP requests and user interface

**Components:**
- **Controllers** - Act as bridges between user requests and business logic
- **Views** - Razor pages that render HTML for users
- **ViewModels** - Data transfer objects for UI binding
- **Static Files** - CSS, JavaScript, images

**Example Flow:**
```csharp
// AccountController receives HTTP POST request
[HttpPost]
public async Task<IActionResult> Login(LoginViewModel model)
{
    // Controller validates input, then calls service
    var result = await _accountService.LoginAsync(new LoginRequest
    {
        Email = model.Email,
        Password = model.Password,
        RememberMe = model.RememberMe
    });
    
    // Controller handles the response and returns appropriate view
    if (result.Succeeded)
        return RedirectToAction("Dashboard");
    
    return View(model);
}
```

### 2. Business Logic Layer (ASI.Basecode.Services)
**Role:** Contains all business rules and application logic

**Components:**
- **Service Interfaces** (`IAccountService`, `IEmailService`) - Define contracts
- **Service Implementations** (`AccountService`, `SendGridEmailService`) - Execute business logic
- **DTOs** (Data Transfer Objects) - Carry data between layers
- **Result Objects** - Standardized response formats

**Example Flow:**
```csharp
// AccountService handles business logic
public async Task<SignInAuthResult> LoginAsync(LoginRequest request)
{
    // 1. Validate business rules
    var user = await _userRepository.FindByEmailAsync(request.Email);
    if (user == null)
        return SignInAuthResult.Failed("Invalid login attempt");
    
    // 2. Delegate data operations to repository
    var (succeeded, isLockedOut) = await _authRepository.PasswordSignInAsync(
        user, request.Password, request.RememberMe, lockoutOnFailure: true);
    
    // 3. Apply business logic and return result
    if (succeeded) return SignInAuthResult.Success();
    if (isLockedOut) return SignInAuthResult.LockedOut();
    
    return SignInAuthResult.Failed("Invalid login attempt");
}
```

### 3. Data Access Layer (ASI.Basecode.Data)
**Role:** Manages all database operations and data persistence

**Components:**
- **Models** - Entity Framework entities that map to database tables
- **DbContext** - Entity Framework context for database operations
- **Repositories** - Abstraction layer over Entity Framework operations
- **Migrations** - Database schema version control

**Example Flow:**
```csharp
// UserRepository handles data operations
public class UserRepository : IUserRepository
{
    private readonly UserManager<User> _userManager;
    
    // Repository uses ASP.NET Core Identity's UserManager
    public Task<User?> FindByEmailAsync(string email) 
        => _userManager.FindByEmailAsync(email);
    
    public async Task<(bool Succeeded, IEnumerable<string> Errors)> CreateUserAsync(User user, string password)
    {
        var result = await _userManager.CreateAsync(user, password);
        return (result.Succeeded, result.Errors.Select(e => e.Description));
    }
}
```

### 4. Resource Layer (ASI.Basecode.Resources)
**Role:** Manages application resources and localization

**Components:**
- **Message Resources** - Centralized error and success messages
- **Email Templates** - HTML templates for email communications
- **Validation Messages** - User-friendly validation error messages

## Component Interactions

### Dependency Injection Setup
```csharp
// Program.cs - Services are registered for dependency injection
builder.Services.AddScoped<IAccountService, AccountService>();
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<IAuthRepository, AuthRepository>();
builder.Services.AddHttpClient<IEmailService, SendGridEmailService>();
```

### Real-World Example: User Registration Flow

#### Step 1: User Submits Form
```csharp
// User submits registration form → AccountController.Register(RegisterViewModel)
[HttpPost]
public async Task<IActionResult> Register(RegisterViewModel model)
{
    if (!ModelState.IsValid)
        return View(model);
    
    // Controller calls service with mapped data
    var result = await _accountService.RegisterAsync(new RegisterRequest
    {
        FirstName = model.FirstName,
        LastName = model.LastName,
        Email = model.Email,
        Password = model.Password,
        Role = model.Role
    });
    
    // Handle response
    if (result.Succeeded)
        return RedirectToAction("Login");
    
    // Add errors to model state
    foreach (var error in result.Errors)
        ModelState.AddModelError("", error);
    
    return View(model);
}
```

#### Step 2: Service Applies Business Logic
```csharp
// AccountService.RegisterAsync - Business logic layer
public async Task<AuthResult> RegisterAsync(RegisterRequest request)
{
    // Business rule: Check if email already exists
    bool emailExists = await _userRepository.FindByEmailAsync(request.Email) != null;
    if (emailExists)
        return AuthResult.Failure(new[] { "Email is already in use." });
    
    // Business rule: Create user entity with approval logic
    var user = new User
    {
        UserName = request.Email,
        FirstName = request.FirstName,
        LastName = request.LastName,
        Email = request.Email,
        IsApproved = request.Role == "Student" // Auto-approve students
    };
    
    // Delegate data operations to repository
    var (succeeded, errors) = await _userRepository.CreateUserAsync(user, request.Password);
    if (!succeeded)
        return AuthResult.Failure(errors);
    
    // Assign role
    await _userRepository.AddToRoleAsync(user, request.Role);
    return AuthResult.Success();
}
```

#### Step 3: Repository Handles Data Operations
```csharp
// UserRepository - Data access layer
public async Task<(bool Succeeded, IEnumerable<string> Errors)> CreateUserAsync(User user, string password)
{
    // Use ASP.NET Core Identity for user creation
    var result = await _userManager.CreateAsync(user, password);
    return (result.Succeeded, result.Errors.Select(e => e.Description));
}

public async Task AddToRoleAsync(User user, string role)
{
    await _userManager.AddToRoleAsync(user, role);
}
```

#### Step 4: Response Flow Back
```
Database → Repository → Service → Controller → View
   ↓           ↓          ↓          ↓          ↓
Success → (true, []) → AuthResult.Success() → RedirectToAction("Login") → Login Page
```

## Key Design Principles

### Separation of Concerns
- **Controllers** only handle HTTP concerns (routing, model binding, response formatting)
- **Services** contain all business logic and rules
- **Repositories** only handle data access operations
- **Models** represent data structure without behavior

### Dependency Inversion
- Higher-level modules (Controllers) depend on abstractions (IAccountService)
- Lower-level modules (AccountService) implement these abstractions
- Dependencies are injected, not created directly

### Single Responsibility
- Each class has one reason to change
- `AccountController` handles web requests
- `AccountService` handles account business logic
- `UserRepository` handles user data operations

## Benefits of This Architecture

### 1. Testability
Each layer can be unit tested independently:
```csharp
// Mock dependencies for testing
var mockUserRepository = new Mock<IUserRepository>();
var mockAuthRepository = new Mock<IAuthRepository>();
var mockEmailService = new Mock<IEmailService>();

var accountService = new AccountService(
    mockUserRepository.Object, 
    mockAuthRepository.Object, 
    mockEmailService.Object);
```

### 2. Maintainability
Changes in one layer don't affect others:
- Change database from PostgreSQL to SQL Server → Only update repositories
- Change email provider from SendGrid to AWS SES → Only update email service
- Add new business rules → Only modify services

### 3. Scalability
Easy to add new features following the same pattern:
- Add new entity → Create model, repository, service, controller
- Add new business logic → Extend existing services or create new ones

### 4. Flexibility
Can swap implementations without changing dependent code:
- Different email providers
- Different authentication mechanisms
- Different data storage solutions

### 5. Code Reuse
Services can be used by multiple controllers:
```csharp
// AccountService used by both AccountController and AdminController
public class AccountController : Controller
{
    private readonly IAccountService _accountService;
    // ...
}

public class AdminController : Controller
{
    private readonly IAccountService _accountService;
    // ...
}
```

## Project Dependencies

### Layer Dependencies
```
WebApp → Services → Data
  ↓        ↓        ↓
Views   Business  Database
       Logic     Operations
```

### Specific Project References
- **ASI.Basecode.WebApp** references Services
- **ASI.Basecode.Services** references Data and Resources
- **ASI.Basecode.Data** has no project dependencies (only NuGet packages)
- **ASI.Basecode.Resources** has no dependencies

---

*This architecture ensures the application is maintainable, testable, and scalable. Each layer has clear responsibilities and well-defined interfaces for communication.*