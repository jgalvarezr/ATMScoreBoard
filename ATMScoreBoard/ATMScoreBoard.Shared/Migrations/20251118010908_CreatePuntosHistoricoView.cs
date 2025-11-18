using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ATMScoreBoard.Shared.Migrations
{
    /// <inheritdoc />
    public partial class CreatePuntosHistoricoView : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            var sql = @"
                        CREATE VIEW PuntosHistorico
                        AS
                        WITH PartidosEquipos AS (
                            SELECT 
                                e.Id AS EquipoId, 
                                p.Id AS PartidaId,
                                p.Fecha,
                                CASE WHEN p.EquipoGanadorId = e.Id THEN 1 + IIF(p.FueVictoriaImpecable = 1, 1, 0) ELSE 0 END AS Puntos,
                                CASE WHEN e.Id = p.EquipoAId THEN p.EquipoBId ELSE p.EquipoAId END AS RivalId,
                                DATEDIFF(day, p.Fecha, GETDATE()) AS Dias
                            FROM Equipos e
                            JOIN Partidas p ON e.Id = p.EquipoAId OR e.Id = p.EquipoBId
                            WHERE p.EquipoGanadorId IS NOT NULL
                        ),
                        PartidosNumerados AS (
                            SELECT 
                                *,
                                ROW_NUMBER() OVER (PARTITION BY EquipoId ORDER BY Fecha DESC) AS Orden
                            FROM PartidosEquipos
                        )
                        SELECT 
                            EquipoId,
                            PartidaId,
                            Fecha,
                            Puntos,
                            RivalId,
                            Dias,
                            Orden
                        FROM PartidosNumerados;
                    "; 

            migrationBuilder.Sql(sql);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("DROP VIEW PuntosHistorico;");
        }
    }
}
