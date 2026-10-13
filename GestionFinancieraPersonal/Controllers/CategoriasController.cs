using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using GestionFinancieraPersonal.Models;
using GestionFinancieraPersonal.Servicios;

namespace GestionFinancieraPersonal.Controllers
{
    [Authorize(Roles = "Usuario")]
    public class CategoriasController : Controller
    {
        private readonly CategoriaService _servicio;

        public CategoriasController(CategoriaService servicio)
        {
            _servicio = servicio;
        }

        // GET: /Categorias
        public async Task<IActionResult> Index()
        {
            return View(await ConstruirVistaAsync(new CategoriaUsuarioFormViewModel()));
        }

        // POST: /Categorias/Crear
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Crear(CategoriaUsuarioFormViewModel formulario)
        {
            if (ModelState.IsValid)
            {
                var resultado = await _servicio.CrearAsync(User.ObtenerIdUsuario(), formulario);
                if (resultado.Exito)
                {
                    TempData["Exito"] = $"Categoría \"{formulario.Nombre.Trim()}\" creada.";
                    return RedirectToAction(nameof(Index));
                }
                ModelState.AddModelError(resultado.Campo, resultado.Error!);
            }
            return View("Index", await ConstruirVistaAsync(formulario));
        }

        // GET: /Categorias/Editar/5
        [HttpGet]
        public async Task<IActionResult> Editar(int id)
        {
            var categoria = await _servicio.ObtenerPropiaAsync(User.ObtenerIdUsuario(), id);
            if (categoria == null) return NotFound();

            return View(new CategoriaUsuarioFormViewModel
            {
                Id = categoria.IdCategoria,
                Nombre = categoria.Nombre,
                Tipo = categoria.Tipo,
                Descripcion = categoria.Descripcion
            });
        }

        // POST: /Categorias/Editar
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Editar(CategoriaUsuarioFormViewModel formulario)
        {
            if (ModelState.IsValid)
            {
                var resultado = await _servicio.EditarAsync(
                    User.ObtenerIdUsuario(), formulario.Id, formulario.Nombre, formulario.Descripcion);
                if (resultado.Exito)
                {
                    TempData["Exito"] = "Categoría actualizada.";
                    return RedirectToAction(nameof(Index));
                }
                ModelState.AddModelError(resultado.Campo, resultado.Error!);
            }
            return View(formulario);
        }

        // POST: /Categorias/Eliminar/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Eliminar(int id)
        {
            var (resultado, desactivada) = await _servicio.EliminarAsync(User.ObtenerIdUsuario(), id);
            if (!resultado.Exito) TempData["Error"] = resultado.Error;
            else TempData["Exito"] = desactivada
                ? "La categoría tiene movimientos o presupuestos asociados, así que se desactivó en lugar de borrarse. Puedes reactivarla cuando quieras."
                : "Categoría eliminada.";
            return RedirectToAction(nameof(Index));
        }

        // POST: /Categorias/Reactivar/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Reactivar(int id)
        {
            var resultado = await _servicio.ReactivarAsync(User.ObtenerIdUsuario(), id);
            if (resultado.Exito) TempData["Exito"] = "Categoría reactivada.";
            else TempData["Error"] = resultado.Error;
            return RedirectToAction(nameof(Index));
        }

        private async Task<MisCategoriasViewModel> ConstruirVistaAsync(CategoriaUsuarioFormViewModel formulario)
        {
            return new MisCategoriasViewModel
            {
                Categorias = await _servicio.ObtenerVisiblesAsync(User.ObtenerIdUsuario()),
                Formulario = formulario
            };
        }
    }
}