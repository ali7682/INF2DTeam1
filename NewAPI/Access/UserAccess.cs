using Dapper;
using MySqlConnector;

public static class UserAccess
{
    public static readonly string TableName = "users";
    private static IConfiguration _config;

    public static void SetConfig(IConfiguration config) => _config = config;

    private static string Cs => _config.GetConnectionString("DefaultConnection")!;

    private const string SqlSelectBase = """
        SELECT
            id            AS Id,
            username      AS Username,
            password      AS Password,
            name          AS Name,
            email         AS Email,
            phone         AS Phone,
            CAST(role AS CHAR) AS Role,
            created_at    AS CreatedAt,
            birth_year    AS BirthYear,
            active        AS Active
        FROM users
    """;

    private static bool IsValueHashed(string value)
    {
        return value.StartsWith("$2a$") || value.StartsWith("$2b$") || value.StartsWith("$2y$");
    }

    public static async Task<int> CreateUserAsync(UserModel user, CancellationToken ct = default)
    {
        if (!IsValueHashed(user.Password))
            user.Password = BCrypt.Net.BCrypt.HashPassword(user.Password, workFactor: 12);

        var dbUser = new
        {
            Username  = EncryptionService.Encrypt(user.Username),
            Password  = user.Password,
            Name      = user.Name,
            Email     = EncryptionService.Encrypt(user.Email),
            Phone     = user.Phone,
            Role      = user.Role,
            CreatedAt = user.CreatedAt,
            BirthYear = user.BirthYear,
            Active    = user.Active
        };

        const string query = """
            INSERT INTO users
                (username, password, name, email, phone, role, created_at, birth_year, active)
            VALUES
                (@Username, @Password, @Name, @Email, @Phone, @Role, @CreatedAt, @BirthYear, @Active);
            SELECT LAST_INSERT_ID();
        """;

        await using var conn = new MySqlConnection(Cs);
        await conn.OpenAsync(ct);

        var cmd = new CommandDefinition(query, dbUser, cancellationToken: ct, commandTimeout: 5);
        return await conn.ExecuteScalarAsync<int>(cmd);
    }

    public static async Task<UserModel?> GetUserByIdAsync(int userId, CancellationToken ct = default)
    {
        var sql = $"{SqlSelectBase} WHERE id = @userId LIMIT 1;";
        await using var conn = new MySqlConnection(Cs);
        await conn.OpenAsync(ct);

        var cmd = new CommandDefinition(sql, new { userId }, cancellationToken: ct, commandTimeout: 5);
        var user = await conn.QueryFirstOrDefaultAsync<UserModel>(cmd);

        if (user is null)
            return null;

        user.Username = EncryptionService.Decrypt(user.Username);
        user.Email    = EncryptionService.Decrypt(user.Email);

        return user;
    }

    public static async Task<UserModel?> GetUserByUsernameAsync(string userName, CancellationToken ct = default)
    {
        await using var conn = new MySqlConnection(Cs);
        await conn.OpenAsync(ct);

        // Encrypt the input for new users
        var encryptedInput = EncryptionService.Encrypt(userName);

        // Query for either encrypted or plain-text username
        var sql = $"{SqlSelectBase} WHERE username = @EncryptedUsername OR username = @PlainUsername LIMIT 1;";

        var cmd = new CommandDefinition(
            sql,
            new
            {
                EncryptedUsername = encryptedInput,
                PlainUsername     = userName
            },
            cancellationToken: ct,
            commandTimeout: 5
        );

        var user = await conn.QueryFirstOrDefaultAsync<UserModel>(cmd);

        if (user is null)
            return null;

        // Decrypt if needed (only if it’s encrypted)
        try
        {
            user.Username = EncryptionService.Decrypt(user.Username);
            user.Email    = EncryptionService.Decrypt(user.Email);
        }
        catch
        {
            // If decryption fails, it was plain-text — leave as-is
        }

        return user;
    }


    public static async Task<bool> UpdateUserAsync(UserModel user, CancellationToken ct = default)
    {
        const string sql = """
            UPDATE users
            SET
                username   = @Username,
                password   = @Password,
                name       = @Name,
                email      = @Email,
                phone      = @Phone,
                role       = @Role,
                birth_year = @BirthYear,
                active     = @Active
            WHERE id = @Id;
        """;

        await using var conn = new MySqlConnection(Cs);
        await conn.OpenAsync(ct);

        var cmd = new CommandDefinition(sql, new
        {
            user.Id,
            Username = EncryptionService.Encrypt(user.Username),
            user.Password,
            user.Name,
            Email = EncryptionService.Encrypt(user.Email),
            user.Phone,
            user.Role,
            user.BirthYear,
            user.Active
        }, cancellationToken: ct, commandTimeout: 5);

        var rows = await conn.ExecuteAsync(cmd);
        return rows > 0;
    }

    public static async Task<bool> DeleteUserAsync(int userId, CancellationToken ct = default)
    {
        const string sql = "DELETE FROM users WHERE id = @userId;";

        await using var conn = new MySqlConnection(Cs);
        await conn.OpenAsync(ct);

        var cmd = new CommandDefinition(sql, new { userId }, cancellationToken: ct, commandTimeout: 5);
        var rows = await conn.ExecuteAsync(cmd);
        return rows > 0;
    }
}
