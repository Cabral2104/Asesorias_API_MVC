namespace Asesorias_API_MVC.Models.Dtos
{
    public class AsesorDetailFullDto
    {
        public string NombreCompleto { get; set; }
        public string Email { get; set; }
        public string Especialidad { get; set; }
        public string Telefono { get; set; }

        // Stats Globales
        public int TotalCursos { get; set; }
        public int TotalAsesorias { get; set; } // Nuevo
        public int TotalEstudiantes { get; set; }
        public decimal TotalIngresos { get; set; }
        public double RatingPromedio { get; set; }

        public List<CursoSimpleDto> Cursos { get; set; }
        public List<AsesoriaSimpleDto> Asesorias { get; set; } // Nuevo
    }

    public class CursoSimpleDto
    {
        public string Titulo { get; set; }
        public decimal Costo { get; set; }
        public bool Estado { get; set; }
        public int Inscritos { get; set; }
    }

    public class AsesoriaSimpleDto // Nuevo
    {
        public string Tema { get; set; }
        public string Materia { get; set; }
        public decimal Precio { get; set; }
        public DateTime Fecha { get; set; }
        public string Estudiante { get; set; }
    }
}
