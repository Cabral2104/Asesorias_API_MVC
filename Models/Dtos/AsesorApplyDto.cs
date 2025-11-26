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

        public int? AnioGraduacion { get; set; }

        // ========== EXPERIENCIA PROFESIONAL ==========
        [Required(ErrorMessage = "Los años de experiencia son obligatorios")]
        public int AniosExperiencia { get; set; }

        [MaxLength(1000)]
        public string ExperienciaLaboral { get; set; }

        [MaxLength(500)]
        public string Certificaciones { get; set; }

        // SIN [Required] aquí. Lo validaremos en el Controlador.
        // Esto evita que el Binder falle si el archivo viene en partes o corrupto.
        // NUEVOS CAMPOS PARA EL ARCHIVO (COMO TEXTO)
        public string? ArchivoBase64 { get; set; } // Aquí viene el contenido del PDF
        public string? NombreArchivo { get; set; } // Ej: "mi_cv.pdf"
    }
}
