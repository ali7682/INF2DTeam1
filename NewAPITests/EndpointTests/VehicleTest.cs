using System;
using Xunit;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using NewAPI.Controllers;

namespace NewAPITests
{
    public class VehicleDeleteTests
    {
        private readonly VehicleController controller;

        public VehicleDeleteTests()
        {
            IConfigurationRoot config = new ConfigurationBuilder()
                .SetBasePath(Path.Combine(Directory.GetCurrentDirectory(), @"..\..\..\..", "NewAPI"))
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                .Build();

            controller = new VehicleController(config);
        }

        // Helper to create a test vehicle in the DB
        private VehicleModel CreateTestVehicle(int userId)
        {
            var tempVehicle = new VehicleModel
            {
                LicensePlate = "TEST" + Guid.NewGuid().ToString("N").Substring(0, 8),
                Make = "Test",
                Model = "Car",
                Color = "Red",
                Year = 2020,
                CreatedAt = DateTime.Now,
                UserID = userId
            };

            int newId = VehicleAccess.CreateVehicle(tempVehicle);

            var vehicleWithId = new VehicleModel
            {
                ID = newId,
                LicensePlate = tempVehicle.LicensePlate,
                Make = tempVehicle.Make,
                Model = tempVehicle.Model,
                Color = tempVehicle.Color,
                Year = tempVehicle.Year,
                CreatedAt = tempVehicle.CreatedAt,
                UserID = tempVehicle.UserID
            };

            return vehicleWithId;
        }

        [Fact]
        public void DeleteVehicle_ValidAdminToken_ReturnsOk()
        {
            var vehicle = CreateTestVehicle(userId: 1);

            string token = Guid.NewGuid().ToString("N");
            SessionManager.AddSession(token, new UserModel { Id = 99, Role = "ADMIN" });

            controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext()
            };
            controller.ControllerContext.HttpContext.Request.Headers.Authorization = token;

            var result = controller.DeleteVehicle(vehicle.ID);
            Assert.NotNull(result);
            Assert.IsType<OkObjectResult>(result);

            var objectResult = result as ObjectResult;
            Assert.NotNull(objectResult);
            Assert.NotNull(objectResult.Value);

            var response = objectResult.Value as IDictionary<string, object>;
            Assert.NotNull(response);
            Assert.Equal("Vehicle deleted", response["message"]);

            SessionManager.RemoveSession(token);
        }

        [Fact]
        public void DeleteVehicle_NotOwnerOrAdmin_ReturnsForbidden()
        {
            var vehicle = CreateTestVehicle(userId: 1);

            string token = Guid.NewGuid().ToString("N");
            SessionManager.AddSession(token, new UserModel { Id = 2, Role = "USER" });

            controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext()
            };
            controller.ControllerContext.HttpContext.Request.Headers.Authorization = token;

            var result = controller.DeleteVehicle(vehicle.ID);
            Assert.NotNull(result);
            Assert.IsType<ObjectResult>(result);

            var objectResult = result as ObjectResult;
            Assert.NotNull(objectResult);
            Assert.Equal(403, objectResult.StatusCode);
            Assert.NotNull(objectResult.Value);

            var response = objectResult.Value as IDictionary<string, object>;
            Assert.NotNull(response);
            Assert.Equal("Access denied", response["message"]);

            VehicleAccess.DeleteVehicleById(vehicle.ID);
            SessionManager.RemoveSession(token);
        }

        [Fact]
        public void DeleteVehicle_VehicleNotFound_ReturnsNotFound()
        {
            string token = Guid.NewGuid().ToString("N");
            SessionManager.AddSession(token, new UserModel { Id = 99, Role = "ADMIN" });

            controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext()
            };
            controller.ControllerContext.HttpContext.Request.Headers.Authorization = token;

            int nonExistentId = 999999;
            var result = controller.DeleteVehicle(nonExistentId);
            Assert.NotNull(result);
            Assert.IsType<NotFoundObjectResult>(result);

            var objectResult = result as ObjectResult;
            Assert.NotNull(objectResult);
            Assert.NotNull(objectResult.Value);

            var response = objectResult.Value as IDictionary<string, object>;
            Assert.NotNull(response);
            Assert.Equal("Vehicle not found", response["message"]);

            SessionManager.RemoveSession(token);
        }

        [Fact]
        public void DeleteVehicle_InvalidToken_ReturnsUnauthorized()
        {
            controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext()
            };
            controller.ControllerContext.HttpContext.Request.Headers.Authorization = "invalid-token";

            var result = controller.DeleteVehicle(1);
            Assert.NotNull(result);
            Assert.IsType<UnauthorizedObjectResult>(result);

            var objectResult = result as ObjectResult;
            Assert.NotNull(objectResult);
            Assert.NotNull(objectResult.Value);

            var response = objectResult.Value as IDictionary<string, object>;
            Assert.NotNull(response);
            Assert.Equal("Unauthorized: Invalid or missing session token", response["message"]);
        }
    }
}
