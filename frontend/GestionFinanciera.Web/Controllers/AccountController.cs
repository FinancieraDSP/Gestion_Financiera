using System.Security.Claims;
using GestionFinancieraPersonal.Models;
using GestionFinancieraPersonal.Servicios;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GestionFinancieraPersonal.Controllers
{
    [AllowAnonymous]
    public class AccountController : Controller
    {
        private readonly ApiClient _api;
        private readonly ILogger<AccountController> _logger;

        public AccountController(ApiClient api, ILogger<AccountController> logger)
        {
            _api = api;
            _logger = logger;
        }

        // ---------- LOGIN ----------

        [HttpGet]
        public IActionResult Login()
        {
            if (User.Identity?.IsAuthenticated == true)
                return RedirigirSegunRol(User.IsInRole("Administrador"));

            return View(new LoginViewModel());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginViewModel modelo)
        {
            if (!ModelState.IsValid)
                return View(modelo);

            var respuesta = await _api.PostAsync<LoginResponse>("api/auth/login", new LoginRequest
            {
                Correo = modelo.Correo.Trim(),
                Contrasena = modelo.Contrasena,
                TipoAcceso = modelo.TipoAcceso
            });

            // Mantenimiento en curso y el correo no es de un administrador
            if (respuesta.Codigo == 423)
                return RedirectToAction("Index", "Mantenimiento");

            if (!respuesta.Exito || respuesta.Datos == null)
            {
                modelo.MensajeError = respuesta.Error ?? "Ocurrió un error al iniciar sesión. Intenta de nuevo.";
                return View(modelo);
            }

            await IniciarSesion(respuesta.Datos);
            return RedirigirSegunRol(respuesta.Datos.Rol == "Administrador");
        }

        // ---------- PASSWORD ----------

        [HttpGet]
        public IActionResult ForgotPassword() => View(new ForgotPasswordViewModel());

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ForgotPassword(ForgotPasswordViewModel modelo)
        {
            if (!ModelState.IsValid)
                return View(modelo);

            var respuesta = await _api.PostAsync("api/auth/olvido-contrasena", new ForgotPasswordViewModel { Correo = modelo.Correo });

            ModelState.Clear();
            if (!respuesta.Exito)
            {
                ModelState.AddModelError(string.Empty, respuesta.Error ?? "No se pudo procesar la solicitud.");
                return View(modelo);
            }

            modelo.Correo = string.Empty;
            modelo.MensajeExito = "Si existe una cuenta asociada a ese correo, se enviarán instrucciones de recuperación.";
            return View(modelo);
        }

        [HttpGet]
        public IActionResult ResetPassword(string token)
        {
            if (string.IsNullOrWhiteSpace(token))
                return RedirectToAction(nameof(Login));

            return View(new ResetPasswordViewModel { Token = token });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ResetPassword(ResetPasswordViewModel modelo)
        {
            if (!ModelState.IsValid)
                return View(modelo);

            var respuesta = await _api.PostAsync("api/auth/restablecer-contrasena", modelo);
            if (!respuesta.Exito)
            {
                modelo.MensajeError = respuesta.Error;
                return View(modelo);
            }

            TempData["Exito"] = "Tu contraseña fue actualizada correctamente.";
            return RedirectToAction(nameof(Login));
        }

        // ---------- REGISTRO ----------

        [HttpGet]
        public IActionResult Registro()
        {
            if (User.Identity?.IsAuthenticated == true)
                return RedirigirSegunRol(User.IsInRole("Administrador"));

            return View(new RegistroViewModel());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Registro(RegistroViewModel modelo)
        {
            if (!ModelState.IsValid)
                return View(modelo);

            var respuesta = await _api.PostAsync<LoginResponse>("api/auth/registro", modelo);
            if (!respuesta.Exito || respuesta.Datos == null)
            {
                modelo.MensajeError = respuesta.Error;
                return View(modelo);
            }

            await IniciarSesion(respuesta.Datos);
            return RedirectToAction("Index", "Dashboard", new { area = "" });
        }

        // ---------- LOGOUT / ACCESO DENEGADO ----------

        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return RedirectToAction("Login");
        }

        public IActionResult AccesoDenegado() => View();

        // ---------- AUXILIARES ----------

        private async Task IniciarSesion(LoginResponse datos)
        {
            var claims = new List<Claim>
            {
                new(ClaimTypes.NameIdentifier, datos.IdUsuario.ToString()),
                new(ClaimTypes.Name, datos.Nombre),
                new(ClaimTypes.Email, datos.Correo),
                new(ClaimTypes.Role, datos.Rol)
            };
            var identidad = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);

            var propiedades = new AuthenticationProperties { ExpiresUtc = datos.Expira };
            propiedades.StoreTokens(new[]
            {
                new AuthenticationToken { Name = "access_token", Value = datos.Token }
            });

            await HttpContext.SignInAsync(
                CookieAuthenticationDefaults.AuthenticationScheme,
                new ClaimsPrincipal(identidad),
                propiedades);

            _logger.LogInformation("Sesión iniciada en el frontend: {Correo} ({Rol})", datos.Correo, datos.Rol);
        }

        private IActionResult RedirigirSegunRol(bool esAdmin)
        {
            return esAdmin
                ? RedirectToAction("Index", "Dashboard", new { area = "Admin" })
                : RedirectToAction("Index", "Dashboard", new { area = "" });
        }
    }
}