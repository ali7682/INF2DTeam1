using Dapper;
using MySqlConnector;

public static class PaymentAccess
{
    public static readonly string TableName = "payments";
    private static IConfiguration _config;

    public static void SetConfig(IConfiguration config) => _config = config;

    private static string Cs => _config.GetConnectionString("DefaultConnection")!;

    private const string SqlSelectBase = """
        SELECT
            transaction_id     AS TransactionId,
            transaction        AS Transaction,
            amount             AS Amount,
            initiator          AS Initiator,
            created_at         AS Created_at,
            completed          AS Completed,
            hash               AS Hash,
            discount_code_id   AS DiscountCodeId
        FROM payments
    """;

    public static async Task<int> CreatePaymentAsync(PaymentModel payment, CancellationToken ct = default)
    {
        const string query = """
        INSERT INTO payments
            (amount, transaction, initiator, created_at, completed, hash, discount_code_id)
        VALUES
            (@Amount, @Transaction, @Initiator, @Created_at, @Completed, @Hash, @DiscountCodeId);
        SELECT LAST_INSERT_ID();
        """;

        var dbPayment = new
        {
            payment.Amount,
            payment.Transaction,
            Initiator = EncryptionService.Encrypt(payment.Initiator),
            payment.Created_at,
            payment.Completed,
            payment.Hash,
            payment.DiscountCodeId
        };

        await using var conn = new MySqlConnection(Cs);
        await conn.OpenAsync(ct);

        var cmd = new CommandDefinition(query, dbPayment, cancellationToken: ct, commandTimeout: 5);
        return await conn.ExecuteScalarAsync<int>(cmd);
    }

    // GET alle payments
    // Endpoint: /payments
    public static async Task<List<PaymentModel>> GetAllPaymentsAsync(CancellationToken ct = default)
    {
        var sql = $"{SqlSelectBase};";
        await using var conn = new MySqlConnection(Cs);
        await conn.OpenAsync(ct);

        // var cmd = new CommandDefinition(sql, cancellationToken: ct, commandTimeout: 5);
        var result = await conn.QueryAsync<PaymentModel>(sql);


        foreach (var payment in result)
            payment.Initiator = EncryptionService.Decrypt(payment.Initiator);

        return result.AsList();
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

        var payment = conn.Query<PaymentModel>(sql, new { transaction_Id }).First();

        payment.Initiator = EncryptionService.Decrypt(payment.Initiator);

        return payment;
    }

    public static PaymentModel GetUserByInitiator(string initiator)
    {
        string cs = _config.GetConnectionString("DefaultConnection")!;
        using MySqlConnection conn = new(cs);
        conn.Open();

        var encryptedInput = EncryptionService.Encrypt(initiator);

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

        var payment = conn.Query<PaymentModel>(sql, new { initiator = encryptedInput }).First();

        payment.Initiator = EncryptionService.Decrypt(payment.Initiator);

        return payment;
    }

    public static async Task<bool> UpdatePaymentAsync(PaymentModel payment, CancellationToken ct = default)
    {
        const string sql = """
            UPDATE payments
            SET
                transaction_id = @TransactionId,
                amount         = @Amount,
                initiator      = @Initiator,
                created_at     = @Created_at,
                completed      = @Completed,
                hash           = @Hash,
                discount_code_id  = @DiscountCodeId
            WHERE transaction_id = @TransactionId;
        """;

        var dbPayment = new
        {
            payment.TransactionId,
            payment.Amount,
            Initiator = EncryptionService.Encrypt(payment.Initiator),
            payment.Created_at,
            payment.Completed,
            payment.Hash,
            payment.DiscountCodeId
        };

        await using var conn = new MySqlConnection(Cs);
        await conn.OpenAsync(ct);

        var cmd = new CommandDefinition(sql, dbPayment, cancellationToken: ct, commandTimeout: 5);
        var rows = await conn.ExecuteAsync(cmd);

        return rows > 0;
    }
}