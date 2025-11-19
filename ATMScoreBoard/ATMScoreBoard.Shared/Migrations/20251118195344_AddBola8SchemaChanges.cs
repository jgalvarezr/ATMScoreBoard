using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ATMScoreBoard.Shared.Migrations
{
    /// <inheritdoc />
    public partial class AddBola8SchemaChanges : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<int>(
                name: "EquipoId",
                table: "PartidasActualesBolas",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.CreateTable(
                name: "RankingEquipos",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false),
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
                    PuntosRanking = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "RankingEquipos");

            migrationBuilder.DropTable(
                name: "RankingJugadores");

            migrationBuilder.AlterColumn<int>(
                name: "EquipoId",
                table: "PartidasActualesBolas",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);
        }
    }
}
