using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using GestionFinancieraPersonal.Models;
using GestionFinancieraPersonal.Servicios;

namespace GestionFinancieraPersonal.Controllers
{
    [Authorize(Roles = "Usuario")]
    public class AhorroController : Controller
    {
        private readonly AhorroService _ahorro;
        private readonly MovimientoService _movimientos;

        public AhorroController(AhorroService ahorro, MovimientoService movimientos)
        {
            _ahorro = ahorro;
            _movimientos = movimientos;
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
                var resultado = await _ahorro.CrearMetaAsync(User.ObtenerIdUsuario(), nuevaMeta);
                if (resultado.Exito)
                {
                    TempData["Exito"] = $"Meta \"{nuevaMeta.Nombre.Trim()}\" creada.";
                    return RedirectToAction(nameof(Index));
                }
                ModelState.AddModelError(resultado.Campo, resultado.Error!);
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
                var resultado = await _ahorro.AportarAsync(User.ObtenerIdUsuario(), aporte);
                if (resultado.Exito)
                {
                    TempData["Exito"] = "Aporte registrado correctamente.";
                    return RedirectToAction(nameof(Index));
                }
                ModelState.AddModelError(resultado.Campo, resultado.Error!);
            }
            return View("Index", await ConstruirVistaAsync(new MetaFormViewModel(), aporte));
        }

        // POST: /Ahorro/EliminarMeta/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EliminarMeta(int id)
        {
            var (resultado, devuelto, cuenta) = await _ahorro.EliminarMetaAsync(User.ObtenerIdUsuario(), id);
            if (!resultado.Exito) TempData["Error"] = resultado.Error;
            else TempData["Exito"] = devuelto > 0
                ? $"Meta eliminada. Se devolvieron {devuelto:C} a {cuenta}."
                : "Meta eliminada.";
            return RedirectToAction(nameof(Index));
        }

        private async Task<AhorroViewModel> ConstruirVistaAsync(MetaFormViewModel nuevaMeta, AporteFormViewModel aporte)
        {
            var idUsuario = User.ObtenerIdUsuario();
            var metas = await _ahorro.ObtenerMetasAsync(idUsuario);

            return new AhorroViewModel
            {
                Metas = metas,
                AhorroTotal = metas.Sum(m => m.MontoActual),
                NuevaMeta = nuevaMeta,
                Aporte = aporte,
                Cuentas = await _movimientos.ObtenerCuentasAsync(idUsuario, aporte.IdCuenta),
                MetasActivas = metas
                    .Where(m => m.Estado == "Activa")
                    .Select(m => new SelectListItem(
                        $"{m.Nombre} (faltan {(m.MontoObjetivo - m.MontoActual):C})",
                        m.IdMeta.ToString(),
                        m.IdMeta == aporte.IdMeta))
                    .ToList()
            };
        }
    }
}