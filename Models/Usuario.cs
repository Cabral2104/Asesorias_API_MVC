using Asesorias_API_MVC.Models.Interfaces;
using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;

namespace Asesorias_API_MVC.Models
{
    // CAMBIO CLAVE: IdentityUser<int>
    public class Usuario : IdentityUser<int>, ISoftDeletable, IAuditable
    {
        [MaxLength(100)]
        public string NombreCompleto { get; set; }

        public bool IsActive { get; set; } = true;
        public DateTime CreatedAt { get; set; }
        public DateTime ModifiedAt { get; set; }
        public DateTime? LastLogin { get; set; }

        public virtual Asesor Asesor { get; set; }
        public virtual ICollection<SolicitudDeAyuda> Solicitudes { get; set; }
        public virtual ICollection<Inscripcion> Inscripciones { get; set; }
    }
}