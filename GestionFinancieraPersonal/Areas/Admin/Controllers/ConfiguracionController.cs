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
        private readonly ConfiguracionService _configuracion;

        public ConfiguracionController(ConfiguracionService configuracion)
        {
            _configuracion = configuracion;
        }

        // GET: /Admin/Configuracion
        public async Task<IActionResult> Index()
        {
            var actual = await _configuracion.ObtenerAsync();
            return View(new ConfiguracionSistema
            {
                UmbralPresupuesto = actual.UmbralPresupuesto,
                DiasAvisoDeuda = actual.DiasAvisoDeuda,
                IntentosPermitidos = actual.IntentosPermitidos
            });
        }

        // POST: /Admin/Configuracion/Guardar
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Guardar(ConfiguracionSistema modelo)
        {
            if (!ModelState.IsValid)
                return View("Index", modelo);

            var resultado = await _configuracion.GuardarAjustesAsync(
                modelo.UmbralPresupuesto!.Value,
                modelo.DiasAvisoDeuda!.Value,
                modelo.IntentosPermitidos!.Value);

            if (resultado.Exito)
                TempData["Exito"] = "Configuración guardada correctamente.";
            else
                TempData["Error"] = resultado.Error;

            return RedirectToAction(nameof(Index));
        }
    }
}
