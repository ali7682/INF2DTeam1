using Microsoft.AspNetCore.Mvc;
using System.Security.Cryptography;
using System.Text.Json.Nodes;
using System.Transactions;

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
        private readonly IConfiguration _config;

        public PaymentController(IConfiguration config)
        {
            _config = config;
            ParkingLotAccess.SetConfig(_config);
        }

        // POST /payments
        [HttpPost]
        public async Task<IActionResult> PostPayments([FromBody] PaymentRequest body, CancellationToken ct)
        {
            string sessionToken = HttpContext.Request.Headers.Authorization.ToString();
            UserModel? user = await SessionManager.GetUserFromSession(sessionToken);

            if (sessionToken == null || user == null)
            {
                return Unauthorized("Unauthorized: Invalid or missing session token");
            }

            if (body is null || string.IsNullOrWhiteSpace(body.Transaction) || body.Amount <= 0)
                return BadRequest(new { error = "Bad request: Missing or invalid payment details" });

            PaymentModel newPayment = new()
            {
                Transaction = body.Transaction,
                Amount = body.Amount,
                Initiator = user.Username,
                Created_at = DateTime.UtcNow,
                Completed = 0,
                Hash = Guid.NewGuid().ToString("N")
            };

            int newId = await PaymentAccess.CreatePaymentAsync(newPayment, ct);

            return Ok(new { message = $"Payment created successfully with ID {newId}" });
        }

        // POST /payments/refund
        [HttpPost("refund")]
        public async Task<IActionResult> PostPaymentsRefunds([FromBody] PaymentRequest body, CancellationToken ct)
        {

            string sessionToken = HttpContext.Request.Headers.Authorization.ToString();
            UserModel? user = await SessionManager.GetUserFromSession(sessionToken);

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

            string input = user.Id + body.Transaction;
            string TransactionsHash;
            using (var md5 = MD5.Create())
            {
                var bytes = md5.ComputeHash(System.Text.Encoding.UTF8.GetBytes(input));
                TransactionsHash = Convert.ToHexString(bytes).ToLower();
            }

            PaymentModel newPayment = new()
            {
                Transaction = TransactionsHash,
                Amount = -Math.Abs(body.Amount),
                Initiator = user.Username,
                Created_at = DateTime.UtcNow,
                Completed = 0,
                Hash = Guid.NewGuid().ToString("N")
            };

            int newId = await PaymentAccess.CreatePaymentAsync(newPayment, ct);

            return Ok(new { message = $"Payment created successfully with ID {newId}" });
        }

        // GET /payments
        [HttpGet]
        public async Task<IActionResult> GetPayments(CancellationToken ct)
        {
            string sessionToken = HttpContext.Request.Headers.Authorization.ToString();
            UserModel? user = await SessionManager.GetUserFromSession(sessionToken);

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
            UserModel? user = await SessionManager.GetUserFromSession(sessionToken);

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

            List<PaymentModel> userPayments = payment.Where(x => x.Initiator == requestUser.Username).ToList();
            
            return Ok(userPayments);
        }
    }
}
    