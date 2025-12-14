using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ATMScoreBoard.Shared.Migrations
{
    /// <inheritdoc />
    public partial class InitialMigration : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Configuraciones",
                columns: table => new
                {
                    Clave = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Valor = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Configuraciones", x => x.Clave);
                });

            migrationBuilder.CreateTable(
                name: "Equipos",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    IsIndividual = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Equipos", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Jugadores",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Nombre = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    FechaCreacion = table.Column<DateTime>(type: "datetime2", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Jugadores", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Mesas",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Nombre = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Mesas", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "PartidasActualesBolas",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    MesaId = table.Column<int>(type: "int", nullable: false),
                    EquipoId = table.Column<int>(type: "int", nullable: true),
                    NumeroBola = table.Column<int>(type: "int", nullable: false),
                    Timestamp = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PartidasActualesBolas", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "RankingEquipos",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false),
                    PosicionRanking = table.Column<long>(type: "bigint", nullable: false),
                    JugadorA = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    JugadorB = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    PuntosRanking = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                });

            migrationBuilder.CreateTable(
                name: "RankingJugadores",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false),
                    Nombre = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    PuntosRanking = table.Column<int>(type: "int", nullable: false),
                    PosicionRanking = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                });

            migrationBuilder.CreateTable(
                name: "Partidas",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    EquipoAId = table.Column<int>(type: "int", nullable: false),
                    EquipoBId = table.Column<int>(type: "int", nullable: false),
                    EquipoGanadorId = table.Column<int>(type: "int", nullable: true),
                    TipoJuegoId = table.Column<int>(type: "int", nullable: false),
                    Fecha = table.Column<DateTime>(type: "datetime2", nullable: false),
                    FueVictoriaImpecable = table.Column<bool>(type: "bit", nullable: false),
                    TipoJuego = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Partidas", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Partidas_Equipos_EquipoAId",
                        column: x => x.EquipoAId,
                        principalTable: "Equipos",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Partidas_Equipos_EquipoBId",
                        column: x => x.EquipoBId,
                        principalTable: "Equipos",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "PartidasActuales",
                columns: table => new
                {
                    MesaId = table.Column<int>(type: "int", nullable: false),
                    TipoJuegoId = table.Column<int>(type: "int", nullable: false),
                    EquipoAId = table.Column<int>(type: "int", nullable: false),
                    EquipoBId = table.Column<int>(type: "int", nullable: false),
                    BandaEquipoA = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    EquipoLisasId = table.Column<int>(type: "int", nullable: true),
                    FechaInicio = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PartidasActuales", x => x.MesaId);
                    table.ForeignKey(
                        name: "FK_PartidasActuales_Equipos_EquipoAId",
                        column: x => x.EquipoAId,
                        principalTable: "Equipos",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_PartidasActuales_Equipos_EquipoBId",
                        column: x => x.EquipoBId,
                        principalTable: "Equipos",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "EquipoJugadores",
                columns: table => new
                {
                    EquipoId = table.Column<int>(type: "int", nullable: false),
                    JugadorId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EquipoJugadores", x => new { x.EquipoId, x.JugadorId });
                    table.ForeignKey(
                        name: "FK_EquipoJugadores_Equipos_EquipoId",
                        column: x => x.EquipoId,
                        principalTable: "Equipos",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_EquipoJugadores_Jugadores_JugadorId",
                        column: x => x.JugadorId,
                        principalTable: "Jugadores",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_EquipoJugadores_JugadorId",
                table: "EquipoJugadores",
                column: "JugadorId");

            migrationBuilder.CreateIndex(
                name: "IX_Jugadores_Nombre",
                table: "Jugadores",
                column: "Nombre",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Partidas_EquipoAId",
                table: "Partidas",
                column: "EquipoAId");

            migrationBuilder.CreateIndex(
                name: "IX_Partidas_EquipoBId",
                table: "Partidas",
                column: "EquipoBId");

            migrationBuilder.CreateIndex(
                name: "IX_PartidasActuales_EquipoAId",
                table: "PartidasActuales",
                column: "EquipoAId");

            migrationBuilder.CreateIndex(
                name: "IX_PartidasActuales_EquipoBId",
                table: "PartidasActuales",
                column: "EquipoBId");

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

            migrationBuilder.DropTable(
                name: "Configuraciones");

            migrationBuilder.DropTable(
                name: "EquipoJugadores");

            migrationBuilder.DropTable(
                name: "Mesas");

            migrationBuilder.DropTable(
                name: "Partidas");

            migrationBuilder.DropTable(
                name: "PartidasActuales");

            migrationBuilder.DropTable(
                name: "PartidasActualesBolas");

            migrationBuilder.DropTable(
                name: "RankingEquipos");

            migrationBuilder.DropTable(
                name: "RankingJugadores");

            migrationBuilder.DropTable(
                name: "Jugadores");

            migrationBuilder.DropTable(
                name: "Equipos");
        }
    }
}
