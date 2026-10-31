using GestionFinancieraPersonal.Models;
using GestionFinancieraPersonal.Servicios;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GestionFinancieraPersonal.Controllers
{
    [ApiController]
    [Authorize(Roles = "Usuario,Administrador")]
    [Route("api/perfil")]
    public class PerfilController : ControllerBase
    {
        private readonly UsuarioService _usuarios;

        public PerfilController(UsuarioService usuarios)
        {
            _usuarios = usuarios;
        }

        // PUT /api/perfil/contrasena
        [HttpPut("contrasena")]
        public async Task<IActionResult> CambiarContrasena(CambiarContrasenaViewModel modelo) =>
            this.Respuesta(await _usuarios.CambiarContrasenaAsync(
                User.ObtenerIdUsuario(), modelo.ContrasenaActual, modelo.NuevaContrasena));
    }
}