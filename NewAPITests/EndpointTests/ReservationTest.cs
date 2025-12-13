using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Xunit;
using System;
using System.IO;
using System.Threading.Tasks;
using NewAPI.Controllers;

public class ReservationDeleteTests
{
    private ReservationController _reservationController;
    private readonly int _userId = 1;
    private readonly int _userAdminId = 2;

    public ReservationDeleteTests()
    {
        var config = TestConfig.CreateConfig();

        ReservationAccess.SetConfig(config);
        _reservationController = new ReservationController(config);
        TestAccessBootstrap.Configure(config);
    }

    // Helper to create a test reservation
    private async Task<ReservationModel> CreateTestReservation(int userId, int vehicleId, int parkingLotId)
    {
        var reservationToCreate = new ReservationModel
        {
            UserID = userId,
            VehicleID = vehicleId,
            ParkinglotID = parkingLotId,
            StartTime = DateTime.Now,
            EndTime = DateTime.Now.AddHours(2),
            Status = "ACTIVE",
            CreatedAt = DateTime.Now,
            Cost = 10.0m
        };

        int newReservationId = await ReservationAccess.CreateReservationAsync(reservationToCreate, TestContext.Current.CancellationToken);

        return new ReservationModel
        {
            ID = newReservationId,
            UserID = reservationToCreate.UserID,
            VehicleID = reservationToCreate.VehicleID,
            ParkinglotID = reservationToCreate.ParkinglotID,
            StartTime = reservationToCreate.StartTime,
            EndTime = reservationToCreate.EndTime,
            Status = reservationToCreate.Status,
            CreatedAt = reservationToCreate.CreatedAt,
            Cost = reservationToCreate.Cost
        };
    }

    private ReservationController CreateControllerWithToken(string token)
    {
        var config = TestConfig.CreateConfig();

        var controller = new ReservationController(config)
        {
            ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext()
            }
        };

        controller.HttpContext.Request.Headers["Authorization"] = token;

        return controller;
    }

    [Fact]
    public async Task DeleteReservation_InvalidToken_ReturnsUnauthorized()
    {
        var controller = CreateControllerWithToken("invalid-token");
        var reservation = await CreateTestReservation(2, 1, 1);

        var result = await controller.DeleteReservation(reservation.ID);

        var objResult = Assert.IsType<UnauthorizedObjectResult>(result);
        Assert.NotNull(objResult.Value);
    }

    [Fact]
    public async Task DeleteReservation_ValidAdminToken_ReturnsOk()
    {
        string token = Guid.NewGuid().ToString("N");
        await SessionManager.AddSession(token, _userAdminId, TestContext.Current.CancellationToken);
        var controller = CreateControllerWithToken(token);
        var reservation = await CreateTestReservation(2, 1, 1);

        var result = await controller.DeleteReservation(reservation.ID);

        var okResult = Assert.IsType<OkObjectResult>(result);
        Assert.NotNull(okResult.Value);
    }

    [Fact]
    public async Task DeleteReservation_ReservationNotFound_ReturnsNotFound()
    {
        string token = Guid.NewGuid().ToString("N");
        await SessionManager.AddSession(token, _userAdminId, TestContext.Current.CancellationToken);
        var controller = CreateControllerWithToken(token);
        int nonExistentReservationId = 999999;

        var result = await controller.DeleteReservation(nonExistentReservationId);

        var notFoundResult = Assert.IsType<NotFoundObjectResult>(result);
        Assert.NotNull(notFoundResult.Value);
    }

    [Fact]
    public async Task DeleteReservation_NonAdminUserNotOwner_ReturnsForbidden()
    {
        string token = Guid.NewGuid().ToString("N");
        await SessionManager.AddSession(token, _userId, TestContext.Current.CancellationToken);
        var controller = CreateControllerWithToken(token);

        var reservation = await CreateTestReservation(2, 1, 1);

        var result = await controller.DeleteReservation(reservation.ID);

        var statusCodeResult = Assert.IsType<ObjectResult>(result);
        Assert.Equal(403, statusCodeResult.StatusCode);
    }
}
