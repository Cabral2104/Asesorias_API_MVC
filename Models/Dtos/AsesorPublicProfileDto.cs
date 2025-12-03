namespace Asesorias_API_MVC.Models.Dtos
{
    public class AsesorPublicProfileDto
    {
        public int AsesorId { get; set; }
        public string Nombre { get; set; }
        public string Especialidad { get; set; }
        public string Descripcion { get; set; }
        public string NivelEstudios { get; set; }
        public int AniosExperiencia { get; set; }
        public double RatingPromedio { get; set; }
        public int TotalEstudiantes { get; set; }
        public List<CursoPublicDto> Cursos { get; set; }
    }
}
