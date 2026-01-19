using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Xunit;
using System;
using System.Threading;
using System.Threading.Tasks;

public class DiscountControllerTests
{
    private readonly int _adminUserId = 2;
    private readonly int _normalUserId = 1;

    private DiscountController CreateControllerWithToken(string token)
    {
        var config = TestConfig.CreateConfig();

        var controller = new DiscountController
        {
            ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext()
            }
        };

        controller.HttpContext.Request.Headers["Authorization"] = token;
        return controller;
    }

    private async Task<int> CreateTestDiscount(CancellationToken ct)
    {
        var discount = new DiscountModel
        {
            Code = "TEST-" + Guid.NewGuid().ToString("N").Substring(0, 8),
            Percentage = 10,
            ValidFrom = DateTime.Now.AddDays(-1),
            ValidTo = DateTime.Now.AddDays(7),
            IsActive = true
        };

        return await DiscountAcces.CreateDiscountAsync(discount, ct);
    }

    [Fact]
    public async Task GetAllDiscounts_ReturnsOk()
    {
        var controller = CreateControllerWithToken("");

        var result = await controller.GetAllAsync();

        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        Assert.NotNull(okResult.Value);
    }


    [Fact]
    public async Task PostDiscount_InvalidToken_ReturnsUnauthorized()
    {
        var controller = CreateControllerWithToken("invalid-token");

        var result = await controller.PostDiscount(new DiscountModel
        {
            Code = "FAIL",
            Percentage = 10
        }, CancellationToken.None);

        Assert.IsType<UnauthorizedObjectResult>(result);
    }

    [Fact]
    public async Task PostDiscount_NonAdmin_ReturnsForbidden()
    {
        string token = Guid.NewGuid().ToString("N");
        await SessionManager.AddSession(token, _normalUserId, CancellationToken.None);

        var controller = CreateControllerWithToken(token);

        var result = await controller.PostDiscount(new DiscountModel
        {
            Code = "NOADMIN",
            Percentage = 10
        }, CancellationToken.None);

        var forbidden = Assert.IsType<ObjectResult>(result);
        Assert.Equal(403, forbidden.StatusCode);
    }

    [Fact]
    public async Task PostDiscount_Admin_ReturnsOk()
    {
        string token = Guid.NewGuid().ToString("N");
        await SessionManager.AddSession(token, _adminUserId, CancellationToken.None);

        var controller = CreateControllerWithToken(token);

        var result = await controller.PostDiscount(new DiscountModel
        {
            Code = "ADMIN-" + Guid.NewGuid().ToString("N").Substring(0, 6),
            Percentage = 15,
            ValidFrom = DateTime.Now,
            ValidTo = DateTime.Now.AddDays(5),
            IsActive = true
        }, CancellationToken.None);

        var okResult = Assert.IsType<OkObjectResult>(result);
        Assert.NotNull(okResult.Value);
    }


    [Fact]
    public async Task ActivateDiscount_NotFound_ReturnsNotFound()
    {
        var controller = CreateControllerWithToken("");

        var result = await controller.ActivateAsync(999999);

        Assert.IsType<NotFoundObjectResult>(result);
    }

    [Fact]
    public async Task ActivateDiscount_ValidId_ReturnsOk()
    {
        int discountId = await CreateTestDiscount(CancellationToken.None);
        var controller = CreateControllerWithToken("");

        var result = await controller.ActivateAsync(discountId);

        Assert.IsType<OkObjectResult>(result);
    }


    [Fact]
    public async Task DeactivateDiscount_NotFound_ReturnsNotFound()
    {
        var controller = CreateControllerWithToken("");

        var result = await controller.DeactivateAsync(999999);

        Assert.IsType<NotFoundObjectResult>(result);
    }

    [Fact]
    public async Task DeactivateDiscount_ValidId_ReturnsOk()
    {
        int discountId = await CreateTestDiscount(CancellationToken.None);
        var controller = CreateControllerWithToken("");

        var result = await controller.DeactivateAsync(discountId);

        Assert.IsType<OkObjectResult>(result);
    }
}
