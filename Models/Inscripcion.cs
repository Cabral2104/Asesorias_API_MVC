using Asesorias_API_MVC.Models.Interfaces;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Asesorias_API_MVC.Models
{
    public class Inscripcion : ISoftDeletable, IAuditable
    {
        [Key]
        public int InscripcionId { get; set; }

        // Eliminamos "FechaInscripcion" y usaremos "CreatedAt"
        // public DateTime FechaInscripcion { get; set; } = DateTime.UtcNow;

        [Required]
        public string EstudianteId { get; set; }
        public int CursoId { get; set; }

        // --- Propiedades de Borrado Lógico y Auditoría ---
        public bool IsActive { get; set; } = true;
        public DateTime CreatedAt { get; set; }
        public DateTime ModifiedAt { get; set; }

        // --- Propiedades de Navegación ---
        [ForeignKey("EstudianteId")]
        public virtual Usuario Estudiante { get; set; }

        [ForeignKey("CursoId")]
        public virtual Curso Curso { get; set; }
    }
}
