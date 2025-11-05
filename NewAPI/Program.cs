var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
builder.Services.AddSingleton<ISqlConnectionFactory, SqlConnectionFactory>();

// Swagger

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

UserAccess.SetConfig(builder.Configuration);
VehicleAccess.SetConfig(builder.Configuration);
ReservationAccess.SetConfig(builder.Configuration);
PaymentAccess.SetConfig(builder.Configuration);

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