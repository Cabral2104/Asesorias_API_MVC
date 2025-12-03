using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Asesorias_API_MVC.Migrations.AnalyticsDb
{
    /// <inheritdoc />
    public partial class UpdateCalificacionSchema : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<int>(
                name: "CursoId",
                table: "Calificaciones",
                type: "integer",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "integer");

            migrationBuilder.AddColumn<int>(
                name: "AsesorId",
                table: "Calificaciones",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "SolicitudId",
                table: "Calificaciones",
                type: "integer",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AsesorId",
                table: "Calificaciones");

            migrationBuilder.DropColumn(
                name: "SolicitudId",
                table: "Calificaciones");

            migrationBuilder.AlterColumn<int>(
                name: "CursoId",
                table: "Calificaciones",
                type: "integer",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "integer",
                oldNullable: true);
        }
    }
}
