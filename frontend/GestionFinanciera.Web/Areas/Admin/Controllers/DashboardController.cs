using GestionFinancieraPersonal.Models;
using GestionFinancieraPersonal.Servicios;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GestionFinancieraPersonal.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Administrador")]
    public class DashboardController : Controller
    {
        private readonly ApiClient _api;

        public DashboardController(ApiClient api)
        {
            _api = api;
        }

        // GET: /Admin/Dashboard
        public async Task<IActionResult> Index()
        {
            var datos = (await _api.GetAsync<AdminDashboardResponse>("api/admin/dashboard")).Requerir();

            // La vista lee estos valores del ViewBag, igual que antes
            ViewBag.UsuariosTotales = datos.UsuariosTotales;
            ViewBag.UsuariosActivos = datos.UsuariosActivos;
            ViewBag.CategoriasActivas = datos.CategoriasActivas;
            ViewBag.RegistrosHoy = datos.RegistrosHoy;
            ViewBag.MovimientosPorMes = datos.MovimientosPorMes;
            ViewBag.MesesEtiquetas = datos.MesesEtiquetas;

            return View(datos.Categorias);
        }
    }
}