using System.Reflection;
using Microsoft.AspNetCore.Mvc;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NewAPI.Controllers;
using NewAPI.Models;

namespace NewAPITest
{
    // Test PUT: /parking-lots/{lid}
    [TestClass]
    public class ParkingLotsControllerTest
    {
        [TestMethod]
        public void TestValidUpdateParkingLotsById()
        {
            ParkingLotController controller = new ParkingLotController();
            ParkingLotModel model = new ParkingLotModel(1, "John Deere Centrum", "JohnDeerLand", "John Deere 1223TD", 15, 1, 1.5, 17.5, "") as OkObjectRequest;

            var result = controller.UpdateParkingLotsById(5, model) as OkObjectResult;

            Assert.IsNotNull(result, "Expected a OK object but got null");
            Assert.AreEqual(200, result.StatusCode, "Expected 200 OK response");
        }
    }
}