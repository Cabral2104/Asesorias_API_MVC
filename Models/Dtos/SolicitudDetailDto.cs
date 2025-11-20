namespace Asesorias_API_MVC.Models.Dtos
{
    public class SolicitudDetailDto
    {
        public int SolicitudId { get; set; }
        public string Tema { get; set; }
        public string Descripcion { get; set; }
        public string Estado { get; set; }
        public string NombreEstudiante { get; set; }
        public DateTime FechaCreacion { get; set; }

        // Lista de ofertas
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
