using Dapper;
using MySql.Data.MySqlClient;
using static Org.BouncyCastle.Math.EC.ECCurve;

public static class PaymentAccess
{
    public static readonly string TableName = "payments";
    private static IConfiguration _config;

    public static void SetConfig(IConfiguration config)
    {
        _config = config;
    }
}