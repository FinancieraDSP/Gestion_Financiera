using GestionFinancieraPersonal.Areas.Admin.Models;
using GestionFinancieraPersonal.Servicios;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GestionFinancieraPersonal.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Administrador")]
    public class CategoriasController : Controller
    {
        private readonly ApiClient _api;

        public CategoriasController(ApiClient api)
        {
            _api = api;
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

            var resultado = await _api.PostAsync("api/admin/categorias", modelo);
            if (resultado.Exito) TempData["Exito"] = $"Categoría \"{modelo.Nombre.Trim()}\" creada.";
            else TempData["Error"] = resultado.Error;

            return RedirectToAction("Index", "Dashboard");
        }

        // GET: /Admin/Categorias/Editar/5
        [HttpGet]
        public async Task<IActionResult> Editar(int id)
        {
            var respuesta = await _api.GetAsync<CategoriaFormViewModel>($"api/admin/categorias/{id}");
            if (respuesta.Codigo == 404) return NotFound();
            return View(respuesta.Requerir());
        }

        // POST: /Admin/Categorias/Editar
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Editar(CategoriaFormViewModel modelo)
        {
            // El tipo no se puede cambiar: se toma siempre el real de la categoría
            var actual = await _api.GetAsync<CategoriaFormViewModel>($"api/admin/categorias/{modelo.Id}");
            if (actual.Codigo == 404) return NotFound();
            modelo.Tipo = actual.Requerir().Tipo;
            ModelState.Remove(nameof(modelo.Tipo));

            if (!ModelState.IsValid)
                return View(modelo);

            var resultado = await _api.PutAsync($"api/admin/categorias/{modelo.Id}", modelo);
            if (resultado.Codigo == 404) return NotFound();
            if (!resultado.Exito)
            {
                ModelState.AddModelError(resultado.Campo, resultado.Error ?? "No se pudo actualizar la categoría.");
                return View(modelo);
            }

            TempData["Exito"] = "Categoría actualizada.";
            return RedirectToAction("Index", "Dashboard");
        }

        // POST: /Admin/Categorias/Alternar/5  (activa o desactiva; no se borra para no perder el historial)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Alternar(int id)
        {
            var resultado = await _api.PostAsync<bool>($"api/admin/categorias/{id}/alternar");
            if (resultado.Codigo == 404) return NotFound();

            if (resultado.Exito)
                TempData["Exito"] = resultado.Datos ? "Categoría activada." : "Categoría desactivada.";
            else
                TempData["Error"] = resultado.Error;

            return RedirectToAction("Index", "Dashboard");
        }
    }
}