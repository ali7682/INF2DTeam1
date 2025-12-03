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
        FROM discount_codes
    """;
}