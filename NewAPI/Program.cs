var builder = WebApplication.CreateBuilder(args);

// Logs

builder.Logging.ClearProviders();
builder.Logging.AddConsole();
builder.Logging.AddDebug();

// Add services to the container.

builder.Services.AddControllers();

// Swagger

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Warmup

builder.Services.AddHostedService<DbWarmupService>();

builder.WebHost.UseUrls("http://0.0.0.0:8000");

var app = builder.Build();

UserAccess.SetConfig(builder.Configuration);
VehicleAccess.SetConfig(builder.Configuration);
ReservationAccess.SetConfig(builder.Configuration);
PaymentAccess.SetConfig(builder.Configuration);
PaymentDetailsAccess.SetConfig(builder.Configuration);
ParkingLotAccess.SetConfig(builder.Configuration);

app.UseMiddleware<AccessLogs>();

// Configure the HTTP request pipeline.
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