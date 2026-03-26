using Infrastructure.Data;
using Microsoft.AspNetCore.Mvc;

namespace Registration.Controllers
{

    [ApiController]
    [Route("health")]
    public class HealthController : ControllerBase
    {
        private readonly RegistrationDbContext _db;

        public HealthController(RegistrationDbContext db)
        {
            _db = db;
        }

        [HttpGet("live")]
        public IActionResult Live()
        {
            return Ok("OK");
        }

        [HttpGet("ready")]
        public async Task<IActionResult> Ready(CancellationToken ct)
        {
            var canConnect = await _db.Database.CanConnectAsync(ct);

            if (!canConnect)
                return StatusCode(503, "DB unavailable");

            return Ok("READY");
        }
    }
}
