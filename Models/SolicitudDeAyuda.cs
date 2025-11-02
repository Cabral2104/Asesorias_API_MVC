using Asesorias_API_MVC.Models.Interfaces;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Asesorias_API_MVC.Models
{
    public class SolicitudDeAyuda : ISoftDeletable
    {
        [Key]
        public int SolicitudId { get; set; }

        [Required]
        [MaxLength(100)]
        public string Tema { get; set; }

        [Required]
        [MaxLength(1000)]
        public string Descripcion { get; set; }

        [Required]
        [MaxLength(50)]
        public string Estado { get; set; } = "Abierta";

        public DateTime FechaCreacion { get; set; } = DateTime.UtcNow;

        [Required]
        public string EstudianteId { get; set; }

        public string? AsesorAsignadoId { get; set; }

        // --- Propiedad de Borrado Lógico ---
        public bool IsActive { get; set; } = true; // <-- AÑADIR ESTA LÍNEA

        // --- Propiedades de Navegación ---
        [ForeignKey("EstudianteId")]
        public virtual Usuario Estudiante { get; set; }

        [ForeignKey("AsesorAsignadoId")]
        public virtual Asesor? AsesorAsignado { get; set; }
    }
}
