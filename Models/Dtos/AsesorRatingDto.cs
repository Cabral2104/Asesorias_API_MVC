namespace Asesorias_API_MVC.Models.Dtos
{
    public class AsesorRatingDto
    {
        public string AsesorId { get; set; }
        public string NombreAsesor { get; set; }
        public int TotalCursos { get; set; }
        public int TotalCalificaciones { get; set; }
        public double RatingPromedio { get; set; }
        public decimal IngresosGenerados { get; set; }
    }
}
