using Microsoft.AspNetCore.Mvc;
using System.Security.Cryptography;
using System.Text.Json.Nodes;
using System.Transactions;

public class PaymentRequest
{
    public string Transaction { get; set; }
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
        

        [HttpPost]
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
                Transaction = body.Transaction,
                Amount = body.Amount,
                Initiator = user.Username,
                Created_at = DateTime.UtcNow,
                Completed = 0,
                Hash = Guid.NewGuid().ToString("N")
            };

            int newId = PaymentAccess.CreatePayment(newPayment);

            return Ok(new { message = $"Payment created successfully with ID {newId}" });        }

        [HttpPost("refund")]
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

            string input = user.Id + body.Transaction;
            string TransactionsHash;
            using (var md5 = MD5.Create())
            {
                var bytes = md5.ComputeHash(System.Text.Encoding.UTF8.GetBytes(input));
                TransactionsHash = Convert.ToHexString(bytes).ToLower();
            }
            
            //Console.WriteLine(-Math.Abs(body.Amount));

            PaymentModel newPayment = new()
            {
                Transaction = TransactionsHash,
                Amount = -Math.Abs(body.Amount),
                Initiator = user.Username,
                Created_at = DateTime.UtcNow,
                Completed = 0,
                Hash = Guid.NewGuid().ToString("N")
            };

            int newId = PaymentAccess.CreatePayment(newPayment);

            return Ok(new { message = $"Payment created successfully with ID {newId}" });
        }
    }
}
    