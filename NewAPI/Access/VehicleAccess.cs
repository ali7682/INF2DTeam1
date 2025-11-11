using Dapper;
using MySql.Data.MySqlClient;

public static class VehicleAccess
{
    public static readonly string TableName = "vehicles";
    private static IConfiguration _config;

    public static void SetConfig(IConfiguration config) => _config = config;

    private static string Cs => _config.GetConnectionString("DefaultConnection")!;

    public static async Task<int> CreateVehicleAsync(VehicleModel vehicle)
    {
        string cs = _config.GetConnectionString("DefaultConnection")!;

        // Async using ensures proper disposal
        await using var conn = new MySqlConnection(cs);
        await conn.OpenAsync();

        const string query = """
        INSERT INTO vehicles
            (user_id, license_plate, make, model, color, year, created_at)
        VALUES
            (@UserID, @LicensePlate, @Make, @Model, @Color, @Year, @CreatedAt);
        SELECT LAST_INSERT_ID();
    """;

        // Use ExecuteScalarAsync for async insertion
        int newId = await conn.ExecuteScalarAsync<int>(query, vehicle);

        return newId;
    }


    public static async Task<VehicleModel?> GetVehicleByIdAsync(int vehicleId, CancellationToken ct = default)
    {
        string cs = _config.GetConnectionString("DefaultConnection")!;
        await using var conn = new MySqlConnection(cs);
        await conn.OpenAsync(ct);

        const string sql = """
        SELECT 
                id AS ID,
                user_id AS UserID,
                license_plate AS LicensePlate,
                make AS Make,
                model AS Model,
                color AS Color,
                year AS Year,
                created_at AS CreatedAt
            FROM vehicles
            WHERE id = @vehicleId
            LIMIT 1;
        """;

        var cmd = new CommandDefinition(sql, new { vehicleId }, cancellationToken: ct, commandTimeout: 5);
        return await conn.QueryFirstOrDefaultAsync<VehicleModel>(cmd);
    }

    public static async Task<VehicleModel?> GetVehicleByLicensePlateAsync(string licensePlate)
    {
        string cs = _config.GetConnectionString("DefaultConnection")!;
        await using MySqlConnection conn = new(cs);
        await conn.OpenAsync();

        const string sql = """
        SELECT 
            id AS ID,
            user_id AS UserID,
            license_plate AS LicensePlate,
            make AS Make,
            model AS Model,
            color AS Color,
            year AS Year,
            created_at AS CreatedAt
        FROM vehicles
        WHERE license_plate = @licensePlate
        LIMIT 1;
    """;

        // Use Dapper's async version to fetch data
        return await conn.QueryFirstOrDefaultAsync<VehicleModel>(sql, new { licensePlate });
    }


    public static bool UpdateVehicle(VehicleModel model)
    {
        string cs = _config.GetConnectionString("DefaultConnection")!;
        using MySqlConnection conn = new(cs);
        conn.Open();

        const string sql = """
            UPDATE vehicles
            SET
                user_id   = @UserID,
                license_plate       = @LicensePlate,
                make      = @Make,
                model      = @Model,
                color       = @Color,
                year = @Year
            WHERE id = @Id;
        """;

        int affectedRows = conn.Execute(sql, new
        {
            model.UserID,
            model.LicensePlate,
            model.Make,
            model.Model,
            model.Color,
            model.Year,
            model.ID
        });

        return affectedRows > 0;
    }

    // DELETE een vehicle met vehicle ID
    // Endpoint: /vehicles/{vid}
    public static async Task<bool> DeleteVehicleByIdAsync(int vehicleId, CancellationToken ct = default)
    {
        await using var conn = new MySqlConnection(Cs);
        await conn.OpenAsync(ct);

        const string sql = """
            DELETE FROM vehicles
            WHERE id = @vehicleId;
        """;

        var cmd = new CommandDefinition(sql, new { vehicleId }, cancellationToken: ct, commandTimeout: 5);
        int affectedRows = await conn.ExecuteAsync(cmd);

        return affectedRows > 0;
    }
}
