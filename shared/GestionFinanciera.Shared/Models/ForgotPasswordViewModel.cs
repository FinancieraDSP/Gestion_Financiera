using System.ComponentModel.DataAnnotations;

namespace GestionFinancieraPersonal.Models
{
    public class ForgotPasswordViewModel
    {
        [Required]
        [EmailAddress]
        public string Correo { get; set; } = string.Empty;

        public string? MensajeExito { get; set; }
    }
}