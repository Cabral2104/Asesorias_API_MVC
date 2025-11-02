using Asesorias_API_MVC.Models.Interfaces;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Asesorias_API_MVC.Models
{
    public class Asesor : ISoftDeletable
    {
        [Key]
        public string UsuarioId { get; set; }

        [Required]
        [MaxLength(100)]
        public string Especialidad { get; set; }

        [MaxLength(500)]
        public string Descripcion { get; set; }

        public string DocumentoVerificacionUrl { get; set; }

        public bool EstaAprobado { get; set; } = false;

        public DateTime FechaCreacion { get; set; } = DateTime.UtcNow;

        // --- Propiedad de Borrado Lógico ---
        public bool IsActive { get; set; } = true; // <-- AÑADIR ESTA LÍNEA

        // --- Propiedades de Navegación ---
        [ForeignKey("UsuarioId")]
        public virtual Usuario Usuario { get; set; }
        public virtual ICollection<Curso> Cursos { get; set; }
        public virtual ICollection<SolicitudDeAyuda> SolicitudesAtendidas { get; set; }
    }
}
