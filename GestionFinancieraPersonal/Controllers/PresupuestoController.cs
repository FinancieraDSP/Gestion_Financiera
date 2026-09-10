using Microsoft.AspNetCore.Mvc;
using GestionFinancieraPersonal.Datos;

namespace GestionFinancieraPersonal.Controllers
{
    public class PresupuestoController : Controller
    {
        public IActionResult Index()
        {
            return View(DatosSimulados.Presupuesto);
        }
    }
}
