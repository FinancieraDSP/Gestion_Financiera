using GestionFinancieraPersonal.Areas.Admin.Models;
using GestionFinancieraPersonal.Datos;
using GestionFinancieraPersonal.Models;
using GestionFinancieraPersonal.Servicios;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace GestionFinancieraPersonal.Controllers.Admin
{
    [ApiController]
    [Authorize(Roles = "Administrador")]
    [Route("api/admin/dashboard")]
    public class AdminDashboardController : ControllerBase
    {
        private readonly ApplicationDbContext _db;

        public AdminDashboardController(ApplicationDbContext db)
        {
            _db = db;
        }

        // GET /api/admin/dashboard
        [HttpGet]
        public async Task<ActionResult<AdminDashboardResponse>> Obtener()
        {
            var hoy = DateTime.Today;

            // KPIs
            var usuariosTotales = await _db.Usuarios.CountAsync(u => u.Rol.Nombre == "Usuario");
            var usuariosActivos = await _db.Usuarios.CountAsync(u => u.Rol.Nombre == "Usuario" && u.Estado);
            var categoriasActivas = await _db.Categorias.CountAsync(c => c.IdUsuario == null && c.Estado);
            var registrosHoy =
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

            return new AdminDashboardResponse
            {
                UsuariosTotales = usuariosTotales,
                UsuariosActivos = usuariosActivos,
                CategoriasActivas = categoriasActivas,
                RegistrosHoy = registrosHoy,
                MovimientosPorMes = movimientos,
                MesesEtiquetas = etiquetas,
                Categorias = categorias.Select(c => new CategoriaGeneral
                {
                    Id = c.IdCategoria,
                    Nombre = c.Nombre,
                    Tipo = c.Tipo,
                    Activa = c.Estado,
                    PorcentajeUso = (totalGastos == 0 || c.Tipo != "Gasto")
                        ? 0
                        : Math.Round((decimal)usoPorCategoria.GetValueOrDefault(c.IdCategoria) / totalGastos * 100, 0)
                }).ToList()
            };
        }
    }
}