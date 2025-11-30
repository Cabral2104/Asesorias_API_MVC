namespace Asesorias_API_MVC.Models.Dtos
{
    public class ProgresoCursoDto
    {
        public int CursoId { get; set; }
        public int LeccionesTotales { get; set; }
        public int LeccionesCompletadas { get; set; }
        public double Porcentaje { get; set; }
        public List<int> LeccionesCompletadasIds { get; set; } // Lista de IDs para marcar en el sidebar
    }
}
