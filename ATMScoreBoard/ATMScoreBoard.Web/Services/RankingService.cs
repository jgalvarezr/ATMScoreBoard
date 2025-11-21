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

            var partidasParaRanking = await _configService.GetIntAsync("RankingPartidas", 50);
            var diasParaRanking = await _configService.GetIntAsync("RankingDias", 90);

            using var context = _dbContextFactory.CreateDbContext();

            // Esta es tu consulta SQL compleja
            string sql = $@"
                            with JugadoresPorEquipoNumerados AS (
                                        select      ej.EquipoId,
                                                    j.Nombre as NombreJugador,
                                                    Row_number() over(partition by ej.EquipoId order by j.Nombre) as JugadorNum
                                        from        EquipoJugadores ej
                                        join        Jugadores j on ej.JugadorId = j.Id
                                    ),
                                    EquiposConIntegrantesEnColumnas AS (
                                        select      jpn.EquipoId,
                                                    max(case when jpn.JugadorNum = 1 then jpn.NombreJugador end) as JugadorA,
                                                    max(case when jpn.JugadorNum = 2 then jpn.NombreJugador end) as JugadorB
                                        from        JugadoresPorEquipoNumerados jpn
                                        group by    jpn.EquipoId
                                    ),
                                    RankingConPuntos AS (                                        
                                        select      e.EquipoId AS Id, 
                                                    e.JugadorA,
                                                    e.JugadorB,
                                                    sum(p.Puntos) as PuntosRanking
                                        from        EquiposConIntegrantesEnColumnas e
                                        inner join  dbo.PuntosHistorico p on p.EquipoId = e.EquipoId
                                        where       p.Orden <= {{0}} and p.Dias < {{1}} and e.JugadorB is not null
                                        group by    e.EquipoId, e.JugadorA, e.JugadorB 
                                    )
                                    select          Id,
                                                    JugadorA,
                                                    coalesce(JugadorB, '') as JugadorB,
                                                    PuntosRanking,                                        
                                                    dense_rank() over (order by PuntosRanking desc) as PosicionRanking
                                    from            RankingConPuntos
                                    order by        PosicionRanking asc;
                        ";

            var ranking = await context.Set<EstadisticaEquipoColRanking>()
                .FromSqlRaw(sql, partidasParaRanking, diasParaRanking)
                .ToListAsync();

            return ranking;
        }

        public async Task<List<EstadisticaJugadorRanking>> ObtenerRankingJugadoresAsync()
        {

            var partidasParaRanking = await _configService.GetIntAsync("RankingPartidas", 50);
            var diasParaRanking = await _configService.GetIntAsync("RankingDias", 90);

            using var context = _dbContextFactory.CreateDbContext();

            string sql = $@"
                            with puntos as (    select          j.Id, 
                                                                j.Nombre, 
                                                                sum(p.Puntos) AS PuntosRanking	
                                                from        dbo.Jugadores j
                                                inner join  dbo.EquipoJugadores ej on ej.JugadorId = j.Id
                                                INNER JOIN  dbo.PuntosHistorico p on p.EquipoId = ej.EquipoId
                                                where       p.Orden <= {{0}} AND p.Dias < {{1}}
                                                group by    j.Id, j.Nombre),
                            jugadoresPuntos as (select		j.Id, 
			                                                j.Nombre,
                                                            isnull(p.PuntosRanking, 0) as PuntosRanking
                                                from		Jugadores j
                                                left join   puntos p on p.Id = j.id)
                            select      id,
                                        nombre,
                                        PuntosRanking,
                                        dense_rank() over (order by PuntosRanking desc) as PosicionRanking
                            from        jugadoresPuntos 
                            order by    PosicionRanking asc, Nombre asc;";

            var ranking = await context.Set<EstadisticaJugadorRanking>()
                .FromSqlRaw(sql, partidasParaRanking, diasParaRanking)
                .ToListAsync();

            return ranking;
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