using Xunit;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using NewAPI.Controllers;

namespace NewAPITests
{
    public class ParkingLotTests
    {
        private readonly CancellationToken ct = CancellationToken.None;

        private readonly ParkingLotController controller;

        // Runt voor elke test
        public ParkingLotTests()
        {
            // Load appsettings.json (gebruikt nu nog de echte db en niet MobyParkTest) van de NewAPI project
            IConfigurationRoot config = new ConfigurationBuilder()
                .SetBasePath(Path.Combine(Directory.GetCurrentDirectory(), @"..\..\..\..", "NewAPI"))
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                .Build();

            controller = new ParkingLotController(config);
        }

        // GET /parking-lots
        [Fact]
        public async Task TestGetAllParkingLots_ReturnsOk()
        {
            OkObjectResult result = await controller.GetAllParkingLots(ct) as OkObjectResult;

            Assert.NotNull(result);
            Assert.Equal(200, result.StatusCode);
        }

        // GET /parking-lots/{lid}
        [Fact]
        public async Task TestGetParkingLot_ValidId_ReturnsOk()
        {
            OkObjectResult result = await controller.GetParkingLot(1, ct) as OkObjectResult;

            Assert.NotNull(result);
            Assert.Equal(200, result.StatusCode);
        }

        [Fact]
        public async Task TestGetParkingLot_InvalidId_ReturnsNotFound()
        {
            NotFoundObjectResult result = await controller.GetParkingLot(99999, ct) as NotFoundObjectResult;

            Assert.NotNull(result);
            Assert.Equal(404, result.StatusCode);
        }

        // GET /parking-lots/{lid}/sessions
        [Fact]
        public async Task TestGetParkingSessions_NoAuthToken_ReturnsUnauthorized()
        {
            // Setup fake HTTP request zonder valid authentication
            controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext()
            };
            controller.ControllerContext.HttpContext.Request.Headers.Authorization = "";

            ObjectResult result = await controller.GetParkingSessions(1, ct) as ObjectResult;

            Assert.NotNull(result);
            Assert.Equal(401, result.StatusCode);
        }

        [Fact]
        public async Task TestGetParkingSessions_ValidAdminToken_ReturnsOk()
        {
            // Setup mock authenticated session
            string token = Guid.NewGuid().ToString("N");
            SessionManager.AddSession(token, new UserModel { Username = "AdminUser", Role = "ADMIN" });

            controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext()
            };
            controller.ControllerContext.HttpContext.Request.Headers.Authorization = token;

            ObjectResult result = await controller.GetParkingSessions(1, ct) as ObjectResult;

            Assert.NotNull(result);
            Assert.Equal(200, result.StatusCode);

            // Clean up, zodat het geen effect heeft op andere tests

            SessionManager.RemoveSession(token);
        }

        // GET /parking-lots/{lid}/sessions/{sid}
        [Fact]
        public async Task TestGetParkingSession_NoAuthToken_ReturnsUnauthorized()
        {
            controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext()
            };
            controller.ControllerContext.HttpContext.Request.Headers.Authorization = "";

            ObjectResult result = await controller.GetParkingSession(1, 1, ct) as ObjectResult;

            Assert.NotNull(result);
            Assert.Equal(401, result.StatusCode);
        }

        [Fact]
        public async Task TestGetParkingSession_ValidAdminToken_ReturnsOk()
        {
            string token = Guid.NewGuid().ToString("N");
            SessionManager.AddSession(token, new UserModel { Username = "AdminUser", Role = "ADMIN" });

            controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext()
            };
            controller.ControllerContext.HttpContext.Request.Headers.Authorization = token;

            ObjectResult result = await controller.GetParkingSession(1, 1, ct) as ObjectResult;

            Assert.NotNull(result);
            Assert.Equal(200, result.StatusCode);

            SessionManager.RemoveSession(token);
        }
    }
}