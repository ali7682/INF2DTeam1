using Microsoft.AspNetCore.Mvc;
using System.Text.Json.Nodes;

public class PaymentRequest
{
    public string? Transaction { get; set; }
    public decimal Amount { get; set; }
}

namespace NewAPI.Controllers
{
    [ApiController]
    [Route("Payments")]
    public class PaymentController : Controller
    {
        public IConfiguration _config;
        public PaymentController(IConfiguration config)
        {
            _config = config;
            PaymentAccess.SetConfig(config);
        }

        [HttpPost]
        public async Task<ActionResult<PaymentModel>> Payments([FromBody] PaymentRequest body, CancellationToken ct)
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

        [HttpPost("refund")]
        public async Task<ActionResult<PaymentModel>> PaymentsRefund([FromBody] PaymentRequest body, CancellationToken ct)
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

        // GET /payments
        [HttpGet]
        public async Task<IActionResult> GetPayments(CancellationToken ct)
        {
            string sessionToken = HttpContext.Request.Headers.Authorization.ToString();
            UserModel? user = SessionManager.GetSession(sessionToken);

            if (sessionToken == null || user == null)
            {
                return Unauthorized("Unauthorized: Invalid or missing session token");
            }

            List<PaymentModel> payment = await PaymentAccess.GetAllPaymentsAsync(ct);
            if (payment == null)
            {
                return NotFound("NotFound: Payment not found");
            }

            List<PaymentModel> userPayments = payment.Where(x => x.Initiator == user.Username).ToList();
            return Ok(userPayments);
        }

        // GET /payments/{username}
        [HttpGet("{username}")]
        public async Task<IActionResult> GetPaymentsByUserName(string userName, CancellationToken ct)
        {
            string sessionToken = HttpContext.Request.Headers.Authorization.ToString();
            UserModel? user = SessionManager.GetSession(sessionToken);

            if (sessionToken == null || user == null)
            {
                return Unauthorized("Unauthorized: Invalid or missing session token");
            }

            if (!user.IsAdmin())
            {
                return StatusCode(403, new { message = "Forbidden: You do not have access to this payment" });
            }

            UserModel? requestUser = await UserAccess.GetUserByUsernameAsync(userName);
            if (requestUser == null)
            {
                return NotFound("NotFound: User not found");
            }

            List<PaymentModel> payment = await PaymentAccess.GetAllPaymentsAsync(ct);
            if (payment == null)
            {
                return NotFound("NotFound: Payment not found");
            }

            List<PaymentModel> userPayments = await payment.Where(x => x.Initiator == requestUser.Username).ToList();
            
            return Ok(userPayments);
        }
    }
}
    