using Dapper;
using MySql.Data.MySqlClient;

public static class ParkingLotAccess
{
    public static readonly string TableName = "parking_lots";
    private static IConfiguration _config;

    public static void SetConfig(IConfiguration config) => _config = config;
    private static string Cs => _config.GetConnectionString("DefaultConnection")!;
    private const string SqlSelectBase = """
        SELECT 
            id              AS ID,
            name            AS Name,
            location        AS Location,
            address         AS Address,
            capacity        AS Capacity,
            reserved        AS Reserved,
            tariff          AS Tariff,
            daytariff       AS DayTariff,
            created_at      AS CreatedAt
        FROM parking_lots
    """;

    // GET alle parking lots
    // Endpoint: /parking-lots
    public static async Task<List<ParkingLotModel>> GetAllParkingLotsAsync(CancellationToken ct = default)
    {
        await using MySqlConnection conn = new(Cs);
        await conn.OpenAsync(ct);

        const string sql = """
            SELECT 
                id              AS ID,
                name            AS Name,
                location        AS Location,
                address         AS Address,
                capacity        AS Capacity,
                reserved        AS Reserved,
                tariff          AS Tariff,
                daytariff       AS DayTariff,
                created_at      AS CreatedAt
            FROM parking_lots;
        """;

        var result = await conn.QueryAsync<ParkingLotModel>(sql);
        return result.AsList();
    }

    // GET een parking lot met parking lot ID
    // Endpoint: /parking-lots/{lid}
    public static async Task<ParkingLotModel?> GetParkingLotByIdAsync(int parkingLotId, CancellationToken ct = default)
    {
        await using MySqlConnection conn = new(Cs);
        await conn.OpenAsync(ct);

        const string sql = """
            SELECT 
                id              AS ID,
                name            AS Name,
                location        AS Location,
                address         AS Address,
                capacity        AS Capacity,
                reserved        AS Reserved,
                tariff          AS Tariff,
                daytariff       AS DayTariff,
                created_at      AS CreatedAt
            FROM parking_lots
            WHERE id = @parkingLotId
            LIMIT 1;
        """;

        var parkingLot = await conn.QueryFirstOrDefaultAsync<ParkingLotModel>(sql, new { parkingLotId });
        return parkingLot;
    }

    // GET alle parking sessions voor een parking lot met parking lot ID
    // Endpoint: /parking-lots/{lid}/sessions
    public static async Task<List<ParkingSessionModel>> GetParkingSessionsByLotIdAsync(int parkingLotId, CancellationToken ct = default)
    {
        await using MySqlConnection conn = new(Cs);
        await conn.OpenAsync(ct);

        const string sql = """
            SELECT 
                id                  AS ID,
                parking_lot_id      AS ParkingLotID,
                licenseplate        AS LicensePlate,
                started             AS Started,
                stopped             AS Stopped,
                user                AS User,
                duration_minutes    AS DurationMinutes,
                cost                AS Cost,
                payment_status      AS PaymentStatus
            FROM parking_sessions
            WHERE parking_lot_id = @parkingLotId;
        """;

        var sessions = await conn.QueryAsync<ParkingSessionModel>(sql, new { parkingLotId });
        return sessions.AsList();
    }

    // GET een parking session met parking lot ID en parking session ID
    // Endpoint: /parking-lots/{lid}/sessions/{sid}
    public static async Task<ParkingSessionModel?> GetParkingSessionByIdAsync(int parkingLotId, int sessionId, CancellationToken ct = default)
    {
        await using MySqlConnection conn = new(Cs);
        await conn.OpenAsync(ct);

        const string sql = """
            SELECT 
                id                  AS ID,
                parking_lot_id      AS ParkingLotID,
                licenseplate        AS LicensePlate,
                started             AS Started,
                stopped             AS Stopped,
                user                AS User,
                duration_minutes    AS DurationMinutes,
                cost                AS Cost,
                payment_status      AS PaymentStatus
            FROM parking_sessions
            WHERE parking_lot_id = @parkingLotId
            AND id = @sessionId
            LIMIT 1;
        """;

        var session = await conn.QueryFirstOrDefaultAsync<ParkingSessionModel>(sql, new { parkingLotId, sessionId });
        return session;
    }

    // DELETE een parking lot met bijbehorende parking sessions met parking lot ID
    // Endpoint: /parking-lots/{lid}
    public static async Task<bool> DeleteParkingLotByIdAsync(int parkingLotId, CancellationToken ct = default)
    {
        await using MySqlConnection conn = new(Cs);
        await conn.OpenAsync(ct);

        // Eerst delete het gerelateerde parking sessions
        const string deleteSessions = """
            DELETE FROM parking_sessions
            WHERE parking_lot_id = @parkingLotId;
        """;
        await conn.ExecuteAsync(deleteSessions, new { parkingLotId });

        // Daarna delete het de parking lot zelf
        const string sql = """
            DELETE FROM parking_lots
            WHERE id = @parkingLotId;
        """;

        int affectedRows = await conn.ExecuteAsync(sql, new { parkingLotId });
        return affectedRows > 0;
    }

    // DELETE een specifieke parking session van een parking lot met parking lot ID
    // Endpoint: /parking-lots/{lid}/sessions/{sid}
    public static async Task<bool> DeleteParkingSessionByIdAsync(int parkingLotId, int sessionId, CancellationToken ct = default)
    {
        await using MySqlConnection conn = new(Cs);
        await conn.OpenAsync(ct);

        const string sql = """
            DELETE FROM parking_sessions
            WHERE parking_lot_id = @parkingLotId
            AND id = @sessionId;
        """;

        int affectedRows = await conn.ExecuteAsync(sql, new { parkingLotId, sessionId });
        return affectedRows > 0;
    }

    // UPDATE een parking lot met parking lot ID
    // Endpoint: /parking-lots/{lid}
    public static async Task<bool> UpdateParkingLotByIdAsync(int parkingLotId, ParkingLotModel model, CancellationToken ct = default)
    {
        const string sql = """
            UPDATE parking_lots
            SET
                name = @Name,
                location = @Location,
                address = @Address,
                capacity = @Capacity,
                reserved = @Reserved,
                tariff = @Tariff,
                daytariff = @DayTariff
            WHERE id = @parkingLotId
        """;

        await using var conn = new MySqlConnection(Cs);
        await conn.OpenAsync(ct);

        var cmd = new CommandDefinition(sql, new
        {
            model.Name,
            model.Location,
            model.Address,
            model.Capacity,
            model.Reserved,
            model.Tariff,
            model.DayTariff,
            parkingLotId
        }, cancellationToken: ct, commandTimeout: 5);

        var affectedRows = await conn.ExecuteAsync(cmd);
        return affectedRows > 0;
    }

    // POST een nieuwe parking lot
    // Endpoint: /parking-lots
    public static int CreateParkinglot(ParkingLotModel parkinglot)
    {
        string cs = _config.GetConnectionString("DefaultConnection")!;
        using MySqlConnection conn = new(cs);
        conn.Open();

        const string query = """
        INSERT INTO parking_lots
            (name, location, address, capacity, reserved, tariff, daytariff)
        VALUES
            (@Name, @Location, @Address, @Capacity, @Reserved, @Tariff, @DayTariff);
        SELECT LAST_INSERT_ID();
        """;

        int newId = conn.ExecuteScalar<int>(query, parkinglot);

        return newId;
    }

    // POST een nieuwe parking session
    // Endpoint: /parking-lots/{lid}/sessions/start
    public static int CreateParkingsession(ParkingSessionModel session)
    {
        string cs = _config.GetConnectionString("DefaultConnection")!;
        using MySqlConnection conn = new(cs);
        conn.Open();

        const string query = """
        INSERT INTO parking_sessions
            (parking_lot_id, licenseplate, started, user)
        VALUES
            (@ParkingLotID, @LicensePlate, @Started, @User);
        SELECT LAST_INSERT_ID();
        """;

        int newId = conn.ExecuteScalar<int>(query, session);

        return newId;
    }

    // PUT een parking session
    // Endpoint: /parking-lots/{lid}/sessions/stop
    public static bool UpdateParkingSession(ParkingSessionModel session)
    {
        string cs = _config.GetConnectionString("DefaultConnection")!;
        using MySqlConnection conn = new(cs);
        conn.Open();

        const string sql = """
        UPDATE parking_sessions
        SET
            stopped = @Stopped,
            duration_minutes = @DurationMinutes,
            cost = @Cost,
            payment_status = @PaymentStatus
        WHERE id = @ID AND parking_lot_id = @ParkingLotID;
        """;

        int affectedRows = conn.Execute(sql, session);
        return affectedRows > 0;
    }

    // GET een parking lot met licenseplate
    // Endpoint: /parking-lots/{lid}/sessions/start
    public static List<ParkingSessionModel> FindParkingSessionsByLicenseplate(string licenseplate)
    {
        string cs = _config.GetConnectionString("DefaultConnection")!;
        using MySqlConnection conn = new(cs);
        conn.Open();

        const string sql = """
        SELECT 
                id                  AS ID,
                parking_lot_id      AS ParkingLotID,
                licenseplate        AS LicensePlate,
                started             AS Started,
                stopped             AS Stopped,
                user                AS User,
                duration_minutes    AS DurationMinutes,
                cost                AS Cost,
                payment_status      AS PaymentStatus
            FROM parking_sessions
            WHERE licenseplate = @licenseplate;
        """;

        List<ParkingSessionModel> sessions = conn.Query<ParkingSessionModel>(sql, new { licenseplate }).ToList();
        return sessions;
    }
}
