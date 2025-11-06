using System.ComponentModel.DataAnnotations;

namespace Asesorias_API_MVC.Models.Dtos
{
    public class AsesorApplyDto
    {
        [Required(ErrorMessage = "La especialidad es requerida.")]
        [MaxLength(100)]
        public string Especialidad { get; set; }

        [Required(ErrorMessage = "La descripción es requerida.")]
        [MaxLength(500)]
        public string Descripcion { get; set; }

        [Required(ErrorMessage = "La información de estudios es requerida.")]
        [MaxLength(1000)]
        public string Estudios { get; set; }

        [MaxLength(1000)]
        public string Experiencia { get; set; }

        // --- CAMBIO CLAVE ---
        // Pedimos la URL como texto, en lugar del archivo.
        [Required(ErrorMessage = "El enlace a tu CV (PDF) es requerido.")]
        [Url(ErrorMessage = "Debes ingresar una URL válida (ej. https://...)")]
        public string DocumentoVerificacionUrl { get; set; }
    }
}
