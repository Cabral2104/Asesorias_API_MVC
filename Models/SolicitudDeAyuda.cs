using Asesorias_API_MVC.Models.Interfaces;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Asesorias_API_MVC.Models
{
    public class SolicitudDeAyuda : ISoftDeletable, IAuditable
    {
        [Key]
        public int SolicitudId { get; set; }

        [Required]
        public int EstudianteId { get; set; }

        // --- CAMPOS NUEVOS ---
        [Required]
        [MaxLength(50)]
        public string Materia { get; set; } // Ej: Matemáticas

        [Required]
        public DateTime FechaLimite { get; set; } // Para cuándo lo necesita

        public string? ArchivoUrl { get; set; } // Link a Drive/Docs
        // ---------------------

        [Required]
        [MaxLength(100)]
        public string Tema { get; set; }

        [Required]
        [MaxLength(1000)]
        public string Descripcion { get; set; }

        [Required]
        [MaxLength(50)]
        public string Estado { get; set; } = "Abierta"; // Abierta, EnProceso, Finalizada

        public int? AsesorAsignadoId { get; set; }

        // Auditoría estándar (IAuditable)
        public bool IsActive { get; set; } = true;
        public DateTime CreatedAt { get; set; }
        public DateTime ModifiedAt { get; set; }

        [ForeignKey("EstudianteId")]
        public virtual Usuario Estudiante { get; set; }

        [ForeignKey("AsesorAsignadoId")]
        public virtual Asesor? AsesorAsignado { get; set; }

        public virtual ICollection<OfertaSolicitud> Ofertas { get; set; }
    }
}