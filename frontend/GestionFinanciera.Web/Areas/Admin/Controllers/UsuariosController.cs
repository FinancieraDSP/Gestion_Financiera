using GestionFinancieraPersonal.Areas.Admin.Models;
using GestionFinancieraPersonal.Models;
using GestionFinancieraPersonal.Servicios;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GestionFinancieraPersonal.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Administrador")]
    public class UsuariosController : Controller
    {
        private readonly ApiClient _api;

        public UsuariosController(ApiClient api)
        {
            _api = api;
        }

        // GET: /Admin/Usuarios
        public async Task<IActionResult> Index(string? busqueda)
        {
            var url = string.IsNullOrWhiteSpace(busqueda)
                ? "api/admin/usuarios"
                : $"api/admin/usuarios?busqueda={Uri.EscapeDataString(busqueda.Trim())}";

            var lista = (await _api.GetAsync<List<UsuarioAdmin>>(url)).Requerir();
            return View(lista);
        }

        // GET: /Admin/Usuarios/Acciones/3
        public async Task<IActionResult> Acciones(int id)
        {
            var respuesta = await _api.GetAsync<UsuarioAdmin>($"api/admin/usuarios/{id}");
            if (respuesta.Codigo == 404) return NotFound();
            return View(respuesta.Requerir());
        }

        // GET: /Admin/Usuarios/Crear
        [HttpGet]
        public IActionResult Crear()
        {
            return View(new UsuarioCrearViewModel());
        }

        // POST: /Admin/Usuarios/Crear
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Crear(UsuarioCrearViewModel modelo)
        {
            if (!ModelState.IsValid)
                return View(modelo);

            var resultado = await _api.PostAsync("api/admin/usuarios", modelo);
            if (!resultado.Exito)
            {
                ModelState.AddModelError(resultado.Campo, resultado.Error ?? "No se pudo crear el usuario.");
                return View(modelo);
            }

            TempData["Exito"] = $"Usuario {modelo.Correo.Trim()} creado correctamente.";
            return RedirectToAction(nameof(Index));
        }

        // POST: /Admin/Usuarios/Bloquear/3
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Bloquear(int id)
        {
            var resultado = await _api.PostAsync($"api/admin/usuarios/{id}/bloquear");
            Notificar(resultado, "Cuenta bloqueada.");
            return RedirectToAction(nameof(Index));
        }

        // POST: /Admin/Usuarios/Desbloquear/3
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Desbloquear(int id)
        {
            var resultado = await _api.PostAsync($"api/admin/usuarios/{id}/desbloquear");
            Notificar(resultado, "Cuenta desbloqueada.");
            return RedirectToAction(nameof(Index));
        }

        // POST: /Admin/Usuarios/Eliminar/3
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Eliminar(int id)
        {
            var resultado = await _api.DeleteAsync($"api/admin/usuarios/{id}");
            Notificar(resultado, "Cuenta eliminada junto con todos sus datos.");
            return RedirectToAction(nameof(Index));
        }

        private void Notificar(ApiResultado resultado, string mensajeExito)
        {
            if (resultado.Exito) TempData["Exito"] = mensajeExito;
            else TempData["Error"] = resultado.Error;
        }
    }
}