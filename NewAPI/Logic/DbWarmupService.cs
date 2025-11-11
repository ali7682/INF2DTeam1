using Dapper;
using MySqlConnector;

public class DbWarmupService : IHostedService
{
    private readonly IConfiguration _cfg;
    public DbWarmupService(IConfiguration cfg) => _cfg = cfg;

    private static async Task Warm(string cs, CancellationToken ct)
    {
        await using var c = new MySqlConnection(cs);
        await c.OpenAsync(ct);
        await c.ExecuteScalarAsync("SELECT 1;", ct);
    }

    public async Task StartAsync(CancellationToken ct)
    {
        var cs = _cfg.GetConnectionString("DefaultConnection")!;
        var tasks = Enumerable.Range(0, 50).Select(_ => Warm(cs, ct));
        await Task.WhenAll(tasks);
        await using var conn = new MySqlConnection(cs);
        await conn.OpenAsync(ct);
        await conn.ExecuteScalarAsync("SELECT 1 FROM users LIMIT 1;", ct);
    }

    public Task StopAsync(CancellationToken ct) => Task.CompletedTask;
}
