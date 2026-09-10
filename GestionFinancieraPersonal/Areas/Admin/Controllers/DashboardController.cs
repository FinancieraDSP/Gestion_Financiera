using Microsoft.AspNetCore.Mvc;
using GestionFinancieraPersonal.Areas.Admin.Datos;

namespace GestionFinancieraPersonal.Areas.Admin.Controllers
{
    [Area("Admin")]
    // [Authorize(Roles = "Administrador")] // habilitar cuando exista autenticación real por rol
    public class DashboardController : Controller
    {
        // GET: /Admin/Dashboard
        public IActionResult Index()
        {
            ViewBag.UsuariosTotales = AdminDatosSimulados.UsuariosTotales;
            ViewBag.UsuariosActivos = AdminDatosSimulados.UsuariosActivos;
            ViewBag.CategoriasActivas = AdminDatosSimulados.CategoriasActivas;
            ViewBag.RegistrosHoy = 6; // placeholder hasta tener conteo real por fecha
            ViewBag.MovimientosPorMes = AdminDatosSimulados.MovimientosPorMes;
            return View(AdminDatosSimulados.Categorias);
        }
    }
}
