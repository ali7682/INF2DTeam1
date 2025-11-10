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
    public async Task<IActionResult> GetVehicleByUserName(string userName, int vId)
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

        UserModel? requestedUser = await UserAccess.GetUserByUsernameAsync(userName);

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
    public async Task<IActionResult> GetReservationsByUserName(string userName, int vId)
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

        UserModel? requestedUser = await UserAccess.GetUserByUsernameAsync(userName);

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

    public async Task<IActionResult> GetHistory(string userName, int vId)
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

        UserModel? requestedUser = await UserAccess.GetUserByUsernameAsync(userName);

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
    public async Task<IActionResult> DeleteVehicle(int vid)
    {
        string token = HttpContext.Request.Headers.Authorization.ToString();
        UserModel? sessionUser = SessionManager.GetSession(token);

        if (string.IsNullOrEmpty(token) || sessionUser == null)
            return Unauthorized(new { message = "Unauthorized: Invalid or missing session token" });

        VehicleModel? vehicle = await VehicleAccess.GetVehicleByIdAsync(vid);

        if (vehicle == null)
            return NotFound(new { message = "Vehicle not found" });

        if (sessionUser.Role != "ADMIN" && sessionUser.Id != vehicle.UserID)
            return StatusCode(403, new { message = "Access denied" });

        bool deleted = await VehicleAccess.DeleteVehicleByIdAsync(vid);

        if (!deleted)
            return NotFound(new { message = "Vehicle not found" });

        return Ok(new { message = "Vehicle deleted" });
    }

    [HttpPut("{vId:int}")]
    public IActionResult PutVehicle(int vId, [FromBody] VehicleModel updatedVehicle)
    {
        if (updatedVehicle == null)
        {
            return BadRequest("Invalid vehicle data");
        }

        var authHeader = HttpContext.Request.Headers.Authorization.FirstOrDefault();
        if (string.IsNullOrEmpty(authHeader))
        {
            return Unauthorized("Unauthorized: Missing session token");
        }

        UserModel? user = SessionManager.GetSession(authHeader);
        if (user == null)
        {
            return Unauthorized("Unauthorized: Invalid session token");
        }

        if (!user.IsAdmin())
        {
            return StatusCode(403, new { message = "Forbidden: You do not have access to this vehicle" });
        }

        VehicleModel? vehicle = VehicleAccess.GetVehicleById(vId);
        if (vehicle == null)
        {
            return NotFound($"Vehicle with ID {vId} not found");
        }

        // Update vehicle data
        vehicle.Make = updatedVehicle.Make;
        vehicle.Model = updatedVehicle.Model;
        vehicle.Year = updatedVehicle.Year;
        vehicle.Color = updatedVehicle.Color;

        VehicleAccess.UpdateVehicle(vehicle);

        return Ok(new { message = "Vehicle updated successfully", vehicle });
    }
    [HttpPost]
    public IActionResult PostVehicle([FromBody] VehicleModel newVehicle)
    {
        if (newVehicle == null)
        {
            return BadRequest("Invalid vehicle data");
        }

        var authHeader = HttpContext.Request.Headers.Authorization.FirstOrDefault();
        if (string.IsNullOrEmpty(authHeader))
        {
            return Unauthorized("Unauthorized: Missing session token");
        }

        UserModel? user = SessionManager.GetSession(authHeader);
        if (user == null)
        {
            return Unauthorized("Unauthorized: Invalid session token");
        }

        if (!user.IsAdmin())
        {
            return StatusCode(403, new { message = "Forbidden: You do not have access to create vehicles" });
        }

        if (string.IsNullOrWhiteSpace(newVehicle.LicensePlate) ||
            string.IsNullOrWhiteSpace(newVehicle.Make) ||
            string.IsNullOrWhiteSpace(newVehicle.Model))
        {
            return BadRequest("Vehicle must have a license plate, make, and model");
        }

        var vehicleToCreate = new VehicleModel
        {
            LicensePlate = newVehicle.LicensePlate,
            Make = newVehicle.Make,
            Model = newVehicle.Model,
            Color = newVehicle.Color,
            Year = newVehicle.Year,
            CreatedAt = DateTime.Now,
            UserID = user.Id
        };

        int newVehicleId = VehicleAccess.CreateVehicle(vehicleToCreate);

        if (newVehicleId <= 0)
        {
            return StatusCode(500, new { message = "Failed to create vehicle" });
        }

        vehicleToCreate = new VehicleModel
        {
            ID = newVehicleId,
            LicensePlate = newVehicle.LicensePlate,
            Make = newVehicle.Make,
            Model = newVehicle.Model,
            Color = newVehicle.Color,
            Year = newVehicle.Year,
            CreatedAt = DateTime.Now,
            UserID = user.Id
        };

        return Ok(new
        {
            message = "Vehicle created successfully",
            vehicle = vehicleToCreate
        });
    }

    [HttpPost("{licensePlate}/entry")]
    public IActionResult VehicleEntry(string licensePlate, [FromBody] Dictionary<string, string> data)
    {
        var authHeader = HttpContext.Request.Headers.Authorization.FirstOrDefault();
        if (string.IsNullOrEmpty(authHeader))
            return StatusCode(401, new { status = "error", message = "Missing session token" });

        UserModel? user = SessionManager.GetSession(authHeader);
        if (user == null)
            return StatusCode(401, new { status = "error", message = "Invalid session token" });

        if (!data.ContainsKey("parkinglot") || string.IsNullOrWhiteSpace(data["parkinglot"]))
            return StatusCode(400, new { status = "error", message = "Required field missing", field = "parkinglot" });

        VehicleModel? vehicle = VehicleAccess.GetVehicleByLicensePlate(licensePlate);

        if (vehicle == null)
            return StatusCode(404, new { status = "error", message = "Vehicle not found", licensePlate });

        if (vehicle.UserID != user.Id)
            return StatusCode(403, new { status = "error", message = "Vehicle does not belong to this user", licensePlate });

        return StatusCode(200, new
        {
            status = "success",
            vehicle,
            parkinglot = data["parkinglot"]
        });
    }
}
