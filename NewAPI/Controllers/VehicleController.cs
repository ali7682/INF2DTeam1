using Microsoft.AspNetCore.Mvc;

[ApiController]
[Route("vehicles")]
public class VehiclesController : ControllerBase
{
    [HttpGet("{vId:int}")]
    public IActionResult GetVehicle(int vId)
    {
        string sessionToken = HttpContext.Request.Headers.Authorization.ToString();
        UserModel? user = SessionManager.GetSession(sessionToken);

        if (sessionToken == null || user == null)
        {
            return Unauthorized("Unauthorized: Invalid or missing session token");
        }

        VehicleModel? vehicle = VehicleAccess.GetVehicleById(vId);

        if (vehicle == null)
        {
            return NotFound("NotFound: Vehicle not found");
        }

        if (!(user.Id == vehicle.UserID))
        {
            //return Forbid("Forbidden: You do not have access to this vehicle");
            return StatusCode(403, new { message = "Forbidden: You do not have access to this vehicle" });
        }

        return Ok(vehicle);
    }

    [HttpGet("{userName}/{vId:int}")]
    public IActionResult GetVehicleByUserName(string userName, int vId)
    {
        string sessionToken = HttpContext.Request.Headers.Authorization.ToString();
        UserModel? user = SessionManager.GetSession(sessionToken);

        if (sessionToken == null || user == null)
        {
            return Unauthorized("Unauthorized: Invalid or missing session token");
        }

        if (!user.IsAdmin())
        {
            //return Forbid("Forbidden: You do not have access to this vehicle");
            return StatusCode(403, new { message = "Forbidden: You do not have access to this vehicle" });
        }

        UserModel? requestedUser = UserAccess.GetUserByUsername(userName);

        if (requestedUser == null)
        {
            return NotFound("NotFound: User not found");
        }

        VehicleModel? vehicle = VehicleAccess.GetVehicleById(vId);

        if (vehicle == null)
        {
            return NotFound("NotFound: Vehicle not found");
        }

        return Ok(vehicle);
    }

    [HttpGet("{vId:int}/reservations")]
    public IActionResult GetReservations(int vId)
    {
        string sessionToken = HttpContext.Request.Headers.Authorization.ToString();
        UserModel? user = SessionManager.GetSession(sessionToken);

        if (sessionToken == null || user == null)
        {
            return Unauthorized("Unauthorized: Invalid or missing session token");
        }

        VehicleModel? vehicle = VehicleAccess.GetVehicleById(vId);

        if (vehicle == null)
        {
            return NotFound("NotFound: Vehicle not found");
        }

        if (!(user.Id == vehicle.UserID))
        {
            //return Forbid("Forbidden: You do not have access to this vehicle\'s reservations");
            return StatusCode(403, new { message = "Forbidden: You do not have access to this vehicle\'s reservations" });
        }

        return Ok(ReservationAccess.GetReservationsByVehicleId(vId));
    }

    [HttpGet("{userName}/{vId:int}/reservations")]
    public IActionResult GetReservationsByUserName(string userName, int vId)
    {
        string sessionToken = HttpContext.Request.Headers.Authorization.ToString();
        UserModel? user = SessionManager.GetSession(sessionToken);

        if (sessionToken == null || user == null)
        {
            return Unauthorized("Unauthorized: Invalid or missing session token");
        }

        if (!user.IsAdmin())
        {
            //return Forbid("Forbidden: You do not have access to this vehicle\'s reservations");
            return StatusCode(403, new { message = "Forbidden: You do not have access to this vehicle\'s reservations" });
        }

        UserModel? requestedUser = UserAccess.GetUserByUsername(userName);

        if (requestedUser == null)
        {
            return NotFound("NotFound: User not found");
        }

        VehicleModel? vehicle = VehicleAccess.GetVehicleById(vId);

        if (vehicle == null)
        {
            return NotFound("NotFound: Vehicle not found");
        }

        return Ok(ReservationAccess.GetReservationsByVehicleId(vId));
    }

    [HttpGet("{vId:int}/history")]
    public IActionResult GetHistory(int vId)
    {
        string sessionToken = HttpContext.Request.Headers.Authorization.ToString();
        UserModel? user = SessionManager.GetSession(sessionToken);

        if (sessionToken == null || user == null)
        {
            return Unauthorized("Unauthorized: Invalid or missing session token");
        }

        VehicleModel? vehicle = VehicleAccess.GetVehicleById(vId);

        if (vehicle == null)
        {
            return NotFound("NotFound: Vehicle not found");
        }

        if (!(user.Id == vehicle.UserID))
        {
            //return Forbid("Forbidden: You do not have access to this vehicle\'s history");
            return StatusCode(403, new { message = "Forbidden: You do not have access to this vehicle\'s reservations" });
        }

        return Ok(ReservationAccess.GetReservationsByVehicleId(vId, "confirmed"));
    }

    [HttpGet("{userName}/{vId:int}/history")]

    public IActionResult GetHistory(string userName, int vId)
    {
        string sessionToken = HttpContext.Request.Headers.Authorization.ToString();
        UserModel? user = SessionManager.GetSession(sessionToken);

        if (sessionToken == null || user == null)
        {
            return Unauthorized("Unauthorized: Invalid or missing session token");
        }

        if (!user.IsAdmin())
        {
            //return Forbid("Forbidden: You do not have access to this vehicle\'s history");
            return StatusCode(403, new { message = "Forbidden: You do not have access to this vehicle\'s history" });
        }

        UserModel? requestedUser = UserAccess.GetUserByUsername(userName);

        if (requestedUser == null)
        {
            return NotFound("NotFound: User not found");
        }

        VehicleModel? vehicle = VehicleAccess.GetVehicleById(vId);

        if (vehicle == null)
        {
            return NotFound("NotFound: Vehicle not found");
        }

        return Ok(ReservationAccess.GetReservationsByVehicleId(vId, "confirmed"));
    }
}
