using GestionFinancieraPersonal.Servicios;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GestionFinancieraPersonal.Controllers
{
    [ApiController]
    [Authorize(Roles = "Usuario")]
    [Route("api/notificaciones")]
    public class NotificacionesController : ControllerBase
    {
        private readonly NotificacionService _servicio;

        public NotificacionesController(NotificacionService servicio)
        {
            _servicio = servicio;
        }

        // GET /api/notificaciones
        [HttpGet]
        public async Task<IActionResult> Obtener() =>
            Ok(await _servicio.ObtenerAsync(User.ObtenerIdUsuario()));

        // GET /api/notificaciones/no-leidas   (número para la campana)
        [HttpGet("no-leidas")]
        public async Task<IActionResult> ContarNoLeidas() =>
            Ok(await _servicio.ContarNoLeidasAsync(User.ObtenerIdUsuario()));

        // POST /api/notificaciones/5/leida
        [HttpPost("{id:int}/leida")]
        public async Task<IActionResult> MarcarLeida(int id)
        {
            await _servicio.MarcarLeidaAsync(User.ObtenerIdUsuario(), id);
            return Ok();
        }

        // POST /api/notificaciones/leidas
        [HttpPost("leidas")]
        public async Task<IActionResult> MarcarTodasLeidas()
        {
            await _servicio.MarcarTodasLeidasAsync(User.ObtenerIdUsuario());
            return Ok();
        }

        // DELETE /api/notificaciones/5
        [HttpDelete("{id:int}")]
        public async Task<IActionResult> Eliminar(int id)
        {
            await _servicio.EliminarAsync(User.ObtenerIdUsuario(), id);
            return Ok();
        }

        // DELETE /api/notificaciones   (devuelve cuántas eliminó)
        [HttpDelete]
        public async Task<IActionResult> EliminarTodas() =>
            Ok(await _servicio.EliminarTodasAsync(User.ObtenerIdUsuario()));
    }
}