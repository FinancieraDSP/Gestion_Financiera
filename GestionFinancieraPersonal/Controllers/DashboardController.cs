using Microsoft.AspNetCore.Mvc;
using GestionFinancieraPersonal.Datos;
using GestionFinancieraPersonal.Models;

namespace GestionFinancieraPersonal.Controllers
{
    public class DashboardController : Controller
    {
        public IActionResult Index()
        {
            var modelo = new DashboardViewModel
            {
                NombreUsuario = "Carlos",
                PeriodoActual = "Resumen de julio 2026",
                SaldoDisponible = DatosSimulados.SaldoDisponible,
                TotalIngresos = DatosSimulados.TotalIngresos,
                TotalGastos = DatosSimulados.TotalGastos,
                AhorroMensual = 430.00m, // placeholder hasta tener regla de cálculo real
                GastosPorCategoria = DatosSimulados.GastosPorCategoria(),
                UltimosGastos = DatosSimulados.Gastos.OrderByDescending(g => g.Fecha).Take(3).ToList()
            };
            return View(modelo);
        }
    }
}
