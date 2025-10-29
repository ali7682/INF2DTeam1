using System.Net.Sockets;
using Microsoft.AspNetCore.Mvc;

public class ParkinglotRequest
{
    public string Name { get; set; }
    public string Location { get; set; }
    public string Address { get; set; }
    public int Capacity { get; set; }
    public int Reserved { get; set; }
    public decimal Tariff { get; set; }
    public decimal DayTariff { get; set; }
}

public class LicenseplateRequest
{
    public string Licenseplate { get; set; }
}

namespace NewAPI.Controllers
{
    [ApiController]
    [Route("Parking-Lots")]
    public class ParkingLotController : ControllerBase
    {
        private readonly IConfiguration _config;

        public ParkingLotController(IConfiguration config)
        {
            _config = config;
            ParkingLotAccess.SetConfig(_config);
        }

        // DELETE /parking-lots/{lid}
        [HttpDelete("{lid:int}")]
        public IActionResult DeleteParkingLot(int lid)
        {
            string token = HttpContext.Request.Headers.Authorization.ToString();
            UserModel? sessionUser = SessionManager.GetSession(token);

            if (string.IsNullOrEmpty(token) || sessionUser == null)
                return Unauthorized(new { message = "Unauthorized: Invalid or missing session token" });

            if (sessionUser.Role != "ADMIN")
                return StatusCode(403, new { message = "Access denied" });
            // return Forbid("Access denied");

            bool deleted = ParkingLotAccess.DeleteParkingLotById(lid);

            if (!deleted)
                return NotFound("Parking lot not found");

            return Ok(new { message = "Parking lot deleted" });
        }

        // DELETE /parking-lots/{lid}/sessions/{sid}
        [HttpDelete("{lid:int}/sessions/{sid:int}")]
        public IActionResult DeleteParkingSession(int lid, int sid)
        {
            string token = HttpContext.Request.Headers.Authorization.ToString();
            UserModel? sessionUser = SessionManager.GetSession(token);

            if (string.IsNullOrEmpty(token) || sessionUser == null)
                return Unauthorized(new { message = "Unauthorized: Invalid or missing session token" });

            if (sessionUser.Role != "ADMIN")
                return StatusCode(403, new { message = "Access denied" });
            // return Forbid("Access denied");

            bool deleted = ParkingLotAccess.DeleteParkingSessionById(lid, sid);

            if (!deleted)
                return NotFound("Parking session not found");

            return Ok(new { message = "Parking session deleted" });
        }

        //POST /parking-lots
        [HttpPost("parking-lots")]
        public async Task<ActionResult<ParkingLotModel>> PostParkinglot([FromBody] ParkinglotRequest body, CancellationToken ct)
        {
            string sessionToken = HttpContext.Request.Headers.Authorization.ToString();
            UserModel? user = SessionManager.GetSession(sessionToken);

            if (sessionToken == null || user == null)
                return Unauthorized("Unauthorized: Invalid or missing session token");

            if (user.Role != "ADMIN")
                return StatusCode(403, new { message = "Access denied" });

            if (body is null || string.IsNullOrWhiteSpace(body.Name) || string.IsNullOrWhiteSpace(body.Location) || string.IsNullOrWhiteSpace(body.Address) ||
                body.Capacity <= 0 || body.Tariff < 0 || body.DayTariff < 0)
                return BadRequest(new { error = "Bad request: Missing or invalid parking lot details" });

            ParkingLotModel newParkinglot = new()
            {
                Name = body.Name,
                Location = body.Location,
                Address = body.Address,
                Capacity = body.Capacity,
                Reserved = body.Reserved,
                Tariff = body.Tariff,
                DayTariff = body.DayTariff
            };

            int newId = ParkingLotAccess.CreateParkinglot(newParkinglot);

            return Ok(new { message = $"Parking lot created successfully with ID {newId}" });

        }

        //POST /parking-lots/{lid}/sessions/start
        [HttpPost("/parking-lots/{lid}/sessions/start")]
        public async Task<ActionResult<ParkingLotModel>> PostParkinglotStart([FromBody] LicenseplateRequest body, CancellationToken ct, int lid)
        {
            string sessionToken = HttpContext.Request.Headers.Authorization.ToString();
            UserModel? user = SessionManager.GetSession(sessionToken);

            if (sessionToken == null || user == null)
                return Unauthorized("Unauthorized: Invalid or missing session token");

            if (body is null || string.IsNullOrWhiteSpace(body.Licenseplate))
                return BadRequest(new { error = "Bad request: Missing or invalid licenseplate details" });

        }
    }
}
