using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Asesorias_API_MVC.Migrations
{
    /// <inheritdoc />
    public partial class AddOfertasSolicitud : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "AsesorRatingDto",
                columns: table => new
                {
                    AsesorId = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    NombreAsesor = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    TotalCursos = table.Column<int>(type: "int", nullable: false),
                    TotalCalificaciones = table.Column<int>(type: "int", nullable: false),
                    RatingPromedio = table.Column<double>(type: "float", nullable: false),
                    IngresosGenerados = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false)
                },
                constraints: table =>
                {
                });

            migrationBuilder.CreateTable(
                name: "OfertasSolicitud",
                columns: table => new
                {
                    OfertaId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    SolicitudId = table.Column<int>(type: "int", nullable: false),
                    AsesorId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    PrecioOferta = table.Column<decimal>(type: "decimal(10,2)", nullable: false),
                    Mensaje = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    FueAceptada = table.Column<bool>(type: "bit", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ModifiedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OfertasSolicitud", x => x.OfertaId);
                    table.ForeignKey(
                        name: "FK_OfertasSolicitud_Asesores_AsesorId",
                        column: x => x.AsesorId,
                        principalTable: "Asesores",
                        principalColumn: "UsuarioId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_OfertasSolicitud_SolicitudesDeAyuda_SolicitudId",
                        column: x => x.SolicitudId,
                        principalTable: "SolicitudesDeAyuda",
                        principalColumn: "SolicitudId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_OfertasSolicitud_AsesorId",
                table: "OfertasSolicitud",
                column: "AsesorId");

            migrationBuilder.CreateIndex(
                name: "IX_OfertasSolicitud_SolicitudId",
                table: "OfertasSolicitud",
                column: "SolicitudId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AsesorRatingDto");

            migrationBuilder.DropTable(
                name: "OfertasSolicitud");
        }
    }
}
