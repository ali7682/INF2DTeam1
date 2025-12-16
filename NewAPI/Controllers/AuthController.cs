using Microsoft.AspNetCore.Mvc;

public class LoginRequest
{
    public string Username { get; set; }
    public string Password { get; set; }
}

public class LoginResponse
{
    public string Message { get; set; }
    public string SessionToken { get; set; }

    public LoginResponse(string message, string sessionToken)
    {
        Message = message;
        SessionToken = sessionToken;
    }
}

public class ChangeProfileRequest
{
    public string Username { get; set; }
}

public class RegisterRequest
{
    public string SessionToken { get; set; }
}

namespace NewAPI.Controllers
{
    [ApiController]
    public class AuthController : Controller
    {
        private readonly IConfiguration _config;

        public AuthController(IConfiguration config)
        {
            _config = config;
            UserAccess.SetConfig(_config);
        }

        [HttpPost("Login")]
        public async Task<IActionResult> Login([FromBody] LoginRequest body, CancellationToken ct)
        {
            if (body is null || string.IsNullOrWhiteSpace(body.Username) || string.IsNullOrWhiteSpace(body.Password))
                return BadRequest(new { error = "Bad request: Missing credentials" });

            UserModel? user = await UserAccess.GetUserByUsernameAsync(body.Username, ct);

            if (user is null || !BCrypt.Net.BCrypt.Verify(body.Password, user.Password))
                return Unauthorized(new { message = "Unauthorized: Invalid credentials" });

            string sessionToken = Guid.NewGuid().ToString("N");

            await SessionManager.AddSession(sessionToken, user.Id, ct);

            LoginResponse loginResponse = new($"User logged in successfully as {user.Username}", sessionToken);

            return Ok(loginResponse);
        }

        [HttpPost("Register")]
        public async Task<IActionResult> Register([FromBody] UserModel body, CancellationToken ct)
        {
            if (body is null || string.IsNullOrWhiteSpace(body.Username) || string.IsNullOrWhiteSpace(body.Password))
                return BadRequest(new { error = "Missing user data" });

            UserModel newUser = new()
            {
                Username = body.Username,
                Password = BCrypt.Net.BCrypt.HashPassword(body.Password, workFactor: 12),
                Name = body.Name,
                Email = body.Email,
                Phone = body.Phone,
                Role = body.Role,
                CreatedAt = DateTime.UtcNow,
                BirthYear = body.BirthYear,
                Active = true,
            };

            int newUserId = await UserAccess.CreateUserAsync(newUser, ct);

            return Ok(new { message = $"User created successfully with ID {newUserId}" });
        }

        [HttpPost("Logout")]
        public async Task<IActionResult> Logout()
        {
            string sessionToken = HttpContext.Request.Headers.Authorization.ToString();

            if (sessionToken == null || sessionToken == string.Empty)
                return Unauthorized(new { message = "Unauthorized: Missing session token" });

            bool doesSessionExist = await SessionManager.DoesSessionExist(sessionToken);

            if (!doesSessionExist)
                return Unauthorized(new { message = "Unauthorized: Invalid session token" });

            await SessionManager.RemoveSession(sessionToken);

            return Ok(new { message = "User logged out successfully" });
        }

        [HttpPut("Profile")]
        public async Task<IActionResult> Profile([FromBody] ChangeProfileRequest body)
        {
            string sessionToken = HttpContext.Request.Headers.Authorization.ToString();
            UserModel? user = await SessionManager.GetUserFromSession(sessionToken);

            if (sessionToken == null || user == null)
            {
                return Unauthorized("Unauthorized: Invalid or missing session token");
            }

            user.Username = body.Username;

            bool success = await user.Update();

            return success ? Ok("Ok: Changed username") : NotFound("NotFound: No rows were changed");
        }

        [HttpGet("Profile")]
        public async Task<IActionResult> Profile()
        {
            string sessionToken = HttpContext.Request.Headers.Authorization.ToString();
            UserModel? user = await SessionManager.GetUserFromSession(sessionToken);

            if (sessionToken == null || user == null)
            {
                return Unauthorized("Unauthorized: Invalid or missing session token");
            }

            return Ok(user);
        }
    }
}
    