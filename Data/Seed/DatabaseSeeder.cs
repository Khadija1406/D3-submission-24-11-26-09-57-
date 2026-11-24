using Dapper;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using PulseMvc.Models;

namespace PulseMvc.Data.Seed;

public class DatabaseSeeder
{
    private readonly IDbConnectionFactory _connectionFactory;
    private readonly IPasswordHasher<ApplicationUser> _passwordHasher;
    private readonly ILogger<DatabaseSeeder> _logger;

    public DatabaseSeeder(
        IDbConnectionFactory connectionFactory,
        IPasswordHasher<ApplicationUser> passwordHasher,
        ILogger<DatabaseSeeder> logger)
    {
        _connectionFactory = connectionFactory;
        _passwordHasher = passwordHasher;
        _logger = logger;
    }

    public async Task SeedAsync()
    {
        using var connection = _connectionFactory.CreateConnection();

        // Check if any users exist
        var userCount = await connection.ExecuteScalarAsync<int>("SELECT COUNT(*) FROM AspNetUsers;");
        if (userCount > 0)
        {
            _logger.LogInformation("Database already seeded with users.");
            return;
        }

        _logger.LogInformation("Seeding initial user and database records...");

        // 1. Create a real customized identity user
        var user = new ApplicationUser
        {
            Id = Guid.NewGuid().ToString("D"),
            UserName = "alex@pulse.app",
            NormalizedUserName = "ALEX@PULSE.APP",
            Email = "alex@pulse.app",
            NormalizedEmail = "ALEX@PULSE.APP",
            FullName = "Alex Rivera",
            EmailConfirmed = true,
            SecurityStamp = Guid.NewGuid().ToString("D"),
            ConcurrencyStamp = Guid.NewGuid().ToString("D"),
            CreatedAt = DateTime.UtcNow
        };
        user.PasswordHash = _passwordHasher.HashPassword(user, "Password123!");

        var insertUserSql = @"
            INSERT INTO AspNetUsers (
                Id, UserName, NormalizedUserName, Email, NormalizedEmail, EmailConfirmed,
                PasswordHash, SecurityStamp, ConcurrencyStamp, PhoneNumberConfirmed,
                TwoFactorEnabled, LockoutEnabled, AccessFailedCount, FullName, CreatedAt
            ) VALUES (
                @Id, @UserName, @NormalizedUserName, @Email, @NormalizedEmail, @EmailConfirmed,
                @PasswordHash, @SecurityStamp, @ConcurrencyStamp, 0,
                0, 0, 0, @FullName, @CreatedAt
            );";

        await connection.ExecuteAsync(insertUserSql, new
        {
            user.Id,
            user.UserName,
            user.NormalizedUserName,
            user.Email,
            user.NormalizedEmail,
            EmailConfirmed = 1,
            user.PasswordHash,
            user.SecurityStamp,
            user.ConcurrencyStamp,
            user.FullName,
            CreatedAt = user.CreatedAt.ToString("o")
        });

        // 2. Insert initial problems for this user
        var now = DateTime.UtcNow;
        var p1 = new Problem
        {
            Id = "p-wifi",
            UserId = user.Id,
            Title = "Wi-Fi keeps dropping in the evening",
            Description = "The connection disconnects around 8–10pm, mostly during video calls. Restarting the router helps for a while.",
            Category = "technical",
            Status = "active",
            Impact = "High",
            CreatedAt = now.AddDays(-20).ToString("yyyy-MM-dd"),
            UpdatedAt = now.ToString("yyyy-MM-dd")
        };

        var p2 = new Problem
        {
            Id = "p-academic",
            UserId = user.Id,
            Title = "Missing assignment deadlines",
            Description = "I keep losing track of due dates when several courses have work due in the same week.",
            Category = "academic",
            Status = "progress",
            Impact = "Medium",
            CreatedAt = now.AddDays(-15).ToString("yyyy-MM-dd"),
            UpdatedAt = now.ToString("yyyy-MM-dd")
        };

        var p3 = new Problem
        {
            Id = "p-household",
            UserId = user.Id,
            Title = "Kitchen tap is leaking",
            Description = "Slow drip from the kitchen tap, gets worse at night.",
            Category = "household",
            Status = "resolved",
            Impact = "Low",
            CreatedAt = now.AddDays(-25).ToString("yyyy-MM-dd"),
            UpdatedAt = now.ToString("yyyy-MM-dd")
        };

        var p4 = new Problem
        {
            Id = "p-lifestyle",
            UserId = user.Id,
            Title = "Sleeping too late on weekdays",
            Description = "Going to bed after 2am, which makes mornings hard and lowers my focus.",
            Category = "lifestyle",
            Status = "active",
            Impact = "Medium",
            CreatedAt = now.AddDays(-10).ToString("yyyy-MM-dd"),
            UpdatedAt = now.ToString("yyyy-MM-dd")
        };

        var p5 = new Problem
        {
            Id = "p-managerial",
            UserId = user.Id,
            Title = "Team tasks overlapping",
            Description = "Two people end up working on the same task because nobody updates the board.",
            Category = "managerial",
            Status = "active",
            Impact = "Medium",
            CreatedAt = now.AddDays(-4).ToString("yyyy-MM-dd"),
            UpdatedAt = now.ToString("yyyy-MM-dd")
        };

        var insertProblemSql = @"
            INSERT INTO Problems (Id, UserId, Title, Description, Category, Status, Impact, CreatedAt, UpdatedAt)
            VALUES (@Id, @UserId, @Title, @Description, @Category, @Status, @Impact, @CreatedAt, @UpdatedAt);";

        await connection.ExecuteAsync(insertProblemSql, new[] { p1, p2, p3, p4, p5 });

        // Insert occurrences
        var insertOccSql = @"
            INSERT INTO ProblemOccurrences (Id, ProblemId, OccurrenceDate, Note)
            VALUES (@Id, @ProblemId, @OccurrenceDate, @Note);";

        var occurrences = new[]
        {
            new { Id = "occ-w1", ProblemId = p1.Id, OccurrenceDate = now.AddDays(-20).ToString("yyyy-MM-dd"), Note = "First noticed during a class call." },
            new { Id = "occ-w2", ProblemId = p1.Id, OccurrenceDate = now.AddDays(-12).ToString("yyyy-MM-dd"), Note = "Dropped twice in one hour." },
            new { Id = "occ-w3", ProblemId = p1.Id, OccurrenceDate = now.AddDays(-6).ToString("yyyy-MM-dd"), Note = "Restarted router, back after 5 min." },
            new { Id = "occ-w4", ProblemId = p1.Id, OccurrenceDate = now.AddDays(-2).ToString("yyyy-MM-dd"), Note = "Dropped during a meeting again." },
            new { Id = "occ-w5", ProblemId = p1.Id, OccurrenceDate = now.ToString("yyyy-MM-dd"), Note = "Slow and disconnecting." },

            new { Id = "occ-a1", ProblemId = p2.Id, OccurrenceDate = now.AddDays(-15).ToString("yyyy-MM-dd"), Note = "Late on the database report." },
            new { Id = "occ-a2", ProblemId = p2.Id, OccurrenceDate = now.AddDays(-8).ToString("yyyy-MM-dd"), Note = "Nearly missed the quiz." },
            new { Id = "occ-a3", ProblemId = p2.Id, OccurrenceDate = now.AddDays(-3).ToString("yyyy-MM-dd"), Note = "Submitted lab 2 hours late." },

            new { Id = "occ-h1", ProblemId = p3.Id, OccurrenceDate = now.AddDays(-25).ToString("yyyy-MM-dd"), Note = "Noticed the drip." },
            new { Id = "occ-h2", ProblemId = p3.Id, OccurrenceDate = now.AddDays(-18).ToString("yyyy-MM-dd"), Note = "Replaced washer — fixed." },

            new { Id = "occ-l1", ProblemId = p4.Id, OccurrenceDate = now.AddDays(-10).ToString("yyyy-MM-dd"), Note = "" },
            new { Id = "occ-l2", ProblemId = p4.Id, OccurrenceDate = now.AddDays(-5).ToString("yyyy-MM-dd"), Note = "Phone scrolling until late." },
            new { Id = "occ-l3", ProblemId = p4.Id, OccurrenceDate = now.AddDays(-1).ToString("yyyy-MM-dd"), Note = "" },

            new { Id = "occ-m1", ProblemId = p5.Id, OccurrenceDate = now.AddDays(-4).ToString("yyyy-MM-dd"), Note = "Duplicate slides made." }
        };

        await connection.ExecuteAsync(insertOccSql, occurrences);
        _logger.LogInformation("Database seeded successfully with user alex@pulse.app and 5 initial problems.");
    }
}
