using Dapper;
using MySql.Data.MySqlClient;

public static class ReservationAccess
{
    public static readonly string TableName = "reservations";
    private static IConfiguration _config;

    public static void SetConfig(IConfiguration config) => _config = config;
    private static string Cs => _config.GetConnectionString("DefaultConnection")!;
    private const string SqlSelectBase = """
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
    """;

    public static async Task<ReservationModel?> GetReservationByIdAsync(int reservationId, CancellationToken ct = default)
    {
        await using var conn = new MySqlConnection(Cs);
        await conn.OpenAsync(ct);

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

        var cmd = new CommandDefinition(sql, new { reservationId }, cancellationToken: ct, commandTimeout: 5);
        return await conn.QueryFirstOrDefaultAsync<ReservationModel>(cmd);
    }

    public static async Task<List<ReservationModel>> GetReservationsByVehicleIdAsync(int vehicleId, string status = "", CancellationToken ct = default)
    {
        string cs = _config.GetConnectionString("DefaultConnection")!;
        await using var conn = new MySqlConnection(cs);
        await conn.OpenAsync(ct);

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

        if (!string.IsNullOrEmpty(status))
            sql += " AND status = @status";

        var cmd = new CommandDefinition(sql, new { vehicleId, status }, cancellationToken: ct);
        var reservations = await conn.QueryAsync<ReservationModel>(cmd);
        return reservations.ToList();
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
    public static async Task<bool> DeleteReservationByIdAsync(int reservationId, CancellationToken ct = default)
    {
        await using var conn = new MySqlConnection(Cs);
        await conn.OpenAsync(ct);

        // Eerst de bijbehorende parking lot ID (proberen) te krijgen
        const string getLotSql = """
            SELECT parking_lot_id
            FROM reservations
            WHERE id = @reservationId;
        """;

        // Als de er geen reservation gevonden is met de parking lot ID, dan bestaat het niet
        var cmdGetLot = new CommandDefinition(getLotSql, new { reservationId }, cancellationToken: ct, commandTimeout: 5);
        int? parkingLotId = await conn.QueryFirstOrDefaultAsync<int?>(cmdGetLot);

        if (parkingLotId == null)
            return false;

        // Als het wel bestaat, delete de reservation
        const string deleteSql = """
            DELETE FROM reservations
            WHERE id = @reservationId;
        """;

        var cmdDelete = new CommandDefinition(deleteSql, new { reservationId }, cancellationToken: ct, commandTimeout: 5);
        int affectedRows = await conn.ExecuteAsync(cmdDelete);

        if (affectedRows > 0)
        {
            // Na de deletion wordt 'reserved' count van parking lot met 1 verminderd
            const string updateLotSql = """
                UPDATE parking_lots
                SET reserved = reserved - 1
                WHERE id = @parkingLotId;
            """;

            var cmdUpdate = new CommandDefinition(updateLotSql, new { parkingLotId }, cancellationToken: ct, commandTimeout: 5);
            await conn.ExecuteAsync(cmdUpdate);
            return true;
        }

        return false;
    }

    // UPDATE een reservation met reservation ID
    // Endpoint: /reservations/{rid}
    public static async Task<bool> UpdateReservationByIdAsync(int reservationId, ReservationModel updatedReservation, CancellationToken ct = default)
    {
        const string sql = """
                    UPDATE reservations
                    SET 
                        user_id         = @UserID,
                        parking_lot_id  = @ParkingLotID,
                        vehicle_id      = @VehicleID,
                        start_time      = @StartTime,
                        end_time        = @EndTime,
                        status          = @Status,
                        cost            = @Cost
                    WHERE id = @Id;
                """;

        await using var conn = new MySqlConnection(Cs);
        await conn.OpenAsync(ct);

        var cmd = new CommandDefinition(sql, new
        {
            Id = reservationId,
            updatedReservation.UserID,
            updatedReservation.ParkinglotID,
            updatedReservation.VehicleID,
            updatedReservation.StartTime,
            updatedReservation.EndTime,
            updatedReservation.Status,
            updatedReservation.Cost,
        }, cancellationToken: ct, commandTimeout: 5);

        var affectedRows = await conn.ExecuteAsync(cmd);
        return affectedRows > 0;
    }

    // POST een nieuwe reservation
    // Endpoint: /reservations
    public static async Task<int> CreateReservationAsync(ReservationModel reservation, CancellationToken ct = default)
    {
        const string query = """
        INSERT INTO reservations 
            (user_id, parking_lot_id, vehicle_id, start_time, end_time, status, created_at, cost)
        VALUES 
            (@UserID, @ParkinglotID, @VehicleID, @StartTime, @EndTime, @Status, @CreatedAt, @Cost);
        SELECT LAST_INSERT_ID();
    """;

        await using var conn = new MySqlConnection(Cs);
        await conn.OpenAsync(ct);

        var insertCmd = new CommandDefinition(query, reservation, cancellationToken: ct, commandTimeout: 5);
        int reservationId = await conn.ExecuteScalarAsync<int>(insertCmd);


        // Na het toevoegen van de reservation wordt 'reserved' count van parking lot met 1 verhoogd
        const string updateLotQuery = """
            UPDATE parking_lots
            SET reserved = reserved + 1
            WHERE id = @ParkinglotID;
        """;

        var updateCmd = new CommandDefinition(updateLotQuery, new { reservation.ParkinglotID }, cancellationToken: ct, commandTimeout: 5);
        await conn.ExecuteAsync(updateCmd);

        return reservationId;

    }
}
