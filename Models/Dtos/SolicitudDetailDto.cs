namespace Asesorias_API_MVC.Models.Dtos
{
    public class SolicitudDetailDto
    {
        public int SolicitudId { get; set; }
        public string Materia { get; set; } // Nuevo
        public string Tema { get; set; }
        public string Descripcion { get; set; }
        public DateTime FechaLimite { get; set; } // Nuevo
        public string? ArchivoUrl { get; set; } // Nuevo
        public string Estado { get; set; }
        public string NombreEstudiante { get; set; }
        public DateTime FechaCreacion { get; set; }
        public List<OfertaDto> Ofertas { get; set; }
    }

    public class OfertaDto
    {
        public int OfertaId { get; set; }
        public string NombreAsesor { get; set; }
        public decimal Precio { get; set; }
        public string Mensaje { get; set; }
        public bool FueAceptada { get; set; }
        public DateTime FechaOferta { get; set; }
    }
}
