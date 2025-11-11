using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Asesorias_API_MVC.Migrations
{
    /// <inheritdoc />
    public partial class UpdateAsesorFieldsToDetailed : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Estudios",
                table: "Asesores");

            migrationBuilder.RenameColumn(
                name: "Experiencia",
                table: "Asesores",
                newName: "ExperienciaLaboral");

            migrationBuilder.AddColumn<int>(
                name: "AnioGraduacion",
                table: "Asesores",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "AniosExperiencia",
                table: "Asesores",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "CampoEstudio",
                table: "Asesores",
                type: "nvarchar(150)",
                maxLength: 150,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Certificaciones",
                table: "Asesores",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "InstitucionEducativa",
                table: "Asesores",
                type: "nvarchar(150)",
                maxLength: 150,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "NivelEstudios",
                table: "Asesores",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AnioGraduacion",
                table: "Asesores");

            migrationBuilder.DropColumn(
                name: "AniosExperiencia",
                table: "Asesores");

            migrationBuilder.DropColumn(
                name: "CampoEstudio",
                table: "Asesores");

            migrationBuilder.DropColumn(
                name: "Certificaciones",
                table: "Asesores");

            migrationBuilder.DropColumn(
                name: "InstitucionEducativa",
                table: "Asesores");

            migrationBuilder.DropColumn(
                name: "NivelEstudios",
                table: "Asesores");

            migrationBuilder.RenameColumn(
                name: "ExperienciaLaboral",
                table: "Asesores",
                newName: "Experiencia");

            migrationBuilder.AddColumn<string>(
                name: "Estudios",
                table: "Asesores",
                type: "nvarchar(1000)",
                maxLength: 1000,
                nullable: false,
                defaultValue: "");
        }
    }
}
