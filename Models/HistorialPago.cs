using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Asesorias_API_MVC.Models
{
    // Este modelo representa la tabla en PostgreSQL
    [Table("HistorialDePagos")]
    public class HistorialPago
    {
        [Key]
        [Column("PagoId")]
        public int PagoId { get; set; }

        [Column("CursoId")]
        public int CursoId { get; set; }

        [Column("EstudianteId")]
        public string EstudianteId { get; set; }

        [Column("Monto")]
        public decimal Monto { get; set; }

        // --- ¡NUEVOS CAMPOS! ---
        [Column("MetodoPago")]
        public string MetodoPago { get; set; }

        [Column("CorreoFacturacion")]
        public string CorreoFacturacion { get; set; }
        // --- FIN ---

        [Column("FechaPago")]
        public DateTime FechaPago { get; set; }
    }
}
