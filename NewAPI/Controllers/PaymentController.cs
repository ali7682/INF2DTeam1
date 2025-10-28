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
    }
}
    