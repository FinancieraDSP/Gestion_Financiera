using Microsoft.EntityFrameworkCore;
using GestionFinancieraPersonal.Datos;
using GestionFinancieraPersonal.Entidades;
using GestionFinancieraPersonal.Models;

namespace GestionFinancieraPersonal.Servicios
{
    public class AhorroService
    {
        private static readonly string[] IconosPermitidos =
            { "ti-target", "ti-device-mobile", "ti-plane", "ti-device-laptop", "ti-car", "ti-home" };

        private readonly ApplicationDbContext _db;
        private readonly ILogger<AhorroService> _logger;

        public AhorroService(ApplicationDbContext db, ILogger<AhorroService> logger)
        {
            _db = db;
            _logger = logger;
        }

        // Activas primero, completadas después
        public Task<List<MetaAhorro>> ObtenerMetasAsync(int idUsuario) =>
            _db.MetasAhorro.AsNoTracking()
                .Where(m => m.IdUsuario == idUsuario && m.Estado != "Cancelada")
                .OrderBy(m => m.Estado).ThenBy(m => m.IdMeta)
                .ToListAsync();

        // ================= CREAR META =================

        public async Task<ResultadoOperacion> CrearMetaAsync(int idUsuario, MetaFormViewModel f)
        {
            try
            {
                var fecha = f.FechaLimite?.Date;
                if (fecha.HasValue && fecha.Value < DateTime.Today)
                    return ResultadoOperacion.Fallo("La fecha meta no puede ser anterior a hoy.", "FechaLimite");

                var nombre = f.Nombre.Trim();
                if (await _db.MetasAhorro.AnyAsync(m => m.IdUsuario == idUsuario && m.Nombre == nombre && m.Estado != "Cancelada"))
                    return ResultadoOperacion.Fallo("Ya tienes una meta con ese nombre.", "Nombre");

                _db.MetasAhorro.Add(new MetaAhorro
                {
                    IdUsuario = idUsuario,
                    Nombre = nombre,
                    Icono = IconosPermitidos.Contains(f.Icono) ? f.Icono : "ti-target",
                    MontoObjetivo = f.MontoObjetivo!.Value,
                    MontoActual = 0,
                    FechaLimite = fecha,
                    Estado = "Activa"
                });
                await _db.SaveChangesAsync();
                return ResultadoOperacion.Ok();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al crear meta del usuario {IdUsuario}", idUsuario);
                return ResultadoOperacion.Fallo("No se pudo crear la meta. Intenta de nuevo.");
            }
        }

        // ================= APORTAR =================

        public async Task<ResultadoOperacion> AportarAsync(int idUsuario, AporteFormViewModel f)
        {
            try
            {
                var meta = await _db.MetasAhorro.FirstOrDefaultAsync(m =>
                    m.IdMeta == f.IdMeta && m.IdUsuario == idUsuario && m.Estado == "Activa");
                if (meta == null)
                    return ResultadoOperacion.Fallo("La meta no existe o ya fue completada.", "IdMeta");

                var cuenta = await _db.Cuentas.FirstOrDefaultAsync(c =>
                    c.IdCuenta == f.IdCuenta && c.IdUsuario == idUsuario && c.Estado);
                if (cuenta == null)
                    return ResultadoOperacion.Fallo("La cuenta seleccionada no es válida.", "IdCuenta");

                var monto = f.Monto!.Value;
                var faltante = meta.MontoObjetivo - meta.MontoActual;

                // Regla: no pasarse del objetivo (CHECK monto_actual <= monto_objetivo)
                if (monto > faltante)
                    return ResultadoOperacion.Fallo($"El aporte supera lo que falta para la meta ({faltante:C}).", "Monto");

                // Regla: no aportar más de lo que hay en la cuenta
                if (cuenta.SaldoActual < monto)
                    return ResultadoOperacion.Fallo($"Saldo insuficiente en {cuenta.Nombre}. Disponible: {cuenta.SaldoActual:C}.", "Monto");

                cuenta.SaldoActual -= monto;
                meta.MontoActual += monto;
                meta.IdCuenta ??= cuenta.IdCuenta; // recuerda de dónde viene el dinero, para devolverlo si se elimina la meta

                if (meta.MontoActual >= meta.MontoObjetivo)
                {
                    meta.Estado = "Completada";
                    _db.Notificaciones.Add(new Notificacion
                    {
                        IdUsuario = idUsuario,
                        Tipo = "Meta",
                        Titulo = "¡Meta alcanzada!",
                        Mensaje = $"Completaste tu meta \"{meta.Nombre}\" con {meta.MontoObjetivo:C} ahorrados."
                    });
                }

                await _db.SaveChangesAsync();
                return ResultadoOperacion.Ok();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al aportar a meta del usuario {IdUsuario}", idUsuario);
                return ResultadoOperacion.Fallo("No se pudo registrar el aporte. Intenta de nuevo.");
            }
        }

        // ================= ELIMINAR (devuelve lo ahorrado a una cuenta) =================

        public async Task<(ResultadoOperacion resultado, decimal devuelto, string? cuenta)> EliminarMetaAsync(int idUsuario, int idMeta)
        {
            try
            {
                var meta = await _db.MetasAhorro.FirstOrDefaultAsync(m =>
                    m.IdMeta == idMeta && m.IdUsuario == idUsuario && m.Estado != "Cancelada");
                if (meta == null)
                    return (ResultadoOperacion.Fallo("La meta no existe."), 0, null);

                decimal devuelto = 0;
                string? nombreCuenta = null;

                if (meta.MontoActual > 0)
                {
                    Cuenta? destino = null;
                    if (meta.IdCuenta.HasValue)
                        destino = await _db.Cuentas.FirstOrDefaultAsync(c =>
                            c.IdCuenta == meta.IdCuenta.Value && c.IdUsuario == idUsuario && c.Estado);

                    destino ??= await _db.Cuentas
                        .Where(c => c.IdUsuario == idUsuario && c.Estado)
                        .OrderBy(c => c.IdCuenta)
                        .FirstOrDefaultAsync();

                    if (destino == null)
                        return (ResultadoOperacion.Fallo("No tienes una cuenta activa donde devolver el dinero ahorrado."), 0, null);

                    destino.SaldoActual += meta.MontoActual;
                    devuelto = meta.MontoActual;
                    nombreCuenta = destino.Nombre;
                }

                _db.MetasAhorro.Remove(meta);
                await _db.SaveChangesAsync();
                return (ResultadoOperacion.Ok(), devuelto, nombreCuenta);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al eliminar meta {IdMeta}", idMeta);
                return (ResultadoOperacion.Fallo("No se pudo eliminar la meta."), 0, null);
            }
        }
    }
}