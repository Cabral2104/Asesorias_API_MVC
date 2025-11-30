using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Asesorias_API_MVC.Models
{
    public class ProgresoLeccion
    {
        [Key]
        public int ProgresoId { get; set; }

        public int EstudianteId { get; set; }
        public int LeccionId { get; set; }

        public bool EstaCompletada { get; set; } = false;
        public DateTime? FechaCompletado { get; set; }

        // Relaciones
        [ForeignKey("EstudianteId")]
        public virtual Usuario Estudiante { get; set; }

        [ForeignKey("LeccionId")]
        public virtual Leccion Leccion { get; set; }
    }
}
