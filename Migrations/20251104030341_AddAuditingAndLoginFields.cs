using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Asesorias_API_MVC.Migrations
{
    /// <inheritdoc />
    public partial class AddAuditingAndLoginFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "FechaCreacion",
                table: "SolicitudesDeAyuda",
                newName: "ModifiedAt");

            migrationBuilder.RenameColumn(
                name: "FechaInscripcion",
                table: "Inscripciones",
                newName: "ModifiedAt");

            migrationBuilder.RenameColumn(
                name: "FechaCreacion",
                table: "Cursos",
                newName: "ModifiedAt");

            migrationBuilder.RenameColumn(
                name: "FechaCreacion",
                table: "Asesores",
                newName: "ModifiedAt");

            migrationBuilder.AddColumn<DateTime>(
                name: "CreatedAt",
                table: "SolicitudesDeAyuda",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<DateTime>(
                name: "CreatedAt",
                table: "Lecciones",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<DateTime>(
                name: "ModifiedAt",
                table: "Lecciones",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<DateTime>(
                name: "CreatedAt",
                table: "Inscripciones",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<DateTime>(
                name: "CreatedAt",
                table: "Cursos",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<DateTime>(
                name: "CreatedAt",
                table: "AspNetUsers",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<DateTime>(
                name: "LastLogin",
                table: "AspNetUsers",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "ModifiedAt",
                table: "AspNetUsers",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<DateTime>(
                name: "CreatedAt",
                table: "Asesores",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CreatedAt",
                table: "SolicitudesDeAyuda");

            migrationBuilder.DropColumn(
                name: "CreatedAt",
                table: "Lecciones");

            migrationBuilder.DropColumn(
                name: "ModifiedAt",
                table: "Lecciones");

            migrationBuilder.DropColumn(
                name: "CreatedAt",
                table: "Inscripciones");

            migrationBuilder.DropColumn(
                name: "CreatedAt",
                table: "Cursos");

            migrationBuilder.DropColumn(
                name: "CreatedAt",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "LastLogin",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "ModifiedAt",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "CreatedAt",
                table: "Asesores");

            migrationBuilder.RenameColumn(
                name: "ModifiedAt",
                table: "SolicitudesDeAyuda",
                newName: "FechaCreacion");

            migrationBuilder.RenameColumn(
                name: "ModifiedAt",
                table: "Inscripciones",
                newName: "FechaInscripcion");

            migrationBuilder.RenameColumn(
                name: "ModifiedAt",
                table: "Cursos",
                newName: "FechaCreacion");

            migrationBuilder.RenameColumn(
                name: "ModifiedAt",
                table: "Asesores",
                newName: "FechaCreacion");
        }
    }
}
