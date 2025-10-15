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
        public async Task<ActionResult<PaymentModel>> Payments([FromBody] PaymentRequest body, CancellationToken ct)
        {
            if (body is null || string.IsNullOrWhiteSpace(body.Transaction) || body.Amount <= 0)
                return BadRequest(new { error = "Bad request: Missing or invalid payment details" });

            PaymentModel newPayment = new()
            {
                Amount = body.Amount,
                Initiator = body.Transaction,
                Created_at = DateTime.UtcNow,
                Completed = null,
                Hash = 
            };

            int newId = PaymentAcces.CreatePayment(newPayment);

            return Ok(new { message = $"Payment created successfully with ID {newId}" });
        }
    }
}
    