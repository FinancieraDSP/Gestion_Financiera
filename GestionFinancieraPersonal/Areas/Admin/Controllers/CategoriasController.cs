using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using GestionFinancieraPersonal.Areas.Admin.Models;
using GestionFinancieraPersonal.Datos;
using GestionFinancieraPersonal.Entidades;

namespace GestionFinancieraPersonal.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Administrador")]
    public class CategoriasController : Controller
    {
        private readonly ApplicationDbContext _db;

        public CategoriasController(ApplicationDbContext db)
        {
            _db = db;
        }

        // POST: /Admin/Categorias/Crear (formulario del panel)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Crear(CategoriaFormViewModel modelo)
        {
            if (!ModelState.IsValid)
            {
                TempData["Error"] = ModelState.Values.SelectMany(v => v.Errors).First().ErrorMessage;
                return RedirectToAction("Index", "Dashboard");
            }

            var nombre = modelo.Nombre.Trim();
            if (await _db.Categorias.AnyAsync(c => c.IdUsuario == null && c.Tipo == modelo.Tipo && c.Nombre == nombre))
            {
                TempData["Error"] = "Ya existe una categoría general con ese nombre.";
                return RedirectToAction("Index", "Dashboard");
            }

            _db.Categorias.Add(new Categoria
            {
                IdUsuario = null, // null = categoría general, visible para todos
                Nombre = nombre,
                Tipo = modelo.Tipo,
                Descripcion = string.IsNullOrWhiteSpace(modelo.Descripcion) ? null : modelo.Descripcion.Trim()
            });
            await _db.SaveChangesAsync();

            TempData["Exito"] = $"Categoría \"{nombre}\" creada.";
            return RedirectToAction("Index", "Dashboard");
        }

        // GET: /Admin/Categorias/Editar/5
        [HttpGet]
        public async Task<IActionResult> Editar(int id)
        {
            var categoria = await _db.Categorias.AsNoTracking()
                .FirstOrDefaultAsync(c => c.IdCategoria == id && c.IdUsuario == null);
            if (categoria == null) return NotFound();

            return View(new CategoriaFormViewModel
            {
                Id = categoria.IdCategoria,
                Nombre = categoria.Nombre,
                Tipo = categoria.Tipo,
                Descripcion = categoria.Descripcion
            });
        }

        // POST: /Admin/Categorias/Editar
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Editar(CategoriaFormViewModel modelo)
        {
            var categoria = await _db.Categorias.FirstOrDefaultAsync(c => c.IdCategoria == modelo.Id && c.IdUsuario == null);
            if (categoria == null) return NotFound();

            modelo.Tipo = categoria.Tipo; // el tipo no se puede cambiar (hay movimientos que dependen de él)
            ModelState.Remove(nameof(modelo.Tipo));

            if (!ModelState.IsValid)
                return View(modelo);

            var nombre = modelo.Nombre.Trim();
            if (await _db.Categorias.AnyAsync(c => c.IdUsuario == null && c.Tipo == categoria.Tipo
                                                  && c.Nombre == nombre && c.IdCategoria != categoria.IdCategoria))
            {
                ModelState.AddModelError(nameof(modelo.Nombre), "Ya existe una categoría general con ese nombre.");
                return View(modelo);
            }

            categoria.Nombre = nombre;
            categoria.Descripcion = string.IsNullOrWhiteSpace(modelo.Descripcion) ? null : modelo.Descripcion.Trim();
            await _db.SaveChangesAsync();

            TempData["Exito"] = "Categoría actualizada.";
            return RedirectToAction("Index", "Dashboard");
        }

        // POST: /Admin/Categorias/Alternar/5  (activa o desactiva; no se borra para no perder el historial)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Alternar(int id)
        {
            var categoria = await _db.Categorias.FirstOrDefaultAsync(c => c.IdCategoria == id && c.IdUsuario == null);
            if (categoria == null) return NotFound();

            categoria.Estado = !categoria.Estado;
            await _db.SaveChangesAsync();

            TempData["Exito"] = categoria.Estado ? "Categoría activada." : "Categoría desactivada.";
            return RedirectToAction("Index", "Dashboard");
        }
    }
}