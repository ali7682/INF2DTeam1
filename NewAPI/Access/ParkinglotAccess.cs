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
}
