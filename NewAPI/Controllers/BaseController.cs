using Microsoft.AspNetCore.Mvc;
using MySql.Data.MySqlClient;
using Dapper;

namespace NewAPI.Controllers
{
    [Route("")]
    [ApiController]
    public class BaseController: ControllerBase
    {
        private readonly IConfiguration _config;
        private readonly ILogger<BaseController> _logger;

        public BaseController(IConfiguration config, ILogger<BaseController> logger)
        {
            _config = config;
            _logger = logger;
        }

        [HttpGet]
        public OkResult Uptime(CancellationToken ct)
        {
            return Ok();
        }
    }
}
