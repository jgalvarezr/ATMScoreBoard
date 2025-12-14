using ATMScoreBoard.Shared;
using ATMScoreBoard.Shared.DTOs;
using Microsoft.EntityFrameworkCore;

namespace ATMScoreBoard.Web.Services
{


    // (Podemos añadir el DTO para el ranking de jugadores aquí más tarde)

    public class RankingService
    {
        private readonly IDbContextFactory<ApplicationDbContext> _dbContextFactory;
        private readonly ConfiguracionService _configService;

        public RankingService(IDbContextFactory<ApplicationDbContext> dbContextFactory, ConfiguracionService configService)
        {
            _dbContextFactory = dbContextFactory;
            _configService = configService;
        }

        public async Task<List<EstadisticaEquipoColRanking>> ObtenerRankingEquiposAsync()
        {
            var partidos = await _configService.GetIntAsync("RankingPartidas", 50);
            var dias = await _configService.GetIntAsync("RankingDias", 90);

            using var context = _dbContextFactory.CreateDbContext();

            // Llamamos al SP usando FromSqlInterpolated
            return await context.Set<EstadisticaEquipoColRanking>()
                .FromSqlInterpolated($"EXEC dbo.RankingEquipo @partidos={partidos}, @dias={dias}")
                .ToListAsync();
        }

        public async Task<List<EstadisticaJugadorRanking>> ObtenerRankingJugadoresAsync()
        {
            var partidos = await _configService.GetIntAsync("RankingPartidas", 50);
            var dias = await _configService.GetIntAsync("RankingDias", 90);

            using var context = _dbContextFactory.CreateDbContext();

            // Llamamos al SP
            return await context.Set<EstadisticaJugadorRanking>()
                .FromSqlInterpolated($"EXEC dbo.RankingJugadores @partidos={partidos}, @dias={dias}")
                .ToListAsync();
        }

        public async Task<RankingParamsDto> ObtenerRankingParamsAsync()
        {
            var partidasParaRanking = await _configService.GetIntAsync("RankingPartidas", 50);
            var diasParaRanking = await _configService.GetIntAsync("RankingDias", 90);
            return new RankingParamsDto()
            {
                PartidasParaRanking = partidasParaRanking,
                DiasParaRanking = diasParaRanking
            };

        }
    }
}