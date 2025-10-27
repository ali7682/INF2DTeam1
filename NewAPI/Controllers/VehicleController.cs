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
            CreatedAt = DateTime.Now
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
            CreatedAt = DateTime.Now
        };

        return Ok(new
        {
            message = "Vehicle created successfully",
            vehicle = vehicleToCreate
        });
    }

}
