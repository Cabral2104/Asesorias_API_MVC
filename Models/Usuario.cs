using Asesorias_API_MVC.Models.Interfaces;
using Microsoft.AspNetCore.Identity;

namespace Asesorias_API_MVC.Models
{
    // Esta será nuestra tabla AspNetUsers
    public class Usuario : IdentityUser, ISoftDeletable
    {
        // --- Propiedad de Borrado Lógico ---
        public bool IsActive { get; set; } = true; // <-- AÑADIR ESTA LÍNEA

        // --- Propiedades de Navegación ---
        public virtual Asesor Asesor { get; set; }
        public virtual ICollection<SolicitudDeAyuda> Solicitudes { get; set; }
        public virtual ICollection<Inscripcion> Inscripciones { get; set; }
    }
}
