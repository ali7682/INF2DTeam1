using Microsoft.AspNetCore.Mvc;
using System.Text.Json.Nodes;

public class PaymentRequest
{
    public string Transaction { get; set; }
    public decimal Amount { get; set; }
}

namespace NewAPI.Controllers
{
    [ApiController]
    public class PaymentController : Controller
    {
        public PaymentController(IConfiguration config) { }

        [HttpPost("Payments")]
        public async Task<ActionResult<PaymentModel>> PostPayments([FromBody] PaymentRequest body, CancellationToken ct)
        {

            string sessionToken = HttpContext.Request.Headers.Authorization.ToString();
            UserModel? user = SessionManager.GetSession(sessionToken);

            if (sessionToken == null || user == null)
            {
                return Unauthorized("Unauthorized: Invalid or missing session token");
            }

            if (body is null || string.IsNullOrWhiteSpace(body.Transaction) || body.Amount <= 0)
                return BadRequest(new { error = "Bad request: Missing or invalid payment details" });

            PaymentModel newPayment = new()
            {
                Amount = body.Amount,
                Initiator = body.Transaction,
                Created_at = DateTime.UtcNow,
                Completed = null,
                Hash = Guid.NewGuid().ToString("N")
            };

            int newId = PaymentAccess.CreatePayment(newPayment);

            return Ok(new { message = $"Payment created successfully with ID {newId}" });
        }

        [HttpPost("Payments/refund")]
        public async Task<ActionResult<PaymentModel>> PostPaymentsRefunds([FromBody] PaymentRequest body, CancellationToken ct)
        {

            string sessionToken = HttpContext.Request.Headers.Authorization.ToString();
            UserModel? user = SessionManager.GetSession(sessionToken);

            if (sessionToken == null || user == null)
            {
                return Unauthorized("Unauthorized: Invalid or missing session token");
            }

            if (user.Role != "ADMIN")
                return StatusCode(403, new { message = "Access denied" });

            if (body is null || body.Amount <= 0)
                return BadRequest(new { error = "Bad request: Missing or invalid payment details" });

            if (string.IsNullOrWhiteSpace(body.Transaction))
                body.Transaction = $"refund-{DateTime.UtcNow:yyyy-MM-dd-HH}-{Guid.NewGuid().ToString("N").Substring(0, 6)}";

            PaymentModel newPayment = new()
            {
                Amount = -Math.Abs(body.Amount),
                Initiator = body.Transaction,
                Created_at = DateTime.UtcNow,
                Completed = null,
                Hash = Guid.NewGuid().ToString("N")
            };

            int newId = PaymentAccess.CreatePayment(newPayment);

            return Ok(new { message = $"Payment created successfully with ID {newId}" });
        }

        ///////////
        
        /// //POST /parking-lotS
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

        //////////
        public static int CreateParkinglot(ParkingLotModel parkinglot)
    {
        string cs = _config.GetConnectionString("DefaultConnection")!;
        using MySqlConnection conn = new(cs);
        conn.Open();

        const string query = """
        INSERT INTO parking_lots
            (name, location, address, capacity, reserved, tariff, daytariff)
        VALUES
            (@Name, @Location, @Address, @Capacity, @Reserved, @Tariff, @DayTariff);
        SELECT LAST_INSERT_ID();
        """;

        int newId = conn.ExecuteScalar<int>(query, parkinglot);

        return newId;
    }
        
    }
}
    