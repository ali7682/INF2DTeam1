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
    }
}