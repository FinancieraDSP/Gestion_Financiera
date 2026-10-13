using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using GestionFinancieraPersonal.Models;
using GestionFinancieraPersonal.Servicios;

namespace GestionFinancieraPersonal.Controllers
{
    [Authorize(Roles = "Usuario")]
    public class PresupuestoController : Controller
    {
        private readonly PresupuestoService _presupuestos;
        private readonly MovimientoService _movimientos;

        public PresupuestoController(PresupuestoService presupuestos, MovimientoService movimientos)
        {
            _presupuestos = presupuestos;
            _movimientos = movimientos;
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
                var resultado = await _presupuestos.GuardarAsync(User.ObtenerIdUsuario(), formulario);
                if (resultado.Exito)
                {
                    TempData["Exito"] = "Presupuesto guardado.";
                    return RedirectToAction(nameof(Index));
                }
                ModelState.AddModelError(resultado.Campo, resultado.Error!);
            }
            return View("Index", await ConstruirVistaAsync(formulario));
        }

        // POST: /Presupuesto/Eliminar/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Eliminar(int id)
        {
            var resultado = await _presupuestos.EliminarAsync(User.ObtenerIdUsuario(), id);
            if (resultado.Exito) TempData["Exito"] = "Presupuesto eliminado.";
            else TempData["Error"] = resultado.Error;
            return RedirectToAction(nameof(Index));
        }

        private async Task<PresupuestoViewModel> ConstruirVistaAsync(PresupuestoFormViewModel formulario)
        {
            var idUsuario = User.ObtenerIdUsuario();
            var periodo = FormatoFechas.MesAnio(DateTime.Today);

            return new PresupuestoViewModel
            {
                Periodo = char.ToUpper(periodo[0]) + periodo.Substring(1),
                Presupuestos = await _presupuestos.ObtenerDelMesAsync(idUsuario),
                Formulario = formulario,
                Categorias = await _movimientos.ObtenerCategoriasAsync(idUsuario, "Gasto", formulario.IdCategoria)
            };
        }
    }
}