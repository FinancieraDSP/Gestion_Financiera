
using Microsoft.EntityFrameworkCore;
using GestionFinancieraPersonal.Datos;
using GestionFinancieraPersonal.Entidades;
using GestionFinancieraPersonal.Models;

namespace GestionFinancieraPersonal.Servicios
{
    public class PresupuestoService
    {
        private readonly ApplicationDbContext _db;
        private readonly ILogger<PresupuestoService> _logger;
        private readonly ConfiguracionService _configuracion;

        public PresupuestoService(ApplicationDbContext db, ILogger<PresupuestoService> logger, ConfiguracionService configuracion)
        {
            _db = db;
            _logger = logger;
            _configuracion = configuracion;
        }

        // Los presupuestos son mensuales: del día 1 al último día del mes actual
        private static (DateTime inicio, DateTime fin) PeriodoActual()
        {
            var hoy = DateTime.Today;
            var inicio = new DateTime(hoy.Year, hoy.Month, 1);
            return (inicio, inicio.AddMonths(1).AddDays(-1));
        }

        private IQueryable<Presupuesto> PresupuestosDelMes(int idUsuario)
        {
            var (inicio, fin) = PeriodoActual();
            return _db.Presupuestos.Where(p =>
                p.IdUsuario == idUsuario && p.Estado && p.PeriodoInicio <= fin && p.PeriodoFin >= inicio);
        }

        // Devuelve los presupuestos del mes con el umbral vigente ya asignado (Presupuesto.EnAlerta lo necesita)
        public async Task<List<Presupuesto>> ObtenerDelMesAsync(int idUsuario)
        {
            var presupuestos = await PresupuestosDelMes(idUsuario).AsNoTracking()
                .Include(p => p.Categoria)
                .OrderBy(p => p.Categoria.Nombre)
                .ToListAsync();

            var umbral = (await _configuracion.ObtenerAsync()).UmbralPresupuesto;
            foreach (var p in presupuestos)
                p.UmbralAplicado = umbral;

            return presupuestos;
        }

        // Crea el presupuesto del mes para la categoría, o actualiza su límite si ya existe
        public async Task<ResultadoOperacion> GuardarAsync(int idUsuario, PresupuestoFormViewModel f)
        {
            try
            {
                var categoria = await _db.Categorias.FirstOrDefaultAsync(c =>
                    c.IdCategoria == f.IdCategoria && c.Tipo == "Gasto" && c.Estado &&
                    (c.IdUsuario == null || c.IdUsuario == idUsuario));
                if (categoria == null)
                    return ResultadoOperacion.Fallo("La categoría seleccionada no es válida.", "IdCategoria");

                var (inicio, fin) = PeriodoActual();

                // Lo que ya se gastó este mes en esa categoría
                var gastado = await _db.Gastos
                    .Where(g => g.IdUsuario == idUsuario && g.IdCategoria == categoria.IdCategoria &&
                                g.Fecha >= inicio && g.Fecha <= fin)
                    .SumAsync(g => (decimal?)g.Monto) ?? 0;

                var presupuesto = await PresupuestosDelMes(idUsuario)
                    .FirstOrDefaultAsync(p => p.IdCategoria == categoria.IdCategoria);

                decimal usoAntes;
                if (presupuesto == null)
                {
                    usoAntes = 0;
                    presupuesto = new Presupuesto
                    {
                        IdUsuario = idUsuario,
                        IdCategoria = categoria.IdCategoria,
                        MontoLimite = f.Limite!.Value,
                        MontoGastado = gastado,
                        PeriodoInicio = inicio,
                        PeriodoFin = fin
                    };
                    _db.Presupuestos.Add(presupuesto);
                }
                else
                {
                    usoAntes = AlertasPresupuesto.Uso(presupuesto);
                    presupuesto.MontoLimite = f.Limite!.Value;
                    presupuesto.MontoGastado = gastado;
                }

                var umbral = (await _configuracion.ObtenerAsync()).UmbralPresupuesto; // siempre desde la BD
                var alerta = AlertasPresupuesto.Evaluar(idUsuario, categoria.Nombre, usoAntes, AlertasPresupuesto.Uso(presupuesto), umbral);
                if (alerta != null) _db.Notificaciones.Add(alerta);

                await _db.SaveChangesAsync();
                return ResultadoOperacion.Ok();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al guardar presupuesto del usuario {IdUsuario}", idUsuario);
                return ResultadoOperacion.Fallo("No se pudo guardar el presupuesto. Intenta de nuevo.");
            }
        }

        public async Task<ResultadoOperacion> EliminarAsync(int idUsuario, int idPresupuesto)
        {
            try
            {
                var presupuesto = await _db.Presupuestos
                    .FirstOrDefaultAsync(p => p.IdPresupuesto == idPresupuesto && p.IdUsuario == idUsuario);
                if (presupuesto == null) return ResultadoOperacion.Fallo("El presupuesto no existe.");

                _db.Presupuestos.Remove(presupuesto);
                await _db.SaveChangesAsync();
                return ResultadoOperacion.Ok();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al eliminar presupuesto {IdPresupuesto}", idPresupuesto);
                return ResultadoOperacion.Fallo("No se pudo eliminar el presupuesto.");
            }
        }
    }
}

