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
            var cs = _config.GetConnectionString("DefaultConnection")!;
            using var conn = new MySqlConnection(cs);
            await conn.OpenAsync(ct);

            const string sql = """
                SELECT 
                    id           AS Id,
                    username     AS Username,
                    password     AS Password,
                    name         AS Name,
                    email        AS Email,
                    phone        AS Phone,
                    CAST(role AS CHAR) AS Role,
                    created_at   AS CreatedAt,
                    birth_year   AS BirthYear,
                    active       AS Active
                FROM users
                WHERE id = @id
                LIMIT 1;
            """;


            var user = await conn.QuerySingleOrDefaultAsync<UserModel>(sql, new { id });
            return user is null ? NotFound() : Ok(user);
        }
    }
}
