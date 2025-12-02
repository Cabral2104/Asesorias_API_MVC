namespace Asesorias_API_MVC.Models.Dtos
{
    public class AsesorJobsDto
    {
        public int SolicitudId { get; set; }
        public string Materia { get; set; }
        public string Tema { get; set; }
        public string Descripcion { get; set; }
        public DateTime FechaLimite { get; set; }
        public string ArchivoUrl { get; set; }
        public decimal Precio { get; set; }
        public string NombreEstudiante { get; set; }
        public string EmailEstudiante { get; set; }
        public DateTime FechaAceptacion { get; set; }

        // --- NUEVOS CAMPOS ---
        public int? Rating { get; set; }
        public string? Comentario { get; set; }
    }
}
