using System.Data;
using Microsoft.Data.Sqlite;
using PulseMvc.Data;
using PulseMvc.Models.ViewModels;
using PulseMvc.Repositories.Interfaces;

namespace PulseMvc.Repositories;

/// <summary>
/// Problem repository implemented strictly with native ADO.NET (SqliteConnection, SqliteCommand, SqliteParameter, SqliteDataReader).
/// </summary>
public class ProblemRepository : IProblemRepository
{
    private readonly IDbConnectionFactory _connectionFactory;

    public ProblemRepository(IDbConnectionFactory connectionFactory)
    {
        _connectionFactory = connectionFactory;
    }

    public async Task<IEnumerable<ProblemDto>> GetAllAsync(string? userId = null)
    {
        using var connection = (SqliteConnection)_connectionFactory.CreateConnection();
        if (connection.State != ConnectionState.Open)
            await connection.OpenAsync();

        var problems = new List<ProblemDto>();

        var sqlProblems = string.IsNullOrEmpty(userId)
            ? "SELECT Id, UserId, Title, Description, Category, Status, Impact, CreatedAt FROM Problems ORDER BY CreatedAt DESC;"
            : "SELECT Id, UserId, Title, Description, Category, Status, Impact, CreatedAt FROM Problems WHERE UserId = @UserId ORDER BY CreatedAt DESC;";

        using (var command = new SqliteCommand(sqlProblems, connection))
        {
            if (!string.IsNullOrEmpty(userId))
            {
                command.Parameters.Add(new SqliteParameter("@UserId", userId));
            }

            using var reader = await command.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                problems.Add(new ProblemDto
                {
                    Id = reader.GetString(reader.GetOrdinal("Id")),
                    UserId = reader.IsDBNull(reader.GetOrdinal("UserId")) ? "" : reader.GetString(reader.GetOrdinal("UserId")),
                    Title = reader.GetString(reader.GetOrdinal("Title")),
                    Desc = reader.IsDBNull(reader.GetOrdinal("Description")) ? "" : reader.GetString(reader.GetOrdinal("Description")),
                    Cat = reader.GetString(reader.GetOrdinal("Category")),
                    Status = reader.GetString(reader.GetOrdinal("Status")),
                    Impact = reader.GetString(reader.GetOrdinal("Impact")),
                    Created = reader.GetString(reader.GetOrdinal("CreatedAt")),
                    Occ = new List<OccurrenceDto>(),
                    Tried = new Dictionary<string, TriedDto>()
                });
            }
        }

        if (problems.Count == 0)
        {
            return Enumerable.Empty<ProblemDto>();
        }

        // Fetch occurrences using ADO.NET
        var problemMap = problems.ToDictionary(p => p.Id);
        const string sqlOcc = "SELECT ProblemId, OccurrenceDate, Note FROM ProblemOccurrences ORDER BY OccurrenceDate DESC;";
        using (var occCommand = new SqliteCommand(sqlOcc, connection))
        {
            using var occReader = await occCommand.ExecuteReaderAsync();
            while (await occReader.ReadAsync())
            {
                var pid = occReader.GetString(occReader.GetOrdinal("ProblemId"));
                if (problemMap.TryGetValue(pid, out var p))
                {
                    p.Occ.Add(new OccurrenceDto
                    {
                        D = occReader.GetString(occReader.GetOrdinal("OccurrenceDate")),
                        N = occReader.IsDBNull(occReader.GetOrdinal("Note")) ? "" : occReader.GetString(occReader.GetOrdinal("Note"))
                    });
                }
            }
        }

        // Fetch tried solutions using ADO.NET
        const string sqlTried = "SELECT ProblemId, SolutionKey, Status, Note FROM ProblemTriedSolutions;";
        using (var triedCommand = new SqliteCommand(sqlTried, connection))
        {
            using var triedReader = await triedCommand.ExecuteReaderAsync();
            while (await triedReader.ReadAsync())
            {
                var pid = triedReader.GetString(triedReader.GetOrdinal("ProblemId"));
                if (problemMap.TryGetValue(pid, out var p))
                {
                    var key = triedReader.GetString(triedReader.GetOrdinal("SolutionKey"));
                    p.Tried[key] = new TriedDto
                    {
                        S = triedReader.GetString(triedReader.GetOrdinal("Status")),
                        N = triedReader.IsDBNull(triedReader.GetOrdinal("Note")) ? "" : triedReader.GetString(triedReader.GetOrdinal("Note"))
                    };
                }
            }
        }

        return problems;
    }

    public async Task<ProblemDto?> GetByIdAsync(string id)
    {
        using var connection = (SqliteConnection)_connectionFactory.CreateConnection();
        if (connection.State != ConnectionState.Open)
            await connection.OpenAsync();

        ProblemDto? problem = null;

        const string sql = "SELECT Id, UserId, Title, Description, Category, Status, Impact, CreatedAt FROM Problems WHERE Id = @Id LIMIT 1;";
        using (var command = new SqliteCommand(sql, connection))
        {
            command.Parameters.Add(new SqliteParameter("@Id", id));

            using var reader = await command.ExecuteReaderAsync();
            if (await reader.ReadAsync())
            {
                problem = new ProblemDto
                {
                    Id = reader.GetString(reader.GetOrdinal("Id")),
                    UserId = reader.IsDBNull(reader.GetOrdinal("UserId")) ? "" : reader.GetString(reader.GetOrdinal("UserId")),
                    Title = reader.GetString(reader.GetOrdinal("Title")),
                    Desc = reader.IsDBNull(reader.GetOrdinal("Description")) ? "" : reader.GetString(reader.GetOrdinal("Description")),
                    Cat = reader.GetString(reader.GetOrdinal("Category")),
                    Status = reader.GetString(reader.GetOrdinal("Status")),
                    Impact = reader.GetString(reader.GetOrdinal("Impact")),
                    Created = reader.GetString(reader.GetOrdinal("CreatedAt")),
                    Occ = new List<OccurrenceDto>(),
                    Tried = new Dictionary<string, TriedDto>()
                };
            }
        }

        if (problem == null) return null;

        // Occurrences
        const string sqlOcc = "SELECT OccurrenceDate, Note FROM ProblemOccurrences WHERE ProblemId = @ProblemId ORDER BY OccurrenceDate DESC;";
        using (var occCmd = new SqliteCommand(sqlOcc, connection))
        {
            occCmd.Parameters.Add(new SqliteParameter("@ProblemId", id));
            using var occReader = await occCmd.ExecuteReaderAsync();
            while (await occReader.ReadAsync())
            {
                problem.Occ.Add(new OccurrenceDto
                {
                    D = occReader.GetString(occReader.GetOrdinal("OccurrenceDate")),
                    N = occReader.IsDBNull(occReader.GetOrdinal("Note")) ? "" : occReader.GetString(occReader.GetOrdinal("Note"))
                });
            }
        }

        // Tried solutions
        const string sqlTried = "SELECT SolutionKey, Status, Note FROM ProblemTriedSolutions WHERE ProblemId = @ProblemId;";
        using (var triedCmd = new SqliteCommand(sqlTried, connection))
        {
            triedCmd.Parameters.Add(new SqliteParameter("@ProblemId", id));
            using var triedReader = await triedCmd.ExecuteReaderAsync();
            while (await triedReader.ReadAsync())
            {
                var key = triedReader.GetString(triedReader.GetOrdinal("SolutionKey"));
                problem.Tried[key] = new TriedDto
                {
                    S = triedReader.GetString(triedReader.GetOrdinal("Status")),
                    N = triedReader.IsDBNull(triedReader.GetOrdinal("Note")) ? "" : triedReader.GetString(triedReader.GetOrdinal("Note"))
                };
            }
        }

        return problem;
    }

    public async Task<string> CreateAsync(CreateProblemRequest request, string userId)
    {
        using var connection = (SqliteConnection)_connectionFactory.CreateConnection();
        if (connection.State != ConnectionState.Open)
            await connection.OpenAsync();

        var id = "p-" + Guid.NewGuid().ToString("N")[..6];
        var created = string.IsNullOrWhiteSpace(request.Created) ? DateTime.UtcNow.ToString("yyyy-MM-dd") : request.Created;

        const string insertSql = @"
            INSERT INTO Problems (Id, UserId, Title, Description, Category, Status, Impact, CreatedAt, UpdatedAt)
            VALUES (@Id, @UserId, @Title, @Description, @Category, @Status, @Impact, @CreatedAt, @UpdatedAt);";

        using (var command = new SqliteCommand(insertSql, connection))
        {
            command.Parameters.Add(new SqliteParameter("@Id", id));
            command.Parameters.Add(new SqliteParameter("@UserId", userId));
            command.Parameters.Add(new SqliteParameter("@Title", request.Title ?? "Untitled problem"));
            command.Parameters.Add(new SqliteParameter("@Description", request.Desc ?? ""));
            command.Parameters.Add(new SqliteParameter("@Category", request.Cat ?? "technical"));
            command.Parameters.Add(new SqliteParameter("@Status", request.Status ?? "active"));
            command.Parameters.Add(new SqliteParameter("@Impact", request.Impact ?? "Medium"));
            command.Parameters.Add(new SqliteParameter("@CreatedAt", created));
            command.Parameters.Add(new SqliteParameter("@UpdatedAt", created));

            await command.ExecuteNonQueryAsync();
        }

        // Insert initial occurrence with ADO.NET
        var occId = Guid.NewGuid().ToString("N")[..8];
        const string occSql = "INSERT INTO ProblemOccurrences (Id, ProblemId, OccurrenceDate, Note) VALUES (@Id, @ProblemId, @Date, @Note);";
        using (var occCmd = new SqliteCommand(occSql, connection))
        {
            occCmd.Parameters.Add(new SqliteParameter("@Id", occId));
            occCmd.Parameters.Add(new SqliteParameter("@ProblemId", id));
            occCmd.Parameters.Add(new SqliteParameter("@Date", created));
            occCmd.Parameters.Add(new SqliteParameter("@Note", "First reported."));

            await occCmd.ExecuteNonQueryAsync();
        }

        return id;
    }

    public async Task<bool> UpdateStatusAsync(string id, string status)
    {
        using var connection = (SqliteConnection)_connectionFactory.CreateConnection();
        if (connection.State != ConnectionState.Open)
            await connection.OpenAsync();

        const string sql = "UPDATE Problems SET Status = @Status, UpdatedAt = @UpdatedAt WHERE Id = @Id;";
        using var command = new SqliteCommand(sql, connection);
        command.Parameters.Add(new SqliteParameter("@Status", status));
        command.Parameters.Add(new SqliteParameter("@UpdatedAt", DateTime.UtcNow.ToString("yyyy-MM-dd")));
        command.Parameters.Add(new SqliteParameter("@Id", id));

        var rows = await command.ExecuteNonQueryAsync();
        return rows > 0;
    }

    public async Task<bool> AddOccurrenceAsync(string problemId, string date, string note)
    {
        using var connection = (SqliteConnection)_connectionFactory.CreateConnection();
        if (connection.State != ConnectionState.Open)
            await connection.OpenAsync();

        var occId = Guid.NewGuid().ToString("N")[..8];
        const string sql = "INSERT INTO ProblemOccurrences (Id, ProblemId, OccurrenceDate, Note) VALUES (@Id, @ProblemId, @Date, @Note);";
        using var command = new SqliteCommand(sql, connection);
        command.Parameters.Add(new SqliteParameter("@Id", occId));
        command.Parameters.Add(new SqliteParameter("@ProblemId", problemId));
        command.Parameters.Add(new SqliteParameter("@Date", date));
        command.Parameters.Add(new SqliteParameter("@Note", note ?? ""));

        var rows = await command.ExecuteNonQueryAsync();
        return rows > 0;
    }

    public async Task<bool> SetTriedSolutionAsync(string problemId, string solutionKey, string status, string note)
    {
        using var connection = (SqliteConnection)_connectionFactory.CreateConnection();
        if (connection.State != ConnectionState.Open)
            await connection.OpenAsync();

        const string checkSql = "SELECT COUNT(*) FROM ProblemTriedSolutions WHERE ProblemId = @ProblemId AND SolutionKey = @SolutionKey;";
        int exists;
        using (var checkCmd = new SqliteCommand(checkSql, connection))
        {
            checkCmd.Parameters.Add(new SqliteParameter("@ProblemId", problemId));
            checkCmd.Parameters.Add(new SqliteParameter("@SolutionKey", solutionKey));
            exists = Convert.ToInt32(await checkCmd.ExecuteScalarAsync());
        }

        if (exists > 0)
        {
            const string updateSql = "UPDATE ProblemTriedSolutions SET Status = @Status, Note = @Note WHERE ProblemId = @ProblemId AND SolutionKey = @SolutionKey;";
            using var updateCmd = new SqliteCommand(updateSql, connection);
            updateCmd.Parameters.Add(new SqliteParameter("@Status", status));
            updateCmd.Parameters.Add(new SqliteParameter("@Note", note ?? ""));
            updateCmd.Parameters.Add(new SqliteParameter("@ProblemId", problemId));
            updateCmd.Parameters.Add(new SqliteParameter("@SolutionKey", solutionKey));

            var affected = await updateCmd.ExecuteNonQueryAsync();
            return affected > 0;
        }
        else
        {
            const string insertSql = "INSERT INTO ProblemTriedSolutions (Id, ProblemId, SolutionKey, Status, Note) VALUES (@Id, @ProblemId, @SolutionKey, @Status, @Note);";
            using var insertCmd = new SqliteCommand(insertSql, connection);
            insertCmd.Parameters.Add(new SqliteParameter("@Id", Guid.NewGuid().ToString("N")[..8]));
            insertCmd.Parameters.Add(new SqliteParameter("@ProblemId", problemId));
            insertCmd.Parameters.Add(new SqliteParameter("@SolutionKey", solutionKey));
            insertCmd.Parameters.Add(new SqliteParameter("@Status", status));
            insertCmd.Parameters.Add(new SqliteParameter("@Note", note ?? ""));

            var affected = await insertCmd.ExecuteNonQueryAsync();
            return affected > 0;
        }
    }

    public async Task<bool> DeleteAsync(string id)
    {
        using var connection = (SqliteConnection)_connectionFactory.CreateConnection();
        if (connection.State != ConnectionState.Open)
            await connection.OpenAsync();

        const string sql = "DELETE FROM Problems WHERE Id = @Id;";
        using var command = new SqliteCommand(sql, connection);
        command.Parameters.Add(new SqliteParameter("@Id", id));

        var rows = await command.ExecuteNonQueryAsync();
        return rows > 0;
    }
}
