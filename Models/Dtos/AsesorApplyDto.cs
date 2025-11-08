using System.ComponentModel.DataAnnotations;

namespace Asesorias_API_MVC.Models.Dtos
{
    public class AsesorApplyDto
    {
        [Required(ErrorMessage = "La especialidad es obligatoria")]
        [MaxLength(100)]
        public string Especialidad { get; set; }

        [MaxLength(500)]
        public string Descripcion { get; set; }

        // ========== INFORMACIÓN ACADÉMICA ==========
        [Required(ErrorMessage = "El nivel de estudios es obligatorio")]
        [MaxLength(100)]
        public string NivelEstudios { get; set; }

        [Required(ErrorMessage = "La institución educativa es obligatoria")]
        [MaxLength(150)]
        public string InstitucionEducativa { get; set; }

        [Required(ErrorMessage = "El campo de estudio es obligatorio")]
        [MaxLength(150)]
        public string CampoEstudio { get; set; }

        [Range(1900, 3000, ErrorMessage = "El año de graduación no es válido")]
        public int? AnioGraduacion { get; set; } // Opcional

        // ========== EXPERIENCIA PROFESIONAL ==========
        [Required(ErrorMessage = "Los años de experiencia son obligatorios")]
        [Range(0, 50, ErrorMessage = "Los años de experiencia deben estar entre 0 y 50")]
        public int AniosExperiencia { get; set; }

        [MaxLength(1000)]
        public string ExperienciaLaboral { get; set; } // Opcional

        [MaxLength(500)]
        public string Certificaciones { get; set; } // Opcional

        [Required(ErrorMessage = "El enlace a tu CV (PDF) es requerido.")]
        [Url(ErrorMessage = "Debes ingresar una URL válida (ej. https://...)")]
        public string DocumentoVerificacionUrl { get; set; }
    }
}
