using Asesorias_API_MVC.Models.Interfaces;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Asesorias_API_MVC.Models
{
    public class Leccion : ISoftDeletable
    {
        [Key]
        public int LeccionId { get; set; }

        [Required]
        [MaxLength(150)]
        public string Titulo { get; set; }

        public string ContenidoUrl { get; set; }

        public int Orden { get; set; } = 1;

        public int CursoId { get; set; }

        // --- Propiedad de Borrado Lógico ---
        public bool IsActive { get; set; } = true; // <-- AÑADIR ESTA LÍNEA

        // --- Propiedades de Navegación ---
        [ForeignKey("CursoId")]
        public virtual Curso Curso { get; set; }
    }
}
