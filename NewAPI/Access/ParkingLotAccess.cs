using Dapper;
using MySql.Data.MySqlClient;

public static class ParkingLotAccess
{
    public static readonly string TableName = "parking_lots";
    private static IConfiguration _config;

    public static void SetConfig(IConfiguration config)
    {
        _config = config;
    }

    // GET alle parking lots
    // Endpoint: /parking-lots
    public static List<ParkingLotModel> GetAllParkingLots()
    {
        string cs = _config.GetConnectionString("DefaultConnection")!;
        using MySqlConnection conn = new(cs);
        conn.Open();

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

        List<ParkingLotModel> result = conn.Query<ParkingLotModel>(sql).ToList();
        return result;
    }

    // GET een parking lot met parking lot ID
    // Endpoint: /parking-lots/{lid}
    public static ParkingLotModel? GetParkingLotById(int parkingLotId)
    {
        string cs = _config.GetConnectionString("DefaultConnection")!;
        using MySqlConnection conn = new(cs);
        conn.Open();

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

        ParkingLotModel? parkingLot = conn.QueryFirstOrDefault<ParkingLotModel>(sql, new { parkingLotId });
        return parkingLot;
    }

    // GET alle parking sessions voor een parking lot met parking lot ID
    // Endpoint: /parking-lots/{lid}/sessions
    public static List<ParkingSessionModel> GetParkingSessionsByLotId(int parkingLotId)
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
            WHERE parking_lot_id = @parkingLotId;
        """;

        List<ParkingSessionModel> sessions = conn.Query<ParkingSessionModel>(sql, new { parkingLotId }).ToList();
        return sessions;
    }

    // GET een parking session met parking lot ID en parking session ID
    // Endpoint: /parking-lots/{lid}/sessions/{sid}
    public static ParkingSessionModel? GetParkingSessionById(int parkingLotId, int sessionId)
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
            WHERE parking_lot_id = @parkingLotId
            AND id = @sessionId
            LIMIT 1;
        """;

        ParkingSessionModel? session = conn.QueryFirstOrDefault<ParkingSessionModel>(sql, new { parkingLotId, sessionId });
        return session;
    }

    // DELETE een parking lot met bijbehorende parking sessions met parking lot ID
    // Endpoint: /parking-lots/{lid}
    public static bool DeleteParkingLotById(int parkingLotId)
    {
        string cs = _config.GetConnectionString("DefaultConnection")!;
        using MySqlConnection conn = new(cs);
        conn.Open();

        // Eerst delete het gerelateerde parking sessions
        const string deleteSessions = """
            DELETE FROM parking_sessions
            WHERE parking_lot_id = @parkingLotId;
        """;
        conn.Execute(deleteSessions, new { parkingLotId });

        // Daarna delete het de parking lot zelf
        const string sql = """
            DELETE FROM parking_lots
            WHERE id = @parkingLotId;
        """;

        int affectedRows = conn.Execute(sql, new { parkingLotId });
        return affectedRows > 0;
    }

    // DELETE een specifieke parking session van een parking lot met parking lot ID
    // Endpoint: /parking-lots/{lid}/sessions/{sid}
    public static bool DeleteParkingSessionById(int parkingLotId, int sessionId)
    {
        string cs = _config.GetConnectionString("DefaultConnection")!;
        using MySqlConnection conn = new(cs);
        conn.Open();

        const string sql = """
            DELETE FROM parking_sessions
            WHERE parking_lot_id = @parkingLotId
            AND id = @sessionId;
        """;

        int affectedRows = conn.Execute(sql, new { parkingLotId, sessionId });
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
