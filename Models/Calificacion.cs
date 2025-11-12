using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Asesorias_API_MVC.Models
{
    // Este modelo representa la tabla en PostgreSQL
    [Table("Calificaciones")]
    public class Calificacion
    {
        [Key]
        [Column("CalificacionId")]
        public int CalificacionId { get; set; }

        [Column("CursoId")]
        public int CursoId { get; set; } // FK "lógica" a SQL Server

        [Column("EstudianteId")]
        public string EstudianteId { get; set; } // FK "lógica" a SQL Server

        [Column("Rating")]
        public int Rating { get; set; }

        [Column("Comentario")]
        public string Comentario { get; set; }

        [Column("CreatedAt")]
        public DateTime CreatedAt { get; set; }
    }
}
