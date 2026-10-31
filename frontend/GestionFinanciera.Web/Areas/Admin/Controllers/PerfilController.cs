using GestionFinancieraPersonal.Models;
using GestionFinancieraPersonal.Servicios;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GestionFinancieraPersonal.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Administrador")]
    public class PerfilController : Controller
    {
        private readonly ApiClient _api;

        public PerfilController(ApiClient api)
        {
            _api = api;
        }

        [HttpGet]
        public IActionResult CambiarContrasena()
        {
            return View(new CambiarContrasenaViewModel());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CambiarContrasena(CambiarContrasenaViewModel modelo)
        {
            if (!ModelState.IsValid)
                return View(modelo);

            var resultado = await _api.PutAsync("api/perfil/contrasena", modelo);
            if (!resultado.Exito)
            {
                ModelState.AddModelError(resultado.Campo, resultado.Error ?? "No se pudo cambiar la contraseña.");
                return View(modelo);
            }

            TempData["Exito"] = "Tu contraseña fue actualizada correctamente.";
            return RedirectToAction(nameof(CambiarContrasena));
        }
    }
}