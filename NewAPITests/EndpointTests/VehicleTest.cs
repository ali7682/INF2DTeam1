using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Xunit;
using System;
using System.IO;

public class VehicleDeleteTests
{
    // Helper to create a test vehicle in the DB
    private async Task<VehicleModel> CreateTestVehicle(int userId)
    {
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

        int newVehicleId = await VehicleAccess.CreateVehicleAsync(vehicleToCreate);

        return new VehicleModel
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
    }

    private VehicleController CreateControllerWithToken(string token)
    {
        var config = TestConfig.CreateConfig();

        var controller = new VehicleController(config)
        {
            ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext()
            }
        };

        controller.HttpContext.Request.Headers["Authorization"] = token;

        return controller;
    }

    [Fact]
    public async Task DeleteVehicle_InvalidToken_ReturnsUnauthorized()
    {
        var controller = CreateControllerWithToken("invalid-token");
        var vehicle = await CreateTestVehicle(2);

        var result = await controller.DeleteVehicle(vehicle.ID);

        var objResult = Assert.IsType<UnauthorizedObjectResult>(result);
        Assert.NotNull(objResult.Value);
    }

    [Fact]
    public async Task DeleteVehicle_ValidAdminToken_ReturnsOk()
    {
        string token = Guid.NewGuid().ToString("N");
        SessionManager.AddSession(token, new UserModel { Username = "AdminUser", Role = "ADMIN" });
        var controller = CreateControllerWithToken(token);
        var vehicle = await CreateTestVehicle(2);

        var result = await controller.DeleteVehicle(vehicle.ID);

        var okResult = Assert.IsType<OkObjectResult>(result);
        Assert.NotNull(okResult.Value);
    }

    [Fact]
    public async Task DeleteVehicle_VehicleNotFound_ReturnsNotFound()
    {
        string token = Guid.NewGuid().ToString("N");
        SessionManager.AddSession(token, new UserModel { Username = "AdminUser", Role = "ADMIN" });
        var controller = CreateControllerWithToken(token);
        int nonExistentVehicleId = 999999;

        var result = await controller.DeleteVehicle(nonExistentVehicleId);

        var notFoundResult = Assert.IsType<NotFoundObjectResult>(result);
        Assert.NotNull(notFoundResult.Value);
    }
}
