namespace Asesorias_API_MVC.Models.Dtos
{
    public class CursoPublicDto
    {
        public int CursoId { get; set; }
        public string Titulo { get; set; }
        public string Descripcion { get; set; }
        public decimal Costo { get; set; }

        public bool EstaPublicado { get; set; }

        // Datos del Asesor
        public string AsesorId { get; set; }
        public string AsesorNombre { get; set; } // ¡Importante para el catálogo!
    }
}
