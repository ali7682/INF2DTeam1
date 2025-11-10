using System.Security.Principal;
using NewAPI.Controllers;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using NewAPI.Controllers;

namespace NewAPITests
{
    [TestClass]
    public class ParkingLotTests
    {
        private ParkingLotController controller;

        // Runt voor elke test
        [TestInitialize]
        public void Setup()
        {
            // Elke test krijgt een controller instance met een empty config
            IConfigurationRoot config = new ConfigurationBuilder().AddInMemoryCollection().Build();
            controller = new ParkingLotController(config);

            // // Load appsettings.json van de NewAPI project
            // IConfigurationRoot config = new ConfigurationBuilder()
            //     .SetBasePath(Path.Combine(Directory.GetCurrentDirectory(), @"..\..\..\..", "NewAPI"))
            //     .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
            //     .Build();

            // controller = new ParkingLotController(config);
        }

        // GET /parking-lots
        [TestMethod]
        public void TestGetAllParkingLots_ReturnsOk()
        {
            OkObjectResult result = controller.GetAllParkingLots() as OkObjectResult;

            Assert.IsNotNull(result, "Expected OkObjectResult but got null");
            Assert.AreEqual(200, result.StatusCode, "Expected status code 200 OK");
        }

        // GET /parking-lots/{lid}
        [TestMethod]
        public void TestGetParkingLot_ValidId_ReturnsOk()
        {
            OkObjectResult result = controller.GetParkingLot(1) as OkObjectResult;

            Assert.IsNotNull(result, "Expected OkObjectResult but got null");
            Assert.AreEqual(200, result.StatusCode, "Expected status code 200 OK");
        }

        [TestMethod]
        public void TestGetParkingLot_InvalidId_ReturnsNotFound()
        {
            NotFoundObjectResult result = controller.GetParkingLot(99999) as NotFoundObjectResult;

            Assert.IsNotNull(result, "Expected NotFoundObjectResult but got null");
            Assert.AreEqual(404, result.StatusCode, "Expected status code 404 Not Found");
        }

        // GET /parking-lots/{lid}/sessions
        [TestMethod]
        public void TestGetParkingSessions_NoAuthToken_ReturnsUnauthorized()
        {
            // Setup fake HTTP request zonder valid authentication
            controller.ControllerContext = new ControllerContext();
            controller.ControllerContext.HttpContext = new DefaultHttpContext();
            controller.ControllerContext.HttpContext.Request.Headers.Authorization = "";

            ObjectResult result = controller.GetParkingSessions(1) as ObjectResult;

            Assert.IsNotNull(result, "Expected UnauthorizedObjectResult but got null");
            Assert.AreEqual(401, result.StatusCode, "Expected status code 401 Unauthorized");
        }

        [TestMethod]
        public void TestGetParkingSessions_ValidAdminToken_ReturnsOk()
        {
            // Setup mock authenticated session
            string token = Guid.NewGuid().ToString("N");
            SessionManager.AddSession(token, new UserModel { Username = "AdminUser", Role = "ADMIN" });

            controller.ControllerContext = new ControllerContext();
            controller.ControllerContext.HttpContext = new DefaultHttpContext();
            controller.ControllerContext.HttpContext.Request.Headers.Authorization = token;

            ObjectResult result = controller.GetParkingSessions(1) as ObjectResult;

            Assert.IsNotNull(result, "Expected OkObjectResult but got null");
            Assert.AreEqual(200, result.StatusCode, "Expected status code 200 OK");

            // Clean up, zodat het geen effect heeft op andere tests

            SessionManager.RemoveSession(token);
        }

        // GET /parking-lots/{lid}/sessions/{sid}
        [TestMethod]
        public void TestGetParkingSession_NoAuthToken_ReturnsUnauthorized()
        {
            controller.ControllerContext = new ControllerContext();
            controller.ControllerContext.HttpContext = new DefaultHttpContext();
            controller.ControllerContext.HttpContext.Request.Headers.Authorization = "";

            ObjectResult result = controller.GetParkingSession(1, 1) as ObjectResult;

            Assert.IsNotNull(result, "Expected UnauthorizedObjectResult but got null");
            Assert.AreEqual(401, result.StatusCode, "Expected status code 401 Unauthorized");
        }

        [TestMethod]
        public void TestGetParkingSession_ValidAdminToken_ReturnsOk()
        {
            string token = Guid.NewGuid().ToString("N");
            SessionManager.AddSession(token, new UserModel { Username = "AdminUser", Role = "ADMIN" });

            controller.ControllerContext = new ControllerContext();
            controller.ControllerContext.HttpContext = new DefaultHttpContext();
            controller.ControllerContext.HttpContext.Request.Headers.Authorization = token;

            ObjectResult result = controller.GetParkingSession(1, 1) as ObjectResult;

            Assert.IsNotNull(result, "Expected OkObjectResult but got null");
            Assert.AreEqual(200, result.StatusCode, "Expected status code 200 OK");

            SessionManager.RemoveSession(token);
        }
    }
}
