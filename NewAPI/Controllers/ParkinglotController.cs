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

        // DELETE /parking-lots/{lid}
        [HttpDelete("{lid:int}")]
        public IActionResult DeleteParkingLot(int lid)
        {
            string token = HttpContext.Request.Headers.Authorization.ToString();
            var sessionUser = SessionManager.GetSession(token);

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
            var sessionUser = SessionManager.GetSession(token);

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
    }
}
