var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();

// Swagger

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Warmup

builder.Services.AddHostedService<DbWarmupService>();

var app = builder.Build();

UserAccess.SetConfig(builder.Configuration);
VehicleAccess.SetConfig(builder.Configuration);
ReservationAccess.SetConfig(builder.Configuration);
PaymentAccess.SetConfig(builder.Configuration);
PaymentDetailsAccess.SetConfig(builder.Configuration);
ParkingLotAccess.SetConfig(builder.Configuration);

// Configure the HTTP request pipeline.
Console.WriteLine($"Environment: {app.Environment.EnvironmentName}");
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();

public partial class Program { }