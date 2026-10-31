using System.ComponentModel.DataAnnotations;

namespace GestionFinancieraPersonal.Models
{
    public class LoginRequest
    {
        [Required] public string Correo { get; set; } = string.Empty;
        [Required] public string Contrasena { get; set; } = string.Empty;
        public string TipoAcceso { get; set; } = "Usuario"; // Usuario o Administrador
    }

    public class LoginResponse
    {
        public string Token { get; set; } = string.Empty;
        public DateTimeOffset Expira { get; set; }
        public int IdUsuario { get; set; }
        public string Nombre { get; set; } = string.Empty;
        public string Correo { get; set; } = string.Empty;
        public string Rol { get; set; } = string.Empty;
    }

    // Forma estándar de los errores que devuelve la API
    public class ErrorApi
    {
        public string Mensaje { get; set; } = string.Empty;
        public string Campo { get; set; } = string.Empty; // campo del formulario al que pertenece el error
    }
}