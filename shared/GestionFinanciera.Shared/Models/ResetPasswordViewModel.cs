using System.ComponentModel.DataAnnotations;

namespace GestionFinancieraPersonal.Models
{
    public class ResetPasswordViewModel
    {
        [Required]
        public string Token { get; set; } = string.Empty;

        [Required(ErrorMessage = "La contraseña es obligatoria.")]
        [MinLength(6, ErrorMessage = "Mínimo 6 caracteres.")]
        [DataType(DataType.Password)]
        public string NuevaContrasena { get; set; } = string.Empty;

        [Required(ErrorMessage = "Confirma tu contraseña.")]
        [Compare(nameof(NuevaContrasena),
            ErrorMessage = "Las contraseñas no coinciden.")]
        [DataType(DataType.Password)]
        public string ConfirmarContrasena { get; set; } = string.Empty;

        public string? MensajeError { get; set; }

        public string? MensajeExito { get; set; }
    }
}