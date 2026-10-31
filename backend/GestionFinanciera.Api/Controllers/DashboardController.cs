using GestionFinancieraPersonal.Datos;
using GestionFinancieraPersonal.Models;
using GestionFinancieraPersonal.Servicios;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace GestionFinancieraPersonal.Controllers
{
    [ApiController]
    [Authorize(Roles = "Usuario")]
    [Route("api/dashboard")]
    public class DashboardController : ControllerBase
    {
        private readonly ApplicationDbContext _db;

        public DashboardController(ApplicationDbContext db)
        {
            _db = db;
        }

        // GET /api/dashboard
        [HttpGet]
        public async Task<ActionResult<DashboardViewModel>> Obtener()
        {
            var idUsuario = User.ObtenerIdUsuario();
            var hoy = DateTime.Today;
            var inicioMes = new DateTime(hoy.Year, hoy.Month, 1);
            var inicioSiguiente = inicioMes.AddMonths(1);

            var totalIngresos = await _db.Ingresos
                .Where(i => i.IdUsuario == idUsuario && i.Fecha >= inicioMes && i.Fecha < inicioSiguiente)
                .SumAsync(i => (decimal?)i.Monto) ?? 0;

            var totalGastos = await _db.Gastos
                .Where(g => g.IdUsuario == idUsuario && g.Fecha >= inicioMes && g.Fecha < inicioSiguiente)
                .SumAsync(g => (decimal?)g.Monto) ?? 0;

            var saldoDisponible = await _db.Cuentas
                .Where(c => c.IdUsuario == idUsuario && c.Estado)
                .SumAsync(c => (decimal?)c.SaldoActual) ?? 0;

            var porCategoria = await _db.Gastos
                .Where(g => g.IdUsuario == idUsuario && g.Fecha >= inicioMes && g.Fecha < inicioSiguiente)
                .GroupBy(g => g.Categoria.Nombre)
                .Select(g => new { Nombre = g.Key, Total = g.Sum(x => x.Monto) })
                .OrderByDescending(x => x.Total)
                .ToListAsync();

            var ultimosGastos = await _db.Gastos.AsNoTracking()
                .Include(g => g.Categoria)
                .Where(g => g.IdUsuario == idUsuario)
                .OrderByDescending(g => g.Fecha).ThenByDescending(g => g.IdGasto)
                .Take(3)
                .ToListAsync();

            return new DashboardViewModel
            {
                NombreUsuario = User.ObtenerNombre().Split(' ', StringSplitOptions.RemoveEmptyEntries).FirstOrDefault() ?? "",
                PeriodoActual = $"Resumen de {FormatoFechas.MesAnio(hoy)}",
                SaldoDisponible = saldoDisponible,
                TotalIngresos = totalIngresos,
                TotalGastos = totalGastos,
                AhorroMensual = totalIngresos - totalGastos,
                GastosPorCategoria = porCategoria.ToDictionary(x => x.Nombre, x => x.Total),
                UltimosGastos = ultimosGastos
            };
        }
    }
}