namespace Asesorias_API_MVC.Models.Dtos
{
    public class SolicitudAsesorDto
    {
        // Datos del Usuario (de AspNetUsers)
        public string UsuarioId { get; set; }
        public string UserName { get; set; }
        public string Email { get; set; }

        // Datos de la Solicitud (de Asesores)
        public string Especialidad { get; set; }
        public string Descripcion { get; set; }
        public string NivelEstudios { get; set; }
        public string InstitucionEducativa { get; set; }
        public string CampoEstudio { get; set; }
        public int? AnioGraduacion { get; set; }
        public int AniosExperiencia { get; set; }
        public string ExperienciaLaboral { get; set; }
        public string Certificaciones { get; set; }
        public string DocumentoVerificacionUrl { get; set; }
        public DateTime FechaSolicitud { get; set; } // (Usaremos CreatedAt)
    }
}
