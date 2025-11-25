using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ATMScoreBoard.Shared.Migrations
{
    /// <inheritdoc />
    public partial class AddLadosIntercambiados : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "LadosIntercambiados",
                table: "PartidasActuales",
                type: "bit",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "LadosIntercambiados",
                table: "PartidasActuales");
        }
    }
}
