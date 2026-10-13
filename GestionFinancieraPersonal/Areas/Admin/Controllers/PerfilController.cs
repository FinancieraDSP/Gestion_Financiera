using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using GestionFinancieraPersonal.Models;
using GestionFinancieraPersonal.Servicios;

namespace GestionFinancieraPersonal.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Administrador")]
    public class PerfilController : Controller
    {
        private readonly UsuarioService _usuarios;

        public PerfilController(UsuarioService usuarios)
        {
            _usuarios = usuarios;
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

            var resultado = await _usuarios.CambiarContrasenaAsync(
                User.ObtenerIdUsuario(), modelo.ContrasenaActual, modelo.NuevaContrasena);

            if (!resultado.Exito)
            {
                ModelState.AddModelError(resultado.Campo, resultado.Error!);
                return View(modelo);
            }

            TempData["Exito"] = "Tu contraseña fue actualizada correctamente.";
            return RedirectToAction(nameof(CambiarContrasena));
        }
    }
}