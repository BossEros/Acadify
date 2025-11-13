using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using ASI.Basecode.Data.Data;
using ASI.Basecode.Data.Models;
using ASI.Basecode.Data.Repositories;
using ASI.Basecode.Services.Interfaces;
using ASI.Basecode.Services.Implementation;


var builder = WebApplication.CreateBuilder(args);

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

// Configure token lifespan for password reset tokens
builder.Services.Configure<DataProtectionTokenProviderOptions>(options =>
{
    options.TokenLifespan = TimeSpan.FromMinutes(30); // Password reset tokens expire in 30 minutes
});

// Services
builder.Services.AddScoped<IAccountService, AccountService>();
builder.Services.AddScoped<IAccountManagementService, AccountManagementService>();
builder.Services.AddScoped<IClassManagementService, ClassManagementService>();
builder.Services.AddHttpClient<IEmailService, SendGridEmailService>();
builder.Services.AddScoped<ICourseManagementService, CourseManagementService>();
builder.Services.AddScoped<IClassReportService, ClassReportService>();
builder.Services.AddScoped<IStudentPerformanceService, StudentPerformanceService>();
builder.Services.AddScoped<IAdminDashboardService, AdminDashboardService>();

// Repositories (Data layer)
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<IAuthRepository, AuthRepository>();
builder.Services.AddScoped<IClassManagementRepository, ClassManagementRepository>();
builder.Services.AddScoped<ICourseManagementRepository, CourseManagementRepository>();

builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<IUserRepository, UserRepository>();

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

app.Run();