using Microsoft.EntityFrameworkCore;
using GestionFinancieraPersonal.Datos;
using GestionFinancieraPersonal.Entidades;

namespace GestionFinancieraPersonal.Servicios
{
    public enum EstadoMantenimiento { SinProgramar, Programado, EnCurso, Finalizado }

    public record InfoMantenimiento(EstadoMantenimiento Estado, DateTime? Inicio, DateTime? Fin);

    // Mantenimiento programado. Todo se guarda en ConfiguracionSistema
    // (MantenimientoActivo = "hay programación", FechaInicioMantenimiento, FechaFinMantenimiento).
    // El estado se deduce de la hora actual: no hay tareas programadas.
    public class MantenimientoService
    {
        private const string TipoNotificacion = "Mantenimiento";

        private readonly ApplicationDbContext _db;
        private readonly ILogger<MantenimientoService> _logger;
        private InfoMantenimiento? _cache; // una sola consulta por petición (filtro + avisos + vistas)

        public MantenimientoService(ApplicationDbContext db, ILogger<MantenimientoService> logger)
        {
            _db = db;
            _logger = logger;
        }

        private static InfoMantenimiento Calcular(Configuracion? c, DateTime ahora)
        {
            if (c == null || !c.MantenimientoActivo ||
                c.FechaInicioMantenimiento == null || c.FechaFinMantenimiento == null)
                return new InfoMantenimiento(EstadoMantenimiento.SinProgramar, null, null);

            var inicio = c.FechaInicioMantenimiento.Value;
            var fin = c.FechaFinMantenimiento.Value;
            var estado = ahora < inicio ? EstadoMantenimiento.Programado
                       : ahora < fin ? EstadoMantenimiento.EnCurso
                       : EstadoMantenimiento.Finalizado;
            return new InfoMantenimiento(estado, inicio, fin);
        }

        public async Task<InfoMantenimiento> ObtenerAsync()
        {
            if (_cache != null) return _cache;
            var c = await _db.Configuraciones.AsNoTracking().FirstOrDefaultAsync();
            return _cache = Calcular(c, DateTime.Now);
        }

        // ================= ADMIN =================

        public async Task<ResultadoOperacion> GuardarAsync(DateTime inicio, DateTime fin)
        {
            try
            {
                var c = await _db.Configuraciones.FirstOrDefaultAsync();
                if (c == null) { c = new Configuracion(); _db.Configuraciones.Add(c); }

                var ahora = DateTime.Now;
                var ahoraMinuto = new DateTime(ahora.Year, ahora.Month, ahora.Day, ahora.Hour, ahora.Minute, 0);
                var enCurso = Calcular(c, ahora).Estado == EstadoMantenimiento.EnCurso;

                if (fin <= inicio)
                    return ResultadoOperacion.Fallo("La fecha y hora de fin deben ser posteriores al inicio.", "Fin");

                // Un mantenimiento en curso conserva su inicio (pasado); solo se puede ajustar el fin
                var inicioSinCambios = enCurso && c.FechaInicioMantenimiento == inicio;
                if (!inicioSinCambios && inicio < ahoraMinuto)
                    return ResultadoOperacion.Fallo("El inicio no puede estar en el pasado.", "Inicio");

                if (fin <= ahora)
                    return ResultadoOperacion.Fallo("El fin no puede estar en el pasado.", "Fin");

                c.MantenimientoActivo = true;
                c.FechaInicioMantenimiento = inicio;
                c.FechaFinMantenimiento = fin;
                await _db.SaveChangesAsync();
                _cache = null;

                // Si ya había avisos de una programación anterior, se actualizan (o se retiran si ya inició)
                if (inicio > ahora)
                {
                    var texto = TextoAviso(inicio, fin);
                    await _db.Notificaciones.Where(n => n.Tipo == TipoNotificacion)
                        .ExecuteUpdateAsync(s => s
                            .SetProperty(n => n.Mensaje, texto)
                            .SetProperty(n => n.Leida, false)
                            .SetProperty(n => n.Eliminada, false)
                            .SetProperty(n => n.Fecha, ahora));
                }
                else
                {
                    await RetirarAvisosAsync();
                }

                _logger.LogWarning("Mantenimiento programado: {Inicio} a {Fin}", inicio, fin);
                return ResultadoOperacion.Ok();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al guardar la programación de mantenimiento");
                return ResultadoOperacion.Fallo("No se pudo guardar la programación. Intenta de nuevo.");
            }
        }

        public async Task<ResultadoOperacion> CancelarAsync()
        {
            try
            {
                var c = await _db.Configuraciones.FirstOrDefaultAsync();
                if (c == null || !c.MantenimientoActivo)
                    return ResultadoOperacion.Fallo("No hay ninguna programación de mantenimiento.");

                c.MantenimientoActivo = false;
                c.FechaInicioMantenimiento = null;
                c.FechaFinMantenimiento = null;
                await _db.SaveChangesAsync();
                _cache = null;

                await RetirarAvisosAsync();
                _logger.LogWarning("Mantenimiento cancelado");
                return ResultadoOperacion.Ok();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al cancelar el mantenimiento");
                return ResultadoOperacion.Fallo("No se pudo cancelar la programación.");
            }
        }

        public async Task<ResultadoOperacion> FinalizarAsync()
        {
            try
            {
                var c = await _db.Configuraciones.FirstOrDefaultAsync();
                if (c == null || Calcular(c, DateTime.Now).Estado != EstadoMantenimiento.EnCurso)
                    return ResultadoOperacion.Fallo("Solo se puede finalizar un mantenimiento en curso.");

                c.FechaFinMantenimiento = DateTime.Now; // el estado pasa a "Finalizado" de inmediato
                await _db.SaveChangesAsync();
                _cache = null;

                await RetirarAvisosAsync();
                _logger.LogWarning("Mantenimiento finalizado manualmente");
                return ResultadoOperacion.Ok();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al finalizar el mantenimiento");
                return ResultadoOperacion.Fallo("No se pudo finalizar el mantenimiento.");
            }
        }

        // ================= AVISO A USUARIOS (generación perezosa, igual que las deudas) =================
        // Lo invoca NotificacionService (la campana lo consulta en cada página). Un aviso por usuario y programación:
        // si la programación cambia, GuardarAsync reescribe el mensaje y el aviso sigue siendo único.
        public async Task GenerarAvisoAsync(int idUsuario)
        {
            try
            {
                var info = await ObtenerAsync();
                if (info.Estado != EstadoMantenimiento.Programado)
                {
                    await _db.Notificaciones
                        .Where(n => n.IdUsuario == idUsuario && n.Tipo == TipoNotificacion)
                        .ExecuteDeleteAsync();
                    return;
                }

                var texto = TextoAviso(info.Inicio!.Value, info.Fin!.Value);
                var existe = await _db.Notificaciones.AnyAsync(n =>
                    n.IdUsuario == idUsuario && n.Tipo == TipoNotificacion && n.Mensaje == texto);
                if (existe) return;

                _db.Notificaciones.Add(new Notificacion
                {
                    IdUsuario = idUsuario,
                    Tipo = TipoNotificacion,
                    Titulo = "Mantenimiento programado",
                    Mensaje = texto
                });
                await _db.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                // Nunca debe romper la página que renderiza la campana
                _logger.LogError(ex, "Error al generar el aviso de mantenimiento del usuario {IdUsuario}", idUsuario);
            }
        }

        private Task RetirarAvisosAsync() =>
            _db.Notificaciones.Where(n => n.Tipo == TipoNotificacion).ExecuteDeleteAsync();

        // "Mantenimiento programado para el día 10/11/2026 de 8:00 PM a 10:00 PM."
        public static string TextoAviso(DateTime inicio, DateTime fin) =>
            inicio.Date == fin.Date
                ? FormattableString.Invariant($"Mantenimiento programado para el día {inicio:dd/MM/yyyy} de {inicio:h:mm tt} a {fin:h:mm tt}.")
                : FormattableString.Invariant($"Mantenimiento programado del {inicio:dd/MM/yyyy} {inicio:h:mm tt} al {fin:dd/MM/yyyy} {fin:h:mm tt}.");
    }
}
