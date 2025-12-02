namespace Asesorias_API_MVC.Models.Dtos
{
    public class AsesorRatingDto
    {
        public int AsesorId { get; set; }
        public string NombreAsesor { get; set; }
        public int TotalCursos { get; set; }
        public int TotalCalificaciones { get; set; }
        public double RatingPromedio { get; set; }

        // --- DESGLOSE DE INGRESOS ---
        public decimal IngresosCursos { get; set; }     // Dinero de Postgres (Cursos)
        public decimal IngresosAsesorias { get; set; }  // Dinero de SQL Server (Solicitudes)
        public decimal IngresosGenerados { get; set; }  // La suma de ambos
    }
}
