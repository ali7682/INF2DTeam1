using System;
using Xunit;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.AspNetCore.Mvc.Testing;
using NewAPI.Controllers;
using System.Runtime.InteropServices;

namespace NewAPITests
{
    public class PaymentsTests
    {
        private readonly PaymentController controller;

        public PaymentsTests()
        {
            IConfigurationRoot config = new ConfigurationBuilder()
                .SetBasePath(Path.Combine(Directory.GetCurrentDirectory(), @"..\..\..\..", "NewAPI"))
                .AddJsonFile("appsetting.json", optional: false, reloadOnChange: true)
                .Build();
            controller = new PaymentController(config);
        }

        // Test GET: /payments
        [Fact]
        public void TestGetPayments()
        {
            var result = controller.GetPayments() as OkObjectResult;

            Assert.NotNull(result);
            Assert.Equal(200, result.StatusCode);
        }

        // Test GET: /payments/{username}
        [Fact]
        public void TestValidGetPaymentsByUserName()
        {
            var result = controller.GetPaymentsByUserName("JohnDeere12") as OkObjectResult;

            Assert.NotNull(result);
            Assert.Equal(200, result.StatusCode);
        }

        [Fact]
        public void TestInvalidGetPaymentsByUserName()
        {
            var result = controller.GetPaymentsByUserName("") as BadRequestObjectResult;

            Assert.NotNull(result);
            Assert.Equal(400, result.StatusCode);
        }
    }
}