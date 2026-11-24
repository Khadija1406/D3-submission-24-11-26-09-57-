using System.Data;
using Microsoft.Data.Sqlite;
using PulseMvc.Data;
using PulseMvc.Models;
using PulseMvc.Repositories.Interfaces;

namespace PulseMvc.Repositories;

/// <summary>
/// User repository implemented strictly with native ADO.NET (SqliteConnection, SqliteCommand, SqliteParameter, SqliteDataReader).
/// </summary>
public class UserRepository : IUserRepository
{
    private readonly IDbConnectionFactory _connectionFactory;

    public UserRepository(IDbConnectionFactory connectionFactory)
    {
        _connectionFactory = connectionFactory;
    }

    public async Task<ApplicationUser?> FindByIdAsync(string id)
    {
        using var connection = (SqliteConnection)_connectionFactory.CreateConnection();
        if (connection.State != ConnectionState.Open)
            await connection.OpenAsync();

        const string sql = "SELECT * FROM AspNetUsers WHERE Id = @Id LIMIT 1;";
        using var command = new SqliteCommand(sql, connection);
        command.Parameters.Add(new SqliteParameter("@Id", id));

        using var reader = await command.ExecuteReaderAsync();
        if (await reader.ReadAsync())
        {
            return MapUser(reader);
        }
        return null;
    }

    public async Task<ApplicationUser?> FindByEmailAsync(string email)
    {
        using var connection = (SqliteConnection)_connectionFactory.CreateConnection();
        if (connection.State != ConnectionState.Open)
            await connection.OpenAsync();

        var normalized = email.ToUpperInvariant();
        const string sql = "SELECT * FROM AspNetUsers WHERE NormalizedEmail = @NormalizedEmail OR NormalizedUserName = @NormalizedEmail LIMIT 1;";
        using var command = new SqliteCommand(sql, connection);
        command.Parameters.Add(new SqliteParameter("@NormalizedEmail", normalized));

        using var reader = await command.ExecuteReaderAsync();
        if (await reader.ReadAsync())
        {
            return MapUser(reader);
        }
        return null;
    }

    public async Task<ApplicationUser?> FindByNameAsync(string normalizedUserName)
    {
        using var connection = (SqliteConnection)_connectionFactory.CreateConnection();
        if (connection.State != ConnectionState.Open)
            await connection.OpenAsync();

        const string sql = "SELECT * FROM AspNetUsers WHERE NormalizedUserName = @NormalizedUserName OR NormalizedEmail = @NormalizedUserName LIMIT 1;";
        using var command = new SqliteCommand(sql, connection);
        command.Parameters.Add(new SqliteParameter("@NormalizedUserName", normalizedUserName));

        using var reader = await command.ExecuteReaderAsync();
        if (await reader.ReadAsync())
        {
            return MapUser(reader);
        }
        return null;
    }

    public async Task<bool> CreateAsync(ApplicationUser user)
    {
        using var connection = (SqliteConnection)_connectionFactory.CreateConnection();
        if (connection.State != ConnectionState.Open)
            await connection.OpenAsync();

        const string sql = @"
            INSERT INTO AspNetUsers (
                Id, UserName, NormalizedUserName, Email, NormalizedEmail, EmailConfirmed,
                PasswordHash, SecurityStamp, ConcurrencyStamp, PhoneNumberConfirmed,
                TwoFactorEnabled, LockoutEnabled, AccessFailedCount, FullName, CreatedAt, AvatarUrl
            ) VALUES (
                @Id, @UserName, @NormalizedUserName, @Email, @NormalizedEmail, @EmailConfirmed,
                @PasswordHash, @SecurityStamp, @ConcurrencyStamp, 0,
                0, 0, 0, @FullName, @CreatedAt, @AvatarUrl
            );";

        using var command = new SqliteCommand(sql, connection);
        command.Parameters.Add(new SqliteParameter("@Id", user.Id));
        command.Parameters.Add(new SqliteParameter("@UserName", user.UserName ?? (object)DBNull.Value));
        command.Parameters.Add(new SqliteParameter("@NormalizedUserName", user.NormalizedUserName ?? (object)DBNull.Value));
        command.Parameters.Add(new SqliteParameter("@Email", user.Email ?? (object)DBNull.Value));
        command.Parameters.Add(new SqliteParameter("@NormalizedEmail", user.NormalizedEmail ?? (object)DBNull.Value));
        command.Parameters.Add(new SqliteParameter("@EmailConfirmed", user.EmailConfirmed ? 1 : 0));
        command.Parameters.Add(new SqliteParameter("@PasswordHash", user.PasswordHash ?? (object)DBNull.Value));
        command.Parameters.Add(new SqliteParameter("@SecurityStamp", user.SecurityStamp ?? (object)DBNull.Value));
        command.Parameters.Add(new SqliteParameter("@ConcurrencyStamp", user.ConcurrencyStamp ?? (object)DBNull.Value));
        command.Parameters.Add(new SqliteParameter("@FullName", user.FullName ?? (object)DBNull.Value));
        command.Parameters.Add(new SqliteParameter("@CreatedAt", user.CreatedAt.ToString("o")));
        command.Parameters.Add(new SqliteParameter("@AvatarUrl", user.AvatarUrl ?? (object)DBNull.Value));

        var rows = await command.ExecuteNonQueryAsync();
        return rows > 0;
    }

    public async Task<bool> UpdateAsync(ApplicationUser user)
    {
        using var connection = (SqliteConnection)_connectionFactory.CreateConnection();
        if (connection.State != ConnectionState.Open)
            await connection.OpenAsync();

        const string sql = @"
            UPDATE AspNetUsers SET
                UserName = @UserName,
                NormalizedUserName = @NormalizedUserName,
                Email = @Email,
                NormalizedEmail = @NormalizedEmail,
                EmailConfirmed = @EmailConfirmed,
                PasswordHash = @PasswordHash,
                SecurityStamp = @SecurityStamp,
                ConcurrencyStamp = @ConcurrencyStamp,
                FullName = @FullName,
                AvatarUrl = @AvatarUrl
            WHERE Id = @Id;";

        using var command = new SqliteCommand(sql, connection);
        command.Parameters.Add(new SqliteParameter("@UserName", user.UserName ?? (object)DBNull.Value));
        command.Parameters.Add(new SqliteParameter("@NormalizedUserName", user.NormalizedUserName ?? (object)DBNull.Value));
        command.Parameters.Add(new SqliteParameter("@Email", user.Email ?? (object)DBNull.Value));
        command.Parameters.Add(new SqliteParameter("@NormalizedEmail", user.NormalizedEmail ?? (object)DBNull.Value));
        command.Parameters.Add(new SqliteParameter("@EmailConfirmed", user.EmailConfirmed ? 1 : 0));
        command.Parameters.Add(new SqliteParameter("@PasswordHash", user.PasswordHash ?? (object)DBNull.Value));
        command.Parameters.Add(new SqliteParameter("@SecurityStamp", user.SecurityStamp ?? (object)DBNull.Value));
        command.Parameters.Add(new SqliteParameter("@ConcurrencyStamp", user.ConcurrencyStamp ?? (object)DBNull.Value));
        command.Parameters.Add(new SqliteParameter("@FullName", user.FullName ?? (object)DBNull.Value));
        command.Parameters.Add(new SqliteParameter("@AvatarUrl", user.AvatarUrl ?? (object)DBNull.Value));
        command.Parameters.Add(new SqliteParameter("@Id", user.Id));

        var rows = await command.ExecuteNonQueryAsync();
        return rows > 0;
    }

    private static ApplicationUser MapUser(SqliteDataReader reader)
    {
        var user = new ApplicationUser
        {
            Id = reader.GetString(reader.GetOrdinal("Id")),
            UserName = reader.IsDBNull(reader.GetOrdinal("UserName")) ? null : reader.GetString(reader.GetOrdinal("UserName")),
            NormalizedUserName = reader.IsDBNull(reader.GetOrdinal("NormalizedUserName")) ? null : reader.GetString(reader.GetOrdinal("NormalizedUserName")),
            Email = reader.IsDBNull(reader.GetOrdinal("Email")) ? null : reader.GetString(reader.GetOrdinal("Email")),
            NormalizedEmail = reader.IsDBNull(reader.GetOrdinal("NormalizedEmail")) ? null : reader.GetString(reader.GetOrdinal("NormalizedEmail")),
            EmailConfirmed = reader.GetInt32(reader.GetOrdinal("EmailConfirmed")) == 1,
            PasswordHash = reader.IsDBNull(reader.GetOrdinal("PasswordHash")) ? null : reader.GetString(reader.GetOrdinal("PasswordHash")),
            SecurityStamp = reader.IsDBNull(reader.GetOrdinal("SecurityStamp")) ? null : reader.GetString(reader.GetOrdinal("SecurityStamp")),
            ConcurrencyStamp = reader.IsDBNull(reader.GetOrdinal("ConcurrencyStamp")) ? null : reader.GetString(reader.GetOrdinal("ConcurrencyStamp")),
            FullName = reader.IsDBNull(reader.GetOrdinal("FullName")) ? string.Empty : reader.GetString(reader.GetOrdinal("FullName")),
            AvatarUrl = reader.IsDBNull(reader.GetOrdinal("AvatarUrl")) ? null : reader.GetString(reader.GetOrdinal("AvatarUrl"))
        };

        if (!reader.IsDBNull(reader.GetOrdinal("CreatedAt")))
        {
            if (DateTime.TryParse(reader.GetString(reader.GetOrdinal("CreatedAt")), out var created))
            {
                user.CreatedAt = created;
            }
        }

        return user;
    }
}
