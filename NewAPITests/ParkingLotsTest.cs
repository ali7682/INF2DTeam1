using System.Reflection;
using Microsoft.AspNetCore.Mvc;
using Xunit;
using NewAPI.Controllers;
using NewAPI.Models;

namespace NewAPITests
{
    public class ParkingLotsControllerTests
    {
        [Fact]
        public void UpdateParkingLotsById_ReturnsOk_WhenAdminAndValidModel()
        {
            // Arrange
            var controller = new ParkingLotController();

            // Fake HttpContext with Authorization header
            var httpContext = new DefaultHttpContext();
            httpContext.Request.Headers["Authorization"] = "FakeAdminToken";
            controller.ControllerContext = new ControllerContext()
            {
                HttpContext = httpContext
            };

            // Mock static dependencies
            SessionManager.AddSession("FakeAdminToken", new UserModel { Username = "AdminUser", Role = "ADMIN" });
            ParkingLotAccess.SetFakeParkingLot(new ParkingLotModel(5, "Old Name", "OldLoc", "OldAddr", 10, 2, 1.0, 10.0, ""));

            // Create new model
            var model = new ParkingLotModel(5, "Updated Name", "NewLoc", "NewAddr", 15, 3, 2.0, 20.0, "");

            // Act
            var result = controller.UpdateParkingLotsById(5, model) as OkObjectResult;

            // Assert
            Assert.NotNull(result);
            Assert.Equal(200, result.StatusCode);

            var responseBody = result.Value as dynamic;
            Assert.Equal("Parking lot updated succesfully", responseBody.message);
            Assert.Equal("Updated Name", responseBody.parkingLot.Name);
        }
    }

    public static class ParkingLotAccess
    {
        private static ParkingLotModel? _fakeLot;

        public static void SetFakeParkingLot(ParkingLotModel lot)
            => _fakeLot = lot;

        public static ParkingLotModel? GetParkingLotById(int id)
            => _fakeLot?.Id == id ? _fakeLot : null;

        public static bool UpdateParkingLotById(int id, ParkingLotModel model)
        {
            _fakeLot = model;
            return true;
        }
    }
}