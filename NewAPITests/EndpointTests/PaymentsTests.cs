using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Xunit;
using System;
using System.IO;
using System.Threading.Tasks;
using NewAPI.Controllers;

public class PaymentsTests
{
    // Create controller using appsettings.json
    private PaymentController CreateControllerWithToken(string token)
    {
        var config = new ConfigurationBuilder()
            .SetBasePath(Path.Combine(Directory.GetCurrentDirectory(), @"..\..\..\..", "NewAPI")) // adjust path
            .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
            .Build();

        var controller = new PaymentController(config)
        {
            ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext()
            }
        };

        controller.HttpContext.Request.Headers["Authorization"] = token;
        return controller;
    }

    // Post /payments
    [Fact]
    public async Task PostPayments_ValidBody_ReturnsOk()
    {
        string token = Guid.NewGuid().ToString("N");
        SessionManager.AddSession(token, new UserModel { Username = "AdminUser", Role = "ADMIN" });

        var controller = CreateControllerWithToken(token);

        var body = new PaymentRequest
        {
            Transaction = "TXN123456",
            Amount = 49.99m
        };

        var result = await controller.PostPayments(body, CancellationToken.None);

        var okResult = Assert.IsType<OkObjectResult>(result);
        Assert.NotNull(okResult.Value);
    }
    
    [Fact]
    public async Task PostPayments_InvalidToken_ReturnsUnauthorized()
    {
        var controller = CreateControllerWithToken("invalid-token");

        var body = new PaymentRequest
        {
            Transaction = "TXN123456",
            Amount = 49.99m
        };

        var result = await controller.PostPayments(body, CancellationToken.None);

        var objResult = Assert.IsType<UnauthorizedObjectResult>(result);
        Assert.NotNull(objResult.Value);
    }

    [Fact]
    public async Task PostPayments_InvalidBody_ReturnsBadRequest()
    {
        string token = Guid.NewGuid().ToString("N");
        SessionManager.AddSession(token, new UserModel { Username = "AdminUser", Role = "ADMIN" });

        var controller = CreateControllerWithToken(token);

        var body = new PaymentRequest
        {
            Transaction = null,
            Amount = -5m
        };

        var result = await controller.PostPayments(body, CancellationToken.None);

        var badResult = Assert.IsType<BadRequestObjectResult>(result);
        Assert.NotNull(badResult.Value);
    }

    // Post /payments/refund
    [Fact]
    public async Task PostPaymentsRefunds_ValidAdminBody_ReturnsOk()
    {
        string token = Guid.NewGuid().ToString("N");
        SessionManager.AddSession(token, new UserModel { Username = "AdminUser", Role = "ADMIN" });

        var controller = CreateControllerWithToken(token);

        var body = new PaymentRequest
        {
            Transaction = "TXN654321",
            Amount = 20.00m
        };

        var result = await controller.PostPaymentsRefunds(body, CancellationToken.None);

        var okResult = Assert.IsType<OkObjectResult>(result);
        Assert.NotNull(okResult.Value);
    }

    [Fact]
    public async Task PostPaymentsRefunds_NonAdminUser_ReturnsForbidden()
    {
        string token = Guid.NewGuid().ToString("N");
        SessionManager.AddSession(token, new UserModel { Username = "RegularUser", Role = "USER" });

        var controller = CreateControllerWithToken(token);

        var body = new PaymentRequest
        {
            Transaction = "TXN654321",
            Amount = 20.00m
        };

        var result = await controller.PostPaymentsRefunds(body, CancellationToken.None);

        var forbiddenResult = Assert.IsType<ObjectResult>(result);
        Assert.Equal(403, forbiddenResult.StatusCode);
        Assert.NotNull(forbiddenResult.Value);
    }

    [Fact]
    public async Task PostPaymentsRefunds_InvalidBody_ReturnsBadRequest()
    {
        string token = Guid.NewGuid().ToString("N");
        SessionManager.AddSession(token, new UserModel { Username = "AdminUser", Role = "ADMIN" });

        var controller = CreateControllerWithToken(token);

        var body = new PaymentRequest
        {
            Transaction = null,
            Amount = -10.00m
        };

        var result = await controller.PostPaymentsRefunds(body, CancellationToken.None);

        var badResult = Assert.IsType<BadRequestObjectResult>(result);
        Assert.NotNull(badResult.Value);
    }
}
