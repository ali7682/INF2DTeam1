using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Xunit;

public class VehicleDeleteTests
{
    // Helper to create a test vehicle in DB
    private VehicleModel CreateTestVehicle(int userId)
    {
        // Vehicle object without ID
        var vehicleToCreate = new VehicleModel
        {
            LicensePlate = "TEST" + Guid.NewGuid().ToString("N").Substring(0, 8),
            Make = "Test",
            Model = "Car",
            Color = "Red",
            Year = 2020,
            CreatedAt = DateTime.Now,
            UserID = userId
        };

        // Insert into DB and get ID
        int newVehicleId = VehicleAccess.CreateVehicle(vehicleToCreate);

        // Assign ID to a new VehicleModel
        var createdVehicle = new VehicleModel
        {
            ID = newVehicleId,
            LicensePlate = vehicleToCreate.LicensePlate,
            Make = vehicleToCreate.Make,
            Model = vehicleToCreate.Model,
            Color = vehicleToCreate.Color,
            Year = vehicleToCreate.Year,
            CreatedAt = vehicleToCreate.CreatedAt,
            UserID = vehicleToCreate.UserID
        };

        return createdVehicle;
    }

    private VehicleController CreateControllerWithToken(string token)
    {
        // Provide the real connection string from your appsettings.json
        var inMemorySettings = new Dictionary<string, string> {
        {"ConnectionStrings:DefaultConnection", "Server=127.0.0.1;Port=3307;Database=MobyPark;User ID=api;Password=S3cure!ApiPW;SslMode=None;"}
    };

        var config = new ConfigurationBuilder()
            .AddInMemoryCollection(inMemorySettings)
            .Build();

        var controller = new VehicleController(config);

        controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext()
        };

        controller.HttpContext.Request.Headers["Authorization"] = token;
        return controller;
    }

    [Fact]
    public void DeleteVehicle_InvalidToken_ReturnsUnauthorized()
    {
        var controller = CreateControllerWithToken("invalid-token");
        var vehicle = CreateTestVehicle(2);

        var result = controller.DeleteVehicle(vehicle.ID);

        var objResult = Assert.IsType<UnauthorizedObjectResult>(result);
        Assert.NotNull(objResult.Value);
    }

    [Fact]
    public void DeleteVehicle_ValidAdminToken_ReturnsOk()
    {
        var adminToken = "VALID_ADMIN_TOKEN"; // Replace with real admin token
        var controller = CreateControllerWithToken(adminToken);
        var vehicle = CreateTestVehicle(2);

        var result = controller.DeleteVehicle(vehicle.ID);

        var okResult = Assert.IsType<OkObjectResult>(result);
        Assert.NotNull(okResult.Value);
    }

    [Fact]
    public void DeleteVehicle_VehicleNotFound_ReturnsNotFound()
    {
        var adminToken = "VALID_ADMIN_TOKEN";
        var controller = CreateControllerWithToken(adminToken);
        int nonExistentVehicleId = 999999; // ID that doesn't exist

        var result = controller.DeleteVehicle(nonExistentVehicleId);

        var notFoundResult = Assert.IsType<NotFoundObjectResult>(result);
        Assert.NotNull(notFoundResult.Value);
    }

    [Fact]
    public void DeleteVehicle_NotOwnerOrAdmin_ReturnsForbidden()
    {
        var userToken = "VALID_USER_TOKEN"; // Token for a non-admin user
        var controller = CreateControllerWithToken(userToken);
        var vehicle = CreateTestVehicle(2); // vehicle owned by another user

        var result = controller.DeleteVehicle(vehicle.ID);

        var statusResult = Assert.IsType<ObjectResult>(result);
        Assert.Equal(403, statusResult.StatusCode);
    }
}
