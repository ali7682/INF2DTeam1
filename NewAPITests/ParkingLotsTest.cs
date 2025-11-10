using System;
using Xunit;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using NewAPI.Controllers;

namespace NewAPITests
{
    public class ParkingLotTests
    {
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
        public void TestGetAllParkingLots_ReturnsOk()
        {
            OkObjectResult result = controller.GetAllParkingLots() as OkObjectResult;

            Assert.NotNull(result);
            Assert.Equal(200, result.StatusCode);
        }

        // GET /parking-lots/{lid}
        [Fact]
        public void TestGetParkingLot_ValidId_ReturnsOk()
        {
            OkObjectResult result = controller.GetParkingLot(1) as OkObjectResult;

            Assert.NotNull(result);
            Assert.Equal(200, result.StatusCode);
        }

        [Fact]
        public void TestGetParkingLot_InvalidId_ReturnsNotFound()
        {
            NotFoundObjectResult result = controller.GetParkingLot(99999) as NotFoundObjectResult;

            Assert.NotNull(result);
            Assert.Equal(404, result.StatusCode);
        }

        // GET /parking-lots/{lid}/sessions
        [Fact]
        public void TestGetParkingSessions_NoAuthToken_ReturnsUnauthorized()
        {
            // Setup fake HTTP request zonder valid authentication
            controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext()
            };
            controller.ControllerContext.HttpContext.Request.Headers.Authorization = "";

            ObjectResult result = controller.GetParkingSessions(1) as ObjectResult;

            Assert.NotNull(result);
            Assert.Equal(401, result.StatusCode);
        }

        [Fact]
        public void TestGetParkingSessions_ValidAdminToken_ReturnsOk()
        {
            // Setup mock authenticated session
            string token = Guid.NewGuid().ToString("N");
            SessionManager.AddSession(token, new UserModel { Username = "AdminUser", Role = "ADMIN" });

            controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext()
            };
            controller.ControllerContext.HttpContext.Request.Headers.Authorization = token;

            ObjectResult result = controller.GetParkingSessions(1) as ObjectResult;

            Assert.NotNull(result);
            Assert.Equal(200, result.StatusCode);

            // Clean up, zodat het geen effect heeft op andere tests

            SessionManager.RemoveSession(token);
        }

        // GET /parking-lots/{lid}/sessions/{sid}
        [Fact]
        public void TestGetParkingSession_NoAuthToken_ReturnsUnauthorized()
        {
            controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext()
            };
            controller.ControllerContext.HttpContext.Request.Headers.Authorization = "";

            ObjectResult result = controller.GetParkingSession(1, 1) as ObjectResult;

            Assert.NotNull(result);
            Assert.Equal(401, result.StatusCode);
        }

        [Fact]
        public void TestGetParkingSession_ValidAdminToken_ReturnsOk()
        {
            string token = Guid.NewGuid().ToString("N");
            SessionManager.AddSession(token, new UserModel { Username = "AdminUser", Role = "ADMIN" });

            controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext()
            };
            controller.ControllerContext.HttpContext.Request.Headers.Authorization = token;

            ObjectResult result = controller.GetParkingSession(1, 1) as ObjectResult;

            Assert.NotNull(result);
            Assert.Equal(200, result.StatusCode);

            SessionManager.RemoveSession(token);
        }
    }
}
