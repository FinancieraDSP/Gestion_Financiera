using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using GestionFinancieraPersonal.Models;
using GestionFinancieraPersonal.Servicios;

namespace GestionFinancieraPersonal.Controllers
{
    [Authorize(Roles = "Usuario")]
    public class DeudasController : Controller
    {
        private readonly DeudaService _deudas;
        private readonly MovimientoService _movimientos;

        public DeudasController(DeudaService deudas, MovimientoService movimientos)
        {
            _deudas = deudas;
            _movimientos = movimientos;
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
                var resultado = await _deudas.CrearAsync(User.ObtenerIdUsuario(), nueva);
                if (resultado.Exito)
                {
                    TempData["Exito"] = $"Deuda \"{nueva.Nombre.Trim()}\" registrada.";
                    return RedirectToAction(nameof(Index));
                }
                ModelState.AddModelError(resultado.Campo, resultado.Error!);
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
                var resultado = await _deudas.PagarAsync(User.ObtenerIdUsuario(), pago);
                if (resultado.Exito)
                {
                    TempData["Exito"] = "Pago registrado correctamente.";
                    return RedirectToAction(nameof(Index));
                }
                ModelState.AddModelError(resultado.Campo, resultado.Error!);
            }
            return View("Index", await ConstruirVistaAsync(NuevaDeudaPorDefecto(), pago));
        }

        // POST: /Deudas/Eliminar/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Eliminar(int id)
        {
            var resultado = await _deudas.EliminarAsync(User.ObtenerIdUsuario(), id);
            if (resultado.Exito) TempData["Exito"] = "Deuda eliminada.";
            else TempData["Error"] = resultado.Error;
            return RedirectToAction(nameof(Index));
        }

        private static DeudaFormViewModel NuevaDeudaPorDefecto() => new() { FechaInicio = DateTime.Today };

        private async Task<DeudasViewModel> ConstruirVistaAsync(DeudaFormViewModel nueva, PagoDeudaFormViewModel pago)
        {
            var idUsuario = User.ObtenerIdUsuario();
            var deudas = await _deudas.ObtenerAsync(idUsuario);

            return new DeudasViewModel
            {
                Deudas = deudas,
                PagosRecientes = await _deudas.ObtenerPagosRecientesAsync(idUsuario),
                DeudaTotal = deudas.Where(d => d.Estado != "Pagada").Sum(d => d.MontoPendiente),
                Nueva = nueva,
                Pago = pago,
                Cuentas = await _movimientos.ObtenerCuentasAsync(idUsuario, pago.IdCuenta),
                DeudasPendientes = deudas
                    .Where(d => d.Estado != "Pagada")
                    .Select(d => new SelectListItem(
                        $"{d.Nombre} (pendiente {d.MontoPendiente:C})",
                        d.IdDeuda.ToString(),
                        d.IdDeuda == pago.IdDeuda))
                    .ToList()
            };
        }
    }
}