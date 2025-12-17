using Dapper;
using MySql.Data.MySqlClient;

public static class DiscountAcces
{
    public static readonly string TableName = "discount_codes";
    private static IConfiguration _config;

    public static void SetConfig(IConfiguration config) => _config = config;

    private static string Cs => _config.GetConnectionString("DefaultConnection")!;


    private const string SqlSelectBase = """
        SELECT
            id                AS ID,
            code              AS Code,
            percentage        AS Percentage,
            valid_from        AS ValidFrom,
            valid_to          AS ValidTo,
            locations_allowed AS LocationsAllowed,
            times_allowed     AS TimesAllowed,
            conditions        AS Conditions,
            created_at        AS CreatedAt,
            updated_at        AS UpdatedAt,
            max_uses          AS MaxUses,
            uses              AS Uses,
            is_active         AS IsActive
            allowed_plates    AS AllowedPlates
        FROM discount_codes
    """;

    // get all discount
    public static async Task<IEnumerable<DiscountModel>> GetAllDiscountCodesAsync(CancellationToken ct = default)
    {
        await using MySqlConnection conn = new(Cs);
        await conn.OpenAsync(ct);

        string sql = $"{SqlSelectBase};";

        var discounts = await conn.QueryAsync<DiscountModel>(sql);
        return discounts;
    }


    public static async Task<bool> ActivateDiscountByIdAsync(int discountId)
    {
        await using var conn = new MySqlConnection(Cs);
        await conn.OpenAsync();

        const string sql = """
        UPDATE discount_codes
        SET is_active = TRUE
        WHERE id = @discountId;
       """;

        int affectedRows = await conn.ExecuteAsync(sql, new { discountId });
        return affectedRows > 0;
    }

    public static async Task<bool> DeactivateDiscountByIdAsync(int discountId)
    {
        await using var conn = new MySqlConnection(Cs);
        await conn.OpenAsync();

        const string sql = """
        UPDATE discount_codes
        SET is_active = FALSE
        WHERE id = @discountId;
    """;

        int affectedRows = await conn.ExecuteAsync(sql, new { discountId });
        return affectedRows > 0;
    }

    public static async Task<int> CreateDiscountAsync(DiscountModel discount, CancellationToken ct = default)
    {
        const string query = """
            INSERT INTO discount_codes
                (code, percentage, valid_from, valid_to, locations_allowed, times_allowed, conditions, max_uses, is_active, allowed_plates, created_at, updated_at)
            VALUES
                (@Code, @Percentage, @ValidFrom, @ValidTo, @LocationsAllowed, @TimesAllowed, @Conditions, @MaxUses, @IsActive, @AllowedPlates, NOW(), NOW());
            SELECT LAST_INSERT_ID();
        """;

        await using var conn = new MySqlConnection(Cs);
        await conn.OpenAsync(ct);

        var cmd = new CommandDefinition(query, discount, cancellationToken: ct, commandTimeout: 5);
        return await conn.ExecuteScalarAsync<int>(cmd);
    }

    public static async Task<bool> SetMaxUsesAsync(int discountId, int maxUses)
    {
        await using var conn = new MySqlConnection(Cs);
        await conn.OpenAsync();

        const string sql = """
        UPDATE discount_codes
        SET Uses = @maxUses
        WHERE id = @discountId;
    """;

        int affectedRows = await conn.ExecuteAsync(sql, new { discountId, maxUses });
        return affectedRows > 0;
    }
}