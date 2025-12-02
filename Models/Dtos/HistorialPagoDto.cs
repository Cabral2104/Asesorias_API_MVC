namespace Asesorias_API_MVC.Models.Dtos
{
    public class HistorialPagoDto
    {
        public int PagoId { get; set; }
        public string NombreCurso { get; set; }
        public decimal Monto { get; set; }
        public string MetodoPago { get; set; }
        public DateTime FechaPago { get; set; }
    }
}
