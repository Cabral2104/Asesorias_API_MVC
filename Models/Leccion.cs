using Asesorias_API_MVC.Models.Interfaces;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Asesorias_API_MVC.Models
{
    public class Leccion : ISoftDeletable, IAuditable
    {
        [Key]
        public int LeccionId { get; set; }
        [Required]
        [MaxLength(150)]
        public string Titulo { get; set; }
        public string ContenidoUrl { get; set; }
        public int Orden { get; set; } = 1;
        public int CursoId { get; set; }

        // --- Propiedades de Borrado Lógico y Auditoría ---
        public bool IsActive { get; set; } = true;
        public DateTime CreatedAt { get; set; }
        public DateTime ModifiedAt { get; set; }

        // --- Propiedades de Navegación ---
        [ForeignKey("CursoId")]
        public virtual Curso Curso { get; set; }
    }
}
