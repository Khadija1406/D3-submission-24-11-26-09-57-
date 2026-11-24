using System.Data;
using Dapper;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Logging;

namespace PulseMvc.Data.Migrations;

public class MigrationRunner : IMigrationRunner
{
    private readonly IDbConnectionFactory _connectionFactory;
    private readonly IWebHostEnvironment _environment;
    private readonly ILogger<MigrationRunner> _logger;

    public MigrationRunner(
        IDbConnectionFactory connectionFactory,
        IWebHostEnvironment environment,
        ILogger<MigrationRunner> logger)
    {
        _connectionFactory = connectionFactory;
        _environment = environment;
        _logger = logger;
    }

    public async Task RunMigrationsAsync()
    {
        using var connection = _connectionFactory.CreateConnection();

        // 1. Ensure migrations history table exists
        await connection.ExecuteAsync(@"
            CREATE TABLE IF NOT EXISTS __MigrationsHistory (
                MigrationId TEXT PRIMARY KEY,
                AppliedAt TEXT NOT NULL
            );
        ");

        var appliedMigrations = (await connection.QueryAsync<string>(
            "SELECT MigrationId FROM __MigrationsHistory"
        )).ToHashSet();

        // 2. Discover scripts directory reliably across Visual Studio & CLI
        var scriptsDir = Path.Combine(_environment.ContentRootPath, "Data", "Migrations", "Scripts");
        if (!Directory.Exists(scriptsDir))
        {
            scriptsDir = Path.Combine(AppContext.BaseDirectory, "Data", "Migrations", "Scripts");
        }

        if (Directory.Exists(scriptsDir))
        {
            var files = Directory.GetFiles(scriptsDir, "*.sql").OrderBy(f => f).ToList();
            foreach (var file in files)
            {
                var migrationId = Path.GetFileNameWithoutExtension(file);
                if (appliedMigrations.Contains(migrationId))
                {
                    continue;
                }

                _logger.LogInformation("Applying migration: {MigrationId}", migrationId);
                var sql = await File.ReadAllTextAsync(file);

                using var trans = connection.BeginTransaction();
                try
                {
                    await connection.ExecuteAsync(sql, transaction: trans);
                    await connection.ExecuteAsync(
                        "INSERT INTO __MigrationsHistory (MigrationId, AppliedAt) VALUES (@MigrationId, @AppliedAt)",
                        new { MigrationId = migrationId, AppliedAt = DateTime.UtcNow.ToString("o") },
                        transaction: trans
                    );
                    trans.Commit();
                    _logger.LogInformation("Successfully applied migration: {MigrationId}", migrationId);
                }
                catch (Exception ex)
                {
                    trans.Rollback();
                    _logger.LogError(ex, "Failed to apply migration: {MigrationId}", migrationId);
                    throw;
                }
            }
        }
        else
        {
            _logger.LogWarning("Migration scripts folder not found: {ScriptsDir}", scriptsDir);
        }
    }
}
