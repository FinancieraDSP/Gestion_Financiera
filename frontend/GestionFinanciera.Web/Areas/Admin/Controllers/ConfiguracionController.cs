using GestionFinancieraPersonal.Areas.Admin.Models;
using GestionFinancieraPersonal.Servicios;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GestionFinancieraPersonal.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Administrador")]
    public class ConfiguracionController : Controller
    {
        private readonly ApiClient _api;

        public ConfiguracionController(ApiClient api)
        {
            _api = api;
        }

        // GET: /Admin/Configuracion
        public async Task<IActionResult> Index()
        {
            var modelo = (await _api.GetAsync<ConfiguracionSistema>("api/admin/configuracion")).Requerir();
            return View(modelo);
        }

        // POST: /Admin/Configuracion/Guardar
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Guardar(ConfiguracionSistema modelo)
        {
            if (!ModelState.IsValid)
                return View("Index", modelo);

            var resultado = await _api.PutAsync("api/admin/configuracion", modelo);

            if (resultado.Exito)
                TempData["Exito"] = "Configuración guardada correctamente.";
            else
                TempData["Error"] = resultado.Error;

            return RedirectToAction(nameof(Index));
        }
    }
}