using GestionFinancieraPersonal.Entidades;
using GestionFinancieraPersonal.Models;
using GestionFinancieraPersonal.Servicios;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GestionFinancieraPersonal.Controllers
{
    [Authorize(Roles = "Usuario")]
    public class GastosController : Controller
    {
        private readonly ApiClient _api;

        public GastosController(ApiClient api)
        {
            _api = api;
        }

        // GET: /Gastos
        public async Task<IActionResult> Index()
        {
            return View(await ConstruirVistaAsync(new MovimientoFormViewModel { Fecha = DateTime.Today }));
        }

        // POST: /Gastos/Registrar
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Registrar(MovimientoFormViewModel formulario)
        {
            if (ModelState.IsValid)
            {
                var resultado = await _api.PostAsync("api/gastos", formulario);
                if (resultado.Exito)
                {
                    TempData["Exito"] = "Gasto registrado correctamente.";
                    return RedirectToAction(nameof(Index));
                }
                ModelState.AddModelError(resultado.Campo, resultado.Error ?? "No se pudo registrar el gasto.");
            }

            return View("Index", await ConstruirVistaAsync(formulario));
        }

        // POST: /Gastos/Eliminar/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Eliminar(int id)
        {
            var resultado = await _api.DeleteAsync($"api/gastos/{id}");
            if (resultado.Exito) TempData["Exito"] = "Gasto eliminado.";
            else TempData["Error"] = resultado.Error;
            return RedirectToAction(nameof(Index));
        }

        private async Task<MovimientosViewModel<Gasto>> ConstruirVistaAsync(MovimientoFormViewModel formulario)
        {
            var url = $"api/gastos?idCuenta={formulario.IdCuenta}&idCategoria={formulario.IdCategoria}";
            var modelo = (await _api.GetAsync<MovimientosViewModel<Gasto>>(url)).Requerir();
            modelo.Formulario = formulario;
            return modelo;
        }
    }
}