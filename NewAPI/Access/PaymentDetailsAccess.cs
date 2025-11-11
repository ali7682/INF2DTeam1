using Dapper;
using MySql.Data.MySqlClient;

public static class PaymentDetailsAccess
{
    public static readonly string TableName = "payment_details";
    private static IConfiguration _config;

    public static void SetConfig(IConfiguration config) => _config = config;

    private static string Cs => _config.GetConnectionString("DefaultConnection")!;

    private const string SqlSelectBase = """
        SELECT
            transaction_id     AS TransactionId,
            amount             AS Amount,
            date               AS Date,
            method             AS Method,
            issuer             AS Issuer,
            bank               AS Bank
        FROM payment_details
    """;

    public static int CreatePaymentDetails(PaymentDetailsModel paymentdetails)
    {
        string cs = _config.GetConnectionString("DefaultConnection")!;
        using MySqlConnection conn = new(cs);
        conn.Open();

        const string query = """
        INSERT INTO payments
            (transaction_id, amount, date, method, issuer, bank)
        VALUES
            (@TransactionId, @Amount, @Date, @Method, @Issuer, @Bank);
        SELECT LAST_INSERT_ID();
        """;

        int newId = conn.ExecuteScalar<int>(query, paymentdetails);

        return newId;
    }

    // GET alle payment details
    // Endpoint: /paymentdetails
    public static async Task<List<PaymentDetailsModel?>> GetAllPaymentDetailsAsync(CancellationToken ct = default)
    {
        var sql = $"{SqlSelectBase};";
        await using var conn = new MySqlConnection(Cs);
        await conn.OpenAsync(ct);

        var cmd = new CommandDefinition(sql, cancellationToken: ct, commandTimeout: 5);
        var result = await conn.QueryAsync<PaymentDetailsModel>(cmd);
        return result.ToList();
    }

    public static PaymentModel GetPaymentByTransactionId(int transaction_Id)
    {
        string cs = _config.GetConnectionString("DefaultConnection")!;
        using MySqlConnection conn = new(cs);
        conn.Open();

        const string sql = """
        SELECT 
                transaction_id           AS TransactionId,
                amount     AS Amount,
                initiator     AS Initiator,
                created_at         AS Created_at,
                completed        AS Completed,
                hash        AS Hash
            FROM payments
            WHERE transaction_id = @transaction_Id
            LIMIT 1;
        """;

        return conn.Query<PaymentModel>(sql, new { transaction_Id }).First();
    }

    public static PaymentModel GetUserByInitiator(string initiator)
    {
        string cs = _config.GetConnectionString("DefaultConnection")!;
        using MySqlConnection conn = new(cs);
        conn.Open();

        const string sql = """
        SELECT 
                transaction_id           AS TransactionId,
                amount     AS Amount,
                initiator     AS Initiator,
                created_at         AS Created_at,
                completed        AS Completed,
                hash        AS Hash
            FROM payments
            WHERE initiator = @initiator
            LIMIT 1;
        """;

        return conn.Query<PaymentModel>(sql, new { initiator }).First();
    }

    public static bool UpdatePayment(PaymentModel payment)
    {
        string cs = _config.GetConnectionString("DefaultConnection")!;
        using MySqlConnection conn = new(cs);
        conn.Open();

        const string sql = """
            UPDATE users
            SET
                transaction_id   = @TransactionId,
                amount   = @Amount,
                initiator       = @Initiator,
                created_at      = @Created_at,
                completed      = @Completed,
                hash       = @Hash
            WHERE transaction_id = @TransactionId;
        """;

        int affectedRows = conn.Execute(sql, new
        {
            payment.TransactionId,
            payment.Amount,
            payment.Initiator,
            payment.Created_at,
            payment.Completed,
            payment.Hash
        });

        return affectedRows > 0;
    }
}