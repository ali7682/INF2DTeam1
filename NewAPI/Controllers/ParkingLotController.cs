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
        
        // GET /parking-lots
        [HttpGet]
        public IActionResult GetAllParkingLots()
        {
            List<ParkingLotModel> parkingLots = ParkingLotAccess.GetAllParkingLots();

            return Ok(parkingLots);
        }

        // GET /parking-lots/{lid}
        [HttpGet("{lid:int}")]
        public IActionResult GetParkingLot(int lid)
        {
            ParkingLotModel? parkingLot = ParkingLotAccess.GetParkingLotById(lid);

            if (parkingLot == null)
                return NotFound(new { message = "Parking lot not found" });

            return Ok(parkingLot);
        }

        // GET /parking-lots/{lid}/sessions
        [HttpGet("{lid:int}/sessions")]
        public IActionResult GetParkingSessions(int lid)
        {
            string token = HttpContext.Request.Headers.Authorization.ToString();
            UserModel? sessionUser = SessionManager.GetSession(token);

            if (string.IsNullOrEmpty(token) || sessionUser == null)
                return Unauthorized(new { message = "Unauthorized: Invalid or missing session token" });

            ParkingLotModel? parkingLot = ParkingLotAccess.GetParkingLotById(lid);

            if (parkingLot == null)
                return NotFound(new { message = "Parking lot not found" });

            List<ParkingSessionModel> sessions = ParkingLotAccess.GetParkingSessionsByLotId(lid);

            if (sessionUser.Role != "ADMIN")
                sessions = sessions.Where(session => session.User == sessionUser.Username).ToList();

            // Python code zou normaal '[]' returnen in dit geval, maar ik heb een message toegevoegd
            if (sessions.Count == 0)
                return Ok(new { message = "No parking sessions found for this user in this lot" });

            return Ok(sessions);
        }

        // GET /parking-lots/{lid}/sessions/{sid}
        [HttpGet("{lid:int}/sessions/{sid:int}")]
        public IActionResult GetParkingSession(int lid, int sid)
        {
            string token = HttpContext.Request.Headers.Authorization.ToString();
            UserModel? sessionUser = SessionManager.GetSession(token);

            if (string.IsNullOrEmpty(token) || sessionUser == null)
                return Unauthorized(new { message = "Unauthorized: Invalid or missing session token" });

            ParkingSessionModel? session = ParkingLotAccess.GetParkingSessionById(lid, sid);

            if (session == null)
                return NotFound(new { message = "Parking session not found" });

            if (sessionUser.Role != "ADMIN" && session.User != sessionUser.Username)
                return StatusCode(403, new { message = "Access denied" });

            return Ok(session);
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
        [HttpPost]
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

            if (ParkingLotAccess.GetParkingSessionsByLotId(lid).Any())
                return StatusCode(401, new { message = "Cannot start a session when another sessions for this licesenplate is already started." });

            ParkingSessionModel newSession = new()
            {
                ParkingLotID = lid,
                LicensePlate = body.Licenseplate,
                Started = DateTime.Now,
                Stopped = null,
                User = user.Username,
                DurationMinutes = null,
                Cost = null,
                PaymentStatus = "pending"
            };

            int newId = ParkingLotAccess.CreateParkingsession(newSession);

            return Ok(new { message = $"Parking session started successfully for licenseplate {body.Licenseplate} with ID {newId}" });
        }

        //POST /parking-lots/{lid}/sessions/stop
        [HttpPost("/parking-lots/{lid}/sessions/stop")]
        public async Task<ActionResult<ParkingLotModel>> PostParkinglotStop([FromBody] LicenseplateRequest body, CancellationToken ct, int lid)
        {
            string sessionToken = HttpContext.Request.Headers.Authorization.ToString();
            UserModel? user = SessionManager.GetSession(sessionToken);

            if (sessionToken == null || user == null)
                return Unauthorized("Unauthorized: Invalid or missing session token");

            if (body is null || string.IsNullOrWhiteSpace(body.Licenseplate))
                return BadRequest(new { error = "Bad request: Missing or invalid licenseplate details" });

            List<ParkingSessionModel> sessions = ParkingLotAccess.FindParkingSessionsByLicenseplate(body.Licenseplate);

            if (!sessions.Any())
                return StatusCode(401, new { message = "Cannot stop a session when there is no session for this licesenplate." });

            ParkingSessionModel sessionToUpdate = sessions.First();

            sessionToUpdate.Stopped = DateTime.Now;
            sessionToUpdate.DurationMinutes = (int?)(sessionToUpdate.Stopped - sessionToUpdate.Started)?.TotalMinutes;

            ParkingLotAccess.UpdateParkingSession(sessionToUpdate);

            return Ok(new { message = $"Parking session stopped successfully for licenseplate {body.Licenseplate}" });
        }

        // PUT /parking-lots/{lid}
        [HttpPut("{lid:int}")]
        public IActionResult UpdateParkingLotsById(int lid, [FromBody] ParkingLotModel updatedParkingLot)
        {
            if (updatedParkingLot == null)
            {
                return BadRequest("Invalid parking lot data");
            }

            string sessionToken = HttpContext.Request.Headers.Authorization.ToString();
            UserModel? user = SessionManager.GetSession(sessionToken);

            if (user == null || sessionToken == null)
            {
                return Unauthorized("Unauthorized: Invalid or missing session token");
            }

            if (!user.IsAdmin())
            {
                return StatusCode(403, new { message = "Forbidden: You do not have access to this parking lot" });
            }

            ParkingLotModel? parkingLot = ParkingLotAccess.GetParkingLotById(lid);
            if (parkingLot == null)
            {
                return NotFound("NotFound: Parking lot not found");
            }

            parkingLot.Name = updatedParkingLot.Name;
            parkingLot.Location = updatedParkingLot.Location;
            parkingLot.Address = updatedParkingLot.Address;
            parkingLot.Capacity = updatedParkingLot.Capacity;
            parkingLot.Reserved = updatedParkingLot.Reserved;
            parkingLot.Tariff = updatedParkingLot.Tariff;
            parkingLot.DayTariff = updatedParkingLot.DayTariff;

            ParkingLotAccess.UpdateParkingLotById(lid, parkingLot);

            return Ok(new { message = "Parking lot updated succesfully", parkingLot });
        }
    }
}

