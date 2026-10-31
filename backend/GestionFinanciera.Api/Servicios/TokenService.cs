using System.Security.Claims;
using System.Text;
using GestionFinancieraPersonal.Entidades;
using GestionFinancieraPersonal.Models;
using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Tokens;

namespace GestionFinancieraPersonal.Servicios
{
    // Genera el JWT que el frontend enviará en cada llamada a la API.
    public class TokenService
    {
        private readonly IConfiguration _config;

        public TokenService(IConfiguration config)
        {
            _config = config;
        }

        public LoginResponse Crear(Usuario usuario, string rol)
        {
            var jwt = _config.GetSection("Jwt");
            var expira = DateTime.UtcNow.AddMinutes(jwt.GetValue("ExpiraMinutos", 60));
            var clave = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwt["Key"]!));

            var descriptor = new SecurityTokenDescriptor
            {
                Issuer = jwt["Issuer"],
                Audience = jwt["Audience"],
                Expires = expira,
                Subject = new ClaimsIdentity(new[]
                {
                    new Claim(ClaimTypes.NameIdentifier, usuario.IdUsuario.ToString()),
                    new Claim(ClaimTypes.Name, usuario.Nombre),
                    new Claim(ClaimTypes.Email, usuario.Correo),
                    new Claim(ClaimTypes.Role, rol)
                }),
                SigningCredentials = new SigningCredentials(clave, SecurityAlgorithms.HmacSha256)
            };

            return new LoginResponse
            {
                Token = new JsonWebTokenHandler().CreateToken(descriptor),
                Expira = new DateTimeOffset(expira, TimeSpan.Zero),
                IdUsuario = usuario.IdUsuario,
                Nombre = usuario.Nombre,
                Correo = usuario.Correo,
                Rol = rol
            };
        }
    }
}