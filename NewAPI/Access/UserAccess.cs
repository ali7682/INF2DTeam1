using Dapper;
using MySql.Data.MySqlClient;

public static class UserAccess
{
    public static readonly string TableName = "users";
    private static IConfiguration _config;

    public static void SetConfig(IConfiguration config)
    {
        _config = config;
    }

    public static int CreateUser(UserModel user)
    {
        string cs = _config.GetConnectionString("DefaultConnection")!;
        using MySqlConnection conn = new(cs);
        conn.Open();

        const string query = """
        INSERT INTO users
            (username, password, name, email, phone, role, created_at, birth_year, active)
        VALUES
            (@Username, @Password, @Name, @Email, @Phone, @Role, @CreatedAt, @BirthYear, @Active);
        SELECT LAST_INSERT_ID();
        """;

        int newId = conn.ExecuteScalar<int>(query, user);

        return newId;
    }

    public static UserModel GetUserById(int userId)
    {
        string cs = _config.GetConnectionString("DefaultConnection")!;
        using MySqlConnection conn = new(cs);
        conn.Open();

        const string sql = """
        SELECT 
                id           AS Id,
                username     AS Username,
                password     AS Password,
                name         AS Name,
                email        AS Email,
                phone        AS Phone,
                CAST(role AS CHAR) AS Role,
                created_at   AS CreatedAt,
                birth_year   AS BirthYear,
                active       AS Active
            FROM users
            WHERE id = @userId
            LIMIT 1;
        """;

        return conn.Query<UserModel>(sql, new { userId }).First();
    }

    public static UserModel GetUserByUsername(string userName)
    {
        string cs = _config.GetConnectionString("DefaultConnection")!;
        using MySqlConnection conn = new(cs);
        conn.Open();

        const string sql = """
        SELECT 
                id           AS Id,
                username     AS Username,
                password     AS Password,
                name         AS Name,
                email        AS Email,
                phone        AS Phone,
                CAST(role AS CHAR) AS Role,
                created_at   AS CreatedAt,
                birth_year   AS BirthYear,
                active       AS Active
            FROM users
            WHERE username = @userName
            LIMIT 1;
        """;

        return conn.Query<UserModel>(sql, new { userName }).First();
    }

    public static bool UpdateUser(UserModel user)
    {
        string cs = _config.GetConnectionString("DefaultConnection")!;
        using MySqlConnection conn = new(cs);
        conn.Open();

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

        int affectedRows = conn.Execute(sql, new
        {
            user.Id,
            user.Username,
            user.Password,
            user.Name,
            user.Email,
            user.Phone,
            user.Role,
            user.BirthYear,
            user.Active
        });

        return affectedRows > 0;
    }
}