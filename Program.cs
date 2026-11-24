using Microsoft.AspNetCore.Identity;
using PulseMvc.Data;
using PulseMvc.Data.Migrations;
using PulseMvc.Data.Seed;
using PulseMvc.Identity;
using PulseMvc.Models;
using PulseMvc.Repositories;
using PulseMvc.Repositories.Interfaces;

var builder = WebApplication.CreateBuilder(args);

// ----------------------------------------------------
// 1. Dependency Injection - Database & Dapper
// ----------------------------------------------------
builder.Services.AddSingleton<IDbConnectionFactory, SqliteConnectionFactory>();
builder.Services.AddScoped<DapperContext>();
builder.Services.AddScoped<IMigrationRunner, MigrationRunner>();
builder.Services.AddScoped<DatabaseSeeder>();

// ----------------------------------------------------
// 2. Dependency Injection - Repositories (Interfaces)
// ----------------------------------------------------
builder.Services.AddScoped<IProblemRepository, ProblemRepository>();
builder.Services.AddScoped<IUserRepository, UserRepository>();

// ----------------------------------------------------
// 3. ASP.NET Core Identity with Custom Dapper Stores
// ----------------------------------------------------
builder.Services.AddScoped<IPasswordHasher<ApplicationUser>, PasswordHasher<ApplicationUser>>();
builder.Services.AddTransient<IUserStore<ApplicationUser>, DapperUserStore>();
builder.Services.AddTransient<IRoleStore<IdentityRole>, DapperRoleStore>();

builder.Services.AddIdentity<ApplicationUser, IdentityRole>(options =>
{
    // Password settings strictly enforcing minimum 8 characters
    options.Password.RequireDigit = false;
    options.Password.RequiredLength = 8;
    options.Password.RequireNonAlphanumeric = false;
    options.Password.RequireUppercase = false;
    options.Password.RequireLowercase = false;
    options.User.RequireUniqueEmail = true;
    options.SignIn.RequireConfirmedAccount = false;
})
.AddDefaultTokenProviders();

builder.Services.ConfigureApplicationCookie(options =>
{
    options.Cookie.HttpOnly = true;
    options.ExpireTimeSpan = TimeSpan.FromDays(14);
    options.LoginPath = "/Account/Login";
    options.LogoutPath = "/Account/Logout";
    options.SlidingExpiration = true;
});

// ----------------------------------------------------
// 4. MVC Framework & Controllers with Views
// ----------------------------------------------------
builder.Services.AddControllersWithViews()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.PropertyNamingPolicy = null; // Preserve casing or use camelCase
    });

var app = builder.Build();

// ----------------------------------------------------
// 5. Automatic Migrations and Database Seeding on Startup
// ----------------------------------------------------
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    var logger = services.GetRequiredService<ILogger<Program>>();
    try
    {
        logger.LogInformation("Executing database migrations...");
        var migrator = services.GetRequiredService<IMigrationRunner>();
        await migrator.RunMigrationsAsync();

        logger.LogInformation("Seeding database with user and initial problems...");
        var seeder = services.GetRequiredService<DatabaseSeeder>();
        await seeder.SeedAsync();
    }
    catch (Exception ex)
    {
        logger.LogError(ex, "An error occurred during database migration or seeding.");
    }
}

// ----------------------------------------------------
// 6. HTTP Request Pipeline
// ----------------------------------------------------
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
