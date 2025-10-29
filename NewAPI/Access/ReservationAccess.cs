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

        return conn.Query<ReservationModel>(sql, new { reservationId }).FirstOrDefault();
    }

    public static List<ReservationModel> GetReservationsByVehicleId(int vehicleId, string status = "")
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
            WHERE vehicle_id = @vehicleId
        """;

        if (status != null && status != "")
            sql += " AND status = @status";

        return conn.Query<ReservationModel>(sql, new { vehicleId, status }).ToList();
    }

    public static List<ReservationModel> GetReservationsByUserId(int userId, string status = "")
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

        return conn.Query<ReservationModel>(sql, new { userId, status }).ToList();
    }

    public static List<ReservationModel> GetReservationsByParkingLotId(int parkingLotId, string status = "")
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

        return conn.Query<ReservationModel>(sql, new { parkingLotId, status }).ToList();
    }

    // DELETE een reservation met reservation ID
    // Endpoint: /reservations/{rid}
    public static bool DeleteReservationById(int reservationId)
    {
        string cs = _config.GetConnectionString("DefaultConnection")!;
        using MySqlConnection conn = new(cs);
        conn.Open();

        // Eerst de bijbehorende parking lot ID (proberen) te krijgen
        const string getLotSql = """
            SELECT parking_lot_id
            FROM reservations
            WHERE id = @reservationId;
        """;

        // Als de er geen reservation gevonden is met de parking lot ID, dan bestaat het niet
        int? parkingLotId = conn.QueryFirstOrDefault<int?>(getLotSql, new { reservationId });
        if (parkingLotId == null)
            return false;

        // Als het wel bestaat, delete de reservation
        const string deleteSql = """
            DELETE FROM reservations
            WHERE id = @reservationId;
        """;

        int affectedRows = conn.Execute(deleteSql, new { reservationId });

        if (affectedRows > 0)
        {
            // Na de deletion wordt 'reserved' count van parking lot met 1 verminderd
            const string updateLotSql = """
                UPDATE parking_lots
                SET reserved = reserved - 1
                WHERE id = @parkingLotId;
            """;
            conn.Execute(updateLotSql, new { parkingLotId });
            return true;
        }

        return false;
    }

    // POST een nieuwe reservation
    // Endpoint: /reservations
    public static int CreateReservation(ReservationModel reservation)
    {
        string cs = _config.GetConnectionString("DefaultConnection")!;
        using MySqlConnection conn = new(cs);
        conn.Open();

        const string sql = """
            INSERT INTO reservations (user_id, parking_lot_id, vehicle_id, start_time, end_time, status, created_at, cost)
            VALUES (@UserId, @ParkingLotId, @VehicleId, @StartTime, @EndTime, @Status, @CreatedAt, @Cost);
            SELECT LAST_INSERT_ID();
        """;

        int newId = conn.QuerySingle<int>(sql, reservation);

        // Na het toevoegen van de reservation wordt 'reserved' count van parking lot met 1 verhoogd
        const string updateLotSql = """
            UPDATE parking_lots
            SET reserved = reserved + 1
            WHERE id = @ParkingLotId;
        """;
        conn.Execute(updateLotSql, new { reservation.ParkinglotID });

        return newId;
    }
}
