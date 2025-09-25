# Development Guidelines

## Overview

This guide provides standards and best practices for developing features in the Student Performance Tracker. Following these guidelines ensures consistency, maintainability, and quality across the codebase.

## Adding New Features

### 1. Planning Phase

Before writing code:
- **Understand Requirements** - Clarify what the feature should do
- **Review Existing Code** - Look for similar patterns in the codebase
- **Plan Database Changes** - Identify any new entities or relationships needed
- **Consider Security** - Think about authorization and data validation

### 2. Implementation Order

Follow this sequence when adding new features:

#### Step 1: Data Layer (ASI.Basecode.Data)
```csharp
// 1. Create or modify entity models
public class Assignment
{
    public int Id { get; set; }
    public int ClassId { get; set; }
    public string Title { get; set; } = null!;
    public string Description { get; set; } = null!;
    public DateTime DueDate { get; set; }
    public decimal MaxPoints { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    
    // Navigation properties
    public virtual Class Class { get; set; } = null!;
    public virtual ICollection<Submission> Submissions { get; set; } = new List<Submission>();
}

// 2. Update DbContext
public class AppDbContext : IdentityDbContext<User, IdentityRole<int>, int>
{
    // Add new DbSet
    public DbSet<Assignment> Assignments { get; set; }
    
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        
        // Configure relationships
        modelBuilder.Entity<Assignment>()
            .HasOne(a => a.Class)
            .WithMany(c => c.Assignments)
            .HasForeignKey(a => a.ClassId);
    }
}

// 3. Create repository interface
public interface IAssignmentRepository
{
    Task<Assignment?> GetByIdAsync(int id);
    Task<IEnumerable<Assignment>> GetByClassIdAsync(int classId);
    Task<Assignment> CreateAsync(Assignment assignment);
    Task<Assignment> UpdateAsync(Assignment assignment);
    Task DeleteAsync(int id);
}

// 4. Implement repository
public class AssignmentRepository : IAssignmentRepository
{
    private readonly AppDbContext _context;
    
    public AssignmentRepository(AppDbContext context)
    {
        _context = context;
    }
    
    public async Task<Assignment?> GetByIdAsync(int id)
    {
        return await _context.Assignments
            .Include(a => a.Class)
            .FirstOrDefaultAsync(a => a.Id == id);
    }
    
    // Implement other methods...
}
```

#### Step 2: Service Layer (ASI.Basecode.Services)
```csharp
// 1. Create DTOs
public class CreateAssignmentRequest
{
    public int ClassId { get; set; }
    public string Title { get; set; } = null!;
    public string Description { get; set; } = null!;
    public DateTime DueDate { get; set; }
    public decimal MaxPoints { get; set; }
}

public class AssignmentResponse
{
    public int Id { get; set; }
    public string Title { get; set; } = null!;
    public string Description { get; set; } = null!;
    public DateTime DueDate { get; set; }
    public decimal MaxPoints { get; set; }
    public string ClassName { get; set; } = null!;
}

// 2. Create service interface
public interface IAssignmentService
{
    Task<AssignmentResponse?> GetByIdAsync(int id);
    Task<IEnumerable<AssignmentResponse>> GetByClassIdAsync(int classId);
    Task<AssignmentResponse> CreateAsync(CreateAssignmentRequest request);
    Task<AssignmentResponse> UpdateAsync(int id, UpdateAssignmentRequest request);
    Task DeleteAsync(int id);
}

// 3. Implement service
public class AssignmentService : IAssignmentService
{
    private readonly IAssignmentRepository _assignmentRepository;
    private readonly IClassRepository _classRepository;
    
    public AssignmentService(IAssignmentRepository assignmentRepository, IClassRepository classRepository)
    {
        _assignmentRepository = assignmentRepository;
        _classRepository = classRepository;
    }
    
    public async Task<AssignmentResponse> CreateAsync(CreateAssignmentRequest request)
    {
        // Business rule: Validate class exists and user has permission
        var classEntity = await _classRepository.GetByIdAsync(request.ClassId);
        if (classEntity == null)
            throw new ArgumentException("Class not found");
        
        // Business rule: Due date must be in the future
        if (request.DueDate <= DateTime.UtcNow)
            throw new ArgumentException("Due date must be in the future");
        
        var assignment = new Assignment
        {
            ClassId = request.ClassId,
            Title = request.Title,
            Description = request.Description,
            DueDate = request.DueDate,
            MaxPoints = request.MaxPoints
        };
        
        var created = await _assignmentRepository.CreateAsync(assignment);
        
        return new AssignmentResponse
        {
            Id = created.Id,
            Title = created.Title,
            Description = created.Description,
            DueDate = created.DueDate,
            MaxPoints = created.MaxPoints,
            ClassName = classEntity.Course.CourseName
        };
    }
}
```

#### Step 3: Presentation Layer (ASI.Basecode.WebApp)
```csharp
// 1. Create ViewModels
public class CreateAssignmentViewModel
{
    [Required]
    public int ClassId { get; set; }
    
    [Required]
    [StringLength(200)]
    public string Title { get; set; } = null!;
    
    [Required]
    public string Description { get; set; } = null!;
    
    [Required]
    [DataType(DataType.DateTime)]
    public DateTime DueDate { get; set; }
    
    [Required]
    [Range(0.1, 1000)]
    public decimal MaxPoints { get; set; }
}

// 2. Create Controller
[Authorize(Roles = "Teacher")]
public class AssignmentController : Controller
{
    private readonly IAssignmentService _assignmentService;
    
    public AssignmentController(IAssignmentService assignmentService)
    {
        _assignmentService = assignmentService;
    }
    
    [HttpGet]
    public IActionResult Create(int classId)
    {
        var model = new CreateAssignmentViewModel { ClassId = classId };
        return View(model);
    }
    
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(CreateAssignmentViewModel model)
    {
        if (!ModelState.IsValid)
            return View(model);
        
        try
        {
            var request = new CreateAssignmentRequest
            {
                ClassId = model.ClassId,
                Title = model.Title,
                Description = model.Description,
                DueDate = model.DueDate,
                MaxPoints = model.MaxPoints
            };
            
            await _assignmentService.CreateAsync(request);
            return RedirectToAction("Details", "Class", new { id = model.ClassId });
        }
        catch (ArgumentException ex)
        {
            ModelState.AddModelError("", ex.Message);
            return View(model);
        }
    }
}
```

#### Step 4: Register Services (Program.cs)
```csharp
// Add to Program.cs
builder.Services.AddScoped<IAssignmentRepository, AssignmentRepository>();
builder.Services.AddScoped<IAssignmentService, AssignmentService>();
```

#### Step 5: Create Database Migration
```bash
dotnet ef migrations add AddAssignmentEntity --project ASI.Basecode.Data --startup-project ASI.Basecode.WebApp
dotnet ef database update --project ASI.Basecode.Data --startup-project ASI.Basecode.WebApp
```

## Database Development

### Creating Migrations

1. **Add or modify entities** in the Data project
2. **Create migration:**
   ```bash
   dotnet ef migrations add [MigrationName] --project ASI.Basecode.Data --startup-project ASI.Basecode.WebApp
   ```
3. **Review generated migration** before applying
4. **Apply migration:**
   ```bash
   dotnet ef database update --project ASI.Basecode.Data --startup-project ASI.Basecode.WebApp
   ```

### Migration Best Practices

- **Descriptive Names** - Use clear migration names like `AddAssignmentEntity` or `UpdateUserProfileFields`
- **Review Before Apply** - Always check generated SQL before running migrations
- **Backup Data** - Backup database before applying migrations in production
- **Rollback Plan** - Know how to rollback if migration fails

### Entity Configuration

Configure entities in `OnModelCreating`:

```csharp
protected override void OnModelCreating(ModelBuilder modelBuilder)
{
    base.OnModelCreating(modelBuilder);
    
    // Configure relationships
    modelBuilder.Entity<Assignment>()
        .HasOne(a => a.Class)
        .WithMany(c => c.Assignments)
        .HasForeignKey(a => a.ClassId)
        .OnDelete(DeleteBehavior.Cascade);
    
    // Configure constraints
    modelBuilder.Entity<Assignment>()
        .Property(a => a.Title)
        .HasMaxLength(200)
        .IsRequired();
    
    // Configure indexes
    modelBuilder.Entity<Assignment>()
        .HasIndex(a => a.DueDate);
}
```

## Security Guidelines

### Authentication & Authorization

#### Controller Authorization
```csharp
[Authorize(Roles = "Teacher")]
public class AssignmentController : Controller
{
    // Only teachers can access these actions
}

[Authorize]
public async Task<IActionResult> Details(int id)
{
    // All authenticated users can access
}

[Authorize(Roles = "Admin,Teacher")]
public async Task<IActionResult> Delete(int id)
{
    // Only admins and teachers can delete
}
```

#### Resource-Based Authorization
```csharp
public async Task<IActionResult> Edit(int id)
{
    var assignment = await _assignmentService.GetByIdAsync(id);
    if (assignment == null)
        return NotFound();
    
    // Check if current user is the teacher of this class
    var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
    if (assignment.Class.TeacherId.ToString() != currentUserId)
        return Forbid();
    
    return View(assignment);
}
```

### Input Validation

#### Model Validation
```csharp
public class CreateAssignmentViewModel
{
    [Required(ErrorMessage = "Title is required")]
    [StringLength(200, ErrorMessage = "Title cannot exceed 200 characters")]
    public string Title { get; set; } = null!;
    
    [Required(ErrorMessage = "Due date is required")]
    [DataType(DataType.DateTime)]
    [FutureDate(ErrorMessage = "Due date must be in the future")]
    public DateTime DueDate { get; set; }
    
    [Range(0.1, 1000, ErrorMessage = "Max points must be between 0.1 and 1000")]
    public decimal MaxPoints { get; set; }
}
```

#### Custom Validation Attributes
```csharp
public class FutureDateAttribute : ValidationAttribute
{
    public override bool IsValid(object? value)
    {
        if (value is DateTime date)
            return date > DateTime.UtcNow;
        return false;
    }
}
```

### CSRF Protection
Always use anti-forgery tokens on forms:

```html
@using (Html.BeginForm())
{
    @Html.AntiForgeryToken()
    <!-- form fields -->
}
```

```csharp
[HttpPost]
[ValidateAntiForgeryToken]
public async Task<IActionResult> Create(CreateAssignmentViewModel model)
{
    // Action implementation
}
```

## Code Quality Standards

### Naming Conventions

- **Classes:** PascalCase (`AssignmentService`, `UserRepository`)
- **Methods:** PascalCase (`GetByIdAsync`, `CreateAsync`)
- **Properties:** PascalCase (`FirstName`, `CreatedAt`)
- **Fields:** camelCase with underscore (`_assignmentRepository`)
- **Parameters:** camelCase (`userId`, `assignmentId`)
- **Constants:** PascalCase (`MaxFileSize`, `DefaultPageSize`)

### Error Handling

#### Service Layer
```csharp
public async Task<AssignmentResponse> GetByIdAsync(int id)
{
    if (id <= 0)
        throw new ArgumentException("Invalid assignment ID", nameof(id));
    
    var assignment = await _assignmentRepository.GetByIdAsync(id);
    if (assignment == null)
        throw new NotFoundException($"Assignment with ID {id} not found");
    
    return MapToResponse(assignment);
}
```

#### Controller Layer
```csharp
public async Task<IActionResult> Details(int id)
{
    try
    {
        var assignment = await _assignmentService.GetByIdAsync(id);
        return View(assignment);
    }
    catch (NotFoundException)
    {
        return NotFound();
    }
    catch (ArgumentException ex)
    {
        ModelState.AddModelError("", ex.Message);
        return BadRequest(ModelState);
    }
}
```

### Async/Await Best Practices

- **Use async/await** for all database operations
- **Don't block async calls** - avoid `.Result` or `.Wait()`
- **Configure await** - use `ConfigureAwait(false)` in libraries
- **Return Task directly** when possible

```csharp
// Good
public async Task<User?> GetUserAsync(int id)
{
    return await _context.Users.FindAsync(id);
}

// Better (when no additional processing needed)
public Task<User?> GetUserAsync(int id)
{
    return _context.Users.FindAsync(id).AsTask();
}
```

## Testing Guidelines

### Unit Testing Structure

```csharp
[TestClass]
public class AssignmentServiceTests
{
    private Mock<IAssignmentRepository> _mockAssignmentRepository;
    private Mock<IClassRepository> _mockClassRepository;
    private AssignmentService _assignmentService;
    
    [TestInitialize]
    public void Setup()
    {
        _mockAssignmentRepository = new Mock<IAssignmentRepository>();
        _mockClassRepository = new Mock<IClassRepository>();
        _assignmentService = new AssignmentService(_mockAssignmentRepository.Object, _mockClassRepository.Object);
    }
    
    [TestMethod]
    public async Task CreateAsync_ValidRequest_ReturnsAssignmentResponse()
    {
        // Arrange
        var request = new CreateAssignmentRequest
        {
            ClassId = 1,
            Title = "Test Assignment",
            Description = "Test Description",
            DueDate = DateTime.UtcNow.AddDays(7),
            MaxPoints = 100
        };
        
        var classEntity = new Class { Id = 1, Course = new Course { CourseName = "Test Course" } };
        _mockClassRepository.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(classEntity);
        
        var assignment = new Assignment { Id = 1, Title = request.Title };
        _mockAssignmentRepository.Setup(r => r.CreateAsync(It.IsAny<Assignment>())).ReturnsAsync(assignment);
        
        // Act
        var result = await _assignmentService.CreateAsync(request);
        
        // Assert
        Assert.IsNotNull(result);
        Assert.AreEqual(request.Title, result.Title);
        _mockAssignmentRepository.Verify(r => r.CreateAsync(It.IsAny<Assignment>()), Times.Once);
    }
}
```

## Configuration Management

### Development Environment
Use User Secrets for sensitive data:

```bash
dotnet user-secrets set "ConnectionStrings:DefaultConnection" "your-connection-string"
dotnet user-secrets set "SendGrid:ApiKey" "your-sendgrid-key"
```

### Production Environment
Use environment variables or Azure Key Vault:

```csharp
// Program.cs
builder.Configuration.AddEnvironmentVariables();

// Access in code
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
```

### Configuration Classes
Create strongly-typed configuration:

```csharp
public class SendGridOptions
{
    public string ApiKey { get; set; } = null!;
    public string FromEmail { get; set; } = null!;
    public string FromName { get; set; } = null!;
}

// Register in Program.cs
builder.Services.Configure<SendGridOptions>(builder.Configuration.GetSection("SendGrid"));

// Use in services
public class SendGridEmailService : IEmailService
{
    private readonly SendGridOptions _options;
    
    public SendGridEmailService(IOptions<SendGridOptions> options)
    {
        _options = options.Value;
    }
}
```

## Performance Considerations

### Database Queries
- **Use Include** for related data to avoid N+1 queries
- **Project only needed fields** with Select
- **Use async methods** for all database operations
- **Add indexes** for frequently queried fields

```csharp
// Good - includes related data in single query
public async Task<IEnumerable<Assignment>> GetByClassIdAsync(int classId)
{
    return await _context.Assignments
        .Include(a => a.Class)
        .ThenInclude(c => c.Course)
        .Where(a => a.ClassId == classId)
        .ToListAsync();
}

// Better - project only needed fields
public async Task<IEnumerable<AssignmentSummary>> GetSummariesByClassIdAsync(int classId)
{
    return await _context.Assignments
        .Where(a => a.ClassId == classId)
        .Select(a => new AssignmentSummary
        {
            Id = a.Id,
            Title = a.Title,
            DueDate = a.DueDate,
            MaxPoints = a.MaxPoints
        })
        .ToListAsync();
}
```

### Caching
Implement caching for frequently accessed data:

```csharp
public class CachedCourseService : ICourseService
{
    private readonly ICourseService _courseService;
    private readonly IMemoryCache _cache;
    
    public async Task<Course?> GetByIdAsync(int id)
    {
        var cacheKey = $"course_{id}";
        
        if (_cache.TryGetValue(cacheKey, out Course? cachedCourse))
            return cachedCourse;
        
        var course = await _courseService.GetByIdAsync(id);
        if (course != null)
        {
            _cache.Set(cacheKey, course, TimeSpan.FromMinutes(30));
        }
        
        return course;
    }
}
```

---

*Following these guidelines ensures consistent, secure, and maintainable code across the entire application.*