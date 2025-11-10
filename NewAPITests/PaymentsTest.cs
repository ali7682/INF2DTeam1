using System.Reflection;
using Microsoft.AspNetCore.Mvc;
using Xunit;
using NewAPI.Controllers;
using NewAPI.Models;

namespace NewAPITests
{
    public class PaymentsControllerTests
    {
        // Test GET: /payments
        [Fact]
        public void TestGetPayments()
        {
            PaymentsController controller = new PaymentsController();
            var result = controller.GetPayments() as OkObjectResult;

            Assert.IsNotNull(result);
            Assert.AreEqual(200, result.StatusCode);
        }

        // Test GET: /payments/{username}
        [Fact]
        public void TestValidGetPaymentsByUserName()
        {
            PaymentsController controller = new PaymentsController();
            var result = controller.GetPaymentsByUserName("JohnDeere12") as OkObjectResult;

            Assert.IsNotNull(result);
            Assert.AreEqual(200, result.StatusCode);
        }

        [Fact]
        public void TestInvalidGetPaymentsByUserName()
        {
            PaymentsController controller = new PaymentsController();
            var result = controller.GetPaymentsByUserName("") as BadRequestObjectResult;

            Assert.IsNotNull(result);
            Assert.AreEqual(400, result.StatusCode);
        }
    }
}