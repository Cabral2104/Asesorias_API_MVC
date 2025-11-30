namespace Asesorias_API_MVC.Models.Dtos
{
    public class AsesorDetailFullDto
    {
        public string NombreCompleto { get; set; }
        public string Email { get; set; }
        public string Especialidad { get; set; }
        public string Telefono { get; set; }
        public string FotoUrl { get; set; } // Si tienes, si no usaremos iniciales

        // Estadísticas
        public int TotalCursos { get; set; }
        public int TotalEstudiantes { get; set; }
        public decimal TotalIngresos { get; set; }
        public double RatingPromedio { get; set; }

        // Lista de sus cursos
        public List<CursoSimpleDto> Cursos { get; set; }
    }

    public class CursoSimpleDto
    {
        public string Titulo { get; set; }
        public decimal Costo { get; set; }
        public bool Estado { get; set; } // Publicado o no
        public int Inscritos { get; set; }
    }
}
