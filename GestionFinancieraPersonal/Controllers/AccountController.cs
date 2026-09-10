using Microsoft.AspNetCore.Mvc;
using GestionFinancieraPersonal.Models;

namespace GestionFinancieraPersonal.Controllers
{
    public class AccountController : Controller
    {
        // GET: /Account/Login
        public IActionResult Login()
        {
            return View(new LoginViewModel());
        }

        // POST: /Account/Login
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Login(LoginViewModel modelo)
        {
            if (!ModelState.IsValid)
                return View(modelo);

            // Validación temporal mientras llega la BDD real.
            if (modelo.Correo == "carlos@mail.com" && modelo.Contrasena == "1234")
            {
                return RedirectToAction("Index", "Dashboard");
            }

            modelo.MensajeError = "Correo o contraseña incorrectos.";
            return View(modelo);
        }

        public IActionResult Logout()
        {
            return RedirectToAction("Login");
        }
    }
}
