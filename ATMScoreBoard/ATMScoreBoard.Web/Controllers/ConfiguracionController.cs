using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace ATMScoreBoard.Web.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ConfiguracionController : ControllerBase
    {
        private readonly IConfiguration _configuration;

        public ConfiguracionController(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        [HttpGet("estacion")]
        public IActionResult GetStationConfig()
        {
            // Por ahora, asumimos que solo hay una mesa (MVP) y la "quemamos" en el código.
            // En el futuro, esto se leería del appsettings.json.
            var mesaId = 1;

            return Ok(new { mesaId = mesaId });
        }
    }
}
