using MySql.Data.MySqlClient;
using System.Data;

public interface ISqlConnectionFactory
{
    Task<IDbConnection> CreateOpenConnectionAsync(CancellationToken ct = default);
}

public class SqlConnectionFactory : ISqlConnectionFactory
{
    private readonly string _cs;
    public SqlConnectionFactory(IConfiguration config) => _cs = config.GetConnectionString("Default")!;

    public async Task<IDbConnection> CreateOpenConnectionAsync(CancellationToken ct = default)
    {
        var conn = new MySqlConnection(_cs);
        await conn.OpenAsync(ct);
        return conn;
    }
}