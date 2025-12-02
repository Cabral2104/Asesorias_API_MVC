using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Asesorias_API_MVC.Migrations
{
    /// <inheritdoc />
    public partial class CamposSolicitud : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ArchivoUrl",
                table: "SolicitudesDeAyuda",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "FechaLimite",
                table: "SolicitudesDeAyuda",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<string>(
                name: "Materia",
                table: "SolicitudesDeAyuda",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ArchivoUrl",
                table: "SolicitudesDeAyuda");

            migrationBuilder.DropColumn(
                name: "FechaLimite",
                table: "SolicitudesDeAyuda");

            migrationBuilder.DropColumn(
                name: "Materia",
                table: "SolicitudesDeAyuda");
        }
    }
}
