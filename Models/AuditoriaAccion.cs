using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Asesorias_API_MVC.Models
{
    // Esta tabla vivirá en PostgreSQL
    [Table("AuditoriaAcciones")]
    public class AuditoriaAccion
    {
        [Key]
        [Column("AuditoriaId")]
        public int AuditoriaId { get; set; }

        [Column("UsuarioId")]
        public int UsuarioId { get; set; } // Quién hizo la acción

        [Column("Accion")]
        public string Accion { get; set; } // Ej: "Crear Solicitud", "Oferta Aceptada"

        [Column("Detalles")]
        public string Detalles { get; set; } // Ej: "Solicitud ID: 5 para Matemáticas"

        [Column("FechaAccion")]
        public DateTime FechaAccion { get; set; } = DateTime.UtcNow;
    }
}