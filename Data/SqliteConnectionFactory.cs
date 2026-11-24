using System.Data;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Data.Sqlite;
using Microsoft.Extensions.Configuration;

namespace PulseMvc.Data;

public class SqliteConnectionFactory : IDbConnectionFactory
{
    private readonly string _connectionString;

    public SqliteConnectionFactory(IConfiguration configuration, IWebHostEnvironment environment)
    {
        var raw = configuration.GetConnectionString("DefaultConnection") ?? "Data Source=pulse.db";
        var filePart = raw.Replace("Data Source=", "").Trim();

        // If the database path is relative, anchor it to ContentRootPath so Visual Studio and IIS Express find it
        if (!Path.IsPathRooted(filePart))
        {
            var fullPath = Path.Combine(environment.ContentRootPath, filePart);
            _connectionString = $"Data Source={fullPath}";
        }
        else
        {
            _connectionString = raw;
        }
    }

    public IDbConnection CreateConnection()
    {
        var connection = new SqliteConnection(_connectionString);
        connection.Open();
        return connection;
    }
}
