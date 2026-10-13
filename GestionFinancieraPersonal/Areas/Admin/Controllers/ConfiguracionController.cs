using GestionFinancieraPersonal.Areas.Admin.Datos;
using GestionFinancieraPersonal.Areas.Admin.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GestionFinancieraPersonal.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Administrador")]
    public class ConfiguracionController : Controller
    {
        // GET: /Admin/Configuracion
        public IActionResult Index()
        {
            return View(AdminDatosSimulados.Configuracion);
        }

        // POST: /Admin/Configuracion/Guardar
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Guardar(ConfiguracionSistema modelo)
        {
            AdminDatosSimulados.Configuracion = modelo;
            return RedirectToAction("Index");
        }
    }
}
