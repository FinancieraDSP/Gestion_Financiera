using GestionFinancieraPersonal.Entidades;
using GestionFinancieraPersonal.Models;
using GestionFinancieraPersonal.Servicios;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GestionFinancieraPersonal.Controllers
{
    [Authorize(Roles = "Usuario")]
    public class IngresosController : Controller
    {
        private readonly ApiClient _api;

        public IngresosController(ApiClient api)
        {
            _api = api;
        }

        // GET: /Ingresos
        public async Task<IActionResult> Index()
        {
            return View(await ConstruirVistaAsync(new MovimientoFormViewModel { Fecha = DateTime.Today }));
        }

        // POST: /Ingresos/Registrar
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Registrar(MovimientoFormViewModel formulario)
        {
            if (ModelState.IsValid)
            {
                var resultado = await _api.PostAsync("api/ingresos", formulario);
                if (resultado.Exito)
                {
                    TempData["Exito"] = "Ingreso registrado correctamente.";
                    return RedirectToAction(nameof(Index));
                }
                ModelState.AddModelError(resultado.Campo, resultado.Error ?? "No se pudo registrar el ingreso.");
            }

            return View("Index", await ConstruirVistaAsync(formulario));
        }

        // POST: /Ingresos/Eliminar/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Eliminar(int id)
        {
            var resultado = await _api.DeleteAsync($"api/ingresos/{id}");
            if (resultado.Exito) TempData["Exito"] = "Ingreso eliminado.";
            else TempData["Error"] = resultado.Error;
            return RedirectToAction(nameof(Index));
        }

        private async Task<MovimientosViewModel<Ingreso>> ConstruirVistaAsync(MovimientoFormViewModel formulario)
        {
            var url = $"api/ingresos?idCuenta={formulario.IdCuenta}&idCategoria={formulario.IdCategoria}";
            var modelo = (await _api.GetAsync<MovimientosViewModel<Ingreso>>(url)).Requerir();
            modelo.Formulario = formulario;
            return modelo;
        }
    }
}