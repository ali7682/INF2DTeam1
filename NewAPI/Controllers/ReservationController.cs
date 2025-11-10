using Microsoft.AspNetCore.Mvc;

public class ReservationRequest
{
    public string Licenseplate { get; set; }
    public string Startdate { get; set; }
    public string Enddate { get; set; }
    public int ParkingLot { get; set; }
    public string User { get; set; }
}

namespace NewAPI.Controllers
{
    [ApiController]
    [Route("Reservations")]
    public class ReservationController : ControllerBase
    {
        private readonly IConfiguration _config;

        public ReservationController(IConfiguration config)
        {
            _config = config;
            ReservationAccess.SetConfig(_config);
        }

        // DELETE /reservations/{rid}
        [HttpDelete("{rid:int}")]
        public async Task<IActionResult> DeleteReservation(int rid)
        {
            string token = HttpContext.Request.Headers.Authorization.ToString();
            UserModel? sessionUser = SessionManager.GetSession(token);

            if (string.IsNullOrEmpty(token) || sessionUser == null)
                return Unauthorized(new { message = "Unauthorized: Invalid or missing session token" });

            ReservationModel? reservation = await ReservationAccess.GetReservationByIdAsync(rid);

            if (reservation == null)
                return NotFound(new { message = "Reservation not found" });

            if (sessionUser.Role != "ADMIN" && sessionUser.Id != reservation.UserID)
                return StatusCode(403, new { message = "Access denied" });

            bool deleted = await ReservationAccess.DeleteReservationByIdAsync(rid);

            if (!deleted)
                return NotFound(new { message = "Reservation not found" });

            return Ok(new { message = "Reservation deleted" });
        }

        // POST /reservations
        [HttpPost]
        public async Task<ActionResult<ReservationModel>> PostReservation([FromBody] ReservationRequest body, CancellationToken ct, int lid)
        {
            string sessionToken = HttpContext.Request.Headers.Authorization.ToString();
            UserModel? user = SessionManager.GetSession(sessionToken);

            if (sessionToken == null || user == null)
                return Unauthorized("Unauthorized: Invalid or missing session token");

            if (body is null || string.IsNullOrWhiteSpace(body.Licenseplate) || string.IsNullOrWhiteSpace(body.Startdate) || string.IsNullOrWhiteSpace(body.Enddate) || body.ParkingLot <= 0)
                return BadRequest(new { error = "Bad request: Missing or invalid reservation details" });

            List<ParkingLotModel> parkingLots = ParkingLotAccess.GetAllParkingLots();

            if (!parkingLots.Any(pl => pl.ID == body.ParkingLot))
            {
                return BadRequest(new { error = "Bad request: Specified parking lot does not exist" });
            }

            VehicleModel? vehicle = VehicleAccess.GetVehicleByLicensePlate(body.Licenseplate);
            if (vehicle == null)
            {
                return BadRequest(new { error = $"No vehicle found with license plate '{body.Licenseplate}'", field = "licenseplate" });
            }

            // Admin-specific user validation
            UserModel? targetUser = user;
            if (user.Role == "ADMIN")
            {
                if (string.IsNullOrWhiteSpace(body.User))
                    return BadRequest(new { error = "Missing required field", field = "user" });

                targetUser = UserAccess.GetUserByUsername(body.User);
                if (targetUser == null)
                    return BadRequest(new { error = $"User not found with username '{body.User}'", field = "user" });
            }

            ReservationModel newReservation = new()
            {
                UserID = user.Role == "ADMIN" ? targetUser.Id : user.Id,
                ParkinglotID = body.ParkingLot,
                VehicleID = vehicle.ID,
                StartTime = DateTime.Parse(body.Startdate),
                EndTime = DateTime.Parse(body.Enddate),
                Status = "pending",
                CreatedAt = DateTime.Now,
                Cost = 0m
            };

            int newId = ReservationAccess.CreateReservation(newReservation);

            return Ok(new { message = $"Reservation created successfully with ID {newId}" });
        }
        
        // PUT /reservations/{rid}
        [HttpPut("{rid:int}")]
        public async Task<IActionResult> UpdateReservationsById(int rid, [FromBody] ReservationModel updatedReservation, CancellationToken ct)
        {
            if (updatedReservation == null)
            {
                return BadRequest("Bad Request: Invalid reservation data");
            }

            string sessionToken = HttpContext.Request.Headers.Authorization.ToString();
            UserModel? user = SessionManager.GetSession(sessionToken);

            if (user == null || sessionToken == null)
            {
                return Unauthorized("Unauthorized: Invalid or missing session token");
            }

            ReservationModel? reservation = await ReservationAccess.GetReservationByIdAsync(rid, ct);
            if (!user.IsAdmin() && reservation.UserID != user.Id)
            {
                return StatusCode(403, new { message = "Forbidden: You do not have access to this reservation" });
            }

            if (reservation == null)
            {
                return NotFound("NotFound: Reservation not found");
            }

            reservation.StartTime = updatedReservation.StartTime;
            reservation.EndTime = updatedReservation.EndTime;
            reservation.Status = updatedReservation.Status;
            reservation.Cost = updatedReservation.Cost;

            ReservationAccess.UpdateReservationByIdAsync(rid, reservation, ct);

            return Ok(new { message = "Reservation updated successfully", reservation });
        }
    }
}
