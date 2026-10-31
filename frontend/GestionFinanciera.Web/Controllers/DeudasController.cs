using GestionFinancieraPersonal.Models;
using GestionFinancieraPersonal.Servicios;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GestionFinancieraPersonal.Controllers
{
    [Authorize(Roles = "Usuario")]
    public class DeudasController : Controller
    {
        private readonly ApiClient _api;

        public DeudasController(ApiClient api)
        {
            _api = api;
        }

        // GET: /Deudas
        public async Task<IActionResult> Index()
        {
            return View(await ConstruirVistaAsync(NuevaDeudaPorDefecto(), new PagoDeudaFormViewModel { Fecha = DateTime.Today }));
        }

        // POST: /Deudas/Crear
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Crear(DeudaFormViewModel nueva)
        {
            if (ModelState.IsValid)
            {
                var resultado = await _api.PostAsync("api/deudas", nueva);
                if (resultado.Exito)
                {
                    TempData["Exito"] = $"Deuda \"{nueva.Nombre.Trim()}\" registrada.";
                    return RedirectToAction(nameof(Index));
                }
                ModelState.AddModelError(resultado.Campo, resultado.Error ?? "No se pudo registrar la deuda.");
            }
            return View("Index", await ConstruirVistaAsync(nueva, new PagoDeudaFormViewModel { Fecha = DateTime.Today }));
        }

        // POST: /Deudas/Pagar
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Pagar(PagoDeudaFormViewModel pago)
        {
            if (ModelState.IsValid)
            {
                var resultado = await _api.PostAsync("api/deudas/pagos", pago);
                if (resultado.Exito)
                {
                    TempData["Exito"] = "Pago registrado correctamente.";
                    return RedirectToAction(nameof(Index));
                }
                ModelState.AddModelError(resultado.Campo, resultado.Error ?? "No se pudo registrar el pago.");
            }
            return View("Index", await ConstruirVistaAsync(NuevaDeudaPorDefecto(), pago));
        }

        // POST: /Deudas/Eliminar/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Eliminar(int id)
        {
            var resultado = await _api.DeleteAsync($"api/deudas/{id}");
            if (resultado.Exito) TempData["Exito"] = "Deuda eliminada.";
            else TempData["Error"] = resultado.Error;
            return RedirectToAction(nameof(Index));
        }

        private static DeudaFormViewModel NuevaDeudaPorDefecto() => new() { FechaInicio = DateTime.Today };

        private async Task<DeudasViewModel> ConstruirVistaAsync(DeudaFormViewModel nueva, PagoDeudaFormViewModel pago)
        {
            var modelo = (await _api.GetAsync<DeudasViewModel>(
                $"api/deudas?idDeuda={pago.IdDeuda}&idCuenta={pago.IdCuenta}")).Requerir();
            modelo.Nueva = nueva;
            modelo.Pago = pago;
            return modelo;
        }
    }
}