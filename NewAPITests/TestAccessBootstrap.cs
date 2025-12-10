using Microsoft.Extensions.Configuration;

public static class TestAccessBootstrap
{
    private static bool _configured;

    public static void Configure(IConfiguration config)
    {
        if (_configured) return;
        _configured = true;

        
        UserAccess.SetConfig(config);
        ReservationAccess.SetConfig(config);
        VehicleAccess.SetConfig(config);
        PaymentAccess.SetConfig(config);
        PaymentDetailsAccess.SetConfig(config);
    }
}
