using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using GestionFinancieraPersonal.Servicios;

namespace GestionFinancieraPersonal.Controllers
{
    [Authorize(Roles = "Usuario")]
    public class NotificacionesController : Controller
    {
        private readonly NotificacionService _servicio;

        public NotificacionesController(NotificacionService servicio)
        {
            _servicio = servicio;
        }

        // GET: /Notificaciones
        public async Task<IActionResult> Index()
        {
            return View(await _servicio.ObtenerAsync(User.ObtenerIdUsuario()));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> MarcarLeida(int id)
        {
            await _servicio.MarcarLeidaAsync(User.ObtenerIdUsuario(), id);
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> MarcarTodasLeidas()
        {
            await _servicio.MarcarTodasLeidasAsync(User.ObtenerIdUsuario());
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Eliminar(int id)
        {
            await _servicio.EliminarAsync(User.ObtenerIdUsuario(), id);
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EliminarTodas()
        {
            var n = await _servicio.EliminarTodasAsync(User.ObtenerIdUsuario());

            TempData["Exito"] =
                n > 0
                    ? "Se eliminaron todas las notificaciones."
                    : "No había notificaciones.";

            return RedirectToAction(nameof(Index));
        }
    }
}
