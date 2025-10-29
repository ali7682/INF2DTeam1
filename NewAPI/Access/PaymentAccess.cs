using Dapper;
using MySql.Data.MySqlClient;

public static class PaymentAccess
{
    public static readonly string TableName = "payments";
    private static IConfiguration _config;

    public static void SetConfig(IConfiguration config)
    {
        _config = config;
    }

    public static int CreatePayment(PaymentModel payment)
    {
        string cs = _config.GetConnectionString("DefaultConnection")!;
        using MySqlConnection conn = new(cs);
        conn.Open();

        const string query = """
        INSERT INTO payments
            (amount, initiator, created_at, completed, hash)
        VALUES
            (@Amount, @Initiator, @Created_at, @Completed, @Hash);
        SELECT LAST_INSERT_ID();
        """;

        int newId = conn.ExecuteScalar<int>(query, payment);

        return newId;
    }

    // GET alle payments
    // Endpoint: /payments
    public static List<PaymentModel> GetAllPayments()
    {
        string cs = _config.GetConnectionString("DefaultConnection")!;
        using MySqlConnection conn = new(cs);
        conn.Open();

        const string sql = """
        SELECT 
            transaction_id     AS TransactionId,
            transaction        AS Transaction
            amount             AS Amount,
            initiator          AS Initiator,
            created_at         AS CreatedAt,
            completed          AS Completed,
            hash               AS Hash
            FROM payments;
        """;

        List<PaymentModel> result = conn.Query<PaymentModel>(sql).ToList();
        return result;
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