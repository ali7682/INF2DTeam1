using System.Reflection;
using Microsoft.AspNetCore.Mvc;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NewAPI.Controllers;
using NewAPI.Models;

namespace NewAPITest
{
    // Test PUT: /reservations/{rid}
    [TestClass]
    public class ReservationControllerTest
    {
        [TestMethod]
        public void TestValidUpdateParkingLotsById()
        {
            ReservationController controller = new ReservationController();
            ReservationModel model = new Reservation(1, 1, 1, 1, "", "", "pending", "", 17.5) as OkObjectRequest;

            var result = controller.UpdateReservationsById(5, model) as OkObjectResult;

            Assert.IsNotNull(result, "Expected a OK object but got null");
            Assert.AreEqual(200, result.StatusCode, "Expected 200 OK response");
        }
    }
}