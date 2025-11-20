using System.ComponentModel.DataAnnotations;

namespace Asesorias_API_MVC.Models.Dtos
{
    public class SolicitudCreateDto
    {
        [Required]
        [MaxLength(100)]
        public string Tema { get; set; }

        [Required]
        [MaxLength(1000)]
        public string Descripcion { get; set; }
    }
}
