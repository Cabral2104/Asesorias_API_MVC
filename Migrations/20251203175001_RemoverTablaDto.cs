using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Asesorias_API_MVC.Migrations
{
    /// <inheritdoc />
    public partial class RemoverTablaDto : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AsesorRatingDto");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "AsesorRatingDto",
                columns: table => new
                {
                    AsesorId = table.Column<int>(type: "int", nullable: false),
                    IngresosGenerados = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    NombreAsesor = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    RatingPromedio = table.Column<double>(type: "float", nullable: false),
                    TotalCalificaciones = table.Column<int>(type: "int", nullable: false),
                    TotalCursos = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                });
        }
    }
}
