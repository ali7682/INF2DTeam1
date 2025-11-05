using System.Reflection;
using Microsoft.AspNetCore.Mvc;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NewAPI.Controllers;
using NewAPI.Models;

namespace NewAPITests
{
    // Test GET: /billings
    [TestClass]
    public class PaymentsDetailsControllerTests
    {
        [TestMethod]
        public void TestGetBillings()
        {
            PaymentsDetailsController controller = new PaymentsDetailsController();
            var result = controller.GetBillings() as OkObjectResult;

            Assert.IsNotNull(result, "Expected a OkObject response but got null");
            Assert.AreEqual(200, result.StatusCode, "Expected 200 OK response");
        }

        // Test GET: /billings/{username}
        [TestMethod]
        public void TestValidGetBillingsByUserName()
        {
            PaymentsDetailsController controller = new PaymentsDetailsController();
            var result = controller.GetBillingByUser("JohnDeere12") as OkObjectResult;

            Assert.IsNotNull(result, "Expected a OkObject response but got null");
            Assert.AreEqual(200, result.StatusCode, "Expected 200 OK response");
        }

        [TestMethod]
        public void TestInvalidGetBillingsByUserName()
        {
            PaymentsDetailsController controller = new PaymentsDetailsController();
            var result = controller.GetBillingsByUser("") as BadRequestObjectResult;

            Assert.IsNotNull(result, "Expected as Bad Request response but got null");
            Assert.AreEqual(400, result.StatusCode, "Expected 400 Bad request response");
        }
    }

}