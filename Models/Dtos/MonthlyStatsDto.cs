namespace Asesorias_API_MVC.Models.Dtos
{
    public class MonthlyStatsDto
    {
        public string Mes { get; set; }
        public decimal IngresosCursos { get; set; }    // Renombrado
        public decimal IngresosAsesorias { get; set; } // Nuevo
    }
}
