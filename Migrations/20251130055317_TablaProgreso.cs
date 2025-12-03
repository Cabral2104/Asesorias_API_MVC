using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Asesorias_API_MVC.Migrations
{
    /// <inheritdoc />
    public partial class TablaProgreso : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Progresos",
                columns: table => new
                {
                    ProgresoId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    EstudianteId = table.Column<int>(type: "int", nullable: false),
                    LeccionId = table.Column<int>(type: "int", nullable: false),
                    EstaCompletada = table.Column<bool>(type: "bit", nullable: false),
                    FechaCompletado = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Progresos", x => x.ProgresoId);
                    table.ForeignKey(
                        name: "FK_Progresos_AspNetUsers_EstudianteId",
                        column: x => x.EstudianteId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Progresos_Lecciones_LeccionId",
                        column: x => x.LeccionId,
                        principalTable: "Lecciones",
                        principalColumn: "LeccionId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Progresos_EstudianteId_LeccionId",
                table: "Progresos",
                columns: new[] { "EstudianteId", "LeccionId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Progresos_LeccionId",
                table: "Progresos",
                column: "LeccionId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Progresos");
        }
    }
}
