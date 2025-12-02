using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Asesorias_API_MVC.Models
{
    [Table("Calificaciones")]
    public class Calificacion
    {
        [Key]
        [Column("CalificacionId")]
        public int CalificacionId { get; set; }

        [Column("CursoId")]
        public int? CursoId { get; set; } // <--- AHORA ES NULLABLE (int?)

        [Column("SolicitudId")]
        public int? SolicitudId { get; set; } // <--- NUEVO

        [Column("EstudianteId")]
        public int EstudianteId { get; set; }

        [Column("AsesorId")]
        public int AsesorId { get; set; } // <--- NUEVO (Requerido para el promedio global)

        [Column("Rating")]
        public int Rating { get; set; }

        [Column("Comentario")]
        public string Comentario { get; set; }

        [Column("CreatedAt")]
        public DateTime CreatedAt { get; set; }
    }
}