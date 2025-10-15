using Dapper;
using MySql.Data.MySqlClient;

public static class VehicleAccess
{
    public static readonly string TableName = "vehicles";
    private static IConfiguration _config;

    public static void SetConfig(IConfiguration config)
    {
        _config = config;
    }

    public static int CreateVehicle(VehicleModel vehicle)
    {
        string cs = _config.GetConnectionString("DefaultConnection")!;
        using MySqlConnection conn = new(cs);
        conn.Open();

        const string query = """
        INSERT INTO vehicles
            (user_id, license_plate, make, model, color, year, created_at)
        VALUES
            (@UserID, @LicensePlate, @Make, @Model, @Color, @Year, @CreatedAt);
        SELECT LAST_INSERT_ID();
        """;

        int newId = conn.ExecuteScalar<int>(query, vehicle);

        return newId;
    }

    public static VehicleModel GetVehicleById(int vehicleId)
    {
        string cs = _config.GetConnectionString("DefaultConnection")!;
        using MySqlConnection conn = new(cs);
        conn.Open();

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

        return conn.Query<VehicleModel>(sql, new { vehicleId }).First();
    }

    public static VehicleModel GetVehicleByLicensePlate(string licensePlate)
    {
        string cs = _config.GetConnectionString("DefaultConnection")!;
        using MySqlConnection conn = new(cs);
        conn.Open();

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

        return conn.Query<VehicleModel>(sql, new { licensePlate }).First();
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
}