namespace Asesorias_API_MVC.Models.Dtos
{
    public class AdminAsesoriasStatsDto
    {
        public int TotalAsesoriasCerradas { get; set; }
        public decimal IngresosTotalesAsesorias { get; set; }
        public List<AsesorJobsDto> UltimasAsesorias { get; set; }
    }
}
