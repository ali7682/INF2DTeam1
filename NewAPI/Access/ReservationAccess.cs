using Dapper;
using MySql.Data.MySqlClient;

public static class ReservationAccess
{
    public static readonly string TableName = "reservations";
    private static IConfiguration _config;

    public static void SetConfig(IConfiguration config)
    {
        _config = config;
    }

    public static ReservationModel GetReservationById(int reservationId)
    {
        string cs = _config.GetConnectionString("DefaultConnection")!;
        using MySqlConnection conn = new(cs);
        conn.Open();

        const string sql = """
            SELECT 
                id              AS Id,
                user_id         AS UserId,
                parking_lot_id  AS ParkingLotId,
                vehicle_id      AS VehicleId,
                start_time      AS StartTime,
                end_time        AS EndTime,
                status          AS Status,
                created_at      AS CreatedAt,
                cost            AS Cost
            FROM reservations
            WHERE id = @reservationId;
        """;

        return conn.Query<ReservationModel>(sql, new { reservationId }).First();
    }

    public static List<ReservationModel> GetReservationsByVehicleId(int vehicleId, string status)
    {
        string cs = _config.GetConnectionString("DefaultConnection")!;
        using MySqlConnection conn = new(cs);
        conn.Open();

        string sql = """
            SELECT 
                id              AS Id,
                user_id         AS UserId,
                parking_lot_id  AS ParkingLotId,
                vehicle_id      AS VehicleId,
                start_time      AS StartTime,
                end_time        AS EndTime,
                status          AS Status,
                created_at      AS CreatedAt,
                cost            AS Cost
            FROM reservations
            WHERE vehicle_id = @vehicleId;
        """;

        if (status != null && status != "")
            sql += " AND status = @status";

        return conn.Query<ReservationModel>(sql, new { vehicleId }).ToList();
    }

    public static List<ReservationModel> GetReservationsByUserId(int userId, string status)
    {
        string cs = _config.GetConnectionString("DefaultConnection")!;
        using MySqlConnection conn = new(cs);
        conn.Open();

        string sql = """
            SELECT 
                id              AS Id,
                user_id         AS UserId,
                parking_lot_id  AS ParkingLotId,
                vehicle_id      AS VehicleId,
                start_time      AS StartTime,
                end_time        AS EndTime,
                status          AS Status,
                created_at      AS CreatedAt,
                cost            AS Cost
            FROM reservations
            WHERE user_id = @userId;
        """;

        if (status != null && status != "")
            sql += " AND status = @status";

        return conn.Query<ReservationModel>(sql, new { userId }).ToList();
    }

    public static List<ReservationModel> GetReservationsByParkingLotId(int parkingLotId, string status)
    {
        string cs = _config.GetConnectionString("DefaultConnection")!;
        using MySqlConnection conn = new(cs);
        conn.Open();

        string sql = """
            SELECT 
                id              AS Id,
                user_id         AS UserId,
                parking_lot_id  AS ParkingLotId,
                vehicle_id      AS VehicleId,
                start_time      AS StartTime,
                end_time        AS EndTime,
                status          AS Status,
                created_at      AS CreatedAt,
                cost            AS Cost
            FROM reservations
            WHERE parking_lot_id = @parkingLotId;
        """;

        if (status != null && status != "")
            sql += " AND status = @status";

        return conn.Query<ReservationModel>(sql, new { parkingLotId }).ToList();
    }
}