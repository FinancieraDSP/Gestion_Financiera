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
using System.Security.Cryptography;
using System.Text;

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
        private readonly EmailService _email;

        public AccountController(
        ApplicationDbContext db,
        ILogger<AccountController> logger,
        UsuarioService usuarios,
        EmailService email)
        {
            _db = db;
            _logger = logger;
            _usuarios = usuarios;
            _email = email;
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

        // ---------- PASSWORD ----------
        [HttpGet]
        public IActionResult ForgotPassword()
        {
            return View(new ForgotPasswordViewModel());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ForgotPassword(
            ForgotPasswordViewModel modelo)
        {
            if (!ModelState.IsValid)
                return View(modelo);

            var usuario = await _db.Usuarios
                .FirstOrDefaultAsync(u => u.Correo == modelo.Correo);

            if (usuario != null)
            {
                var token = Guid.NewGuid().ToString();

                using var sha = SHA256.Create();

                var hashBytes = sha.ComputeHash(
                    System.Text.Encoding.UTF8.GetBytes(token));

                var hash = Convert.ToHexString(hashBytes);

                await _db.PasswordResets
                    .Where(p => p.IdUsuario == usuario.IdUsuario && !p.Usado)
                    .ExecuteUpdateAsync(s =>
                        s.SetProperty(x => x.Usado, true));

                _db.PasswordResets.Add(new PasswordReset
                {
                    IdUsuario = usuario.IdUsuario,
                    TokenHash = hash,
                    FechaCreacion = DateTime.Now,
                    FechaExpiracion = DateTime.Now.AddMinutes(15),
                    Usado = false
                });

                await _db.SaveChangesAsync();

                var enlace =
                    $"{Request.Scheme}://{Request.Host}/Account/ResetPassword?token={token}";

                await _email.EnviarRecuperacionAsync(
                    usuario.Correo,
                    enlace);
            }

            ModelState.Clear();

            modelo.Correo = string.Empty;

            modelo.MensajeExito =
                "Si existe una cuenta asociada a ese correo, se enviarán instrucciones de recuperación.";

            return View(modelo);
        }

        [HttpGet]
        public IActionResult ResetPassword(string token)
        {
            if (string.IsNullOrWhiteSpace(token))
                return RedirectToAction(nameof(Login));

            return View(new ResetPasswordViewModel
            {
                Token = token
            });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ResetPassword(
            ResetPasswordViewModel modelo)
        {
            if (!ModelState.IsValid)
                return View(modelo);

            using var sha = SHA256.Create();

            var tokenHash = Convert.ToHexString(
                sha.ComputeHash(
                    Encoding.UTF8.GetBytes(modelo.Token)));

            var reset = await _db.PasswordResets
                .FirstOrDefaultAsync(r =>
                    r.TokenHash == tokenHash &&
                    !r.Usado);

            if (reset == null)
            {
                modelo.MensajeError =
                    "El enlace de recuperación no es válido.";
                return View(modelo);
            }

            if (reset.FechaExpiracion < DateTime.Now)
            {
                modelo.MensajeError =
                    "El enlace de recuperación ha expirado.";
                return View(modelo);
            }

            var usuario = await _db.Usuarios
                .FirstOrDefaultAsync(u =>
                    u.IdUsuario == reset.IdUsuario);

            if (usuario == null)
            {
                modelo.MensajeError =
                    "El usuario asociado no existe.";
                return View(modelo);
            }

            usuario.PasswordHash =
                _hasher.HashPassword(
                    usuario,
                    modelo.NuevaContrasena);

            usuario.IntentosFallidos = 0;

            reset.Usado = true;

            await _db.SaveChangesAsync();

            TempData["Exito"] =
                "Tu contraseña fue actualizada correctamente.";

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
