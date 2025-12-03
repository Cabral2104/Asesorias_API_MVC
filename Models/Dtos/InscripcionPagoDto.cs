using System.ComponentModel.DataAnnotations;

namespace Asesorias_API_MVC.Models.Dtos
{
    public class InscripcionPagoDto
    {
        [Required]
        public int CursoId { get; set; }

        [Required]
        // [CreditCard] <--- ELIMINA O COMENTA ESTA LÍNEA
        [MinLength(13, ErrorMessage = "La tarjeta es muy corta")]
        [MaxLength(19, ErrorMessage = "La tarjeta es muy larga")]
        // Solo validamos que sean números
        [RegularExpression(@"^[0-9]+$", ErrorMessage = "Solo se permiten números")]
        public string NumeroTarjeta { get; set; }

        [Required]
        // Esta regex espera MM/YY (ej: 12/25)
        [RegularExpression(@"^(0[1-9]|1[0-2])\/([0-9]{2})$", ErrorMessage = "Formato MM/YY inválido")]
        public string Expiracion { get; set; }

        [Required]
        [RegularExpression(@"^[0-9]{3,4}$", ErrorMessage = "CVC inválido")]
        public string CVC { get; set; }

        [Required]
        public string Titular { get; set; }
    }
}