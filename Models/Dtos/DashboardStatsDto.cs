namespace Asesorias_API_MVC.Models.Dtos
{
    public class DashboardStatsDto
    {
        public int TotalUsuarios { get; set; }
        public int TotalAsesoresAprobados { get; set; }
        public int TotalCursosPublicados { get; set; }
        public decimal IngresosTotales { get; set; }
        public int CalificacionesTotales { get; set; }
        public double RatingPromedioGlobal { get; set; }
    }
}
