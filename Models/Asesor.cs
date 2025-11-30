using Asesorias_API_MVC.Models.Interfaces;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Asesorias_API_MVC.Models
{
    public class Asesor : ISoftDeletable, IAuditable
    {
        [Key]
        public int UsuarioId { get; set; }

        [Required]
        [MaxLength(100)]
        public string Especialidad { get; set; }

        [MaxLength(500)]
        public string Descripcion { get; set; }

        // ========== INFORMACIÓN ACADÉMICA (CAMPOS NUEVOS) ==========
        [Required]
        [MaxLength(100)]
        public string NivelEstudios { get; set; }

        [Required]
        [MaxLength(150)]
        public string InstitucionEducativa { get; set; }

        [Required]
        [MaxLength(150)]
        public string CampoEstudio { get; set; }

        public int? AnioGraduacion { get; set; } // Opcional

        // ========== EXPERIENCIA PROFESIONAL (CAMPOS NUEVOS) ==========
        [Required]
        public int AniosExperiencia { get; set; }

        [MaxLength(1000)]
        public string ExperienciaLaboral { get; set; } // Opcional

        [MaxLength(500)]
        public string Certificaciones { get; set; } // Opcional

        // ========== DOCUMENTO Y ESTADO ==========
        public string DocumentoVerificacionUrl { get; set; }

        public bool EstaAprobado { get; set; } = false;

        // --- Propiedades de Borrado Lógico y Auditoría ---
        public bool IsActive { get; set; } = true;
        public DateTime CreatedAt { get; set; }
        public DateTime ModifiedAt { get; set; }

        // --- Propiedades de Navegación ---
        [ForeignKey("UsuarioId")]
        public virtual Usuario Usuario { get; set; }
        public virtual ICollection<Curso> Cursos { get; set; }
        public virtual ICollection<SolicitudDeAyuda> SolicitudesAtendidas { get; set; }
    }
}
