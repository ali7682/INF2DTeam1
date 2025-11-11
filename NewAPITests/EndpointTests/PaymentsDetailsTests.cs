using System;
using Xunit;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.AspNetCore.Mvc.Testing;
using NewAPI.Controllers;
using System.Threading.Tasks;

namespace NewAPITests
{
    public class PaymentsDetailsTests
    {
        private readonly PaymentDetailsController billingController;
        private readonly CancellationToken ct = CancellationToken.None;

        public PaymentsDetailsTests()
        {
            IConfigurationRoot config = new ConfigurationBuilder()
                .SetBasePath(Path.Combine(Directory.GetCurrentDirectory(), @"..\..\..\..", "NewAPI"))
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                .Build();
            PaymentDetailsAccess.SetConfig(config);
            billingController = new PaymentDetailsController(config);
        }

        // Test GET: /billings
        [Fact]
        public async Task TestGetBillings()
        {
            string token = Guid.NewGuid().ToString("N");
            var user = new UserModel { Username = "AdminUser", Role = "ADMIN" };
            SessionManager.AddSession(token, user);
            Assert.NotNull(SessionManager.GetSession(token));

            billingController.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext()
            };
            billingController.ControllerContext.HttpContext.Request.Headers["Authorization"] = token;

            var rawResult = await billingController.GetBilling(ct);
            Console.WriteLine($"Result type: {rawResult?.GetType().Name ?? "NULL"}");

            var result = rawResult as OkObjectResult;

            Assert.NotNull(result);
            Assert.Equal(200, result.StatusCode);
        }

        // TEST: GET /billings/{username}
        [Fact]
        public async Task TestValidGetBillingsByUserName()
        {
            string token = Guid.NewGuid().ToString("N");
            var user = new UserModel { Username = "AdminUser", Role = "ADMIN" };
            SessionManager.AddSession(token, user);
            Assert.NotNull(SessionManager.GetSession(token));

            billingController.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext()
            };
            billingController.ControllerContext.HttpContext.Request.Headers["Authorization"] = token;

            var rawResult = await billingController.GetBillingByUser("JohnDeere12" ,ct);
            Console.WriteLine($"Result type: {rawResult?.GetType().Name ?? "NULL"}");

            var result = rawResult as OkObjectResult;

            Assert.NotNull(result);
            Assert.Equal(200, result.StatusCode);
        }

        [Fact]
        public async Task TestInvalidGetBillingsByUserName()
        {
            string token = Guid.NewGuid().ToString("N");
            var user = new UserModel { Username = "AdminUser", Role = "ADMIN" };
            SessionManager.AddSession(token, user);
            Assert.NotNull(SessionManager.GetSession(token));

            billingController.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext()
            };
            billingController.ControllerContext.HttpContext.Request.Headers["Authorization"] = token;

            var rawResult = await billingController.GetBillingByUser("" ,ct);
            Console.WriteLine($"Result type: {rawResult?.GetType().Name ?? "NULL"}");

            ObjectResult? result = null;

            if (rawResult is BadRequestObjectResult badRequest)
                result = badRequest;
            else if (rawResult is NotFoundObjectResult notFound)
                result = notFound;

            Assert.NotNull(result);
            Assert.True(result.StatusCode == 400 || result.StatusCode == 404,
                $"Expected 400 or 404, but got {result.StatusCode}");
        }
    }
}