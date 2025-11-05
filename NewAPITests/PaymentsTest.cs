using System.Reflection;
using Microsoft.AspNetCore.Mvc;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NewAPI.Controllers;
using NewAPI.Models;

namespace NewAPITests
{
    [TestClass]
    public class PaymentsControllerTests
    {
        // Test GET: /payments
        [TestMethod]
        public void TestGetPayments()
        {
            PaymentsController controller = new PaymentsController();
            var result = controller.GetPayments() as OkObjectResult;

            Assert.IsNotNull(result, "Expected a OK object but got null");
            Assert.AreEqual(200, result.StatusCode, "Expected 200 OK response");
        }

        // Test GET: /payments/{username}
        [TestMethod]
        public void TestValidGetPaymentsByUserName()
        {
            PaymentsController controller = new PaymentsController();
            var result = controller.GetPaymentsByUserName("JohnDeer12") as OkObjectResult;

            Assert.IsNotNull(result, "Expected a OK object but got null");
            Assert.AreEqual(200, result.StatusCode, "Expected 200 OK response");
        }

        [TestMethod]
        public void TestInvalidGetPaymentsByUserName()
        {
            PaymentsController controller = new PaymentsController();
            var result = controller.GetPaymentsByUserName("") as BadRequestObjectResult;

            Assert.IsNotNull(result, "Expected a Bad Request object but got null");
            Assert.AreEqual(400, result.StatusCode, "Expected 400 Bad Request response");
        }
    }
}