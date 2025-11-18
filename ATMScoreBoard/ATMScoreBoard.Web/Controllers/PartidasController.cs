using ATMScoreBoard.Shared;
using ATMScoreBoard.Shared.Models;
using ATMScoreBoard.Web.DTOs;
using ATMScoreBoard.Web.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ATMScoreBoard.Web.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PartidasController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly PartidaService _partidaService;


        public PartidasController(ApplicationDbContext context, PartidaService partidaService)
        {
            _context = context;
            _partidaService = partidaService;
        }

        [HttpGet("actual/pormesa/{mesaId}")]
        public async Task<ActionResult<PartidaActual>> GetPartidaActualPorMesa(int mesaId)
        {
            var partidaActual = await _context.PartidasActuales.FindAsync(mesaId);

            if (partidaActual == null)
            {
                return NotFound();
            }
            return Ok(partidaActual);
        }

        [HttpPost("iniciar")]
        public async Task<ActionResult<PartidaActual>> IniciarPartida([FromBody] IniciarPartidaDto dto)
        {
            try
            {
                var partidaActual = await _partidaService.IniciarPartidaAsync(dto);
                return Ok(partidaActual);
            }
            catch (Exception ex)
            {
                // Devolvemos el mismo tipo de error que antes (Conflict, BadRequest)
                if (ex.Message.Contains("Ya hay una partida"))
                    return Conflict(ex.Message);

                return BadRequest(ex.Message);
            }
        }

        [HttpDelete("actual/pormesa/{mesaId}")]
        public async Task<IActionResult> CancelarPartidaActual(int mesaId)
        {
            var partidaActual = await _context.PartidasActuales.FindAsync(mesaId);
            if (partidaActual == null) return NotFound();

            _context.PartidasActuales.Remove(partidaActual);
            // No limpiamos equipos aquí, porque podrían tener historial.
            // La lógica de equipos huérfanos es compleja y la podemos añadir después si es necesaria.

            await _context.SaveChangesAsync();
            return NoContent();
        }

        [HttpPost("accion")]
        public async Task<IActionResult> RealizarAccion([FromBody] AccionJuegoDto dto)
        {
            try
            {
                await _partidaService.RealizarAccionAsync(dto);
                return Ok(); // La acción fue exitosa
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpGet("chequearestado/{mesaId}")]
        public async Task<ActionResult<ResultadoChequeo>> ChequearEstado(int mesaId)
        {
            try
            {
                var resultado = await _partidaService.ChequearEstadoPartidaAsync(mesaId);
                return Ok(resultado);
            }
            catch (Exception ex)
            {
                return NotFound(ex.Message);
            }
        }
    }
}
