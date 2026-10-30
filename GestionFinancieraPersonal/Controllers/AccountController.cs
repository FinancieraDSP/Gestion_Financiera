using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using GestionFinancieraPersonal.Datos;
using GestionFinancieraPersonal.Entidades;
using GestionFinancieraPersonal.Models;
using GestionFinancieraPersonal.Servicios;

namespace GestionFinancieraPersonal.Controllers
{
    [AllowAnonymous]
    public class AccountController : Controller
    {
        private const string MensajeCuentaBloqueada =
            "Tu cuenta ha sido bloqueada debido a demasiados intentos fallidos de acceso. Contacta con un administrador.";

        private readonly ApplicationDbContext _db;
        private readonly ILogger<AccountController> _logger;
        private readonly UsuarioService _usuarios;
        private readonly PasswordHasher<Usuario> _hasher = new();

        public AccountController(ApplicationDbContext db, ILogger<AccountController> logger, UsuarioService usuarios)
        {
            _db = db;
            _logger = logger;
            _usuarios = usuarios;
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

            try
            {
                var correo = modelo.Correo.Trim();
                var usuario = await _db.Usuarios
                    .Include(u => u.Rol)
                    .FirstOrDefaultAsync(u => u.Correo == correo);

                if (usuario == null ||
                    _hasher.VerifyHashedPassword(usuario, usuario.PasswordHash, modelo.Contrasena) == PasswordVerificationResult.Failed)
                {
                    _logger.LogWarning("Intento de inicio de sesión fallido para {Correo}", correo);

                    // Fase 2: cuenta el intento (admins y cuentas ya bloqueadas no cuentan).
                    // Si este intento alcanza el límite configurado, se bloquea y se avisa.
                    var seBloqueo = usuario != null && await _usuarios.RegistrarIntentoFallidoAsync(correo);

                    modelo.MensajeError = seBloqueo
                        ? MensajeCuentaBloqueada
                        : "Correo o contraseña incorrectos.";
                    return View(modelo);
                }

                if (!usuario.Estado)
                {
                    modelo.MensajeError = MensajeCuentaBloqueada;
                    return View(modelo);
                }

                if (usuario.Rol.Nombre != modelo.TipoAcceso)
                {
                    modelo.MensajeError = $"Esta cuenta no tiene acceso como {modelo.TipoAcceso}.";
                    return View(modelo);
                }

                // Login completo y exitoso: único punto donde se reinicia el contador
                await _usuarios.ReiniciarIntentosAsync(usuario.IdUsuario);

                await IniciarSesion(usuario, usuario.Rol.Nombre);
                _logger.LogInformation("Inicio de sesión: {Correo} ({Rol})", usuario.Correo, usuario.Rol.Nombre);
                return RedirigirSegunRol(usuario.Rol.Nombre == "Administrador");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al iniciar sesión para {Correo}", modelo.Correo);
                modelo.MensajeError = "Ocurrió un error al iniciar sesión. Intenta de nuevo.";
                return View(modelo);
            }
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

            var (resultado, usuario) = await _usuarios.CrearUsuarioAsync(
                modelo.Nombre, modelo.Correo, modelo.Contrasena, "Usuario");

            if (!resultado.Exito)
            {
                modelo.MensajeError = resultado.Error;
                return View(modelo);
            }

            await IniciarSesion(usuario!, "Usuario");
            return RedirectToAction("Index", "Dashboard", new { area = "" });
        }

        // ---------- LOGOUT / ACCESO DENEGADO ----------

        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return RedirectToAction("Login");
        }

        public IActionResult AccesoDenegado()
        {
            return View();
        }

        // ---------- AUXILIARES ----------

        private async Task IniciarSesion(Usuario usuario, string nombreRol)
        {
            var claims = new List<Claim>
            {
                new(ClaimTypes.NameIdentifier, usuario.IdUsuario.ToString()),
                new(ClaimTypes.Name, usuario.Nombre),
                new(ClaimTypes.Email, usuario.Correo),
                new(ClaimTypes.Role, nombreRol)
            };
            var identidad = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);

            await HttpContext.SignInAsync(
                CookieAuthenticationDefaults.AuthenticationScheme,
                new ClaimsPrincipal(identidad));
        }

        private IActionResult RedirigirSegunRol(bool esAdmin)
        {
            return esAdmin
                ? RedirectToAction("Index", "Dashboard", new { area = "Admin" })
                : RedirectToAction("Index", "Dashboard", new { area = "" });
        }
    }
}
