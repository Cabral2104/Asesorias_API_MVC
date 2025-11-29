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

        [Required(ErrorMessage = "El nivel de estudios es obligatorio")]
        public string NivelEstudios { get; set; }

        [Required(ErrorMessage = "La institución educativa es obligatoria")]
        public string InstitucionEducativa { get; set; }

        [Required(ErrorMessage = "El campo de estudio es obligatorio")]
        public string CampoEstudio { get; set; }

        public int? AnioGraduacion { get; set; }

        [Required(ErrorMessage = "Los años de experiencia son obligatorios")]
        public int AniosExperiencia { get; set; }

        public string ExperienciaLaboral { get; set; }
        public string Certificaciones { get; set; }

        // --- CAMBIO CLAVE: AHORA ES UN LINK ---
        [Required(ErrorMessage = "Debes proporcionar un enlace a tu CV o Portafolio.")]
        [Url(ErrorMessage = "El formato debe ser un enlace válido (ej: https://drive.google.com/...).")]
        public string DocumentoUrl { get; set; }
    }
}