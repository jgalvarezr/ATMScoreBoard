using ATMScoreBoard.Shared;
using ATMScoreBoard.Shared.DTOs;
using ATMScoreBoard.Web.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ATMScoreBoard.Web.Components.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class RankingController : ControllerBase
    {
        private readonly IDbContextFactory<ApplicationDbContext> _dbContextFactory;
        private readonly RankingService _rankingService;
        private readonly ConfiguracionService _configService;

        // Inyectamos el RankingService que acabamos de registrar
        public RankingController(IDbContextFactory<ApplicationDbContext> dbContextFactory, RankingService rankingService, ConfiguracionService configuracionService)
        {
            _dbContextFactory = dbContextFactory;
            _rankingService = rankingService;
            _configService = configuracionService;
        }

        // GET: api/ranking/equipos
        [HttpGet("equipos")]
        public async Task<ActionResult<IEnumerable<EstadisticaEquipoColRanking>>> GetRankingEquipos()
        {
            try
            {
                var ranking = await _rankingService.ObtenerRankingEquiposAsync();
                return Ok(ranking);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error interno al obtener el ranking de equipos: {ex.Message}");
            }
        }

        // GET: api/ranking/jugadores
        [HttpGet("jugadores")]
        public async Task<ActionResult<IEnumerable<EstadisticaJugadorRanking>>> GetRankingJugadores()
        {
            try
            {
                var ranking = await _rankingService.ObtenerRankingJugadoresAsync();
                return Ok(ranking);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error interno al obtener el ranking de jugadores: {ex.Message}");
            }
        }
        
        // GET: api/ranking/params
        [HttpGet("params")]
        public async Task<ActionResult<RankingParamsDto>> GetRankingParams()
        {
            try
            {
                var rankingParams = await _rankingService.ObtenerRankingParamsAsync();
                return Ok(rankingParams);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error interno al obtener el ranking de jugadores: {ex.Message}");
            }
        }
    }


}