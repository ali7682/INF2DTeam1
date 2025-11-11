using System;
using Xunit;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.AspNetCore.Mvc.Testing;
using NewAPI.Controllers;
using System.Runtime.InteropServices;
using System.Threading.Tasks;

namespace NewAPITests
{
    public class PaymentsTests
    {
        private readonly CancellationToken ct = CancellationToken.None;
        private readonly PaymentController controller;

        public PaymentsTests()
        {
            IConfigurationRoot config = new ConfigurationBuilder()
                .SetBasePath(Path.Combine(Directory.GetCurrentDirectory(), @"..\..\..\..", "NewAPI"))
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                .Build();
            PaymentAccess.SetConfig(config);
            controller = new PaymentController(config);
        }

        // Test GET: /payments
        [Fact]
        public async Task TestGetPayments()
        {
            string token = Guid.NewGuid().ToString("N");
            var user = new UserModel { Username = "AdminUser", Role = "ADMIN" };
            SessionManager.AddSession(token, user);
            Assert.NotNull(SessionManager.GetSession(token));

            controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext()
            };
            controller.ControllerContext.HttpContext.Request.Headers["Authorization"] = token;

            var rawResult = await controller.GetPayments(ct);
            Console.WriteLine($"Result type: {rawResult?.GetType().Name ?? "NULL"}");

            var result = rawResult as OkObjectResult;

            Assert.NotNull(result);
            Assert.Equal(200, result.StatusCode);
        }

        // Test GET: //payments/{userName}
        [Fact]
        public async Task TestValidGetPaymentsByUserName()
        {
            string token = Guid.NewGuid().ToString("N");
            var user = new UserModel { Username = "AdminUser", Role = "ADMIN" };
            SessionManager.AddSession(token, user);
            Assert.NotNull(SessionManager.GetSession(token));

            controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext()
            };
            controller.ControllerContext.HttpContext.Request.Headers["Authorization"] = token;

            var rawResult = await controller.GetPayments(ct);
            Console.WriteLine($"Result type: {rawResult?.GetType().Name ?? "NULL"}");

            var result = rawResult as OkObjectResult;

            Assert.NotNull(result);
            Assert.Equal(200, result.StatusCode);
        }

        [Fact]
        public async Task TestInvalidGetPaymentsByUserName()
        {
            string token = Guid.NewGuid().ToString("N");
            var user = new UserModel { Username = "AdminUser", Role = "ADMIN" };
            SessionManager.AddSession(token, user);
            Assert.NotNull(SessionManager.GetSession(token));

            controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext()
            };
            controller.ControllerContext.HttpContext.Request.Headers["Authorization"] = token;

            var rawResult = await controller.GetPaymentsByUserName("", ct);
            Console.WriteLine($"Result type: {rawResult?.GetType().Name ?? "NULL"}");

            ObjectResult? result = null;

            if (rawResult is BadRequestObjectResult badRequest)
                result = badRequest;
            else if (rawResult is NotFoundObjectResult notFound)
                result = notFound;

            Assert.NotNull(result);
            Assert.True(result.StatusCode == 400 || result.StatusCode == 404,
                $"Expected 400 or 404, but got {result?.StatusCode}");
        }
    }
}