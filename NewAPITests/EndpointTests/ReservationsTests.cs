using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using NewAPI.Controllers;
using Xunit;

namespace NewAPITests
{
    public class ReservationControllerTests
    {
        private readonly ReservationController controller;
        private readonly CancellationToken ct = CancellationToken.None;
        private readonly int _userId = 1;
        private readonly int _userAdminId = 2;

        public ReservationControllerTests()
        {
            var config = TestConfig.CreateConfig();

            ReservationAccess.SetConfig(config);
            controller = new ReservationController(config);
            TestAccessBootstrap.Configure(config);
        }

        [Fact]
        public async Task TestValidUpdateReservationById()
        {
            string token = Guid.NewGuid().ToString("N");
            await SessionManager.AddSession(token, _userAdminId, TestContext.Current.CancellationToken);
            Assert.NotNull(SessionManager.GetSession(token, TestContext.Current.CancellationToken));

            controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext()
            };
            controller.ControllerContext.HttpContext.Request.Headers["Authorization"] = token;

            // Mock updated reservation data
            var updatedReservation = new ReservationModel
            {
                UserID = 1,
                ParkinglotID = 2,
                VehicleID = 3,
                StartTime = DateTime.Now,
                EndTime = DateTime.Now.AddHours(2),
                Status = "confirmed",
                Cost = 15.75M
            };

            var rawResult = await controller.UpdateReservationsById(5, updatedReservation, ct);
            Console.WriteLine($"Result type: {rawResult?.GetType().Name ?? "NULL"}");

            var result = rawResult as OkObjectResult;

            Assert.NotNull(result);
            Assert.Equal(200, result.StatusCode);
        }

        [Fact]
        public async Task TestInvalidUpdateReservationModel()
        {
            string token = Guid.NewGuid().ToString("N");
            var user = new UserModel { Id = 1, Username = "AdminUser", Role = "ADMIN" };
            await SessionManager.AddSession(token, _userAdminId, TestContext.Current.CancellationToken);
            Assert.NotNull(SessionManager.GetSession(token, TestContext.Current.CancellationToken));

            controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext()
            };
            controller.ControllerContext.HttpContext.Request.Headers["Authorization"] = token;

            var rawResult = await controller.UpdateReservationsById(5, null!, ct);
            Console.WriteLine($"Result type: {rawResult?.GetType().Name ?? "NULL"}");

            var result = rawResult as BadRequestObjectResult;

            Assert.NotNull(result);
            Assert.Equal(400, result.StatusCode);
        }

        [Fact]
        public async Task TestUnauthorizedUpdateReservation()
        {
            controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext()
            };

            var updatedReservation = new ReservationModel
            {
                UserID = 1,
                StartTime = DateTime.Now,
                EndTime = DateTime.Now.AddHours(1),
                Status = "pending",
                Cost = 10.5M
            };

            var rawResult = await controller.UpdateReservationsById(5, updatedReservation, ct);
            Console.WriteLine($"Result type: {rawResult?.GetType().Name ?? "NULL"}");

            var result = rawResult as UnauthorizedObjectResult;

            Assert.NotNull(result);
            Assert.Equal(401, result.StatusCode);
        }

        // POST /reservations
        [Fact]
        public async Task TestValidPostReservation()
        {
            var vehicleToCreate = new VehicleModel
            {
                LicensePlate = "TEST" + Guid.NewGuid().ToString("N").Substring(0, 8),
                Make = "Test",
                Model = "Car",
                Color = "Red",
                Year = 2020,
                CreatedAt = DateTime.Now,
                UserID = 1
            };

            int newVehicleId = await VehicleAccess.CreateVehicleAsync(vehicleToCreate);

            var newVehicle = new VehicleModel
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
            
            string token = Guid.NewGuid().ToString("N");

            await SessionManager.AddSession(token, _userId, TestContext.Current.CancellationToken);
            Assert.NotNull(SessionManager.GetSession(token, TestContext.Current.CancellationToken));

            controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext()
            };
            controller.ControllerContext.HttpContext.Request.Headers["Authorization"] = token;

            var body = new ReservationRequest
            {
                Licenseplate = newVehicle.LicensePlate,
                Startdate = "2025-10-29 10:00:00",
                Enddate = "2025-10-29 12:00:00",
                ParkingLot = 5,
                User = "TestUser"
            };


            var rawResult = await controller.PostReservation(body, ct);
            Console.WriteLine($"Result type: {rawResult?.GetType().Name ?? "NULL"}");

            var result = rawResult as OkObjectResult;

            Assert.NotNull(result);
            Assert.Equal(200, result.StatusCode);
        }
            
        [Fact]
        public async Task TestValidAdminPostReservation()
        {
            var vehicleToCreate = new VehicleModel
            {
                LicensePlate = "TEST" + Guid.NewGuid().ToString("N").Substring(0, 8),
                Make = "Test",
                Model = "Car",
                Color = "Red",
                Year = 2020,
                CreatedAt = DateTime.Now,
                UserID = 1
            };

            int newVehicleId = await VehicleAccess.CreateVehicleAsync(vehicleToCreate);

            var newVehicle = new VehicleModel
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

            string token = Guid.NewGuid().ToString("N");

            await SessionManager.AddSession(token, _userId, TestContext.Current.CancellationToken);
            Assert.NotNull(SessionManager.GetSession(token, TestContext.Current.CancellationToken));

            controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext()
            };
            controller.ControllerContext.HttpContext.Request.Headers["Authorization"] = token;

            var body = new ReservationRequest
            {
                Licenseplate = newVehicle.LicensePlate,
                Startdate = "2025-10-29 10:00:00",
                Enddate = "2025-10-29 12:00:00",
                ParkingLot = 5,
                User = "TestUser"
            };

            var rawResult = await controller.PostReservation(body, ct);
            Console.WriteLine($"Result type: {rawResult?.GetType().Name ?? "NULL"}");

            var result = rawResult as OkObjectResult;

            Assert.NotNull(result);
            Assert.Equal(200, result.StatusCode);
        }
    }
}