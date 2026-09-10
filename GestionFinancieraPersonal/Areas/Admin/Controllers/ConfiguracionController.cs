using Microsoft.AspNetCore.Mvc;
using GestionFinancieraPersonal.Areas.Admin.Datos;
using GestionFinancieraPersonal.Areas.Admin.Models;

namespace GestionFinancieraPersonal.Areas.Admin.Controllers
{
    [Area("Admin")]
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
