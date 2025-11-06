using Asesorias_API_MVC.Models.Interfaces;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Asesorias_API_MVC.Models
{
    public class Asesor : ISoftDeletable, IAuditable
    {
        [Key]
        public string UsuarioId { get; set; }

        [Required]
        [MaxLength(100)]
        public string Especialidad { get; set; }

        [MaxLength(500)]
        public string Descripcion { get; set; }

        // --- CAMPOS NUEVOS AÑADIDOS ---
        [MaxLength(1000)]
        public string Estudios { get; set; }

        [MaxLength(1000)]
        public string Experiencia { get; set; }
        // --- FIN DE CAMPOS NUEVOS ---

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
