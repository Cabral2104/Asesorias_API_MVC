using System.ComponentModel.DataAnnotations;

namespace Asesorias_API_MVC.Models.Dtos
{
    public class UpdateProfileDto
    {
        [Required]
        [MaxLength(100)]
        public string NombreCompleto { get; set; }

        [Phone]
        public string PhoneNumber { get; set; }
    }
}
