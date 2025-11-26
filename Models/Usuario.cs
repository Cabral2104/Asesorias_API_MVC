using Asesorias_API_MVC.Models.Interfaces;
using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;

namespace Asesorias_API_MVC.Models
{
    // Esta será nuestra tabla AspNetUsers
    public class Usuario : IdentityUser, ISoftDeletable, IAuditable
    {
        // --- NUEVO CAMPO: Este sí se puede repetir ---
        [MaxLength(100)]
        public string NombreCompleto { get; set; }

        // --- Propiedad de Borrado Lógico ---
        public bool IsActive { get; set; } = true;

        // --- Propiedades de Auditoría ---
        public DateTime CreatedAt { get; set; }
        public DateTime ModifiedAt { get; set; }
        public DateTime? LastLogin { get; set; } // '?' permite valores nulos

        // --- Propiedades de Navegación ---
        public virtual Asesor Asesor { get; set; }
        public virtual ICollection<SolicitudDeAyuda> Solicitudes { get; set; }
        public virtual ICollection<Inscripcion> Inscripciones { get; set; }
    }
}
