using GestionFinancieraPersonal.Models;
using GestionFinancieraPersonal.Servicios;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GestionFinancieraPersonal.Controllers
{
    [Authorize(Roles = "Usuario")]
    public class AhorroController : Controller
    {
        private readonly ApiClient _api;

        public AhorroController(ApiClient api)
        {
            _api = api;
        }

        // GET: /Ahorro
        public async Task<IActionResult> Index()
        {
            return View(await ConstruirVistaAsync(new MetaFormViewModel(), new AporteFormViewModel()));
        }

        // POST: /Ahorro/CrearMeta
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CrearMeta(MetaFormViewModel nuevaMeta)
        {
            if (ModelState.IsValid)
            {
                var resultado = await _api.PostAsync("api/ahorro/metas", nuevaMeta);
                if (resultado.Exito)
                {
                    TempData["Exito"] = $"Meta \"{nuevaMeta.Nombre.Trim()}\" creada.";
                    return RedirectToAction(nameof(Index));
                }
                ModelState.AddModelError(resultado.Campo, resultado.Error ?? "No se pudo crear la meta.");
            }
            return View("Index", await ConstruirVistaAsync(nuevaMeta, new AporteFormViewModel()));
        }

        // POST: /Ahorro/Aportar
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Aportar(AporteFormViewModel aporte)
        {
            if (ModelState.IsValid)
            {
                var resultado = await _api.PostAsync("api/ahorro/aportes", aporte);
                if (resultado.Exito)
                {
                    TempData["Exito"] = "Aporte registrado correctamente.";
                    return RedirectToAction(nameof(Index));
                }
                ModelState.AddModelError(resultado.Campo, resultado.Error ?? "No se pudo registrar el aporte.");
            }
            return View("Index", await ConstruirVistaAsync(new MetaFormViewModel(), aporte));
        }

        // POST: /Ahorro/EliminarMeta/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EliminarMeta(int id)
        {
            var resultado = await _api.DeleteAsync<EliminarMetaResponse>($"api/ahorro/metas/{id}");
            if (!resultado.Exito)
            {
                TempData["Error"] = resultado.Error;
            }
            else
            {
                var devuelto = resultado.Datos?.Devuelto ?? 0;
                TempData["Exito"] = devuelto > 0
                    ? $"Meta eliminada. Se devolvieron {devuelto:C} a {resultado.Datos?.Cuenta}."
                    : "Meta eliminada.";
            }
            return RedirectToAction(nameof(Index));
        }

        private async Task<AhorroViewModel> ConstruirVistaAsync(MetaFormViewModel nuevaMeta, AporteFormViewModel aporte)
        {
            var modelo = (await _api.GetAsync<AhorroViewModel>(
                $"api/ahorro?idMeta={aporte.IdMeta}&idCuenta={aporte.IdCuenta}")).Requerir();
            modelo.NuevaMeta = nuevaMeta;
            modelo.Aporte = aporte;
            return modelo;
        }
    }
}