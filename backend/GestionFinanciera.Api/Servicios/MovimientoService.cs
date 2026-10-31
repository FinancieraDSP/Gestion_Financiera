
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using GestionFinancieraPersonal.Datos;
using GestionFinancieraPersonal.Entidades;
using GestionFinancieraPersonal.Models;

namespace GestionFinancieraPersonal.Servicios
{
    public class MovimientoService
    {
        private readonly ApplicationDbContext _db;
        private readonly ILogger<MovimientoService> _logger;
        private readonly ConfiguracionService _configuracion;

        public MovimientoService(ApplicationDbContext db, ILogger<MovimientoService> logger, ConfiguracionService configuracion)
        {
            _db = db;
            _logger = logger;
            _configuracion = configuracion;
        }

        // ================= CONSULTAS (siempre filtradas por usuario) =================

        public Task<List<Gasto>> ObtenerGastosAsync(int idUsuario) =>
            _db.Gastos.AsNoTracking()
                .Include(g => g.Categoria).Include(g => g.Cuenta)
                .Where(g => g.IdUsuario == idUsuario)
                .OrderByDescending(g => g.Fecha).ThenByDescending(g => g.IdGasto)
                .ToListAsync();

        public Task<List<Ingreso>> ObtenerIngresosAsync(int idUsuario) =>
            _db.Ingresos.AsNoTracking()
                .Include(i => i.Categoria).Include(i => i.Cuenta)
                .Where(i => i.IdUsuario == idUsuario)
                .OrderByDescending(i => i.Fecha).ThenByDescending(i => i.IdIngreso)
                .ToListAsync();

        public async Task<List<SelectListItem>> ObtenerCuentasAsync(int idUsuario, int? seleccionada = null)
        {
            var cuentas = await _db.Cuentas.AsNoTracking()
                .Where(c => c.IdUsuario == idUsuario && c.Estado)
                .OrderBy(c => c.Nombre)
                .ToListAsync();

            return cuentas
                .Select(c => new SelectListItem($"{c.Nombre} ({c.SaldoActual:C})", c.IdCuenta.ToString(), c.IdCuenta == seleccionada))
                .ToList();
        }

        public async Task<List<SelectListItem>> ObtenerCategoriasAsync(int idUsuario, string tipo, int? seleccionada = null)
        {
            var categorias = await _db.Categorias.AsNoTracking()
                .Where(c => c.Tipo == tipo && c.Estado && (c.IdUsuario == null || c.IdUsuario == idUsuario))
                .OrderBy(c => c.Nombre)
                .ToListAsync();

            return categorias
                .Select(c => new SelectListItem(c.Nombre, c.IdCategoria.ToString(), c.IdCategoria == seleccionada))
                .ToList();
        }

        // ================= REGISTRAR GASTO =================

        public async Task<ResultadoOperacion> RegistrarGastoAsync(int idUsuario, MovimientoFormViewModel f)
        {
            var fecha = f.Fecha!.Value.Date;
            var monto = f.Monto!.Value;

            if (fecha > DateTime.Today)
                return ResultadoOperacion.Fallo("La fecha no puede ser futura.", "Fecha");

            try
            {
                var (cuenta, categoria) = await ObtenerCuentaYCategoriaAsync(idUsuario, f.IdCuenta!.Value, f.IdCategoria!.Value, "Gasto");
                if (cuenta == null) return ResultadoOperacion.Fallo("La cuenta seleccionada no es válida.", "IdCuenta");
                if (categoria == null) return ResultadoOperacion.Fallo("La categoría seleccionada no es válida.", "IdCategoria");

                // Regla: no gastar más de lo que hay en la cuenta
                if (cuenta.SaldoActual < monto)
                    return ResultadoOperacion.Fallo($"Saldo insuficiente en {cuenta.Nombre}. Disponible: {cuenta.SaldoActual:C}.", "Monto");

                _db.Gastos.Add(new Gasto
                {
                    IdUsuario = idUsuario,
                    IdCuenta = cuenta.IdCuenta,
                    IdCategoria = categoria.IdCategoria,
                    Descripcion = f.Descripcion.Trim(),
                    Monto = monto,
                    Fecha = fecha,
                    Comercio = string.IsNullOrWhiteSpace(f.Detalle) ? null : f.Detalle.Trim()
                });

                // Regla: el gasto descuenta el saldo de la cuenta
                cuenta.SaldoActual -= monto;

                /* Regla: el gasto suma al presupuesto de esa categoría (si hay uno vigente en esa fecha)
                 y, si cruza el umbral configurado por el administrador o el 100%, genera una notificación,
                 las notificaciones se guardan en el mismo SaveChanges que el gasto */
                var presupuesto = await BuscarPresupuestoAsync(idUsuario, categoria.IdCategoria, fecha);
                if (presupuesto != null)
                {
                    var umbral = (await _configuracion.ObtenerAsync()).UmbralPresupuesto; // siempre desde la BD
                    var usoAntes = AlertasPresupuesto.Uso(presupuesto);
                    presupuesto.MontoGastado += monto;
                    var alerta = AlertasPresupuesto.Evaluar(idUsuario, categoria.Nombre, usoAntes, AlertasPresupuesto.Uso(presupuesto), umbral);
                    if (alerta != null) _db.Notificaciones.Add(alerta);
                }

                await _db.SaveChangesAsync(); // una sola transacción: o se guarda todo o nada
                return ResultadoOperacion.Ok();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al registrar gasto del usuario {IdUsuario}", idUsuario);
                return ResultadoOperacion.Fallo("No se pudo registrar el gasto. Intenta de nuevo.");
            }
        }

        // ================= REGISTRAR INGRESO =================

        public async Task<ResultadoOperacion> RegistrarIngresoAsync(int idUsuario, MovimientoFormViewModel f)
        {
            var fecha = f.Fecha!.Value.Date;
            var monto = f.Monto!.Value;

            if (fecha > DateTime.Today)
                return ResultadoOperacion.Fallo("La fecha no puede ser futura.", "Fecha");

            try
            {
                var (cuenta, categoria) = await ObtenerCuentaYCategoriaAsync(idUsuario, f.IdCuenta!.Value, f.IdCategoria!.Value, "Ingreso");
                if (cuenta == null) return ResultadoOperacion.Fallo("La cuenta seleccionada no es válida.", "IdCuenta");
                if (categoria == null) return ResultadoOperacion.Fallo("La categoría seleccionada no es válida.", "IdCategoria");

                _db.Ingresos.Add(new Ingreso
                {
                    IdUsuario = idUsuario,
                    IdCuenta = cuenta.IdCuenta,
                    IdCategoria = categoria.IdCategoria,
                    Descripcion = f.Descripcion.Trim(),
                    Monto = monto,
                    Fecha = fecha,
                    Origen = string.IsNullOrWhiteSpace(f.Detalle) ? null : f.Detalle.Trim()
                });

                // Regla: el ingreso suma al saldo de la cuenta
                cuenta.SaldoActual += monto;

                await _db.SaveChangesAsync();
                return ResultadoOperacion.Ok();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al registrar ingreso del usuario {IdUsuario}", idUsuario);
                return ResultadoOperacion.Fallo("No se pudo registrar el ingreso. Intenta de nuevo.");
            }
        }

        // ================= ELIMINAR (revierte saldo y presupuesto) =================

        public async Task<ResultadoOperacion> EliminarGastoAsync(int idUsuario, int idGasto)
        {
            try
            {
                var gasto = await _db.Gastos.FirstOrDefaultAsync(g => g.IdGasto == idGasto && g.IdUsuario == idUsuario);
                if (gasto == null) return ResultadoOperacion.Fallo("El gasto no existe.");

                var cuenta = await _db.Cuentas.FirstAsync(c => c.IdCuenta == gasto.IdCuenta);
                cuenta.SaldoActual += gasto.Monto;

                var presupuesto = await BuscarPresupuestoAsync(idUsuario, gasto.IdCategoria, gasto.Fecha);
                if (presupuesto != null)
                    presupuesto.MontoGastado = Math.Max(0, presupuesto.MontoGastado - gasto.Monto);

                _db.Gastos.Remove(gasto);
                await _db.SaveChangesAsync();
                return ResultadoOperacion.Ok();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al eliminar gasto {IdGasto}", idGasto);
                return ResultadoOperacion.Fallo("No se pudo eliminar el gasto.");
            }
        }

        public async Task<ResultadoOperacion> EliminarIngresoAsync(int idUsuario, int idIngreso)
        {
            try
            {
                var ingreso = await _db.Ingresos.FirstOrDefaultAsync(i => i.IdIngreso == idIngreso && i.IdUsuario == idUsuario);
                if (ingreso == null) return ResultadoOperacion.Fallo("El ingreso no existe.");

                var cuenta = await _db.Cuentas.FirstAsync(c => c.IdCuenta == ingreso.IdCuenta);
                if (cuenta.SaldoActual < ingreso.Monto)
                    return ResultadoOperacion.Fallo($"No se puede eliminar: el saldo de {cuenta.Nombre} quedaría negativo.");

                cuenta.SaldoActual -= ingreso.Monto;
                _db.Ingresos.Remove(ingreso);
                await _db.SaveChangesAsync();
                return ResultadoOperacion.Ok();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al eliminar ingreso {IdIngreso}", idIngreso);
                return ResultadoOperacion.Fallo("No se pudo eliminar el ingreso.");
            }
        }

        // ================= AUXILIARES =================

        // Verifica que la cuenta y la categoría existan Y pertenezcan al usuario (o sean globales)
        private async Task<(Cuenta? cuenta, Categoria? categoria)> ObtenerCuentaYCategoriaAsync(
            int idUsuario, int idCuenta, int idCategoria, string tipo)
        {
            var cuenta = await _db.Cuentas.FirstOrDefaultAsync(c =>
                c.IdCuenta == idCuenta && c.IdUsuario == idUsuario && c.Estado);

            var categoria = await _db.Categorias.FirstOrDefaultAsync(c =>
                c.IdCategoria == idCategoria && c.Tipo == tipo && c.Estado &&
                (c.IdUsuario == null || c.IdUsuario == idUsuario));

            return (cuenta, categoria);
        }

        private Task<Presupuesto?> BuscarPresupuestoAsync(int idUsuario, int idCategoria, DateTime fecha) =>
            _db.Presupuestos.FirstOrDefaultAsync(p =>
                p.IdUsuario == idUsuario && p.IdCategoria == idCategoria && p.Estado &&
                p.PeriodoInicio <= fecha && p.PeriodoFin >= fecha);
    }
}

