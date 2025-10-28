using Microsoft.AspNetCore.Mvc;

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
