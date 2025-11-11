using System;
using Xunit;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.AspNetCore.Mvc.Testing;
using NewAPI.Controllers;

namespace NewAPITests
{
    public class PaymentsDetailsTests
    {
        private readonly PaymentsDetailsController controller;

        public PaymentsDetailsTests()
        {
            IConfigurationRoot config = new ConfigurationBuilder()
                .SetBasePath(Path.Combine(Directory.GetCurrentDirectory(), @"..\..\..\..", "NewAPI"))
                .AddJsonFile("appsetting.json", optional: false, reloadOnChange: true)
                .Build();
            controller = new PaymentController(config);
        }

        // Test GET: /billings
        [Fact]
        public void TestGetBillings()
        {
            var result = controller.GetBillings() as OkObjectResult;

            Assert.NotNull(result, "Expected a OkObject response but got null");
            Assert.Equal(200, result.StatusCode, "Expected 200 OK response");
        }

        // Test GET: /billings/{username}
        [Fact]
        public void TestValidGetBillingsByUserName()
        {
            var result = controller.GetBillingByUser("JohnDeere12") as OkObjectResult;

            Assert.NotNull(result, "Expected a OkObject response but got null");
            Assert.Equal(200, result.StatusCode, "Expected 200 OK response");
        }

        [Fact]
        public void TestInvalidGetBillingsByUserName()
        {
            var result = controller.GetBillingsByUser("") as BadRequestObjectResult;

            Assert.NotNull(result, "Expected as Bad Request response but got null");
            Assert.Equal(400, result.StatusCode, "Expected 400 Bad request response");
        }
    }
}