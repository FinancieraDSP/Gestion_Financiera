using System.Security.Cryptography;
using System.Text;
using GestionFinancieraPersonal.Datos;
using GestionFinancieraPersonal.Entidades;
using GestionFinancieraPersonal.Models;
using GestionFinancieraPersonal.Servicios;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace GestionFinancieraPersonal.Controllers
{
    [ApiController]
    [AllowAnonymous]
    [Route("api/auth")]
    public class AuthController : ControllerBase
    {
        private const string MensajeCuentaBloqueada =
            "Tu cuenta ha sido bloqueada debido a demasiados intentos fallidos de acceso. Contacta con un administrador.";

        private readonly ApplicationDbContext _db;
        private readonly ILogger<AuthController> _logger;
        private readonly UsuarioService _usuarios;
        private readonly EmailService _email;
        private readonly TokenService _tokens;
        private readonly IConfiguration _config;
        private readonly PasswordHasher<Usuario> _hasher = new();
        private readonly MantenimientoService _mantenimiento;

        public AuthController(
            ApplicationDbContext db,
            ILogger<AuthController> logger,
            UsuarioService usuarios,
            EmailService email,
            TokenService tokens,
            IConfiguration config,
            MantenimientoService mantenimiento)
        {
            _db = db;
            _logger = logger;
            _usuarios = usuarios;
            _email = email;
            _tokens = tokens;
            _config = config;
            _mantenimiento = mantenimiento;
        }

        // POST /api/auth/login
        [HttpPost("login")]
        public async Task<ActionResult<LoginResponse>> Login(LoginRequest solicitud)
        {
            var correo = solicitud.Correo.Trim();
            var usuario = await _db.Usuarios
                .Include(u => u.Rol)
                .FirstOrDefaultAsync(u => u.Correo == correo);
            // Mantenimiento en curso: solo los administradores pueden entrar.
            // Para cualquier otro correo no se evalúa ninguna credencial.
            var mantenimiento = await _mantenimiento.ObtenerAsync();
            if (mantenimiento.Estado == EstadoMantenimiento.EnCurso && usuario?.Rol.Nombre != "Administrador")
                return StatusCode(StatusCodes.Status423Locked, new ErrorApi { Mensaje = "El sistema está en mantenimiento." });

            if (usuario == null ||
                _hasher.VerifyHashedPassword(usuario, usuario.PasswordHash, solicitud.Contrasena) == PasswordVerificationResult.Failed)
            {
                _logger.LogWarning("Intento de inicio de sesión fallido para {Correo}", correo);

                var seBloqueo = usuario != null && await _usuarios.RegistrarIntentoFallidoAsync(correo);
                return Unauthorized(new ErrorApi
                {
                    Mensaje = seBloqueo ? MensajeCuentaBloqueada : "Correo o contraseña incorrectos."
                });
            }

            if (!usuario.Estado)
                return StatusCode(StatusCodes.Status403Forbidden, new ErrorApi { Mensaje = MensajeCuentaBloqueada });

            if (usuario.Rol.Nombre != solicitud.TipoAcceso)
                return StatusCode(StatusCodes.Status403Forbidden,
                    new ErrorApi { Mensaje = $"Esta cuenta no tiene acceso como {solicitud.TipoAcceso}." });

            await _usuarios.ReiniciarIntentosAsync(usuario.IdUsuario);

            _logger.LogInformation("Inicio de sesión: {Correo} ({Rol})", usuario.Correo, usuario.Rol.Nombre);
            return Ok(_tokens.Crear(usuario, usuario.Rol.Nombre));
        }

        // POST /api/auth/registro
        [HttpPost("registro")]
        public async Task<ActionResult<LoginResponse>> Registro(RegistroViewModel solicitud)
        {
            var (resultado, usuario) = await _usuarios.CrearUsuarioAsync(
                solicitud.Nombre, solicitud.Correo, solicitud.Contrasena, "Usuario");

            if (!resultado.Exito)
                return BadRequest(new ErrorApi { Mensaje = resultado.Error ?? "No se pudo crear la cuenta." });

            return Ok(_tokens.Crear(usuario!, "Usuario"));
        }

        // POST /api/auth/olvido-contrasena  (siempre responde 200: no revela si el correo existe)
        [HttpPost("olvido-contrasena")]
        public async Task<IActionResult> OlvidoContrasena(ForgotPasswordViewModel solicitud)
        {
            var usuario = await _db.Usuarios.FirstOrDefaultAsync(u => u.Correo == solicitud.Correo);

            if (usuario != null)
            {
                var token = Guid.NewGuid().ToString();
                var hash = HashToken(token);

                await _db.PasswordResets
                    .Where(p => p.IdUsuario == usuario.IdUsuario && !p.Usado)
                    .ExecuteUpdateAsync(s => s.SetProperty(x => x.Usado, true));

                _db.PasswordResets.Add(new PasswordReset
                {
                    IdUsuario = usuario.IdUsuario,
                    TokenHash = hash,
                    FechaCreacion = DateTime.Now,
                    FechaExpiracion = DateTime.Now.AddMinutes(15),
                    Usado = false
                });
                await _db.SaveChangesAsync();

                // El enlace apunta al FRONTEND, que es quien tiene la pantalla de restablecer
                var baseUrl = _config["Frontend:BaseUrl"]?.TrimEnd('/')
                    ?? throw new InvalidOperationException("Falta 'Frontend:BaseUrl'.");
                var enlace = $"{baseUrl}/Account/ResetPassword?token={token}";

                await _email.EnviarRecuperacionAsync(usuario.Correo, enlace);
            }

            return Ok();
        }

        // POST /api/auth/restablecer-contrasena
        [HttpPost("restablecer-contrasena")]
        public async Task<IActionResult> RestablecerContrasena(ResetPasswordViewModel solicitud)
        {
            var tokenHash = HashToken(solicitud.Token);

            var reset = await _db.PasswordResets
                .FirstOrDefaultAsync(r => r.TokenHash == tokenHash && !r.Usado);

            if (reset == null)
                return BadRequest(new ErrorApi { Mensaje = "El enlace de recuperación no es válido." });

            if (reset.FechaExpiracion < DateTime.Now)
                return BadRequest(new ErrorApi { Mensaje = "El enlace de recuperación ha expirado." });

            var usuario = await _db.Usuarios.FirstOrDefaultAsync(u => u.IdUsuario == reset.IdUsuario);
            if (usuario == null)
                return BadRequest(new ErrorApi { Mensaje = "El usuario asociado no existe." });

            usuario.PasswordHash = _hasher.HashPassword(usuario, solicitud.NuevaContrasena);
            usuario.IntentosFallidos = 0;
            reset.Usado = true;
            await _db.SaveChangesAsync();

            return Ok();
        }

        private static string HashToken(string token) =>
            Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(token)));
    }
}