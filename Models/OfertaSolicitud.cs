using Asesorias_API_MVC.Models.Interfaces;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Asesorias_API_MVC.Models
{
    public class OfertaSolicitud : ISoftDeletable, IAuditable
    {
        [Key]
        public int OfertaId { get; set; }

        public int SolicitudId { get; set; } // A qué solicitud pertenece
        public string AsesorId { get; set; } // Qué asesor la hizo

        [Required]
        [Column(TypeName = "decimal(10, 2)")]
        public decimal PrecioOferta { get; set; } // La cotización

        [Required]
        [MaxLength(500)]
        public string Mensaje { get; set; } // Ej: "Puedo ayudarte mañana a las 5"

        public bool FueAceptada { get; set; } = false;

        // --- Auditoría ---
        public bool IsActive { get; set; } = true;
        public DateTime CreatedAt { get; set; }
        public DateTime ModifiedAt { get; set; }

        // --- Relaciones ---
        [ForeignKey("SolicitudId")]
        public virtual SolicitudDeAyuda Solicitud { get; set; }

        [ForeignKey("AsesorId")]
        public virtual Asesor Asesor { get; set; }
    }
}
