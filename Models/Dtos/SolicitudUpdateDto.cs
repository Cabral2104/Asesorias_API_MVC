using System.ComponentModel.DataAnnotations;

namespace Asesorias_API_MVC.Models.Dtos
{
    public class SolicitudUpdateDto
    {
        [Required] public string Materia { get; set; }
        [Required] public string Tema { get; set; }
        [Required] public string Descripcion { get; set; }
        [Required] public DateTime FechaLimite { get; set; }
        public string? ArchivoUrl { get; set; }
    }
}
