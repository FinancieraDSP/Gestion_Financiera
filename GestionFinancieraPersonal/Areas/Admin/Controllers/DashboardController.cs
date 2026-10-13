using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using GestionFinancieraPersonal.Areas.Admin.Models;
using GestionFinancieraPersonal.Datos;
using GestionFinancieraPersonal.Servicios;

namespace GestionFinancieraPersonal.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Administrador")]
    public class DashboardController : Controller
    {
        private readonly ApplicationDbContext _db;

        public DashboardController(ApplicationDbContext db)
        {
            _db = db;
        }

        // GET: /Admin/Dashboard
        public async Task<IActionResult> Index()
        {
            var hoy = DateTime.Today;

            // KPIs
            ViewBag.UsuariosTotales = await _db.Usuarios.CountAsync(u => u.Rol.Nombre == "Usuario");
            ViewBag.UsuariosActivos = await _db.Usuarios.CountAsync(u => u.Rol.Nombre == "Usuario" && u.Estado);
            ViewBag.CategoriasActivas = await _db.Categorias.CountAsync(c => c.IdUsuario == null && c.Estado);
            ViewBag.RegistrosHoy =
                await _db.Ingresos.CountAsync(i => i.Fecha == hoy) +
                await _db.Gastos.CountAsync(g => g.Fecha == hoy);

            // Movimientos (ingresos + gastos) de los últimos 5 meses
            var primerMes = new DateTime(hoy.Year, hoy.Month, 1).AddMonths(-4);

            var ingresosPorMes = await _db.Ingresos
                .Where(i => i.Fecha >= primerMes)
                .GroupBy(i => new { i.Fecha.Year, i.Fecha.Month })
                .Select(g => new { g.Key.Year, g.Key.Month, Total = g.Count() })
                .ToListAsync();

            var gastosPorMes = await _db.Gastos
                .Where(g => g.Fecha >= primerMes)
                .GroupBy(g => new { g.Fecha.Year, g.Fecha.Month })
                .Select(g => new { g.Key.Year, g.Key.Month, Total = g.Count() })
                .ToListAsync();

            var movimientos = new List<int>();
            var etiquetas = new List<string>();
            for (var k = 4; k >= 0; k--)
            {
                var mes = new DateTime(hoy.Year, hoy.Month, 1).AddMonths(-k);
                movimientos.Add(
                    ingresosPorMes.Where(x => x.Year == mes.Year && x.Month == mes.Month).Sum(x => x.Total) +
                    gastosPorMes.Where(x => x.Year == mes.Year && x.Month == mes.Month).Sum(x => x.Total));
                etiquetas.Add(FormatoFechas.MesCorto(mes));
            }
            ViewBag.MovimientosPorMes = movimientos;
            ViewBag.MesesEtiquetas = etiquetas;

            // Categorías generales y su uso (solo conteos agregados, nunca movimientos individuales)
            var totalGastos = await _db.Gastos.CountAsync();
            var usoPorCategoria = await _db.Gastos
                .GroupBy(g => g.IdCategoria)
                .Select(g => new { IdCategoria = g.Key, Total = g.Count() })
                .ToDictionaryAsync(x => x.IdCategoria, x => x.Total);

            var categorias = await _db.Categorias.AsNoTracking()
                .Where(c => c.IdUsuario == null)
                .OrderBy(c => c.Tipo).ThenBy(c => c.Nombre)
                .ToListAsync();

            var modelo = categorias.Select(c => new CategoriaGeneral
            {
                Id = c.IdCategoria,
                Nombre = c.Nombre,
                Tipo = c.Tipo,
                Activa = c.Estado,
                PorcentajeUso = (totalGastos == 0 || c.Tipo != "Gasto")
                    ? 0
                    : Math.Round((decimal)usoPorCategoria.GetValueOrDefault(c.IdCategoria) / totalGastos * 100, 0)
            }).ToList();

            return View(modelo);
        }
    }
}