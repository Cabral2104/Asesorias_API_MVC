using Asesorias_API_MVC.Models.Interfaces;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Asesorias_API_MVC.Models
{
    public class Curso : ISoftDeletable, IAuditable
    {
        [Key]
        public int CursoId { get; set; }
        [Required]
        [MaxLength(150)]
        public string Titulo { get; set; }
        [MaxLength(1000)]
        public string Descripcion { get; set; }
        [Required]
        [Column(TypeName = "decimal(10, 2)")]
        public decimal Costo { get; set; }
        public bool EstaPublicado { get; set; } = false;

        // Eliminamos "FechaCreacion"
        // public DateTime FechaCreacion { get; set; } = DateTime.UtcNow;

        [Required]
        public string AsesorId { get; set; }

        // --- Propiedades de Borrado Lógico y Auditoría ---
        public bool IsActive { get; set; } = true;
        public DateTime CreatedAt { get; set; }
        public DateTime ModifiedAt { get; set; }

        // --- Propiedades de Navegación ---
        [ForeignKey("AsesorId")]
        public virtual Asesor Asesor { get; set; }
        public virtual ICollection<Leccion> Lecciones { get; set; }
        public virtual ICollection<Inscripcion> Inscripciones { get; set; }
    }
}
