using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using GestionFinancieraPersonal.Entidades;
using GestionFinancieraPersonal.Models;
using GestionFinancieraPersonal.Servicios;

namespace GestionFinancieraPersonal.Controllers
{
    [Authorize(Roles = "Usuario")]
    public class GastosController : Controller
    {
        private readonly MovimientoService _servicio;

        public GastosController(MovimientoService servicio)
        {
            _servicio = servicio;
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
                var resultado = await _servicio.RegistrarGastoAsync(User.ObtenerIdUsuario(), formulario);
                if (resultado.Exito)
                {
                    TempData["Exito"] = "Gasto registrado correctamente.";
                    return RedirectToAction(nameof(Index));
                }
                ModelState.AddModelError(resultado.Campo, resultado.Error!);
            }

            return View("Index", await ConstruirVistaAsync(formulario));
        }

        // POST: /Gastos/Eliminar/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Eliminar(int id)
        {
            var resultado = await _servicio.EliminarGastoAsync(User.ObtenerIdUsuario(), id);
            if (resultado.Exito) TempData["Exito"] = "Gasto eliminado.";
            else TempData["Error"] = resultado.Error;
            return RedirectToAction(nameof(Index));
        }

        private async Task<MovimientosViewModel<Gasto>> ConstruirVistaAsync(MovimientoFormViewModel formulario)
        {
            var idUsuario = User.ObtenerIdUsuario();
            return new MovimientosViewModel<Gasto>
            {
                Items = await _servicio.ObtenerGastosAsync(idUsuario),
                Formulario = formulario,
                Cuentas = await _servicio.ObtenerCuentasAsync(idUsuario, formulario.IdCuenta),
                Categorias = await _servicio.ObtenerCategoriasAsync(idUsuario, "Gasto", formulario.IdCategoria)
            };
        }
    }
}