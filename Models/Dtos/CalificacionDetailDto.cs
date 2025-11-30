namespace Asesorias_API_MVC.Models.Dtos
{
    public class CalificacionDetailDto
    {
        public int CalificacionId { get; set; }
        public int Rating { get; set; }
        public string Comentario { get; set; }
        public string NombreEstudiante { get; set; }
        public DateTime Fecha { get; set; }
    }
}
