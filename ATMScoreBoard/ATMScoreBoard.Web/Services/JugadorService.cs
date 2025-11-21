using ATMScoreBoard.Shared;
using ATMScoreBoard.Shared.Models;
using ATMScoreBoard.Web.Hubs; // Necesario para MarcadorHub
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
namespace ATMScoreBoard.Web.Services
{
    public class JugadorService
    {
        private readonly IDbContextFactory<ApplicationDbContext> _dbContextFactory;
        private readonly IHubContext<MarcadorHub> _hubContext;


        public JugadorService(IDbContextFactory<ApplicationDbContext> dbContextFactory, IHubContext<MarcadorHub> hubContext)
        {
            _dbContextFactory = dbContextFactory;
            _hubContext = hubContext;
        }

        public async Task GuardarJugadorAsync(Jugador jugador)
        {
            using var context = _dbContextFactory.CreateDbContext();

            // Verificamos si el nombre ya existe
            var nombreNormalizado = jugador.Nombre.Trim().ToLower();
            var existe = await context.Jugadores
                .AnyAsync(j => j.Nombre.ToLower() == nombreNormalizado && j.Id != jugador.Id);

            if (existe)
            {
                throw new Exception("Este nombre de jugador ya está en uso.");
            }

            if (jugador.Id == 0) // Es nuevo
            {
                context.Jugadores.Add(jugador);
            }
            else // Es una actualización
            {
                context.Jugadores.Update(jugador);
            }

            await context.SaveChangesAsync();
                        
            await _hubContext.Clients.All.SendAsync("RankingsActualizados");
        }
    }
}

