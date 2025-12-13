using Xunit;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using NewAPI.Controllers;
using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using System.Reflection.Metadata.Ecma335;
using System.Reflection.Metadata;

namespace NewAPITests.ControllerTests
{
    public class ParkingLotTests
    {
        private readonly CancellationToken ct = CancellationToken.None;
        private readonly ParkingLotController controller;
        private readonly int _userAdminId = 2;

        public ParkingLotTests()
        {
            var config = TestConfig.CreateConfig();
            controller = new ParkingLotController(config);
            TestAccessBootstrap.Configure(config);
        }

        // Helper to create a temporary parking lot for tests
        private async Task<ParkingLotModel> CreateTestParkingLot()
        {
            int newId;

            // First insert the parking lot and get the new ID
            var tempLot = new ParkingLotModel
            {
                Name = "TestLot_" + Guid.NewGuid().ToString("N").Substring(0, 6),
                Location = "TestLocation",
                Address = "123 Test St",
                Capacity = 50,
                Reserved = 0,
                Tariff = 2.5,
                DayTariff = 20,
                CreatedAt = DateTime.Now
            };

            newId = await ParkingLotAccess.CreateParkinglotAsync(tempLot);

            // Now create a new object with ID in the initializer
            var lot = new ParkingLotModel
            {
                ID = newId,
                Name = tempLot.Name,
                Location = tempLot.Location,
                Address = tempLot.Address,
                Capacity = tempLot.Capacity,
                Reserved = tempLot.Reserved,
                Tariff = tempLot.Tariff,
                DayTariff = tempLot.DayTariff,
                CreatedAt = tempLot.CreatedAt
            };

            return lot;
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
            string token = Guid.NewGuid().ToString("N");
            await SessionManager.AddSession(token, 2, TestContext.Current.CancellationToken);

            controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext()
            };
            controller.ControllerContext.HttpContext.Request.Headers.Authorization = token;

            ObjectResult result = await controller.GetParkingSessions(1, ct) as ObjectResult;

            Assert.NotNull(result);
            Assert.Equal(200, result.StatusCode);

            SessionManager.RemoveSession(token, TestContext.Current.CancellationToken);
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
            SessionManager.AddSession(token, 2, TestContext.Current.CancellationToken);

            var tempLot = await CreateTestParkingLot();

            var tempSessionModel = new ParkingSessionModel
            {
                ParkingLotID = tempLot.ID,
                LicensePlate = "TEST" + Guid.NewGuid().ToString("N").Substring(0, 6),
                Started = DateTime.Now,
                User = "TestUser"
            };
            int tempSessionId = await ParkingLotAccess.CreateParkingsessionAsync(tempSessionModel, ct);

            controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext()
            };
            controller.ControllerContext.HttpContext.Request.Headers.Authorization = token;

            var result = await controller.GetParkingSession(tempLot.ID, tempSessionId, ct) as ObjectResult;

            Assert.NotNull(result);
            Assert.Equal(200, result.StatusCode);

            SessionManager.RemoveSession(token, TestContext.Current.CancellationToken);
        }

        // DELETE ParkingLots
        [Fact]
        public async Task DeleteParkingLot_Existing_ReturnsOk()
        {
            var testLot = await CreateTestParkingLot();

            string token = Guid.NewGuid().ToString("N");
            SessionManager.AddSession(token, 2, TestContext.Current.CancellationToken);

            controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext()
            };
            controller.ControllerContext.HttpContext.Request.Headers.Authorization = token;

            var result = await controller.DeleteParkingLot(testLot.ID, CancellationToken.None);

            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.NotNull(okResult.Value);

            SessionManager.RemoveSession(token, TestContext.Current.CancellationToken);
        }

        [Fact]
        public async Task DeleteParkingLot_NotFound_ReturnsNotFound()
        {
            string token = Guid.NewGuid().ToString("N");
            SessionManager.AddSession(token, 2, TestContext.Current.CancellationToken);

            controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext()
            };
            controller.ControllerContext.HttpContext.Request.Headers.Authorization = token;

            int nonExistentId = 999999;
            var result = await controller.DeleteParkingLot(nonExistentId, CancellationToken.None);

            var notFoundResult = Assert.IsType<NotFoundObjectResult>(result);
            Assert.NotNull(notFoundResult.Value);

            SessionManager.RemoveSession(token, TestContext.Current.CancellationToken);
        }


        // DELETE /parking-lots/{lid}/sessions/{sid}
        [Fact]
        public async Task DeleteParkingSession_Existing_ReturnsOk()
        {
            var testLot = await CreateTestParkingLot();
            var tempSession = new ParkingSessionModel
            {
                ParkingLotID = testLot.ID,
                LicensePlate = "TEST" + Guid.NewGuid().ToString("N").Substring(0, 6),
                Started = DateTime.Now,
                User = "TestUser"
            };
            int sessionId = await ParkingLotAccess.CreateParkingsessionAsync(tempSession, ct);

            string token = Guid.NewGuid().ToString("N");
            SessionManager.AddSession(token, 2, TestContext.Current.CancellationToken);

            controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext()
            };
            controller.ControllerContext.HttpContext.Request.Headers.Authorization = token;

            var result = await controller.DeleteParkingSession(testLot.ID, sessionId, ct);

            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.NotNull(okResult.Value);

            var deletedSession = await ParkingLotAccess.GetParkingSessionByIdAsync(testLot.ID, sessionId, TestContext.Current.CancellationToken);
            Assert.Null(deletedSession);

            SessionManager.RemoveSession(token, TestContext.Current.CancellationToken);
        }

        [Fact]
        public async Task DeleteParkingSession_NotFound_ReturnsNotFound()
        {
            var testLot = await CreateTestParkingLot();

            string token = Guid.NewGuid().ToString("N");
            await SessionManager.AddSession(token, 2, TestContext.Current.CancellationToken);

            controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext()
            };
            controller.ControllerContext.HttpContext.Request.Headers.Authorization = token;

            int nonExistentSessionId = 999999;

            var result = await controller.DeleteParkingSession(testLot.ID, nonExistentSessionId, ct);

            var notFoundResult = Assert.IsType<NotFoundObjectResult>(result);
            Assert.NotNull(notFoundResult.Value);

            await SessionManager.RemoveSession(token, TestContext.Current.CancellationToken);
        }

        [Fact]
        public async Task DeleteParkingSession_NoAuthToken_ReturnsUnauthorized()
        {
            var testLot = await CreateTestParkingLot();
            var tempSession = new ParkingSessionModel
            {
                ParkingLotID = testLot.ID,
                LicensePlate = "TEST" + Guid.NewGuid().ToString("N").Substring(0, 6),
                Started = DateTime.Now,
                User = "TestUser"
            };
            int sessionId = await ParkingLotAccess.CreateParkingsessionAsync(tempSession, ct);

            controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext()
            };
            controller.ControllerContext.HttpContext.Request.Headers.Authorization = "";

            var result = await controller.DeleteParkingSession(testLot.ID, sessionId, ct);

            var unauthorizedResult = Assert.IsType<UnauthorizedObjectResult>(result);
            Assert.Equal(401, unauthorizedResult.StatusCode);
        }

        // PUT /parking-lots/{lid}
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
            var updatedParkingLot = new ParkingLotModel
            {
                Name = "Ali's Winkelcentrum World Trade Center",
                Location = "World Trade Center",
                Address = "Beursplein 37, 3011 AM Rotterdam",
                Capacity = 768,
                Reserved = 243,
                Tariff = 3,
                DayTariff = 16
            };

            var rawResult = await controller.UpdateParkingLotsById(5, updatedParkingLot, ct);
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
            await SessionManager.AddSession(token, _userAdminId);
            Assert.NotNull(SessionManager.GetSession(token, TestContext.Current.CancellationToken));

            controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext()
            };
            controller.ControllerContext.HttpContext.Request.Headers["Authorization"] = token;

            var rawResult = await controller.UpdateParkingLotsById(5, null!, ct);
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

            var updatedParkingLot = new ParkingLotModel
            {
                Name = "Ali's Winkelcentrum World Trade Center",
                Location = "World Trade Center",
                Address = "Beursplein 37, 3011 AM Rotterdam",
                Capacity = 768,
                Reserved = 243,
                Tariff = 3,
                DayTariff = 16
            };

            var rawResult = await controller.UpdateParkingLotsById(5, updatedParkingLot, ct);
            Console.WriteLine($"Result type: {rawResult?.GetType().Name ?? "NULL"}");

            var result = rawResult as UnauthorizedObjectResult;

            Assert.NotNull(result);
            Assert.Equal(401, result.StatusCode);
        }

        // post /parking-lots
        [Fact]
        public async Task TestCreateParkingLot_ValidBody_ReturnsOk()
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

            var parkingLot = new ParkinglotRequest
            {
                Name = "Central Parking",
                Location = "Downtown",
                Address = "123 Main Street",
                Capacity = 200,
                Reserved = 50,
                Tariff = 2.5,
                DayTariff = 20.0
            };

            var rawResult = await controller.PostParkinglot(parkingLot, ct);
            Console.WriteLine($"Result type: {rawResult?.GetType().Name ?? "NULL"}");

            var result = rawResult as OkObjectResult;

            Assert.NotNull(result);
            Assert.Equal(200, result.StatusCode);
        }

        [Fact]
        public async Task TestCreateParkingLot_InvalidBody_ReturnsBadRequest()
        {
            string token = Guid.NewGuid().ToString("N");
            var user = new UserModel { Id = 1, Username = "AdminUser", Role = "ADMIN" };
            await SessionManager.AddSession(token, _userAdminId);
            Assert.NotNull(SessionManager.GetSession(token));

            controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext()
            };
            controller.ControllerContext.HttpContext.Request.Headers["Authorization"] = token;

            var parkingLot = new ParkinglotRequest
            {
                Name = "",
                Location = "Downtown",
                Address = "123 Main Street",
                Capacity = -10,
                Reserved = 50,
                Tariff = 2.5,
                DayTariff = 20.0
            };

            var rawResult = await controller.PostParkinglot(parkingLot, ct);
            Console.WriteLine($"Result type: {rawResult?.GetType().Name ?? "NULL"}");

            var result = rawResult as BadRequestObjectResult;

            Assert.NotNull(result);
            Assert.Equal(400, result.StatusCode);
        }

        [Fact]
        public async Task TestCreateParkingLot_Unauthorized_ReturnsUnauthorized()
        {
            controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext()
            };

            var parkingLot = new ParkinglotRequest
            {
                Name = "Central Parking",
                Location = "Downtown",
                Address = "123 Main Street",
                Capacity = 200,
                Reserved = 50,
                Tariff = 2.5,
                DayTariff = 20.0
            };

            var rawResult = await controller.PostParkinglot(parkingLot, ct);
            Console.WriteLine($"Result type: {rawResult?.GetType().Name ?? "NULL"}");

            var result = rawResult as UnauthorizedObjectResult;

            Assert.NotNull(result);
            Assert.Equal(401, result.StatusCode);
        }

        [Fact]
        public async Task TestCreateParkingLot_NonAdmin_ReturnsForbidden()
        {
            string token = Guid.NewGuid().ToString("N");
            var user = new UserModel { Id = 1, Username = "RegularUser", Role = "USER" };
            await SessionManager.AddSession(token, _userAdminId);
            Assert.NotNull(SessionManager.GetSession(token));

            controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext()
            };
            controller.ControllerContext.HttpContext.Request.Headers["Authorization"] = token;

            var parkingLot = new ParkinglotRequest
            {
                Name = "Central Parking",
                Location = "Downtown",
                Address = "123 Main Street",
                Capacity = 200,
                Reserved = 50,
                Tariff = 2.5,
                DayTariff = 20.0
            };

            var rawResult = await controller.PostParkinglot(parkingLot, ct);
            Console.WriteLine($"Result type: {rawResult?.GetType().Name ?? "NULL"}");

            var result = rawResult as ObjectResult;

            Assert.NotNull(result);
            Assert.Equal(403, result.StatusCode);
        }

        // Post /parking-lots/{lid}/sessions/start
        [Fact]
        public async Task TestStartParkingSession_ValidBody_ReturnsOk()
        {
            var testLot = await CreateTestParkingLot();

            string token = Guid.NewGuid().ToString("N");
            var user = new UserModel { Id = 1, Username = "RegularUser", Role = "USER" };
            await SessionManager.AddSession(token, _userAdminId);
            Assert.NotNull(SessionManager.GetSession(token));

            controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext()
            };
            controller.ControllerContext.HttpContext.Request.Headers["Authorization"] = token;

            var body = new LicenseplateRequest
            {
                Licenseplate = "XYZ-123"
            };

            var rawResult = await controller.PostParkinglotStart(body, ct, testLot.ID);
            Console.WriteLine($"Result type: {rawResult?.GetType().Name ?? "NULL"}");

            var result = rawResult as OkObjectResult;

            Assert.NotNull(result);
            Assert.Equal(200, result.StatusCode);
        }

        [Fact]
        public async Task TestStartParkingSession_InvalidBody_ReturnsBadRequest()
        {
            var testLot = await CreateTestParkingLot();

            string token = Guid.NewGuid().ToString("N");
            var user = new UserModel { Id = 1, Username = "RegularUser", Role = "USER" };
            await SessionManager.AddSession(token, _userAdminId);
            Assert.NotNull(SessionManager.GetSession(token));

            controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext()
            };
            controller.ControllerContext.HttpContext.Request.Headers["Authorization"] = token;

            var body = new LicenseplateRequest
            {
                Licenseplate = ""
            };

            var rawResult = await controller.PostParkinglotStart(body, ct, testLot.ID);
            Console.WriteLine($"Result type: {rawResult?.GetType().Name ?? "NULL"}");

            var result = rawResult as BadRequestObjectResult;

            Assert.NotNull(result);
            Assert.Equal(400, result.StatusCode);
        }

        [Fact]
        public async Task TestStartParkingSession_Unauthorized_ReturnsUnauthorized()
        {
            var testLot = await CreateTestParkingLot();

            controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext()
            };

            var body = new LicenseplateRequest
            {
                Licenseplate = "XYZ-123"
            };

            var rawResult = await controller.PostParkinglotStart(body, ct, testLot.ID);
            Console.WriteLine($"Result type: {rawResult?.GetType().Name ?? "NULL"}");

            var result = rawResult as UnauthorizedObjectResult;

            Assert.NotNull(result);
            Assert.Equal(401, result.StatusCode);
        }

        [Fact]
        public async Task TestStartParkingSession_NonUser_ReturnsForbidden()
        {
            var testLot = await CreateTestParkingLot();

            string token = Guid.NewGuid().ToString("N");
            var user = new UserModel { Id = 1, Username = "AdminUser", Role = "ADMIN" };
            await SessionManager.AddSession(token, _userAdminId, TestContext.Current.CancellationToken);
            Assert.NotNull(SessionManager.GetSession(token, TestContext.Current.CancellationToken));

            controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext()
            };
            controller.ControllerContext.HttpContext.Request.Headers["Authorization"] = token;

            var body = new LicenseplateRequest
            {
                Licenseplate = "ABC-123" // license plate already has a session
            };

            var rawResult = await controller.PostParkinglotStart(body, ct, testLot.ID);
            rawResult = await controller.PostParkinglotStart(body, ct, testLot.ID); // try to start again
            Console.WriteLine($"Result type: {rawResult?.GetType().Name ?? "NULL"}");

            var result = rawResult as ObjectResult;

            Assert.NotNull(result);
            Assert.Equal(401, result.StatusCode);
        }

        // Post /parking-lots/{lid}/sessions/stop
        [Fact]
        public async Task TestStopParkingSession_ValidBody_ReturnsOk()
        {
            var testLot = await CreateTestParkingLot();

            string token = Guid.NewGuid().ToString("N");
            var user = new UserModel { Id = 1, Username = "RegularUser", Role = "USER" };
            await SessionManager.AddSession(token, _userAdminId, TestContext.Current.CancellationToken);
            Assert.NotNull(SessionManager.GetSession(token, TestContext.Current.CancellationToken));

            controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext()
            };
            controller.ControllerContext.HttpContext.Request.Headers["Authorization"] = token;

            var body = new LicenseplateRequest
            {
                Licenseplate = "XYZ-123"
            };

            var rawResult = await controller.PostParkinglotStart(body, ct, testLot.ID); // first start a session
            rawResult = await controller.PostParkinglotStop(body, ct, testLot.ID);
            Console.WriteLine($"Result type: {rawResult?.GetType().Name ?? "NULL"}");

            var result = rawResult as OkObjectResult;

            Assert.NotNull(result);
            Assert.Equal(200, result.StatusCode);
        }

        [Fact]
        public async Task TestStopParkingSession_InvalidBody_ReturnsBadRequest()
        {
            var testLot = await CreateTestParkingLot();

            string token = Guid.NewGuid().ToString("N");
            var user = new UserModel { Id = 1, Username = "RegularUser", Role = "USER" };
            await SessionManager.AddSession(token, _userAdminId, TestContext.Current.CancellationToken);
            Assert.NotNull(SessionManager.GetSession(token, TestContext.Current.CancellationToken));

            controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext()
            };
            controller.ControllerContext.HttpContext.Request.Headers["Authorization"] = token;

            var body = new LicenseplateRequest
            {
                Licenseplate = ""
            };

            var rawResult = await controller.PostParkinglotStop(body, ct, testLot.ID);
            Console.WriteLine($"Result type: {rawResult?.GetType().Name ?? "NULL"}");

            var result = rawResult as BadRequestObjectResult;

            Assert.NotNull(result);
            Assert.Equal(400, result.StatusCode);
        }

        [Fact]
        public async Task TestStopParkingSession_Unauthorized_ReturnsUnauthorized()
        {
            var testLot = await CreateTestParkingLot();

            controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext()
            };

            var body = new LicenseplateRequest
            {
                Licenseplate = "XYZ-123"
            };

            var rawResult = await controller.PostParkinglotStop(body, ct, testLot.ID);
            Console.WriteLine($"Result type: {rawResult?.GetType().Name ?? "NULL"}");

            var result = rawResult as UnauthorizedObjectResult;

            Assert.NotNull(result);
            Assert.Equal(401, result.StatusCode);
        }

        [Fact]
        public async Task TestStopParkingSession_NoSessionLicenseplate_ReturnsForbidden()
        {
            var testLot = await CreateTestParkingLot();

            string token = Guid.NewGuid().ToString("N");
            var user = new UserModel { Id = 1, Username = "AdminUser", Role = "ADMIN" };
            await SessionManager.AddSession(token, _userAdminId, TestContext.Current.CancellationToken);
            Assert.NotNull(SessionManager.GetSession(token, TestContext.Current.CancellationToken));

            controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext()
            };
            controller.ControllerContext.HttpContext.Request.Headers["Authorization"] = token;

            var body = new LicenseplateRequest
            {
                Licenseplate = "000-000" // license plate does not have an active session
            };

            var rawResult = await controller.PostParkinglotStop(body, ct, testLot.ID);
            Console.WriteLine($"Result type: {rawResult?.GetType().Name ?? "NULL"}");

            var result = rawResult as ObjectResult;

            Assert.NotNull(result);
            Assert.Equal(401, result.StatusCode);
        }
    }
}