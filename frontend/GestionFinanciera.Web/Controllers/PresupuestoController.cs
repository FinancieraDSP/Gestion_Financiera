using GestionFinancieraPersonal.Models;
using GestionFinancieraPersonal.Servicios;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GestionFinancieraPersonal.Controllers
{
    [Authorize(Roles = "Usuario")]
    public class PresupuestoController : Controller
    {
        private readonly ApiClient _api;

        public PresupuestoController(ApiClient api)
        {
            _api = api;
        }

        // GET: /Presupuesto
        public async Task<IActionResult> Index()
        {
            return View(await ConstruirVistaAsync(new PresupuestoFormViewModel()));
        }

        // POST: /Presupuesto/Guardar
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Guardar(PresupuestoFormViewModel formulario)
        {
            if (ModelState.IsValid)
            {
                var resultado = await _api.PostAsync("api/presupuestos", formulario);
                if (resultado.Exito)
                {
                    TempData["Exito"] = "Presupuesto guardado.";
                    return RedirectToAction(nameof(Index));
                }
                ModelState.AddModelError(resultado.Campo, resultado.Error ?? "No se pudo guardar el presupuesto.");
            }
            return View("Index", await ConstruirVistaAsync(formulario));
        }

        // POST: /Presupuesto/Eliminar/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Eliminar(int id)
        {
            var resultado = await _api.DeleteAsync($"api/presupuestos/{id}");
            if (resultado.Exito) TempData["Exito"] = "Presupuesto eliminado.";
            else TempData["Error"] = resultado.Error;
            return RedirectToAction(nameof(Index));
        }

        private async Task<PresupuestoViewModel> ConstruirVistaAsync(PresupuestoFormViewModel formulario)
        {
            var modelo = (await _api.GetAsync<PresupuestoViewModel>(
                $"api/presupuestos?idCategoria={formulario.IdCategoria}")).Requerir();
            modelo.Formulario = formulario;
            return modelo;
        }
    }
}