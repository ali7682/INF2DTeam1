using Microsoft.AspNetCore.Mvc;

public class ReservationRequest
{
    public string Licenseplate { get; set; }
    public string Stardate { get; set; }
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
        public IActionResult DeleteReservation(int rid)
        {
            string token = HttpContext.Request.Headers.Authorization.ToString();
            UserModel? sessionUser = SessionManager.GetSession(token);

            if (string.IsNullOrEmpty(token) || sessionUser == null)
                return Unauthorized(new { message = "Unauthorized: Invalid or missing session token" });

            ReservationModel? reservation = ReservationAccess.GetReservationById(rid);

            if (reservation == null)
                return NotFound(new { message = "Reservation not found" });

            if (sessionUser.Role != "ADMIN" && sessionUser.Id != reservation.UserID)
                return StatusCode(403, new { message = "Access denied" });

            bool deleted = ReservationAccess.DeleteReservationById(rid);

            if (!deleted)
                return NotFound(new { message = "Reservation not found" });

            return Ok(new { message = "Reservation deleted" });
        }

        // POST /reservations
        [HttpPost("/reservations")]
        public async Task<ActionResult<ReservationModel>> PostReservation([FromBody] ReservationRequest body, CancellationToken ct, int lid)
        {
            string sessionToken = HttpContext.Request.Headers.Authorization.ToString();
            UserModel? user = SessionManager.GetSession(sessionToken);

            if (sessionToken == null || user == null)
                return Unauthorized("Unauthorized: Invalid or missing session token");

            if (body is null || string.IsNullOrWhiteSpace(body.Licenseplate) || string.IsNullOrWhiteSpace(body.Stardate) || string.IsNullOrWhiteSpace(body.Enddate) || body.ParkingLot <= 0)
                return BadRequest(new { error = "Bad request: Missing or invalid reservation details" });

            List<ParkingLotModel> parkingLots = ParkingLotAccess.GetAllParkingLots();

            if (!parkingLots.Any(pl => pl.ID == body.ParkingLot))
            {
                return BadRequest(new { error = "Bad request: Specified parking lot does not exist" });
            }

            if (user.Role == "ADMIN")
            {
                if (body.User == null)
                    return StatusCode(403, new { message = "Access denied" });
            }

            ReservationModel newReservation = new()
            {
                UserID = user.Role == "ADMIN" ? UserAccess.GetUserByUsername(body.User).Id : user.Id,
                ParkinglotID = body.ParkingLot,
                VehicleID = VehicleAccess.GetVehicleByLicensePlate(body.Licenseplate).ID,
                StartTime = DateTime.Parse(body.Stardate),
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
        public IActionResult UpdateReservationsById(int rid, [FromBody] ReservationModel updatedReservation)
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

            ReservationModel? reservation = ReservationAccess.GetReservationById(rid);
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

            ReservationAccess.UpdateReservationById(rid, reservation);

            return Ok(new { message = "Reservation updated successfully", reservation });
        }
    }
}
