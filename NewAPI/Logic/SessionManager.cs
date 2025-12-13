using Dapper;
using MySqlConnector;

public static class SessionManager
{
    public static int SessionCount { get; private set; }
    private static string _connectionString;

    public static void SetConfig(IConfiguration config)
    {
        _connectionString = config.GetConnectionString("DefaultConnection");
    }

    public static async Task AddSession(
        string token,
        int userId,
        CancellationToken ct = default)
    {
        DateTime expiresAt = DateTime.UtcNow.AddHours(12);
        const string query = """
        INSERT INTO api_sessions (token, user_id, expires_at)
        VALUES (@Token, @UserId, @ExpiresAt);
        """;

        await using var conn = new MySqlConnection(_connectionString);
        await conn.OpenAsync(ct);

        var cmd = new CommandDefinition(
            query,
            new { Token = token, UserId = userId, ExpiresAt = expiresAt },
            cancellationToken: ct,
            commandTimeout: 5);

        await conn.ExecuteAsync(cmd);
        SessionCount++;
    }

    public static async Task<int?> GetSession(
        string token,
        CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(token))
            return null;

        const string query = """
        SELECT user_id
        FROM api_sessions
        WHERE token = @Token
        """;

        await using var conn = new MySqlConnection(_connectionString);
        await conn.OpenAsync(ct);

        var cmd = new CommandDefinition(
            query,
            new { Token = token },
            cancellationToken: ct,
            commandTimeout: 5);

        return await conn.ExecuteScalarAsync<int?>(cmd);
    }

    public static async Task<UserModel?> GetUserFromSession(
        string token,
        CancellationToken ct = default)
    {
        var userId = await GetSession(token, ct);
        if (userId == null)
            return null;

        return await UserAccess.GetUserByIdAsync(userId.Value, ct);
    }

    public static async Task<bool> RemoveSession(
        string token,
        CancellationToken ct = default)
    {
        const string query = """
        DELETE FROM api_sessions
        WHERE token = @Token;
        """;

        await using var conn = new MySqlConnection(_connectionString);
        await conn.OpenAsync(ct);

        var cmd = new CommandDefinition(
            query,
            new { Token = token },
            cancellationToken: ct,
            commandTimeout: 5);

        SessionCount--;

        return await conn.ExecuteAsync(cmd) > 0;
    }

    public static async Task<bool> DoesSessionExist(
        string token,
        CancellationToken ct = default)
    {
        const string query = """
        SELECT COUNT(*)
        FROM api_sessions
        WHERE token = @Token
        """;

        await using var conn = new MySqlConnection(_connectionString);
        await conn.OpenAsync(ct);

        var cmd = new CommandDefinition(
            query,
            new { Token = token },
            cancellationToken: ct,
            commandTimeout: 5);

        return await conn.ExecuteScalarAsync<int>(cmd) > 0;
    }
}
