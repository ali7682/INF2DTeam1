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

        public ReservationControllerTests()
        {
            // Load appsettings.json from NewAPI project folder
            IConfigurationRoot config = new ConfigurationBuilder()
                .SetBasePath(Path.Combine(Directory.GetCurrentDirectory(), @"..\..\..\..", "NewAPI"))
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                .Build();

            ReservationAccess.SetConfig(config);
            controller = new ReservationController(config);
        }

        [Fact]
        public async Task TestValidUpdateReservationById()
        {
            string token = Guid.NewGuid().ToString("N");
            var user = new UserModel { Id = 1, Username = "AdminUser", Role = "ADMIN" };
            SessionManager.AddSession(token, user);
            Assert.NotNull(SessionManager.GetSession(token));

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
            SessionManager.AddSession(token, user);
            Assert.NotNull(SessionManager.GetSession(token));

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
    }
}