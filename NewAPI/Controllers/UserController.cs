using Microsoft.AspNetCore.Mvc;
using MySql.Data.MySqlClient;
using Dapper;

namespace NewAPI.Controllers
{
    [ApiController]
    [Route("Users")]
    public class UsersController: ControllerBase
    {
        private readonly IConfiguration _config;
        private readonly ILogger<UsersController> _logger;

        public UsersController(IConfiguration config, ILogger<UsersController> logger)
        {
            _config = config;
            _logger = logger;
        }

        [HttpGet("{id:int}")]
        public async Task<ActionResult<UserModel>> Get(int id, CancellationToken ct)
        {
            UserModel user = UserAccess.GetUserById(id);

            return user is null ? NotFound() : Ok(user);
        }
    }
}
