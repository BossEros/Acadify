using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using ASI.Basecode.Data.Data;
using ASI.Basecode.Data.Models;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Logging;
using ASI.Basecode.Data.Repositories;
using ASI.Basecode.Services.Interfaces;
using ASI.Basecode.Services.Implementation;
using System.IO;


// Configure WebRootPath - use source wwwroot if output wwwroot doesn't exist
// This handles cases where wwwroot isn't copied to the output directory
var contentRoot = Directory.GetCurrentDirectory();
var outputWwwRoot = Path.Combine(contentRoot, "wwwroot");
string? webRootPath = null;

if (!Directory.Exists(outputWwwRoot))
{
    // If running from bin/Debug/net9.0, go up to find source wwwroot
    var sourceWwwRoot = Path.Combine(contentRoot, "..", "..", "..", "wwwroot");
    sourceWwwRoot = Path.GetFullPath(sourceWwwRoot); // Resolve relative path
    
    // Alternative: try going up from ContentRootPath if it's in bin directory
    if (!Directory.Exists(sourceWwwRoot) && contentRoot.Contains("bin"))
    {
        var projectRoot = Directory.GetParent(contentRoot)?.Parent?.Parent?.FullName;
        if (projectRoot != null)
        {
            sourceWwwRoot = Path.Combine(projectRoot, "wwwroot");
        }
    }
    
    if (Directory.Exists(sourceWwwRoot))
    {
        webRootPath = sourceWwwRoot;
    }
}

// Create builder with WebApplicationOptions to set web root
var options = new WebApplicationOptions
{
    Args = args,
    ContentRootPath = contentRoot,
    WebRootPath = webRootPath
};

var builder = WebApplication.CreateBuilder(options);

// Add services to the container.
builder.Services.AddControllersWithViews()
    .AddJsonOptions(options =>
    {
        // Handle circular references in JSON serialization
        options.JsonSerializerOptions.ReferenceHandler = System.Text.Json.Serialization.ReferenceHandler.IgnoreCycles;
        options.JsonSerializerOptions.WriteIndented = true; // Pretty print for development
    });


// Add DbContext
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

// Add Identity services
builder.Services.AddIdentity<User, IdentityRole<int>>(options =>
{
    // Password rules
    options.Password.RequireDigit = true;
    options.Password.RequireLowercase = true;
    options.Password.RequireUppercase = true;
    options.Password.RequireNonAlphanumeric = true;
    options.Password.RequiredUniqueChars = 2;
    options.Password.RequiredLength = 8;    

    // User rules
    options.User.RequireUniqueEmail = true;
    options.SignIn.RequireConfirmedEmail = false;

    // Lockout rules
    options.Lockout.MaxFailedAccessAttempts = 5;
    options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(15);

})
.AddEntityFrameworkStores<AppDbContext>()
.AddDefaultTokenProviders();


// Services
builder.Services.AddScoped<IAccountService, AccountService>();
builder.Services.AddScoped<IAccountManagementService, AccountManagementService>();
builder.Services.AddScoped<IClassManagementService, ClassManagementService>();
builder.Services.AddHttpClient<IEmailService, SendGridEmailService>();
builder.Services.AddScoped<ICourseManagementService, CourseManagementService>();

// Repositories (Data layer)
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<IAuthRepository, AuthRepository>();
builder.Services.AddScoped<IClassManagementRepository, ClassManagementRepository>();
builder.Services.AddScoped<ICourseManagementRepository, CourseManagementRepository>();
builder.Services.AddScoped<IUserService, UserService>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles(); // Correctly serve static files
app.UseRouting();

// Add Authentication and Authorization middleware
app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Account}/{action=Login}/{id?}");

await SeedDataAsync(app);

app.Run();

static async Task SeedDataAsync(WebApplication app)
{
    using var scope = app.Services.CreateScope();
    var loggerFactory = scope.ServiceProvider.GetRequiredService<ILoggerFactory>();
    var logger = loggerFactory.CreateLogger("DataSeeder");

    var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();

    try
    {
        await context.Database.MigrateAsync();
    }
    catch (Exception ex)
    {
        logger.LogError(ex, "Database migration failed during seeding.");
    }

    var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole<int>>>();
    string[] roles = { "Admin", "Teacher", "Student" };

    foreach (var role in roles)
    {
        if (!await roleManager.RoleExistsAsync(role))
        {
            var identityRole = new IdentityRole<int>(role);
            var roleResult = await roleManager.CreateAsync(identityRole);

            if (roleResult.Succeeded)
            {
                logger.LogInformation("Created role '{Role}'", role);
            }
            else
            {
                foreach (var error in roleResult.Errors)
                {
                    logger.LogError("Failed to create role '{Role}': {Error}", role, error.Description);
                }
            }
        }
    }

    var userManager = scope.ServiceProvider.GetRequiredService<UserManager<User>>();
    var adminEmail = app.Configuration["SeedAdmin:Email"] ?? "admin@acadify.local";
    var adminPassword = app.Configuration["SeedAdmin:Password"] ?? "Admin123!";

    var adminUser = await userManager.FindByEmailAsync(adminEmail);
    if (adminUser == null)
    {
        adminUser = new User
        {
            UserName = adminEmail,
            Email = adminEmail,
            FirstName = "System",
            LastName = "Administrator",
            EmailConfirmed = true,
            IsApproved = true
        };

        var createResult = await userManager.CreateAsync(adminUser, adminPassword);
        if (createResult.Succeeded)
        {
            await userManager.AddToRoleAsync(adminUser, "Admin");
            logger.LogInformation("Seeded default admin account '{Email}'", adminEmail);
        }
        else
        {
            foreach (var error in createResult.Errors)
            {
                logger.LogError("Failed to seed admin account '{Email}': {Error}", adminEmail, error.Description);
            }
        }
    }

    var teacherEmail = app.Configuration["SeedTeacher:Email"] ?? "teacher@acadify.local";
    var teacherPassword = app.Configuration["SeedTeacher:Password"] ?? "Teacher123!";

    var teacherUser = await userManager.FindByEmailAsync(teacherEmail);
    if (teacherUser == null)
    {
        teacherUser = new User
        {
            UserName = teacherEmail,
            Email = teacherEmail,
            FirstName = "Taylor",
            LastName = "Reyes",
            EmailConfirmed = true,
            IsApproved = true
        };

        var createTeacher = await userManager.CreateAsync(teacherUser, teacherPassword);
        if (createTeacher.Succeeded)
        {
            await userManager.AddToRoleAsync(teacherUser, "Teacher");
            logger.LogInformation("Seeded sample teacher account '{Email}'", teacherEmail);
        }
        else
        {
            foreach (var error in createTeacher.Errors)
            {
                logger.LogError("Failed to seed teacher account '{Email}': {Error}", teacherEmail, error.Description);
            }
        }
    }
    else if (!await userManager.IsInRoleAsync(teacherUser, "Teacher"))
    {
        await userManager.AddToRoleAsync(teacherUser, "Teacher");
    }

    var studentEmail = app.Configuration["SeedStudent:Email"] ?? "student@acadify.local";
    var studentPassword = app.Configuration["SeedStudent:Password"] ?? "Student123!";

    var studentUser = await userManager.FindByEmailAsync(studentEmail);
    if (studentUser == null)
    {
        studentUser = new User
        {
            UserName = studentEmail,
            Email = studentEmail,
            FirstName = "Jordan",
            LastName = "Castro",
            EmailConfirmed = true,
            IsApproved = true
        };

        var createStudent = await userManager.CreateAsync(studentUser, studentPassword);
        if (createStudent.Succeeded)
        {
            await userManager.AddToRoleAsync(studentUser, "Student");
            logger.LogInformation("Seeded sample student account '{Email}'", studentEmail);
        }
        else
        {
            foreach (var error in createStudent.Errors)
            {
                logger.LogError("Failed to seed student account '{Email}': {Error}", studentEmail, error.Description);
            }
        }
    }
    else if (!await userManager.IsInRoleAsync(studentUser, "Student"))
    {
        await userManager.AddToRoleAsync(studentUser, "Student");
    }

    // Seed sample courses
    var sampleCourses = new[]
    {
        new Course { CourseCode = "CS101", CourseName = "Introduction to Computer Science", Description = "Foundations of computing and algorithms.", Units = 3, YearLevel = 1, AvailableSemester = 1 },
        new Course { CourseCode = "CS102", CourseName = "Data Structures", Description = "Core data structures and their applications.", Units = 3, YearLevel = 1, AvailableSemester = 2 },
        new Course { CourseCode = "IT201", CourseName = "Networks and Communications", Description = "Networking fundamentals and protocols.", Units = 4, YearLevel = 2, AvailableSemester = 1 },
        new Course { CourseCode = "IT205", CourseName = "Database Systems", Description = "Relational databases and SQL.", Units = 4, YearLevel = 2, AvailableSemester = 2 },
        new Course { CourseCode = "CS301", CourseName = "Software Engineering Practices", Description = "Team-based software development lifecycle.", Units = 3, YearLevel = 3, AvailableSemester = 1 }
    };

    var coursesToAdd = new List<Course>();
    foreach (var sampleCourse in sampleCourses)
    {
        if (!await context.Courses.AnyAsync(c => c.CourseCode == sampleCourse.CourseCode))
        {
            coursesToAdd.Add(sampleCourse);
        }
    }

    if (coursesToAdd.Any())
    {
        await context.Courses.AddRangeAsync(coursesToAdd);
        await context.SaveChangesAsync();
        logger.LogInformation("Seeded sample courses: {Courses}", string.Join(", ", coursesToAdd.Select(c => c.CourseCode)));
    }

    // Seed sample classes
    var sampleClassDefinitions = new[]
    {
        new { JoinCode = "CS101-A", CourseCode = "CS101", Semester = (short)1, YearLevel = (short)1, Schedule = "Mon/Wed 09:00-10:30", Room = "Lab 201" },
        new { JoinCode = "CS102-B", CourseCode = "CS102", Semester = (short)2, YearLevel = (short)1, Schedule = "Tue/Thu 13:00-14:30", Room = "Room 305" },
        new { JoinCode = "IT201-A", CourseCode = "IT201", Semester = (short)1, YearLevel = (short)2, Schedule = "Mon 14:00-17:00", Room = "Lab 110" },
        new { JoinCode = "IT205-C", CourseCode = "IT205", Semester = (short)2, YearLevel = (short)2, Schedule = "Wed 10:00-12:00", Room = "Room 409" },
        new { JoinCode = "CS301-A", CourseCode = "CS301", Semester = (short)1, YearLevel = (short)3, Schedule = "Fri 08:00-11:00", Room = "Innovation Hub" }
    };

    var classesToAdd = new List<Class>();
    var defaultTeacher = teacherUser ?? adminUser;

    foreach (var definition in sampleClassDefinitions)
    {
        if (await context.Classes.AnyAsync(c => c.JoinCode == definition.JoinCode))
        {
            continue;
        }

        var course = await context.Courses.FirstOrDefaultAsync(c => c.CourseCode == definition.CourseCode);
        if (course == null || defaultTeacher == null)
        {
            continue;
        }

        classesToAdd.Add(new Class
        {
            CourseId = course.Id,
            TeacherId = defaultTeacher.Id,
            Semester = definition.Semester,
            YearLevel = definition.YearLevel,
            Schedule = definition.Schedule,
            Room = definition.Room,
            JoinCode = definition.JoinCode,
            JoinCodeGeneratedAt = DateTime.UtcNow,
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        });
    }

    if (classesToAdd.Any())
    {
        await context.Classes.AddRangeAsync(classesToAdd);
        await context.SaveChangesAsync();
        logger.LogInformation("Seeded sample classes: {Classes}", string.Join(", ", classesToAdd.Select(cls => $"{cls.JoinCode} -> {sampleClassDefinitions.First(d => d.JoinCode == cls.JoinCode).CourseCode}")));
    }

    // Enroll the sample student in the first class for demo purposes
    if (studentUser != null)
    {
        var firstClass = await context.Classes.OrderBy(c => c.Id).FirstOrDefaultAsync();
        if (firstClass != null && !await context.Enrollments.AnyAsync(e => e.ClassId == firstClass.Id && e.StudentId == studentUser.Id))
        {
            context.Enrollments.Add(new Enrollment
            {
                ClassId = firstClass.Id,
                StudentId = studentUser.Id,
                EnrolledAt = DateTime.UtcNow
            });
            await context.SaveChangesAsync();
            logger.LogInformation("Enrolled sample student '{Email}' in class '{JoinCode}'", studentEmail, firstClass.JoinCode);
        }
    }

    var availableEdpCodes = await context.Classes
        .Include(c => c.Course)
        .Select(c => $"{c.Course.CourseCode} (Join Code: {c.JoinCode})")
        .ToListAsync();

    if (availableEdpCodes.Any())
    {
        logger.LogInformation("Available course / join codes: {Codes}", string.Join(", ", availableEdpCodes));
    }
}