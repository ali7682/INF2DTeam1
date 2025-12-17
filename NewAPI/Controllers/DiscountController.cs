using Microsoft.AspNetCore.Mvc;

[ApiController]
[Route("Discounts")]
public class DiscountController : ControllerBase
{
    // GET: discount
    [HttpGet]
    public async Task<ActionResult<IEnumerable<DiscountModel>>> GetAllAsync()
    {
        var discounts = await DiscountAcces.GetAllDiscountCodesAsync(HttpContext.RequestAborted);
        return Ok(discounts);
    }

    // discount/{id}/deactivate
    [HttpPatch("{id}/deactivate")]
    public async Task<ActionResult> DeactivateAsync(int id)
    {
        bool success = await DiscountAcces.DeactivateDiscountByIdAsync(id);

        if (!success)
            return NotFound(new { message = "Discount code not found" });

        // Return a success message
        return Ok(new { message = $"Discount code {id} deactivated successfully" });
    }

    // discount/{id}/activate
    [HttpPatch("{id}/activate")]
    public async Task<ActionResult> ActivateAsync(int id)
    {
        bool success = await DiscountAcces.ActivateDiscountByIdAsync(id);

        if (!success)
            return NotFound(new { message = "Discount code not found" });

        // Return a success message
        return Ok(new { message = $"Discount code {id} activated successfully" });
    }

    // discount/{id}/activate
    [HttpPatch("{id}/Maxuses")]
    public async Task<ActionResult> SetUsesAsync(int id, int maxUse)
    {
        bool success = await DiscountAcces.SetMaxUsesAsync(id, maxUse);

        if (!success)
            return NotFound(new { message = "Discount code not found" });

        // Return a success message
        return Ok(new { message = $"Succesfully changed MaxUses from Discount code {id} " });
    }

    // POST: api/discount
    [HttpPost]
    public async Task<IActionResult> PostDiscount([FromBody] DiscountModel body, CancellationToken ct)
    {
        // Get session token and user
        string sessionToken = HttpContext.Request.Headers.Authorization.ToString();
        UserModel? user = SessionManager.GetSession(sessionToken);

        if (string.IsNullOrWhiteSpace(sessionToken) || user == null)
            return Unauthorized("Unauthorized: Invalid or missing session token");

        if (user.Role != "ADMIN")
            return StatusCode(403, new { message = "Access denied" });

        // Validate input
        if (body is null || string.IsNullOrWhiteSpace(body.Code) || body.Percentage <= 0)
            return BadRequest(new { error = "Bad request: Missing or invalid discount details" });

        // Insert into DB
        int newId = await DiscountAcces.CreateDiscountAsync(body, ct);

        return Ok(new { message = $"Discount created successfully with ID {newId}" });
    }
}