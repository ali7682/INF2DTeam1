var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();

var app = builder.Build();

UserAccess.SetConfig(builder.Configuration);
VehicleAccess.SetConfig(builder.Configuration);
ReservationAccess.SetConfig(builder.Configuration);
PaymentAccess.SetConfig(builder.Configuration);
ParkingLotAccess.SetConfig(builder.Configuration);

// Configure the HTTP request pipeline.

app.UseAuthorization();

app.MapControllers();

app.Run();