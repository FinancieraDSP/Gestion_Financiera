
using Microsoft.EntityFrameworkCore;
using GestionFinancieraPersonal.Datos;
using GestionFinancieraPersonal.Entidades;
using GestionFinancieraPersonal.Models;

namespace GestionFinancieraPersonal.Servicios
{
    public class DeudaService
    {
        private readonly ApplicationDbContext _db;
        private readonly ILogger<DeudaService> _logger;
        private readonly ConfiguracionService _configuracion;

        public DeudaService(ApplicationDbContext db, ILogger<DeudaService> logger, ConfiguracionService configuracion)
        {
            _db = db;
            _logger = logger;
            _configuracion = configuracion;
        }

        // El estado se deduce de los montos y de la fecha de vencimiento
        private static string CalcularEstado(Deuda d)
        {
            if (d.MontoPagado >= d.MontoTotal) return "Pagada";
            if (d.FechaVencimiento.HasValue && d.FechaVencimiento.Value.Date < DateTime.Today) return "Vencida";
            return d.MontoPagado > 0 ? "En proceso" : "Pendiente";
        }

        // Devuelve las deudas del usuario y deja guardado el estado correcto (por ejemplo, si una venció ayer)
        public async Task<List<Deuda>> ObtenerAsync(int idUsuario)
        {
            var deudas = await _db.Deudas.Where(d => d.IdUsuario == idUsuario).ToListAsync();

            var hayCambios = false;
            foreach (var d in deudas)
            {
                var estado = CalcularEstado(d);
                if (d.Estado != estado) { d.Estado = estado; hayCambios = true; }
            }
            if (hayCambios) await _db.SaveChangesAsync();

            // Ventana de "Por vencer" vigente (ConfiguracionSistema.DiasAvisoDeuda): Deuda.PorVencer la necesita
            var diasAviso = (await _configuracion.ObtenerAsync()).DiasAvisoDeuda;
            foreach (var d in deudas)
                d.DiasAvisoAplicado = diasAviso;

            return deudas
                .OrderBy(d => d.Estado == "Pagada")
                .ThenBy(d => d.FechaVencimiento ?? DateTime.MaxValue)
                .ToList();
        }

        // ================= AVISOS DE VENCIMIENTO (generación perezosa) =================
        // Se invoca desde NotificacionService (la campana lo consulta en cada página). No hay tareas programadas:
        // el aviso se crea la primera vez que el usuario abre la aplicación dentro de la ventana de aviso.
        // Una sola notificación por deuda y fecha de vencimiento: la columna aviso_vencimiento_notificado
        // guarda para qué vencimiento ya se avisó y se marca con un UPDATE atómico.
        public async Task GenerarAvisosVencimientoAsync(int idUsuario)
        {
            try
            {
                var diasAviso = (await _configuracion.ObtenerAsync()).DiasAvisoDeuda; // siempre desde la BD
                var hoy = DateTime.Today;
                var limite = hoy.AddDays(diasAviso);

                // Deudas sin pagar con vencimiento dentro de la ventana y sin aviso para ese vencimiento
                var pendientes = await _db.Deudas.AsNoTracking()
                    .Where(d => d.IdUsuario == idUsuario
                                && d.MontoPagado < d.MontoTotal
                                && d.FechaVencimiento != null
                                && d.FechaVencimiento >= hoy && d.FechaVencimiento <= limite
                                && (d.AvisoVencimientoNotificado == null || d.AvisoVencimientoNotificado != d.FechaVencimiento))
                    .Select(d => new { d.IdDeuda, d.Nombre, d.FechaVencimiento })
                    .ToListAsync();

                foreach (var d in pendientes)
                {
                    await using var transaccion = await _db.Database.BeginTransactionAsync();

                    // Solo la petición que consigue marcar la deuda crea la notificación (evita duplicados concurrentes)
                    var marcadas = await _db.Deudas
                        .Where(x => x.IdDeuda == d.IdDeuda
                                    && (x.AvisoVencimientoNotificado == null || x.AvisoVencimientoNotificado != x.FechaVencimiento))
                        .ExecuteUpdateAsync(s => s.SetProperty(x => x.AvisoVencimientoNotificado, x => x.FechaVencimiento));
                    if (marcadas == 0) continue;

                    var diasRestantes = (d.FechaVencimiento!.Value.Date - hoy).Days;
                    var cuando = diasRestantes switch
                    {
                        0 => "vence hoy",
                        1 => "vence en 1 día",
                        _ => $"vence en {diasRestantes} días"
                    };

                    _db.Notificaciones.Add(new Notificacion
                    {
                        IdUsuario = idUsuario,
                        Tipo = "Deuda",
                        Titulo = "Deuda por vencer",
                        Mensaje = $"La deuda {d.Nombre} {cuando}."
                    });
                    await _db.SaveChangesAsync();
                    await transaccion.CommitAsync();
                }
            }
            catch (Exception ex)
            {
                // Un fallo aquí nunca debe romper la página que renderiza la campana
                _logger.LogError(ex, "Error al generar avisos de vencimiento del usuario {IdUsuario}", idUsuario);
            }
        }

        public Task<List<PagoResumenViewModel>> ObtenerPagosRecientesAsync(int idUsuario, int cantidad = 8) =>
            (from p in _db.PagosDeuda.AsNoTracking()
             join d in _db.Deudas.AsNoTracking() on p.IdDeuda equals d.IdDeuda
             where d.IdUsuario == idUsuario
             orderby p.FechaPago descending, p.IdPago descending
             select new PagoResumenViewModel
             {
                 Deuda = d.Nombre,
                 Monto = p.Monto,
                 Fecha = p.FechaPago,
                 Descripcion = p.Descripcion
             }).Take(cantidad).ToListAsync();

        // ================= CREAR DEUDA =================

        public async Task<ResultadoOperacion> CrearAsync(int idUsuario, DeudaFormViewModel f)
        {
            try
            {
                var inicio = f.FechaInicio!.Value.Date;
                var vencimiento = f.FechaVencimiento?.Date;

                if (vencimiento.HasValue && vencimiento.Value < inicio)
                    return ResultadoOperacion.Fallo("El vencimiento no puede ser anterior a la fecha de inicio.", "FechaVencimiento");

                var deuda = new Deuda
                {
                    IdUsuario = idUsuario,
                    Nombre = f.Nombre.Trim(),
                    Acreedor = f.Acreedor.Trim(),
                    MontoTotal = f.MontoTotal!.Value,
                    MontoPagado = 0,
                    TasaInteres = f.TasaInteres ?? 0,
                    FechaInicio = inicio,
                    FechaVencimiento = vencimiento
                };
                deuda.Estado = CalcularEstado(deuda);

                _db.Deudas.Add(deuda);
                await _db.SaveChangesAsync();
                return ResultadoOperacion.Ok();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al crear deuda del usuario {IdUsuario}", idUsuario);
                return ResultadoOperacion.Fallo("No se pudo crear la deuda. Intenta de nuevo.");
            }
        }

        // ================= REGISTRAR PAGO =================

        public async Task<ResultadoOperacion> PagarAsync(int idUsuario, PagoDeudaFormViewModel f)
        {
            try
            {
                var fecha = (f.Fecha ?? DateTime.Today).Date;
                if (fecha > DateTime.Today)
                    return ResultadoOperacion.Fallo("La fecha de pago no puede ser futura.", "Fecha");

                var deuda = await _db.Deudas.FirstOrDefaultAsync(d => d.IdDeuda == f.IdDeuda && d.IdUsuario == idUsuario);
                if (deuda == null)
                    return ResultadoOperacion.Fallo("La deuda seleccionada no existe.", "IdDeuda");
                if (deuda.MontoPagado >= deuda.MontoTotal)
                    return ResultadoOperacion.Fallo("Esta deuda ya está pagada.", "IdDeuda");

                var cuenta = await _db.Cuentas.FirstOrDefaultAsync(c =>
                    c.IdCuenta == f.IdCuenta && c.IdUsuario == idUsuario && c.Estado);
                if (cuenta == null)
                    return ResultadoOperacion.Fallo("La cuenta seleccionada no es válida.", "IdCuenta");

                var monto = f.Monto!.Value;
                var pendiente = deuda.MontoTotal - deuda.MontoPagado;

                // Regla: no pagar más de lo que se debe (CHECK monto_pagado <= monto_total)
                if (monto > pendiente)
                    return ResultadoOperacion.Fallo($"El pago supera el monto pendiente ({pendiente:C}).", "Monto");

                // Regla: no pagar con más dinero del que hay en la cuenta
                if (cuenta.SaldoActual < monto)
                    return ResultadoOperacion.Fallo($"Saldo insuficiente en {cuenta.Nombre}. Disponible: {cuenta.SaldoActual:C}.", "Monto");

                _db.PagosDeuda.Add(new PagoDeuda
                {
                    IdDeuda = deuda.IdDeuda,
                    IdCuenta = cuenta.IdCuenta,
                    Monto = monto,
                    FechaPago = fecha,
                    Descripcion = string.IsNullOrWhiteSpace(f.Descripcion) ? null : f.Descripcion.Trim()
                });

                cuenta.SaldoActual -= monto;
                deuda.MontoPagado += monto;
                deuda.Estado = CalcularEstado(deuda);

                if (deuda.Estado == "Pagada")
                {
                    _db.Notificaciones.Add(new Notificacion
                    {
                        IdUsuario = idUsuario,
                        Tipo = "Deuda",
                        Titulo = "¡Deuda pagada!",
                        Mensaje = $"Saldaste por completo la deuda \"{deuda.Nombre}\"."
                    });
                }

                await _db.SaveChangesAsync();
                return ResultadoOperacion.Ok();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al registrar pago del usuario {IdUsuario}", idUsuario);
                return ResultadoOperacion.Fallo("No se pudo registrar el pago. Intenta de nuevo.");
            }
        }

        // ================= ELIMINAR (solo si no tiene pagos) =================

        public async Task<ResultadoOperacion> EliminarAsync(int idUsuario, int idDeuda)
        {
            try
            {
                var deuda = await _db.Deudas.FirstOrDefaultAsync(d => d.IdDeuda == idDeuda && d.IdUsuario == idUsuario);
                if (deuda == null) return ResultadoOperacion.Fallo("La deuda no existe.");

                if (deuda.MontoPagado > 0 || await _db.PagosDeuda.AnyAsync(p => p.IdDeuda == idDeuda))
                    return ResultadoOperacion.Fallo("No se puede eliminar una deuda con pagos registrados.");

                _db.Deudas.Remove(deuda);
                await _db.SaveChangesAsync();
                return ResultadoOperacion.Ok();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al eliminar deuda {IdDeuda}", idDeuda);
                return ResultadoOperacion.Fallo("No se pudo eliminar la deuda.");
            }
        }
    }
}

