using GestionFinancieraPersonal.Entidades;
using GestionFinancieraPersonal.Models;
using GestionFinancieraPersonal.Servicios;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GestionFinancieraPersonal.Controllers
{
    [Authorize(Roles = "Usuario")]
    public class CategoriasController : Controller
    {
        private readonly ApiClient _api;

        public CategoriasController(ApiClient api)
        {
            _api = api;
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
                var resultado = await _api.PostAsync("api/categorias", formulario);
                if (resultado.Exito)
                {
                    TempData["Exito"] = $"Categoría \"{formulario.Nombre.Trim()}\" creada.";
                    return RedirectToAction(nameof(Index));
                }
                ModelState.AddModelError(resultado.Campo, resultado.Error ?? "No se pudo crear la categoría.");
            }
            return View("Index", await ConstruirVistaAsync(formulario));
        }

        // GET: /Categorias/Editar/5
        [HttpGet]
        public async Task<IActionResult> Editar(int id)
        {
            var respuesta = await _api.GetAsync<Categoria>($"api/categorias/{id}");
            if (respuesta.Codigo == 404) return NotFound();

            var categoria = respuesta.Requerir();
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
                var resultado = await _api.PutAsync($"api/categorias/{formulario.Id}", formulario);
                if (resultado.Exito)
                {
                    TempData["Exito"] = "Categoría actualizada.";
                    return RedirectToAction(nameof(Index));
                }
                ModelState.AddModelError(resultado.Campo, resultado.Error ?? "No se pudo actualizar la categoría.");
            }
            return View(formulario);
        }

        // POST: /Categorias/Eliminar/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Eliminar(int id)
        {
            var resultado = await _api.DeleteAsync<EliminarCategoriaResponse>($"api/categorias/{id}");
            if (!resultado.Exito)
                TempData["Error"] = resultado.Error;
            else
                TempData["Exito"] = resultado.Datos?.Desactivada == true
                    ? "La categoría tiene movimientos o presupuestos asociados, así que se desactivó en lugar de borrarse. Puedes reactivarla cuando quieras."
                    : "Categoría eliminada.";
            return RedirectToAction(nameof(Index));
        }

        // POST: /Categorias/Reactivar/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Reactivar(int id)
        {
            var resultado = await _api.PostAsync($"api/categorias/{id}/reactivar");
            if (resultado.Exito) TempData["Exito"] = "Categoría reactivada.";
            else TempData["Error"] = resultado.Error;
            return RedirectToAction(nameof(Index));
        }

        private async Task<MisCategoriasViewModel> ConstruirVistaAsync(CategoriaUsuarioFormViewModel formulario)
        {
            var modelo = (await _api.GetAsync<MisCategoriasViewModel>("api/categorias")).Requerir();
            modelo.Formulario = formulario;
            return modelo;
        }
    }
}