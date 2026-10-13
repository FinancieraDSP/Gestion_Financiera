using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using GestionFinancieraPersonal.Datos;
using GestionFinancieraPersonal.Models;
using GestionFinancieraPersonal.Servicios;

namespace GestionFinancieraPersonal.Controllers
{
    [Authorize(Roles = "Usuario")]
    public class DashboardController : Controller
    {
        private readonly ApplicationDbContext _db;

        public DashboardController(ApplicationDbContext db)
        {
            _db = db;
        }

        public async Task<IActionResult> Index()
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

            var modelo = new DashboardViewModel
            {
                NombreUsuario = User.ObtenerNombre().Split(' ', StringSplitOptions.RemoveEmptyEntries).FirstOrDefault() ?? "",
                PeriodoActual = $"Resumen de {FormatoFechas.MesAnio(hoy)}",
                SaldoDisponible = saldoDisponible,
                TotalIngresos = totalIngresos,
                TotalGastos = totalGastos,
                AhorroMensual = totalIngresos - totalGastos, // lo que sobró del mes
                GastosPorCategoria = porCategoria.ToDictionary(x => x.Nombre, x => x.Total),
                UltimosGastos = ultimosGastos
            };
            return View(modelo);
        }
    }
}