using System.Data;

namespace PulseMvc.Data;

/// <summary>
/// Factory interface for creating database connections (Dependency Injection)
/// </summary>
public interface IDbConnectionFactory
{
    IDbConnection CreateConnection();
}
