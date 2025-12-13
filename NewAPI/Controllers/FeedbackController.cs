using Microsoft.AspNetCore.Mvc;
using NewAPI.Models;
using System.Security.Cryptography;

namespace NewAPI.Controllers
{
    [ApiController]
    [Route("Feedback")]
    public class FeedbackController : Controller
    {
        [HttpPost("Submit")]
        public async Task<IActionResult> Submit([FromBody] FeedbackModel? feedback, CancellationToken ct)
        {
            string token = HttpContext.Request.Headers.Authorization.ToString();
            UserModel? sessionUser = await SessionManager.GetUserFromSession(token);

            if (string.IsNullOrEmpty(token) || sessionUser == null)
                return Unauthorized(new { message = "Unauthorized: Invalid or missing session token" });

            if (feedback == null || feedback.Rating < 1 || feedback.Rating > 5 || string.IsNullOrWhiteSpace(feedback.Description))
                return BadRequest(new { message = "Bad request: Invalid feedback data" });

            // bool doesParkingLotExist = await ParkingLotAccess.DoesParkingLotExistAsync(feedback.ParkingSessionId);

            return Ok(feedback);
        }
    }
}
