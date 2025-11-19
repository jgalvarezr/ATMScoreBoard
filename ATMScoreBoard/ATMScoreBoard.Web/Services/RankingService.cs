using ATMScoreBoard.Shared;
using ATMScoreBoard.Shared.Models;
using Microsoft.EntityFrameworkCore;

namespace ATMScoreBoard.Web.Services
{


    // (Podemos añadir el DTO para el ranking de jugadores aquí más tarde)

    public class RankingService
    {
        private readonly IDbContextFactory<ApplicationDbContext> _dbContextFactory;

        public RankingService(IDbContextFactory<ApplicationDbContext> dbContextFactory)
        {
            _dbContextFactory = dbContextFactory;
        }

        public async Task<List<EstadisticaEquipoColRanking>> ObtenerRankingEquiposAsync(int partidasParaRanking, int diasParaRanking)
        {
            using var context = _dbContextFactory.CreateDbContext();

            // Esta es tu consulta SQL compleja
            string sql = $@"
                            WITH 
                            JugadoresPorEquipoNumerados AS (
                                SELECT 
                                    ej.EquipoId,
                                    j.Nombre AS NombreJugador,
                                    ROW_NUMBER() OVER(PARTITION BY ej.EquipoId ORDER BY j.Nombre) AS JugadorNum
                                FROM dbo.EquipoJugadores ej
                                JOIN dbo.Jugadores j ON ej.JugadorId = j.Id
                            ),
                            EquiposConIntegrantesEnColumnas AS (
                                SELECT
                                    jpn.EquipoId,
                                    MAX(CASE WHEN jpn.JugadorNum = 1 THEN jpn.NombreJugador END) AS JugadorA,
                                    MAX(CASE WHEN jpn.JugadorNum = 2 THEN jpn.NombreJugador END) AS JugadorB
                                FROM JugadoresPorEquipoNumerados jpn
                                GROUP BY jpn.EquipoId
                            ),
                            RankingFinal AS (
                                SELECT 
                                    e.EquipoId AS Id, 
                                    e.JugadorA,
                                    e.JugadorB,
                                    SUM(p.Puntos) AS PuntosRanking
                                FROM EquiposConIntegrantesEnColumnas e
                                INNER JOIN dbo.PuntosHistorico p ON p.EquipoId = e.EquipoId
                                WHERE p.Orden <= {{0}} AND p.Dias < {{1}}
                                GROUP BY e.EquipoId, e.JugadorA, e.JugadorB
                            )
                            SELECT 
                                Id,
                                JugadorA,
                                COALESCE(JugadorB, '') AS JugadorB,
                                PuntosRanking
                            FROM RankingFinal
                            ORDER BY PuntosRanking DESC;
                        ";

            // EF Core necesita que el DTO esté registrado para poder usar FromSqlRaw sobre él
            // La forma más fácil es añadir un DbSet<T> sin clave al DbContext.
            // OJO: Hay que hacer un pequeño ajuste en DbContext.

            var ranking = await context.Set<EstadisticaEquipoColRanking>()
                .FromSqlRaw(sql, partidasParaRanking, diasParaRanking)
                .ToListAsync();

            return ranking;
        }

        public async Task<List<EstadisticaJugadorRanking>> ObtenerRankingJugadoresAsync(int partidasParaRanking, int diasParaRanking)
        {
            using var context = _dbContextFactory.CreateDbContext();

            string sql = $@"
                            with ranking as (   select          j.Id, 
                                                                j.Nombre, 
                                                                sum(p.Puntos) AS PuntosRanking	
                                                from        dbo.Jugadores j
                                                inner join  dbo.EquipoJugadores ej on ej.JugadorId = j.Id
                                                INNER JOIN  dbo.PuntosHistorico p on p.EquipoId = ej.EquipoId
                                                where       p.Orden <= 20 AND p.Dias < 90
                                                group by    j.Id, j.Nombre)

                            select		j.Id, 
			                            j.Nombre,
                                        isnull(r.PuntosRanking, 0) as PuntosRanking
                            from		Jugadores j
                            left join   ranking r on r.Id = j.id
                            order by    PuntosRanking desc, j.Nombre;";

            var ranking = await context.Set<EstadisticaJugadorRanking>()
                .FromSqlRaw(sql, partidasParaRanking, diasParaRanking)
                .ToListAsync();

            return ranking;
        }
    }
}