using Microsoft.AspNetCore.Mvc;

namespace NewAPI.Controllers
{
    [ApiController]
    [Route("Billings")]
    public class PaymentDetailsController : ControllerBase
    {
        private readonly IConfiguration _config;

        public PaymentDetailsController(IConfiguration config)
        {
            _config = config;
            PaymentDetailsAccess.SetConfig(_config);
        }

        // GET /billings
        [HttpGet]
        public async IActionResult GetBilling(CancellationToken ct)
        {
            string sessionToken = HttpContext.Request.Headers.Authorization.ToString();
            UserModel? user = SessionManager.GetSession(sessionToken);

            if (sessionToken == null || user == null)
            {
                return Unauthorized("Unauthorized: Invalid or missing session token");
            }

            List<PaymentDetailsModel> paymentDetails = await PaymentDetailsAccess.GetAllPaymentDetails(ct);

            if (paymentDetails == null)
            {
                return NotFound("NotFound: Billing not found");
            }

            List<PaymentDetailsModel> userPaymentDetails = await paymentDetails.Where(x => x.Issuer == user.Username).ToList();

            return Ok(userPaymentDetails);
        }

        // GET /billings/{username}
        [HttpGet("{userName}")]
        public async Task<IActionResult> GetBillingByUser(string userName, CancellationToken ct)
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

            UserModel? requestUser = await UserAccess.GetUserByUsernameAsync(userName);

            if (requestUser == null)
            {
                return NotFound("NotFound: User not found");
            }

            List<PaymentDetailsModel> paymentDetails = await PaymentDetailsAccess.GetAllPaymentDetails(ct);

            if (paymentDetails == null)
            {
                return NotFound("NotFound: Billing not found");
            }

            List<PaymentDetailsModel> userPaymentDetails = await paymentDetails.Where(x => x.Issuer == requestUser.Username).ToList();

            return Ok(userPaymentDetails);
        }
    }
}