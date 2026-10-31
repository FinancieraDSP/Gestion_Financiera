using GestionFinancieraPersonal.Entidades;
using GestionFinancieraPersonal.Servicios;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GestionFinancieraPersonal.Controllers
{
    [Authorize(Roles = "Usuario")]
    public class NotificacionesController : Controller
    {
        private readonly ApiClient _api;

        public NotificacionesController(ApiClient api)
        {
            _api = api;
        }

        // GET: /Notificaciones
        public async Task<IActionResult> Index()
        {
            var lista = (await _api.GetAsync<List<Notificacion>>("api/notificaciones")).Requerir();
            return View(lista);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> MarcarLeida(int id)
        {
            await _api.PostAsync($"api/notificaciones/{id}/leida");
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> MarcarTodasLeidas()
        {
            await _api.PostAsync("api/notificaciones/leidas");
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Eliminar(int id)
        {
            await _api.DeleteAsync($"api/notificaciones/{id}");
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EliminarTodas()
        {
            var resultado = await _api.DeleteAsync<int>("api/notificaciones");
            var n = resultado.Exito ? resultado.Datos : 0;

            TempData["Exito"] = n > 0
                ? "Se eliminaron todas las notificaciones."
                : "No había notificaciones.";

            return RedirectToAction(nameof(Index));
        }
    }
}