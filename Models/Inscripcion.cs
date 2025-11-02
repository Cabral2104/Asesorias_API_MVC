using Asesorias_API_MVC.Models.Interfaces;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Asesorias_API_MVC.Models
{
    public class Inscripcion : ISoftDeletable
    {
        [Key]
        public int InscripcionId { get; set; }

        public DateTime FechaInscripcion { get; set; } = DateTime.UtcNow;

        [Required]
        public string EstudianteId { get; set; }

        public int CursoId { get; set; }

        // --- Propiedad de Borrado Lógico ---
        public bool IsActive { get; set; } = true; // <-- AÑADIR ESTA LÍNEA

        // --- Propiedades de Navegación ---
        [ForeignKey("EstudianteId")]
        public virtual Usuario Estudiante { get; set; }

        [ForeignKey("CursoId")]
        public virtual Curso Curso { get; set; }
    }
}
