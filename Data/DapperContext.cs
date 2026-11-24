using System.Data;

namespace PulseMvc.Data;

public class DapperContext
{
    private readonly IDbConnectionFactory _connectionFactory;

    public DapperContext(IDbConnectionFactory connectionFactory)
    {
        _connectionFactory = connectionFactory;
    }

    public IDbConnection CreateConnection() => _connectionFactory.CreateConnection();
}
