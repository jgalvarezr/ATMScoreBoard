using ATMScoreBoard.Shared.Models;
using ATMScoreBoard.Web.Services;
using Microsoft.AspNetCore.Mvc;

namespace ATMScoreBoard.Web.Components.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class RankingController : ControllerBase
    {
        private readonly RankingService _rankingService;

        // Inyectamos el RankingService que acabamos de registrar
        public RankingController(RankingService rankingService)
        {
            _rankingService = rankingService;
        }

        // GET: api/ranking/equipos
        [HttpGet("equipos")]
        public async Task<ActionResult<IEnumerable<EstadisticaEquipoColRanking>>> GetRankingEquipos()
        {
            try
            {                
                int partidasParaRanking = 20; // Ejemplo
                int diasParaRanking = 90;     // Ejemplo

                var ranking = await _rankingService.ObtenerRankingEquiposAsync(partidasParaRanking, diasParaRanking);
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
                int partidasParaRanking = 20;
                int diasParaRanking = 90;

                var ranking = await _rankingService.ObtenerRankingJugadoresAsync(partidasParaRanking, diasParaRanking);
                return Ok(ranking);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error interno al obtener el ranking de jugadores: {ex.Message}");
            }
        }
    }


}