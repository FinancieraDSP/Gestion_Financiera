using GestionFinancieraPersonal.Datos;
using GestionFinancieraPersonal.Entidades;
using Microsoft.AspNetCore.Identity;
using System.Security.Cryptography;
using Microsoft.EntityFrameworkCore;

namespace GestionFinancieraPersonal.Servicios
{
    public class UsuarioService
    {
        private readonly ApplicationDbContext _db;
        private readonly ILogger<UsuarioService> _logger;
        private readonly ConfiguracionService _configuracion;
        private readonly PasswordHasher<Usuario> _hasher = new();

        public UsuarioService(ApplicationDbContext db, ILogger<UsuarioService> logger, ConfiguracionService configuracion)
        {
            _db = db;
            _logger = logger;
            _configuracion = configuracion;
        }

        // ================= CREAR (registro público y creación desde Admin) =================

        public async Task<(ResultadoOperacion resultado, Usuario? usuario)> CrearUsuarioAsync(
            string nombre, string correo, string contrasena, string nombreRol)
        {
            try
            {
                correo = correo.Trim();

                if (await _db.Usuarios.AnyAsync(u => u.Correo == correo))
                    return (ResultadoOperacion.Fallo("Ya existe una cuenta con ese correo.", "Correo"), null);

                var rol = await _db.Roles.FirstOrDefaultAsync(r => r.Nombre == nombreRol);
                if (rol == null)
                    return (ResultadoOperacion.Fallo("El rol seleccionado no es válido.", "Rol"), null);

                await using var transaccion = await _db.Database.BeginTransactionAsync();

                var usuario = new Usuario { IdRol = rol.IdRol, Nombre = nombre.Trim(), Correo = correo };
                usuario.PasswordHash = _hasher.HashPassword(usuario, contrasena);
                _db.Usuarios.Add(usuario);
                await _db.SaveChangesAsync();

                // Los usuarios normales arrancan con dos cuentas para poder registrar movimientos
                if (nombreRol == "Usuario")
                {
                    _db.Cuentas.AddRange(
                        new Cuenta { IdUsuario = usuario.IdUsuario, Nombre = "Efectivo", Tipo = "Efectivo", SaldoInicial = 0, SaldoActual = 0 },
                        new Cuenta { IdUsuario = usuario.IdUsuario, Nombre = "Cuenta principal", Tipo = "Banco", SaldoInicial = 0, SaldoActual = 0 });
                    await _db.SaveChangesAsync();
                }

                await transaccion.CommitAsync();
                _logger.LogInformation("Usuario creado: {Correo} ({Rol})", usuario.Correo, nombreRol);
                return (ResultadoOperacion.Ok(), usuario);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al crear usuario {Correo}", correo);
                return (ResultadoOperacion.Fallo("No se pudo crear la cuenta. Intenta de nuevo."), null);
            }
        }

        // ================= INTENTOS DE ACCESO =================

        /// <summary>
        /// Registra una contraseña incorrecta para el correo indicado.
        /// Devuelve true SOLO si este intento provocó el bloqueo automático de la cuenta.
        /// No hace nada (devuelve false) si el correo no existe, la cuenta ya está bloqueada
        /// o el usuario es administrador (exentos del bloqueo automático).
        /// El límite se lee siempre de ConfiguracionSistema.
        /// </summary>
        public async Task<bool> RegistrarIntentoFallidoAsync(string correo)
        {
            try
            {
                correo = correo.Trim();

                var objetivo = await _db.Usuarios.AsNoTracking()
                    .Where(u => u.Correo == correo)
                    .Select(u => new { u.IdUsuario, u.Estado, Rol = u.Rol.Nombre })
                    .FirstOrDefaultAsync();

                if (objetivo == null || !objetivo.Estado || objetivo.Rol == "Administrador")
                    return false;

                var limite = (await _configuracion.ObtenerAsync()).IntentosPermitidos;
                var id = objetivo.IdUsuario;

                // Incremento atómico: evita perder intentos si llegan peticiones simultáneas
                await _db.Usuarios
                    .Where(u => u.IdUsuario == id && u.Estado)
                    .ExecuteUpdateAsync(s => s.SetProperty(u => u.IntentosFallidos, u => u.IntentosFallidos + 1));

                // ">=" y no "==": si el admin baja el límite en caliente, el siguiente fallo ya bloquea
                var bloqueados = await _db.Usuarios
                    .Where(u => u.IdUsuario == id && u.Estado && u.IntentosFallidos >= limite)
                    .ExecuteUpdateAsync(s => s.SetProperty(u => u.Estado, false));

                if (bloqueados > 0)
                    _logger.LogWarning("Cuenta bloqueada automáticamente por intentos fallidos: {Correo} (límite {Limite})", correo, limite);

                return bloqueados > 0;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al registrar intento fallido para {Correo}", correo);
                return false;
            }
        }

        /// <summary>Reinicia el contador. Se llama solo tras un login completo y exitoso.</summary>
        public async Task ReiniciarIntentosAsync(int idUsuario)
        {
            try
            {
                await _db.Usuarios
                    .Where(u => u.IdUsuario == idUsuario && u.IntentosFallidos != 0)
                    .ExecuteUpdateAsync(s => s.SetProperty(u => u.IntentosFallidos, 0));
            }
            catch (Exception ex)
            {
                // Un fallo aquí no debe impedir que el usuario entre
                _logger.LogError(ex, "Error al reiniciar intentos fallidos del usuario {IdUsuario}", idUsuario);
            }
        }

        // ================= BLOQUEAR / DESBLOQUEAR =================

        public async Task<ResultadoOperacion> CambiarEstadoAsync(int idUsuario, bool activo)
        {
            try
            {
                var usuario = await _db.Usuarios.Include(u => u.Rol).FirstOrDefaultAsync(u => u.IdUsuario == idUsuario);
                if (usuario == null) return ResultadoOperacion.Fallo("El usuario no existe.");
                if (usuario.Rol.Nombre == "Administrador")
                    return ResultadoOperacion.Fallo("No se pueden modificar cuentas de administrador.");

                usuario.Estado = activo;

                // Desbloqueo manual: Estado = true e IntentosFallidos = 0
                if (activo)
                    usuario.IntentosFallidos = 0;

                await _db.SaveChangesAsync();
                _logger.LogInformation("Usuario {Correo} {Accion}", usuario.Correo, activo ? "desbloqueado" : "bloqueado");
                return ResultadoOperacion.Ok();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al cambiar estado del usuario {IdUsuario}", idUsuario);
                return ResultadoOperacion.Fallo("No se pudo cambiar el estado de la cuenta.");
            }
        }

        // ================= RESTABLECER CONTRASEÑA =================

        public async Task<(ResultadoOperacion resultado, string? clave, string? correo)> RestablecerContrasenaAsync(int idUsuario)
        {
            try
            {
                var usuario = await _db.Usuarios.Include(u => u.Rol).FirstOrDefaultAsync(u => u.IdUsuario == idUsuario);
                if (usuario == null) return (ResultadoOperacion.Fallo("El usuario no existe."), null, null);
                if (usuario.Rol.Nombre == "Administrador")
                    return (ResultadoOperacion.Fallo("No se pueden modificar cuentas de administrador."), null, null);

                const string caracteres = "ABCDEFGHJKLMNPQRSTUVWXYZabcdefghijkmnpqrstuvwxyz23456789";
                var temporal = RandomNumberGenerator.GetString(caracteres, 10);

                usuario.PasswordHash = _hasher.HashPassword(usuario, temporal);
                usuario.IntentosFallidos = 0; // restablecer contraseña reinicia el contador
                await _db.SaveChangesAsync();
                return (ResultadoOperacion.Ok(), temporal, usuario.Correo);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al restablecer contraseña del usuario {IdUsuario}", idUsuario);
                return (ResultadoOperacion.Fallo("No se pudo restablecer la contraseña."), null, null);
            }
        }

        // ================= CAMBIAR CONTRASEÑA (la hace el propio usuario) =================

        public async Task<ResultadoOperacion> CambiarContrasenaAsync(int idUsuario, string actual, string nueva)
        {
            try
            {
                var usuario = await _db.Usuarios.FirstOrDefaultAsync(u => u.IdUsuario == idUsuario);
                if (usuario == null) return ResultadoOperacion.Fallo("El usuario no existe.");

                if (_hasher.VerifyHashedPassword(usuario, usuario.PasswordHash, actual) == PasswordVerificationResult.Failed)
                {
                    _logger.LogWarning("Cambio de contraseña rechazado (contraseña actual incorrecta): {Correo}", usuario.Correo);
                    return ResultadoOperacion.Fallo("La contraseña actual no es correcta.", "ContrasenaActual");
                }

                if (actual == nueva)
                    return ResultadoOperacion.Fallo("La nueva contraseña debe ser distinta a la actual.", "NuevaContrasena");

                usuario.PasswordHash = _hasher.HashPassword(usuario, nueva);
                await _db.SaveChangesAsync();

                _logger.LogInformation("Contraseña actualizada por el propio usuario: {Correo}", usuario.Correo);
                return ResultadoOperacion.Ok();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al cambiar la contraseña del usuario {IdUsuario}", idUsuario);
                return ResultadoOperacion.Fallo("No se pudo cambiar la contraseña. Intenta de nuevo.");
            }
        }

        // ================= ELIMINAR CUENTA Y TODOS SUS DATOS =================

        public async Task<ResultadoOperacion> EliminarUsuarioAsync(int idUsuario)
        {
            try
            {
                var usuario = await _db.Usuarios.Include(u => u.Rol).FirstOrDefaultAsync(u => u.IdUsuario == idUsuario);
                if (usuario == null) return ResultadoOperacion.Fallo("El usuario no existe.");
                if (usuario.Rol.Nombre == "Administrador")
                    return ResultadoOperacion.Fallo("No se pueden eliminar cuentas de administrador.");

                // El orden importa por las llaves foráneas: primero lo que depende de otras tablas
                await using var transaccion = await _db.Database.BeginTransactionAsync();

                await _db.PagosDeuda.Where(p => _db.Deudas.Any(d => d.IdDeuda == p.IdDeuda && d.IdUsuario == idUsuario)).ExecuteDeleteAsync();
                await _db.Deudas.Where(d => d.IdUsuario == idUsuario).ExecuteDeleteAsync();
                await _db.MetasAhorro.Where(m => m.IdUsuario == idUsuario).ExecuteDeleteAsync();
                await _db.Notificaciones.Where(n => n.IdUsuario == idUsuario).ExecuteDeleteAsync();
                await _db.Recomendaciones.Where(r => r.IdUsuario == idUsuario).ExecuteDeleteAsync();
                await _db.Presupuestos.Where(p => p.IdUsuario == idUsuario).ExecuteDeleteAsync();
                await _db.Gastos.Where(g => g.IdUsuario == idUsuario).ExecuteDeleteAsync();
                await _db.Ingresos.Where(i => i.IdUsuario == idUsuario).ExecuteDeleteAsync();
                await _db.Cuentas.Where(c => c.IdUsuario == idUsuario).ExecuteDeleteAsync();
                await _db.Categorias.Where(c => c.IdUsuario == idUsuario).ExecuteDeleteAsync();
                await _db.Usuarios.Where(u => u.IdUsuario == idUsuario).ExecuteDeleteAsync();

                await transaccion.CommitAsync();
                _logger.LogWarning("Usuario eliminado: {Correo}", usuario.Correo);
                return ResultadoOperacion.Ok();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al eliminar usuario {IdUsuario}", idUsuario);
                return ResultadoOperacion.Fallo("No se pudo eliminar la cuenta.");
            }
        }
    }
}
