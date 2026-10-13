using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using GestionFinancieraPersonal.Entidades;
using GestionFinancieraPersonal.Models;
using GestionFinancieraPersonal.Servicios;

namespace GestionFinancieraPersonal.Controllers
{
    [Authorize(Roles = "Usuario")]
    public class IngresosController : Controller
    {
        private readonly MovimientoService _servicio;

        public IngresosController(MovimientoService servicio)
        {
            _servicio = servicio;
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
                var resultado = await _servicio.RegistrarIngresoAsync(User.ObtenerIdUsuario(), formulario);
                if (resultado.Exito)
                {
                    TempData["Exito"] = "Ingreso registrado correctamente.";
                    return RedirectToAction(nameof(Index));
                }
                ModelState.AddModelError(resultado.Campo, resultado.Error!);
            }

            return View("Index", await ConstruirVistaAsync(formulario));
        }

        // POST: /Ingresos/Eliminar/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Eliminar(int id)
        {
            var resultado = await _servicio.EliminarIngresoAsync(User.ObtenerIdUsuario(), id);
            if (resultado.Exito) TempData["Exito"] = "Ingreso eliminado.";
            else TempData["Error"] = resultado.Error;
            return RedirectToAction(nameof(Index));
        }

        private async Task<MovimientosViewModel<Ingreso>> ConstruirVistaAsync(MovimientoFormViewModel formulario)
        {
            var idUsuario = User.ObtenerIdUsuario();
            return new MovimientosViewModel<Ingreso>
            {
                Items = await _servicio.ObtenerIngresosAsync(idUsuario),
                Formulario = formulario,
                Cuentas = await _servicio.ObtenerCuentasAsync(idUsuario, formulario.IdCuenta),
                Categorias = await _servicio.ObtenerCategoriasAsync(idUsuario, "Ingreso", formulario.IdCategoria)
            };
        }
    }
}