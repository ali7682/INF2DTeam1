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
        private readonly PaymentDetailsController controller;

        public PaymentsDetailsTests()
        {
            IConfigurationRoot config = new ConfigurationBuilder()
                .SetBasePath(Path.Combine(Directory.GetCurrentDirectory(), @"..\..\..\..", "NewAPI"))
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                .Build();
            controller = new PaymentDetailsController(config);
        }

        // Test GET: /billings
        [Fact]
        public void TestGetBillings()
        {
            var result = controller.GetBilling() as OkObjectResult;

            Assert.NotNull(result);
            Assert.Equal(200, result.StatusCode);
        }

        // Test GET: /billings/{username}
        [Fact]
        public void TestValidGetBillingsByUserName()
        {
            var result = controller.GetBillingByUser("JohnDeere12") as OkObjectResult;

            Assert.NotNull(result);
            Assert.Equal(200, result.StatusCode);
        }

        [Fact]
        public void TestInvalidGetBillingsByUserName()
        {
            var result = controller.GetBillingByUser("") as BadRequestObjectResult;

            Assert.NotNull(result);
            Assert.Equal(400, result.StatusCode);
        }
    }
}