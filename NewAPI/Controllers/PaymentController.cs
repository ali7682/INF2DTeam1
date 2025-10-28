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
    public class PaymentController : Controller
    {
        public PaymentController(IConfiguration config) { }

        [HttpPost("Payments")]
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

        [HttpPost("Payments/refund")]
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

        [HttpGet]
        public IActionResult GetPayments()
        {
            string sessionToken = HttpContext.Request.Headers.Authorization.ToString();
            UserModel? user = SessionManager.GetSession(sessionToken);

            if (sessionToken == null || user == null)
            {
                return Unauthorized("Unauthorized: Invalid or missing session token");
            }

            List<PaymentModel> payment = PaymentAccess.GetAllPayments();
            if (payment == null)
            {
                return NotFound("NotFound: Payment not found");
            }
            
            List<PaymentModel> userPayments = payment.Where(x => x.Initiator == user.Username).ToList();
            return Ok(userPayments);
        }

        [HttpGet("{username}")]
        public IActionResult GetPaymentsByUserName(string userName)
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

            UserModel? requestUser = UserAccess.GetUserByUsername(userName);
            if (requestUser == null)
            {
                return NotFound("NotFound: User not found");
            }

            List<PaymentModel> payment = PaymentAccess.GetAllPayments();
            if (payment == null)
            {
                return NotFound("NotFound: Payment not found");
            }

            List<PaymentModel> userPayments = payment.Where(x => x.Initiator == requestUser.Username).ToList();
            
            return Ok(userPayments);
        }

        [HttpGet("billing/{pId:int}")]
        public IActionResult GetBilling(int pId)
        {
            string sessionToken = HttpContext.Request.Headers.Authorization.ToString();
            UserModel? user = SessionManager.GetSession(sessionToken);

            if (sessionToken == null || user == null)
            {
                return Unauthorized("Unauthorized: Invalid or missing session token");
            }

            PaymentModel? payment = PaymentAccess.GetPaymentByTransactionId(pId);

            if (payment == null)
            {
                return NotFound("NotFound: Billing not found");
            }

            return Ok(pId);
        }

        [HttpGet("billing/{userName}/{pId:int}")]
        public IActionResult GetBillingByUser(string userName, int pId)
        {
            string sessionToken = HttpContext.Request.Headers.Authorization.ToString();
            UserModel? user = SessionManager.GetSession(sessionToken);

            if (sessionToken == null || user == null)
            {
                return Unauthorized("Unauthorized: Invalid or missing session token");
            }

            if (!user.IsAdmin())
            {
                return StatusCode(403, new { message = "Forbidden: you do not have access to biling" });
            }

            UserModel? requestUser = UserAccess.GetUserByUsername(userName);

            if (requestUser == null)
            {
                return NotFound("NotFound: User not found");
            }

            PaymentModel? payment = PaymentAccess.GetPaymentByTransactionId(pId);

            if (payment == null)
            {
                return NotFound("NotFound: Billing not found");
            }

            if (!(user.Id == payment.TransactionId))
            {
                return StatusCode(403, new { message = "Forbidden: You do not have access to this billing" });
            }

            return Ok(payment);
        }
    }
}
    