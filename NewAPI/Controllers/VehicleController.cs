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

    // DELETE /vehicles/{vid}
    [HttpDelete("{vid:int}")]
    public IActionResult DeleteVehicle(int vid)
    {
        string token = HttpContext.Request.Headers.Authorization.ToString();
        UserModel? sessionUser = SessionManager.GetSession(token);

        if (string.IsNullOrEmpty(token) || sessionUser == null)
            return Unauthorized(new { message = "Unauthorized: Invalid or missing session token" });

        VehicleModel? vehicle = VehicleAccess.GetVehicleById(vid);

        if (vehicle == null)
            return NotFound(new { message = "Vehicle not found" });

        if (sessionUser.Role != "ADMIN" && sessionUser.Id != vehicle.UserID)
            return StatusCode(403, new { message = "Access denied" });

        bool deleted = VehicleAccess.DeleteVehicleById(vid);

        if (!deleted)
            return NotFound(new { message = "Vehicle not found" });

        return Ok(new { message = "Vehicle deleted" });
    }
}
